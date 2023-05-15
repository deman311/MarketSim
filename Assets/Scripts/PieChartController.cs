using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PieChartController : MonoBehaviour
{
    private RectTransform graphContainer;
    private List<float> storeValues;
    private List<float> aiValues;
    private Image aiShare;
    private Image storeShare;
    private Text precentage;

    // Start is called before the first frame update
    void Awake()
    {
        graphContainer = GetComponent<RectTransform>();
        aiShare= graphContainer.Find("aiShare").GetComponent<Image>();
        storeShare= graphContainer.Find("storeShare").GetComponent<Image>();
        precentage = graphContainer.Find("precentage").GetComponent<Text>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void ShowPieGraph(float storeValue, float aiValue)
    {
        float total = storeValue + aiValue;
        storeShare.fillAmount = storeValue/total;
        aiShare.fillAmount = 1 - aiValue/total;
        precentage.text = ((int)(aiValue / total * 100)).ToString() + '%';
    }
}
