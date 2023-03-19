using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Sensors;
using UnityEditorInternal;
using UnityEngine;

public class AIStoreController : Agent
{
    StoreController store;
    Teacher teacher;
    StockManager sm;

    bool isBankrupt = false;

    private void Awake()
    {
        store = GetComponent<StoreController>();
        teacher = GameObject.Find("Academy").GetComponent<Teacher>();
        sm = GameObject.Find("SimulationController").GetComponent<StockManager>();
        store.isAI = true;
        store.Awake();
    }

    void Update()
    {
        store.Update();
        if (store.step != 0 && store.step % MLParams.TRANSACTION_DELTA == 0 && !store.isSelling)
        {
            RequestDecision();
            if (store.step != 0 && store.step % (MLParams.TRANSACTION_DELTA * 5) == 0)
                EndEpoch();
            store.isSelling = true;
        }
    }

    private void EndEpoch()
    {
        // tax
        float taxReward = (store.GetBalance() - store.GetTotalTax()) / (100 * Mathf.Pow(store.GetLevel(), 3));
        store.Tax(StoreParams.BASE_TAX);
        AddReward(taxReward);

        // finish episode
        store.step = 0;
        EndEpisode();

        if (isBankrupt)
            Initialize();
    }

    public override void Initialize()
    {
        if (!isBankrupt)
            return;

        base.Initialize();
        DestroyImmediate(store);
        store = gameObject.AddComponent(typeof(StoreController)) as StoreController;
        store.isAI = true;
        isBankrupt = false;
    }

    public override void OnEpisodeBegin()
    {
        base.OnEpisodeBegin();

        // restock
        if (store.GetBalance() < 0)
        {
            AddReward(-20);
            isBankrupt = true;
        }
        else
            AddReward(5);

        store.isSelling = true;
        for (int i = 0; i < 5; i++)
            for (int j = 0; j < MLParams.TRANSACTION_DELTA; j++)
                store.SafeEnqueue(teacher.GetACustomer());
    }

    public override void CollectObservations(VectorSensor sensor)
    {
        base.CollectObservations(sensor);
        Debug.Log("Collecting observations on step " + store.step);

        List<int> sold = new List<int> { 0, 0, 0, 0, 0 };   // validate to always contain 5 values
        for (int i = 0; i < store.GetSoldProducts().Count; i++)
            sold[i] = store.GetSoldProducts()[i];

        // delta sold reward function
        List<int> held = new List<int> { 0, 0, 0, 0, 0 };
        var prods = store.GetProducts().Select(p => p.Value.amount).ToList();
        for (int i = 0; i < prods.Count; i++)
            held.Add(prods[i]);
        var rewards = sm.GetRewardsPerProduct(sold, held);
        rewards.ForEach(reward => AddReward(reward));
        store.ClearSoldProducts();

        // collect the total inputs from the store, 13 in total.
        sensor.AddObservation(sold.Select(s => (float)s).ToList()); // 5
        sensor.AddObservation(sm.GetAllAvgPrices()); // 5
        sensor.AddObservation(store.GetBalance());
        sensor.AddObservation(store.GetTotalTax());
        sensor.AddObservation(store.GetLevel());

        //Debug.Log("SIZE: " + sensor.ObservationSize());
    }

    public override void OnActionReceived(ActionBuffers actions)
    {
        base.OnActionReceived(actions);

        // get the 11 outputs from the model
        var outputs = actions.ContinuousActions.ToList();
        var dPrices = outputs.GetRange(0, 5);
        var dITs = outputs.GetRange(5, 5);
        var upgradeProb = outputs[10];

        store.UpdateFromModelOutput(outputs); // take decisions based on model output
    }

    /*async public bool checkIsQueueEmpty(StoreController store)
    {
        return true;
    }*/
}

/*
    Transaction = Step
    9 Transactions = episode
    3 Steps = Action

*/
