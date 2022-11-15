using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.AI;
using Random = UnityEngine.Random;

public class CustomerController : MonoBehaviour
{
    readonly int EPSILON = 10;

    List<GameObject> storePath = new();
    List<Product> shoppingList = new List<Product>();

    StockManager sm;
    public int lifeExpectancy = 3;
    NavMeshAgent agent;
    bool isSelling = false, isDone = false, isIdle = false;

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        sm = GameObject.Find("SimulationController").GetComponent<StockManager>();
        InitShoppingList();

        storePath = GameObject.Find("SimulationController").GetComponent<PathfindingManager>().GetPathList(transform.position);
        if (storePath.Count > 0)
            agent.SetDestination(storePath[0].transform.position);

        //PrintList();
    }

    void Update()
    {
        if (storePath.Count > 0 && agent.remainingDistance < 0.01f && !isSelling)
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
        lifeExpectancy--;
        if (lifeExpectancy == 0 || shoppingList.TrueForAll(p => p.amount == 0))
            CustomerManager.KillMe(this);
    }

    public void GoToEnd()
    {
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

    private void InitShoppingList()
    {
        foreach (string prodName in sm.BuyList(sm.GetMaxLevel()))
        {
            if (Random.Range(0, 2) == 0) // 50%
            {
                int amount = Random.Range(1, 6);
                float cpp = sm.GetMaxPrice(prodName) / 2 + sm.GetMaxPrice(prodName) * Random.Range(-EPSILON, EPSILON) / 100;
                shoppingList.Add(new Product(prodName, amount, cpp));
            }
        }
    }

    public ref List<Product> GetProducts()
    {
        return ref shoppingList;
    }

    public void CompleteTransaction()
    {
        isSelling = false;
        if (agent != null && agent.isActiveAndEnabled)
            agent.isStopped = false;

        if (shoppingList.TrueForAll(p => p.amount == 0))
            storePath.Clear();
    }
}
