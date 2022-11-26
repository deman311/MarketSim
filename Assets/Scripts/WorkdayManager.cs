using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WorkdayManager : MonoBehaviour
{
    [SerializeField] GameObject customersFolder;
    [SerializeField] GameObject storesFolder;

    [SerializeField] int TAX = 100;

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
        bool alldone = true;

        foreach (CustomerController c in customersFolder.GetComponentsInChildren<CustomerController>())
            if (!c.IsIdle())
                alldone = false;

        return alldone;
    }

    void FinishWorkDay()
    {
        foreach (StoreController s in storesFolder.GetComponentsInChildren<StoreController>())
        {
            s.Tax(TAX);
            // check-decide if to upgrade
            if (s.GetLevel() == 1 && s.GetBalance() > 1200 && Random.Range(0, 2) == 0)
                s.LevelUp();
            else if (s.GetLevel() == 2 && s.GetBalance() > 10000 && Random.Range(0, 2) == 0)
                s.LevelUp();
        }
        foreach (CustomerController c in customersFolder.GetComponentsInChildren<CustomerController>())
        {
            c.FinishDay();
        }
    }

    void StartWorkDay()
    {
        foreach (CustomerController c in customersFolder.GetComponentsInChildren<CustomerController>())
        {
            if (c.isActiveAndEnabled && !c.isKillable)
                c.StartNewStartingPointAndPath();
        }

        foreach (StoreController s in storesFolder.GetComponentsInChildren<StoreController>())
            s.Restock();
    }
}
