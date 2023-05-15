using System.Collections.Generic;
using System.Linq;
using System.Text;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UI;
using Random = UnityEngine.Random;

public class CustomerController : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI TMP_ttl;
    [SerializeField] RawImage mood;
    [SerializeField] float customerSpeed = 20f;

    private const int HAPPY = 1, SAD = 2;

    List<GameObject> storePath = new List<GameObject>();
    List<Product> shoppingList = new List<Product>();
    StockManager sm;
    CustomerManager cm;
    public int ttl = CustomerManager.GetMaxTTL();
    NavMeshAgent agent;
    bool isSelling = false, isDone = false, isIdle = false;
    public bool isKillable = false;

    // store visiting
    List<StoreController> storeStack = new List<StoreController>();
    bool isVisiting = true;

    void Start()
    {
        gameObject.GetComponentsInChildren<Renderer>()[1].material.color = new Color(Random.value, Random.value, Random.value); // set random shirt color
        sm = GameObject.Find("SimulationController").GetComponent<StockManager>();
        cm = GameObject.Find("SimulationController").GetComponent<CustomerManager>();
        InitShoppingList(CustomerParams.ALPHA);

        if (cm == null) // mlagents scenario
            return;

        CreateNavMesh(); // create navmesh dynamically to avoid error will only create if not Training Mode
        storePath = GameObject.Find("SimulationController").GetComponent<PathfindingManager>().GetPathList(transform.position);
        if (storePath.Count > 0)
            agent.SetDestination(storePath[0].transform.position);

        //PrintList();
    }

    private void CreateNavMesh()
    {
        gameObject.AddComponent(typeof(NavMeshAgent));
        agent = GetComponent<NavMeshAgent>();
        agent.speed = customerSpeed;
        agent.angularSpeed = 100;
        agent.acceleration = 500;
        agent.radius = 0.2f;
    }

    void Update()
    {
        if (cm == null) return; // true for MLTraining
        if (agent.speed != customerSpeed) // check update speed change
            agent.speed = customerSpeed;

        HandleCanvas();

        if (storePath.Count > 0 && !isSelling && !isDone && agent.remainingDistance < CustomerParams.CUSTOMER_WAYPOINT_PROXIMITY) // hasPath is for a bug when just spawning and AIlib delay
        {
            if (isVisiting)
            {
                storeStack.Add(storePath[0].GetComponent<StoreController>());
            }
            else
            {
                isSelling = true;
                agent.isStopped = true;
                isVisiting = true;
                storePath[0].GetComponent<StoreController>().SafeEnqueue(this);
            }
            if (storePath.Count > 1)
                agent.SetDestination(storePath[1].transform.position);
            storePath.Remove(storePath[0]);
        }
        else if (storePath.Count == 0 && !isDone)
        {
            isDone = true;
            GoToEnd();
        }
        else if (isDone && agent.remainingDistance < CustomerParams.CUSTOMER_WAYPOINT_PROXIMITY)
        {
            isIdle = true;
            //CustomerManager.KillMe(this);
        }

        if (storeStack.Count == CustomerParams.VISIT_COUNT || (storePath.Count == 0 && storeStack.Count > 0))
        {
            CompareStores();
        }
    }

    private void CompareStores()
    {
        Dictionary<StoreController, float> storeToScore = new Dictionary<StoreController, float>();
        foreach (StoreController store in storeStack)
        {
            if (storeToScore.ContainsKey(store))
                continue;

            var storeProducts = store.GetProducts();
            float score = 0;
            foreach (Product cp in shoppingList)
            {
                if (storeProducts.TryGetValue(cp.name, out var sp))
                {
                    if (sp.Price <= cp.Price)
                        score += (cp.Price - sp.Price) * sp.amount + 1;
                    else
                        score -= sp.amount > 0 ? sp.amount : 1;
                }
            }
            storeToScore.Add(store, score);
        }
        var sorted = storeToScore.ToList();
        sorted.Sort((a, b) => b.Value.CompareTo(a.Value));
        if (storePath.Count > 0)
            storePath.Insert(0, sorted[0].Key.gameObject);
        else
            storePath.Add(sorted[0].Key.gameObject);
        isVisiting = false;
        storeStack.Clear();
    }

    private void SetMood(int moodID)
    {
        if (this.IsDestroyed()) return; // race during training ML

        Dictionary<int, Texture> moodTextures = new Dictionary<int, Texture>
        {
            { HAPPY, Resources.Load("Icons/happy") as Texture },
            { SAD, Resources.Load("Icons/sad") as Texture }
        };

        mood.texture = moodTextures[moodID];
        mood.gameObject.SetActive(true);
    }


    private void HandleCanvas()
    {
        // face camera
        TMP_ttl.transform.parent.LookAt(Camera.main.transform.position);
        //TMP_ttl.transform.parent.Rotate(new Vector3(0, 180, 0));

        TMP_ttl.text = "" + ttl;

        // decice on color
        // [?] change the color marker to be the customer shirts?
        switch (ttl)
        {
            case 3: TMP_ttl.color = Color.white; break;
            case 2: TMP_ttl.color = Color.yellow; break;
            case 1: TMP_ttl.color = Color.red; break;
        }
    }

    public void StartNewStartingPointAndPath()
    {
        transform.position = GameObject.Find("SimulationController").GetComponent<CustomerManager>().GetRandomPositionInBounds();
        storePath = GameObject.Find("SimulationController").GetComponent<PathfindingManager>().GetPathList(transform.position);
        isDone = false;
        isIdle = false;

        if (storePath.Count > 0 && agent != null)
            agent.SetDestination(storePath[0].transform.position);
    }

    public bool IsIdle()
    {
        return isIdle;
    }

    public void FinishDay()
    {
        ttl--;
        mood.gameObject.SetActive(false);
        if (ttl == 0 || isKillable)
            CustomerManager.KillMe(this);
    }

    public void GoToEnd()
    {
        if (!mood.gameObject.activeInHierarchy)
            SetMood(SAD);

        Bounds endBounds = PathfindingManager.endPos.GetComponent<Renderer>().bounds;
        Vector3 endPos = new Vector3(
            Random.Range(endBounds.min.x, endBounds.max.x),
            endBounds.max.y,
            Random.Range(endBounds.min.z, endBounds.max.z)
            );
        agent.SetDestination(endPos);
    }

    public void OnDrawGizmosSelected()
    {
        if (storePath.Count > 0)
            Gizmos.color = Color.green;
        else
            Gizmos.color = Color.red;
        Gizmos.DrawLine(transform.position, agent.destination);

        PrintList();
    }

    private void PrintList()
    {
        StringBuilder sb = new();
        if (shoppingList.Count > 0)
            sb.Append("[C]" + this.name + " ");
        foreach (Product p in shoppingList)
            sb.Append(p + ", ");
        sb.Append("isIdle: " + agent.isStopped);

        if (!string.IsNullOrEmpty(sb.ToString()))
            Debug.Log(sb.ToString());
    }

    /// <summary>
    /// Initialize the shopping list of the customer by taking the average price in the market and applying a alpha.
    /// Each item has a 50% chance to be included. Item list takes into consideration the max level store in the market.
    /// 
    /// [?] chance of appearing in the shopping list is decreased with each item level.
    /// </summary>
    private void InitShoppingList(int alpha)
    {
        List<string> buylist = sm.GetBuyList(sm.GetMaxLevel());
        foreach (string prodName in buylist)
        {
            if (Random.Range(0, 100) < sm.GetScarsityOfProduct(prodName)) // Precentage for each product to spawn, +1 because exclusive
            {
                // try to get the avg from Customers, if no customers exists then get avg from stores.
                float avg = 0;
                if (cm != null) // check null for mlagents fictional customer scenario
                    cm.GetAveragePrice(prodName);
                if (avg == 0)
                    avg = sm.GetAveragePrice(prodName);

                int amount = Random.Range(CustomerParams.SHOPPING_LIST_PRODUCT_AMOUNT_MIN, CustomerParams.SHOPPING_LIST_PRODUCT_AMOUNT_MAX + 1);
                float cpp = avg + sm.GetMaxPrice(prodName) * Random.Range(-alpha, alpha) / 100;
                shoppingList.Add(new Product(prodName, amount, cpp));
            }
        }

        // if empty, add a random product
        if (shoppingList.Count == 0)
            shoppingList.Add(new Product(buylist[Random.Range(0, buylist.Count)], Random.Range(CustomerParams.SHOPPING_LIST_PRODUCT_AMOUNT_MIN, CustomerParams.SHOPPING_LIST_PRODUCT_AMOUNT_MAX + 1)));
    }

    public ref List<Product> GetProducts()
    {
        return ref shoppingList;
    }

    public void CompleteTransaction(bool hasBoughtSomething)
    {
        isSelling = false;
        if (agent != null && agent.isActiveAndEnabled)
            agent.isStopped = false;

        if (shoppingList.TrueForAll(p => p.amount == 0))
        {
            storePath.Clear();
            SetMood(HAPPY);
            isKillable = true;
        }

        if (hasBoughtSomething && !this.IsDestroyed())
            Instantiate(Resources.Load("CashThrow") as GameObject, transform);
    }

    public Dictionary<string, float> GetProductPrices()
    {
        Dictionary<string, float> temp = new Dictionary<string, float>();
        foreach (Product p in shoppingList)
            temp.Add(p.name, p.Price);
        return temp;
    }
}
