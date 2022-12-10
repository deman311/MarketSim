using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;


public class StoreManager : MonoBehaviour
{
    StoreParams sp = new StoreParams();
    [SerializeField] GameObject markersFolder;
    static int currentStoreCount = 0;
    static int _MAX_STORES;
    public bool isReady = false;
    void Awake()
    {
        _MAX_STORES = sp.MAX_SHOP_COUNT;
        Spawn();
        isReady = true;
/*        pf = GameObject.Find("SimulationController").GetComponent<PathfindingManager>();
        pf.CreatePaths();*/
    }

    void Update()
    {

    }


    private void Spawn()
    {
        //int[MAX_STORES] positions = { -1 };
        GameObject storeFolder = GameObject.Find("Stores");
        List<Transform> markers = markersFolder.GetComponentsInChildren<Transform>().ToList();
        int rand;
        for (int i = 0; i < _MAX_STORES; i++)
        {
            rand = Random.Range(1, markers.Count);

            GameObject store = Instantiate<GameObject>(Resources.Load<GameObject>("Store"));
            store.transform.position = markers[rand].position;
            store.transform.rotation = markers[rand].rotation;
            store.transform.SetParent(storeFolder.transform);
            markers.RemoveAt(rand);
            currentStoreCount++;
        }
        markersFolder.SetActive(false);

    }

    public bool Ready()
    {
        return isReady;
    }

}