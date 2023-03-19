using System.Collections;
using System.Collections.Generic;
using Unity.MLAgents;
using UnityEditor;
using UnityEngine;

public class Teacher : MonoBehaviour
{
    // Start is called before the first frame update
    void Awake()
    {

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
}
