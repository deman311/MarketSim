using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;


public class StoreManager : MonoBehaviour
{
    [SerializeField] GameObject markersFolder;
    readonly StoreParams sp = new StoreParams();

    private int currentStoreCount = 0;

    void Awake()
    {
        SpawnStores();
    }

    private void SpawnStores()
    {
        GameObject storeFolder = GameObject.Find("Stores");
        List<Transform> markers = markersFolder.GetComponentsInChildren<Transform>().ToList();
        int rand;
        for (int i = 0; i < sp.MAX_SHOP_COUNT; i++)
        {
            rand = Random.Range(1, markers.Count);

            GameObject store = Instantiate<GameObject>(Resources.Load<GameObject>("Store"),
                markers[rand].transform.position, markers[rand].transform.rotation);
            store.transform.SetParent(storeFolder.transform);
            markers.RemoveAt(rand);
            currentStoreCount++;
        }
        markersFolder.SetActive(false);

        // send the stores to the PathfindingManager script
        GetComponent<PathfindingManager>().SetStores(storeFolder.GetComponentsInChildren<StoreController>().ToList().Select(script => script.gameObject).ToList());
    }
}