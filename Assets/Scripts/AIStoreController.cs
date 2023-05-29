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
    int bankruptCount = 0;

    public void Awake()
    {
        // validate after reset
        if (Phase == 2)
        {
            isBankrupt = false; awaitingDecision = false; isLast = false; readyForReset = false;
            R1 = 0;
        }

        store = GetComponent<StoreController>();
        if (Phase != 0)
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
                /*                if (store.GetLevel() == 0)
                                {
                                    SetReward(bankruptCount * -100f);
                                    Debug.Log(GetCumulativeReward());
                                    EndEpisode();
                                    return;
                                }
                                // calculate R2, R1 and scoreboard
                                *//*                float R2 = GetCumulativeReward();
                                                if (R2 == 0)
                                                    R2 = 0.001f;
                                                var scoreboard = teacher.GetScoreboard();
                                                int pos = scoreboard[store];
                                                float marketshare = StatisticsController.GetMarketShare(store) / 100;
                                                //SetReward(R2 + 1000 * scoreboard.Count / pos + 20 * StatisticsController.GetMarketShare(store));
                                                float delta_improvment = (R2 - R1) / R2;
                                                *//*                if (delta_improvment < 0)
                                                                    delta_improvment = 0;*//*
                                                //SetReward(20 * scoreboard.Count / pos + StatisticsController.GetMarketShare(store));
                                                //SetReward(delta_improvment + 100 * scoreboard.Count / pos);
                                                SetReward(store.GetBalance());
                                                R1 = R2;*//*

                                float reward = store.GetBalance() * store.GetLevel() + GetCumulativeReward();
                                if (reward - bankruptCount * reward / 2f > 0)
                                    SetReward(reward - bankruptCount * reward / 2f);
                                else
                                    SetReward(bankruptCount * -100f);
                                */
                var marketshare = StatisticsController.GetMarketShare(store);
                /*                if (marketshare > 20)
                                    reward *= marketshare */

                if (store.GetBalance() < 0)
                    SetReward(store.GetBalance());
                else
                    SetReward(store.GetBalance() / (1 + bankruptCount));
                Debug.Log(GetCumulativeReward());
                EndEpisode();
                break;
        }
    }

    public override void OnEpisodeBegin()
    {
        bankruptCount = 0;
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

    public void checkBankrupt()
    {
        if (store.GetBalance() < 0 && !isLast)
            bankruptCount++;
    }

    public override void CollectObservations(VectorSensor sensor)
    {
        //Debug.Log("Collecting observations on step " + store.step);

        List<float> pprices = new List<float> { 0, 0, 0, 0, 0 };   // validate to always contain 5 values
        for (int i = 0; i < store.GetProductPrices().Count; i++)
            pprices[i] = store.GetProductPrices().Values.ToList()[i];
        List<int> sold = new List<int> { 0, 0, 0, 0, 0 };
        for (int i = 0; i < store.GetSoldProducts().Count; i++)
            sold[i] = store.GetSoldProducts()[i];

        if (!this.isActiveAndEnabled) return;

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

            //Debug.Log("Observations: " + string.Join(", ", variables));
        }

        // Because EndEpisode() calls CollectObservations again for some reason,
        // I don't want to lose the find observation before passing to the model
        if (isLast)
        {
            store.ClearSoldProducts();
            isLast = false;
            readyForReset = true;
        }
        else if (store.step != 0 && store.step % (MLParams.Transaction_Delta * MLParams.Workdays) == 0)
        {
            isLast = true;
            float R2 = store.GetBalance() - store.GetTotalTax();
            if (R2 == 0)
                R2 = 0.001f;
            float balanceReward = (R2 - R1) / R2;
            AddReward(balanceReward);
            /*            float balanceReward = store.GetBalance() - store.GetTotalTax();
                        *//*            switch (store.GetLevel()) // clip 1-2 levels reward to incentivise the model to upgrade to level 3.
                                    {
                                        case 1:
                                            if (balanceReward > StoreParams.UPGRADE_LEVEL_TWO_PRICE * 2)
                                                balanceReward = StoreParams.UPGRADE_LEVEL_TWO_PRICE * 2;
                                            break;
                                        case 2:
                                            AddReward(StoreParams.UPGRADE_LEVEL_TWO_PRICE);
                                            if (balanceReward > StoreParams.UPGRADE_LEVEL_THREE_PRICE * 2)
                                                balanceReward = StoreParams.UPGRADE_LEVEL_THREE_PRICE * 2;
                                            break;
                                        case 3: AddReward(StoreParams.UPGRADE_LEVEL_THREE_PRICE); break;
                                    }*//*
                        if (Phase == 1)
                            AddReward(balanceReward);*/
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
        else if (Phase == 1 && store.GetLevel() < 3 &&
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
