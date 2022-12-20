using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WorkdayManager : MonoBehaviour
{
    [SerializeField] GameObject customersFolder;
    [SerializeField] GameObject storesFolder;

    StoreParams sp;

    void Awake()
    {
        sp = new StoreParams();    
    }

    void Update()
    {
        if (CheckIfAllDone())
        {
            FinishWorkDay();
            StartWorkDay();
        }
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
            store.Tax(sp.BASE_TAX);
            // check-decide if to upgrade
            if (store.GetLevel() == 1 && store.GetBalance() > 2000 && Random.Range(0, 2) == 0)
                store.LevelUp();
            else if (store.GetLevel() == 2 && store.GetBalance() > 10000 && Random.Range(0, 2) == 0)
                store.LevelUp();
        }
        foreach (CustomerController c in customersFolder.GetComponentsInChildren<CustomerController>())
        {
            c.FinishDay();
        }

        GameObject.Find("SimulationController").GetComponent<CustomerManager>().Spawn();
    }

    void StartWorkDay()
    {
        foreach (StoreController store in storesFolder.GetComponentsInChildren<StoreController>())
            store.Restock();

        foreach (CustomerController customer in customersFolder.GetComponentsInChildren<CustomerController>())
        {
            if (customer.isActiveAndEnabled && !customer.isKillable)
                customer.StartNewStartingPointAndPath();
        }
    }
}
