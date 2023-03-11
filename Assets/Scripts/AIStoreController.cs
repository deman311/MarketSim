using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TMPro;
using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.VisualScripting;
using UnityEngine;
using Debug = UnityEngine.Debug;

public class AIStoreController : Agent
{
    public StoreController store;
    public Teacher teacher;
    public int steps = 0;
    private void Awake()
    {
        store = GetComponent<StoreController>();
        teacher = GameObject.Find("Academy").GetComponent<Teacher>();
        store.Awake();
    }

    private void Update()
    {
        store.Update();
    }

    public async override void OnEpisodeBegin()
    {
        steps = 0;
        base.OnEpisodeBegin();
        int[][] sold = new int[3][];
        for (int i = 0; i < 3; i++)
        {
            for (int j = 0; j < 3; j++)
                store.SafeEnqueue(teacher.GetACustomer());
            await Task.Run(() => { while (store.GetQueueSize() > 0) { } }); //Might not work, so if there is bug check this line
            sold[i] = store.GetSoldProducts();

        }
        base.RequestAction();
        base.EndEpisode();
    }

    public override void OnActionReceived(ActionBuffers actions)
    {
        base.OnActionReceived(actions);

    }

    /*async public bool checkIsQueueEmpty(StoreController store)
    {
        
       
        return true;
    }*/
}

/*
    Transaction = Step
    9 Transactions = episode
    3 Steps = Action

*/
