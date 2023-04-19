using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.MLAgents;
using UnityEditor;
using UnityEngine;

public class Teacher : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI avgPricesUI;
    StoreManager storeManager;

    // Start is called before the first frame update
    void Awake()
    {
        storeManager = GameObject.Find("SimulationController").GetComponent<StoreManager>();
    }

    // Update is called once per frame
    void Update()
    {

    }

    public CustomerController GetACustomer()
    {
        var customer = Instantiate(Resources.Load("Customer"), transform.position, Quaternion.identity) as GameObject;
        return customer.GetComponent<CustomerController>();
    }

    /// <summary>
    /// Store to it's position in the Scoreboard
    /// </summary>
    /// <returns></returns>
    public Dictionary<StoreController, int> GetScoreboard()
    {
        var scoreboard = storeManager.GetAllStores();
        scoreboard.Sort((s1, s2) => s1.GetBalance().CompareTo(s2.GetBalance()));
        Dictionary<StoreController, int> dict = new Dictionary<StoreController, int>();
        for (int i = 0; i < scoreboard.Count; i++)
            dict.Add(scoreboard[i], i + 1);
        return dict;
    }
}
