using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;


public class StoreManager : MonoBehaviour
{
    [SerializeField] GameObject markersFolder;
    [SerializeField, Range(1, 5)] int MAX_STORES = 5;
    static int currentStoreCount = 0;
    static int _MAX_STORES;
    static int balance;// = BASE_BALANCE;
    static object _lock = new object();
    private bool bankcrupt;
    
    void Start()
    {
        _MAX_STORES = MAX_STORES;
        Spawn();

    }

    void Update()
    {

    }


    private void Spawn()
    {
        List<Transform> markers = markersFolder.GetComponentsInChildren<Transform>().ToList<Transform>();
        int rand;
        for (int i = 0; i < MAX_STORES; i++)
        {
            rand = Random.Range(1, markers.Count);

            GameObject stores = Instantiate<GameObject>(Resources.Load<GameObject>("Store"));
            stores.transform.position = markers[rand].position;
            stores.transform.rotation = markers[rand].rotation;
            stores.transform.localScale = markers[rand].localScale;
            markers.RemoveAt(rand);
            currentStoreCount++;
        }

    }
}