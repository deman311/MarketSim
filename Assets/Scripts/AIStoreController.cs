using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TMPro;
using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Sensors;
using UnityEditor.UIElements;
using UnityEditorInternal;
using UnityEngine;

public class AIStoreController : Agent
{
    StoreController store;
    Teacher teacher;
    StockManager sm;
    [SerializeField] TextMeshProUGUI pricesUI;

    bool isBankrupt = false, awaitingDecision = false;

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

        if (store.GetBalance() < 0)
        {
            AddReward(-20);
            isBankrupt = true;
        }

        // finish episode
        EndEpisode();

        store.step = 0;
        if (isBankrupt && store.GetLevel() == 0)
        {
            isBankrupt = false;
            store.Awake(); // resets the store
        }
    }

    public override void OnEpisodeBegin()
    {
        lastBalance = store.GetBalance();
        store.Restock();
        if (store.GetBalance() < 0)
        {
            AddReward(-5);
            isBankrupt = true;
        }

        store.isSelling = true;
        for (int i = 0; i < MLParams.TRANSACTION_CYCLES; i++)
            for (int j = 0; j < MLParams.TRANSACTION_DELTA; j++)
                store.SafeEnqueue(teacher.GetACustomer());
    }

    bool isLast = false;
    float lastBalance = 0;
    public override void CollectObservations(VectorSensor sensor)
    {
        Debug.Log("Collecting observations on step " + store.step);

        List<int> sold = new List<int> { 0, 0, 0, 0, 0 };   // validate to always contain 5 values
        for (int i = 0; i < store.GetSoldProducts().Count; i++)
            sold[i] = store.GetSoldProducts()[i];

        // profit reward
        float profit = store.GetBalance() - lastBalance;
        if (!isLast)
            AddReward(store.GetBalance() - store.GetTotalTax());
        lastBalance = store.GetBalance();

        /*        // delta sold reward function
                List<int> held = new List<int> { 0, 0, 0, 0, 0 };
                var prods = store.GetProducts().Select(p => p.Value.amount).ToList();
                for (int i = 0; i < prods.Count; i++)
                    held[i] = prods[i];
                var rewards = sm.GetRewardsPerProduct(sold, held);
                rewards.ForEach(reward =>
                {
                    AddReward(reward);
                });
        */
        // Because EndEpisode() calls CollectObservations again for some reason,
        // I don't want to lose the find observation before passing to the model
        if (store.step == MLParams.TRANSACTION_DELTA * MLParams.TRANSACTION_CYCLES)
            isLast = true;
        if (isLast)
        {
            store.ClearSoldProducts();
            isLast = false;
        }

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

        // reward function for an upgrade, if upgraded on this action
        if (upgradeProb > 0.5 && store.GetLevel() < 3)
        {
            if (store.GetBalance() > 0)
                AddReward(10 * store.GetLevel());
            else
                AddReward(-10 * store.GetLevel());
        }
        else if (store.GetLevel() < 3 &&
                ((store.GetLevel() == 1 && store.GetBalance() >= StoreParams.UPGRADE_LEVEL_TWO_PRICE + 300)
                    || ((store.GetLevel() == 2 && store.GetBalance() >= StoreParams.UPGRADE_LEVEL_THREE_PRICE + 2000))))
        {
            float deltaPrice = store.GetBalance() / store.GetLevel() == 1 ? StoreParams.UPGRADE_LEVEL_TWO_PRICE : StoreParams.UPGRADE_LEVEL_THREE_PRICE;
            AddReward(-1 * deltaPrice * store.GetLevel() * 10);
        }

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
