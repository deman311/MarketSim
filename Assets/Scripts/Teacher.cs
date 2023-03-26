using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.MLAgents;
using UnityEditor;
using UnityEngine;

public class Teacher : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI avgPricesUI;
    StockManager sm;

    // Start is called before the first frame update
    void Awake()
    {
        sm = GameObject.Find("SimulationController").GetComponent<StockManager>();
    }

    // Update is called once per frame
    void Update()
    {
        List<float> prices = sm.GetAllAvgPrices();
        avgPricesUI.text = "Average Prices:\n";
        avgPricesUI.text += "Apples: " + prices[0] + "\n";
        avgPricesUI.text += "Shirts: " + prices[1] + "\n";
        avgPricesUI.text += "Phones: " + prices[2] + "\n";
        avgPricesUI.text += "GPUs: " + prices[3] + "\n";
        avgPricesUI.text += "Rolexes: " + prices[4] + "\n";
    }

    public CustomerController GetACustomer()
    {
        var customer = Instantiate(Resources.Load("Customer"), transform.position, Quaternion.identity) as GameObject;
        return customer.GetComponent<CustomerController>();
    }
}
