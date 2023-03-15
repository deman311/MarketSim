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
    int step = 9;
    bool sensorLock = false;

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
    }

    public override void OnEpisodeBegin()
    {
        base.OnEpisodeBegin();

        for (int i = 0; i < 3; i++)
        {
            for (int j = 0; j < 3; j++)
            {
                var cc = teacher.GetACustomer();
                store.SafeEnqueue(cc);
                Destroy(cc);
            }
        }
        //EndEpisode();
    }

    public override void CollectObservations(VectorSensor sensor)
    {
        base.CollectObservations(sensor);

        List<float> sold = new List<float> { 0, 0, 0, 0, 0 };
        for (int i = 0; i < store.GetSoldProducts().Count; i++)
        {
            sold[i] = store.GetSoldProducts()[i];
        }

        // collect the total inputs from the store, 13 in total.
        sensor.AddObservation(sold); // 5
        sensor.AddObservation(sm.GetAllAvgPrices()); // 5
        sensor.AddObservation(store.GetBalance());
        sensor.AddObservation(store.GetTotalTax());
        sensor.AddObservation(store.GetLevel());

        Debug.Log("SIZE: " + sensor.ObservationSize());
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
