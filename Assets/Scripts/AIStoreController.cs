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
    [SerializeField] int Phase;

    bool isBankrupt = false, awaitingDecision = false, isLast = false, readyForReset = false;
    float R1 = 0;

    public void Awake()
    {
        // validate after reset
        isBankrupt = false; awaitingDecision = false; isLast = false; readyForReset = false;
        R1 = 0;

        store = GetComponent<StoreController>();
        teacher = GameObject.Find("Academy").GetComponent<Teacher>();
        sm = GameObject.Find("SimulationController").GetComponent<StockManager>();
        store.isAI = true;
        store.phase = Phase;
        store.Awake();
    }

    void Update()
    {
        string pricesText = "";
        store.GetProductPrices().Select(kvp => kvp.Key + " " + kvp.Value + "\n").ToList().ForEach(t => pricesText += t);
        pricesUI.text = pricesText;

        store.Update();
        if (store.step != 0 && store.step % MLParams.Transaction_Delta == 0 && !store.isSelling && !awaitingDecision)
        {
            awaitingDecision = true;
            RequestDecision();
        }
    }

    public void EndEpoch(int phase)
    {
        switch (phase) // decide by phase
        {
            case 1:
                store.Tax(StoreParams.BASE_TAX);
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
                break;
            case 2:
                // calculate R2, R1 and scoreboard
                float R2 = GetCumulativeReward();
                if (R2 == 0)
                    R2 = 0.001f;
                var scoreboard = teacher.GetScoreboard();
                int pos = scoreboard[store];
                SetReward((R2 - R1) / R2 + 10 * scoreboard.Count / pos);
                R1 = R2;
                EndEpisode();
                break;
        }
    }

    public override void OnEpisodeBegin()
    {
        store.Restock();
        if (store.GetBalance() < 0)
        {
            AddReward(-5);
            isBankrupt = true;
        }

        store.isSelling = true;
        if (Phase == 1)
            for (int i = 0; i < MLParams.Workdays; i++)
                for (int j = 0; j < MLParams.Transaction_Delta; j++)
                    store.SafeEnqueue(teacher.GetACustomer());
    }

    public override void CollectObservations(VectorSensor sensor)
    {
        Debug.Log("Collecting observations on step " + store.step);

        List<float> pprices = new List<float> { 0, 0, 0, 0, 0 };   // validate to always contain 5 values
        for (int i = 0; i < store.GetProductPrices().Count; i++)
            pprices[i] = store.GetProductPrices().Values.ToList()[i];
        List<int> sold = new List<int> { 0, 0, 0, 0, 0 };
        for (int i = 0; i < store.GetSoldProducts().Count; i++)
            sold[i] = store.GetSoldProducts()[i];

        // collect the total inputs from the store, 13 in total.
        sensor.AddObservation(pprices); // 5
        sensor.AddObservation(sold.Select(s => (float)s).ToList()); // 5
        sensor.AddObservation(sm.GetAllAvgPrices()); // 5
        sensor.AddObservation(store.GetBalance());
        sensor.AddObservation(store.GetTotalTax());
        sensor.AddObservation(store.GetLevel());

        // print for debugging
        if (isLast)
        {
            List<string> variables = new List<string>();
            variables.AddRange(pprices.Select(s => s.ToString())); // 5
            variables.AddRange(sold.Select(s => s.ToString())); // 5
            variables.AddRange(sm.GetAllAvgPrices().Select(p => p.ToString())); // 5
            variables.Add(store.GetBalance().ToString());
            variables.Add(store.GetTotalTax().ToString());
            variables.Add(store.GetLevel().ToString());

            Debug.Log("Observations: " + string.Join(", ", variables));
        }

        // Because EndEpisode() calls CollectObservations again for some reason,
        // I don't want to lose the find observation before passing to the model
        if (isLast)
        {
            store.ClearSoldProducts();
            isLast = false;
            readyForReset = true;
        }
        else if (store.step == MLParams.Transaction_Delta * MLParams.Workdays)
        {
            isLast = true;
            float balanceReward = store.GetBalance() - store.GetTotalTax();
            /*            if (balanceReward < -1000) // clamp negative loss
                            balanceReward = -1000;*/
            AddReward(balanceReward);
        }
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
            AddReward(-1 * store.GetLevel() * 10);
        }

        if (Phase == 1 && store.step != 0 && store.step % (MLParams.Transaction_Delta * MLParams.Workdays) == 0)
            EndEpoch(1);

        store.isSelling = true;
        awaitingDecision = false;
    }

    public bool IsReadyForReset()
    {
        return readyForReset;
    }
}
