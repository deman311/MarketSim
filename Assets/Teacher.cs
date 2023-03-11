using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class Teacher : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public CustomerController GetACustomer()
    {
        CustomerController customer = new CustomerController(true);


        return customer;
    }
}
