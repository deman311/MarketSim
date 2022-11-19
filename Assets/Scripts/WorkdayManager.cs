using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WorkdayManager : MonoBehaviour
{
    [SerializeField] GameObject customersFolder;
    [SerializeField] GameObject storesFolder;

    [SerializeField] int TAX = 300;

    void Start()
    {
        
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
            if (s.GetLevel() == 1 && s.GetBalance() > 1200 && Random.Range(0, 2) == 0)
                s.LevelUp();
        }

        // EACH STORE DECIDES IF TO UPGRADE ALSO
    }

    void StartWorkDay()
    {
        foreach (CustomerController c in customersFolder.GetComponentsInChildren<CustomerController>())
        {
            c.StartNewStartingPointAndPath();
            c.FinishDay();
        }

        foreach (StoreController s in storesFolder.GetComponentsInChildren<StoreController>())
            s.Restock();
    }
}
