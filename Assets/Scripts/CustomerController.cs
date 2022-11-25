using System.Collections.Generic;
using System.Text;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UI;
using Random = UnityEngine.Random;

public class CustomerController : MonoBehaviour
{
    private const int HAPPY = 1, SAD = 2;

    List<GameObject> storePath = new List<GameObject>();
    List<Product> shoppingList = new List<Product>();

    StockManager sm;
    public int ttl = CustomerManager.GetMaxTTL();
    NavMeshAgent agent;
    bool isSelling = false, isDone = false, isIdle = false;

    [SerializeField] TextMeshProUGUI TMP_ttl;
    [SerializeField] RawImage mood;

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        sm = GameObject.Find("SimulationController").GetComponent<StockManager>();
        InitShoppingList(15);

        storePath = GameObject.Find("SimulationController").GetComponent<PathfindingManager>().GetPathList(transform.position);
        if (storePath.Count > 0)
            agent.SetDestination(storePath[0].transform.position);

        //PrintList();
    }

    void Update()
    {
        HandleCanvas();

        if (storePath.Count > 0 && agent.remainingDistance < 0.1f && !isSelling)
        {
            isSelling = true;
            agent.isStopped = true;
            storePath[0].GetComponent<StoreController>().SafeEnqueue(this);
            if (storePath.Count > 1)
                agent.SetDestination(storePath[1].transform.position);
            storePath.Remove(storePath[0]);
        }

        if (storePath.Count == 0 && !isDone)
        {
            GoToEnd();
            isDone = true;
        }
        else if (isDone && agent.remainingDistance < 0.01f)
        {
            isIdle = true;
            //CustomerManager.KillMe(this);
        }
    }

    private void SetMood(int modID)
    {
        switch (modID)
        {
            case HAPPY:
                mood.texture = Resources.Load("Icons/happy") as Texture;
                break;
            case SAD:
                mood.texture = Resources.Load("Icons/sad") as Texture;
                break;
        }
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

        if (storePath.Count > 0)
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
        if (ttl == 0 || shoppingList.TrueForAll(p => p.amount == 0))
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
    }

    private void PrintList()
    {
        StringBuilder sb = new();
        if (shoppingList.Count > 0)
            sb.Append("[C]" + this.name + " ");
        foreach (Product p in shoppingList)
            sb.Append(p + ", ");

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
        foreach (string prodName in sm.BuyList(sm.GetMaxLevel()))
        {
            if (Random.Range(0, sm.GetScarsityOfProduct(prodName) / 10 + 1) == 0) // Precentage for each product to spawn, +1 because exclusive
            {
                int amount = Random.Range(1, 6);
                float cpp = sm.GetAveragePrice(prodName) + sm.GetMaxPrice(prodName) * Random.Range(-alpha, alpha) / 100;
                shoppingList.Add(new Product(prodName, amount, cpp));
            }
        }
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
        }

        if (hasBoughtSomething)
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
