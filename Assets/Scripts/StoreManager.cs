using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;


public class StoreManager : MonoBehaviour
{
    [SerializeField] GameObject markersFolder;
    [SerializeField] GameObject storeFolder;
    readonly StoreParams sp = new StoreParams();

    private int currentStoreCount = 0;

    void Awake()
    {
        SpawnStores();
    }

    public void CheckSpawn()
    {
        StoreController[] stores = storeFolder.GetComponentsInChildren<StoreController>();
        for (int i = 0; i < stores.Length; i++)
        {
            if (stores[i].GetLevel() == 0 && Random.Range(0, 10) < 3)
            {
                stores[i].SetLevel(1);
                stores[i].Awake();
                GameObject.Find("SimulationController").GetComponent<PathfindingManager>().SafeAdd(stores[i].gameObject);
            }
        }
    }

    private void SpawnStores()
    {
        GameObject storeFolder = GameObject.Find("Stores");
        List<Transform> markers = markersFolder.GetComponentsInChildren<Transform>().ToList();
        int rand;
        for (int i = 0; i < sp.MAX_STORE_COUNT; i++)
        {
            rand = Random.Range(1, markers.Count);

            GameObject store = Instantiate<GameObject>(Resources.Load<GameObject>("Store"),
               markers[rand].transform.position, markers[rand].transform.rotation, storeFolder.transform);
            markers.RemoveAt(rand);
            currentStoreCount++;
        }
        markersFolder.SetActive(false);

        // send the stores to the PathfindingManager script
        GetComponent<PathfindingManager>().SetStores(storeFolder.GetComponentsInChildren<StoreController>().ToList().Select(script => script.gameObject).ToList());
    }
}