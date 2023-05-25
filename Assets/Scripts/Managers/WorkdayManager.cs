using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;

public class WorkdayManager : MonoBehaviour
{
    [SerializeField] GameObject customersFolder;
    [SerializeField] GameObject storesFolder;
    bool readyForReset = false;

    void Awake()
    {
        readyForReset = false;
    }

    void Update()
    {
        if (!readyForReset && CheckIfAllDone())
        {
            FinishWorkDay();
            StartWorkDay();
        }

        if (readyForReset && storesFolder.GetComponentsInChildren<AIStoreController>().ToList().All(x => x.IsReadyForReset()))
            ResetMarket();
    }

    bool CheckIfAllDone()
    {
        if (customersFolder.GetComponentsInChildren<CustomerController>().Length == 0)
            return false;

        bool alldone = true;

        foreach (CustomerController customer in customersFolder.GetComponentsInChildren<CustomerController>())
            if (!customer.IsIdle())
                alldone = false;

        return alldone;
    }

    void FinishWorkDay()
    {
        foreach (StoreController store in storesFolder.GetComponentsInChildren<StoreController>())
        {
            if (store.GetLevel() == 0) // don't update bankrupt stores.
                continue;

            store.Tax(StoreParams.BASE_TAX);
            if (!store.isAI)
            {
                // check-decide if to upgrade
                if (store.GetLevel() == 1 && store.GetBalance() > 2000 && Random.Range(0, 2) == 0)
                    store.SetLevel(2);
                else if (store.GetLevel() == 2 && store.GetBalance() > 10000 && Random.Range(0, 2) == 0)
                    store.SetLevel(3);
            }
            else if (store.phase != 0 && StatisticsController.daysPassed != 0 && StatisticsController.daysPassed % MLParams.Workdays == 0)
            {
                store.GetComponent<AIStoreController>().checkBankrupt();

                if (StatisticsController.daysPassed % MLParams.Phase == 0)
                {
                    store.GetComponent<AIStoreController>().EndEpoch(2);
                    readyForReset = true;
                }
            }
        }
        foreach (CustomerController c in customersFolder.GetComponentsInChildren<CustomerController>())
        {
            c.FinishDay();
        }

        GameObject.Find("SimulationController").GetComponent<CustomerManager>().CheckSpawn();
        GameObject.Find("SimulationController").GetComponent<StoreManager>().CheckSpawn();
        StatisticsController.daysPassed++;
        StatisticsController.updateGraphs = true;

    }

    public void ResetMarket()
    {
        foreach (var store in storesFolder.GetComponentsInChildren<StoreController>())
            store.Awake();
        foreach (var aiStore in storesFolder.GetComponentsInChildren<AIStoreController>())
            aiStore.Awake();
        readyForReset = false;
    }

    void StartWorkDay()
    {
        foreach (StoreController store in storesFolder.GetComponentsInChildren<StoreController>())
            if (store.GetLevel() == 0)
                continue;
            else
                store.Restock();

        foreach (CustomerController customer in customersFolder.GetComponentsInChildren<CustomerController>())
        {
            if (customer.isActiveAndEnabled && !customer.isKillable)
                customer.StartNewStartingPointAndPath();
        }
    }
}
