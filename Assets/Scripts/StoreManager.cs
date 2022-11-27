using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class StoreManager: MonoBehaviour
{
	[SerializeField] GameObject sfolder; // Store folder
	[SerializeField] GameObject ground;
	[SerializeField] GameObject stores;
	[SerializeField] int MAX_STORES = 10;
	static int currentStoreCount = 0;
	static int _MAX_STORES;
	static int balance;// = BASE_BALANCE;
	static object _lock = new object();

	private Bounds storeBounds;
	private Bounds bounds;
	private bool bankcrupt;

	void Start()
    {
		_MAX_STORES = MAX_STORES;
        bounds = ground.GetComponent<Renderer>().bounds;
		storeBounds = stores.GetComponent<Renderer>().bounds;
    }

	void Update()
    {
		if (currentStoreCount < MAX_STORES)
			Spawn();
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
		for (int i=0; currentStoreCount < MAX_STORES; i++) 
		{
			GameObject store = Instantiate<GameObject>(Resources.Load<GameObject>("Store"));
			store.transform.position = GetRandomPostionInBounds();
			store.transform.SetParent(sfolder.transform);
			
			currentStoreCount++;
		}
    }

	public Vector3 GetRandomPostionInBounds()
    {
		return new Vector3(
			Random.Range(bounds.min.x - storeBounds.min.x, bounds.max.x - storeBounds.max.x),
			bounds.max.y,
			Random.Range(bounds.min.z - storeBounds.min.z, bounds.max.z - storeBounds.max.z)
			);
    }
}