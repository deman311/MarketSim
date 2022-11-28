using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;


public class StoreManager: MonoBehaviour
{
	[SerializeField] GameObject storeFolder; // Store folder
	[SerializeField] GameObject markerFolder;
	[SerializeField, Range(1, 32)] int MAX_STORES = 20;
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

    public static void SusspendsMe(StoreController sc)
	{
		lock(_lock)
		{
			currentStoreCount--;
			// need to think how we actually suspend the store from business
		}

	}

	private void Spawn()
    {
        List<Transform> markers = markerFolder.GetComponentsInChildren<Transform>().ToList<Transform>();
        int rand;
        for (int i = 0; i < MAX_STORES; i++)
        {
            rand = Random.Range(1, markers.Count);
			
            GameObject stores = Instantiate<GameObject>(Resources.Load<GameObject>("Store"));
            stores.transform.position = markers[rand].position;
			stores.transform.rotation = markers[rand].rotation;
			stores.transform.localScale = markers[rand].localScale;
            markers.RemoveAt(rand);
        }

    }
}