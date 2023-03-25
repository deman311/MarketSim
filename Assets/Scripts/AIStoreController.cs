using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TMPro;
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
    [SerializeField] TextMeshProUGUI pricesUI;

    bool isBankrupt = false, awaitingDecision = false;
    int epoch = 0;
    float accumelativeReward = 0;

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
        string pricesText = "";
        store.GetProductPrices().Select(kvp => kvp.Key + " " + kvp.Value + "\n").ToList().ForEach(t => pricesText += t);
        pricesUI.text = pricesText;

        store.Update();
        if (store.step != 0 && store.step % MLParams.TRANSACTION_DELTA == 0 && !store.isSelling && !awaitingDecision)
        {
            awaitingDecision = true;
            RequestDecision();
        }
    }

    private void EndEpoch()
    {
        // tax
        float taxReward = (store.GetBalance() - store.GetTotalTax()) / (100 * MathF.Pow(store.GetLevel(), 3));
        store.Tax(StoreParams.BASE_TAX);
        AddReward(taxReward);
        accumelativeReward += taxReward;

        if (store.GetBalance() < 0)
        {
            AddReward(-50);
            accumelativeReward -= 50;
            isBankrupt = true;
        }

        // finish episode

        EndEpisode();
        Debug.Log("Reward " + accumelativeReward + " epoch: " + epoch);

        epoch++;
        store.step = 0;
        accumelativeReward = 0;
        if (isBankrupt)
        {
            isBankrupt = false;
            store.Awake(); // resets the store
        }
    }

    public override void OnEpisodeBegin()
    {
        // restock
        store.Restock();
        if (store.GetBalance() < 0)
        {
            AddReward(-20);
            accumelativeReward -= 20;
            isBankrupt = true;
        }
        else
        {
            AddReward(5);
            accumelativeReward += 5;
        }

        store.isSelling = true;
        for (int i = 0; i < MLParams.TRANSACTION_CYCLES; i++)
            for (int j = 0; j < MLParams.TRANSACTION_DELTA; j++)
                store.SafeEnqueue(teacher.GetACustomer());
    }

    public override void CollectObservations(VectorSensor sensor)
    {
        Debug.Log("Collecting observations on step " + store.step);

        List<int> sold = new List<int> { 0, 0, 0, 0, 0 };   // validate to always contain 5 values
        for (int i = 0; i < store.GetSoldProducts().Count; i++)
            sold[i] = store.GetSoldProducts()[i];

        // delta sold reward function
        List<int> held = new List<int> { 0, 0, 0, 0, 0 };
        var prods = store.GetProducts().Select(p => p.Value.amount).ToList();
        for (int i = 0; i < prods.Count; i++)
            held[i] = prods[i];
        var rewards = sm.GetRewardsPerProduct(sold, held);
        rewards.ForEach(reward =>
        {
            AddReward(reward);
            accumelativeReward += reward;
        });

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
        // get the 11 outputs from the model
        var outputs = actions.ContinuousActions.ToList();
        var dPrices = outputs.GetRange(0, 5);
        var dITs = outputs.GetRange(5, 5);
        var upgradeProb = outputs[10];

        store.UpdateFromModelOutput(outputs); // take decisions based on model output

        if (store.step != 0 && store.step % (MLParams.TRANSACTION_DELTA * MLParams.TRANSACTION_CYCLES) == 0)
            EndEpoch();
        store.isSelling = true;
        awaitingDecision = false;
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
