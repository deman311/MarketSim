using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GraphManager : MonoBehaviour
{
    [SerializeField] GraphsController aiMarketShareGraph;
    [SerializeField] GraphsController appleAvgPriceGraph;
    [SerializeField] GraphsController shirtAvgPriceGraph;
    [SerializeField] GraphsController phoneAvgPriceGraph;
    [SerializeField] GraphsController gpuAvgPriceGraph;
    [SerializeField] GraphsController rolexAvgPriceGraph;
    //[SerializeField] StatisticsController statisticsController;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void UpdateGraphs(Dictionary<string, List<float>> productAvgPrice, Dictionary<string, float> marketShareData)
    {

    }

}
