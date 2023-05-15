using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PieChartController : MonoBehaviour
{
    [SerializeField] public Color aiColor, regColor;
    private RectTransform graphContainer;
    private List<float> storeValues;
    private List<float> aiValues;
    private Image aiShare;
    private Image storeShare;
    private Text precentage;

    // Start is called before the first frame update
    void Awake()
    {
        // set set default color
        ColorUtility.TryParseHtmlString("#7FFF00AA", out aiColor);
        ColorUtility.TryParseHtmlString("#FF6347AA", out regColor);

        graphContainer = GetComponent<RectTransform>();
        aiShare = graphContainer.Find("aiShare").GetComponent<Image>();
        aiShare.color = aiColor;
        storeShare = graphContainer.Find("storeShare").GetComponent<Image>();
        storeShare.color = regColor;
        precentage = graphContainer.Find("precentage").GetComponent<Text>();
    }

    // Update is called once per frame
    void Update()
    {

    }

    public void ShowPieGraph(float storeValue, float aiValue)
    {
        float total = storeValue + aiValue;
        //storeShare.fillAmount = storeValue / total;
        aiShare.fillAmount = (total - storeValue) / total;
        storeShare.fillAmount = 1;
        precentage.text = ((aiValue / total) * 100).ToString("0.00") + '%';
    }
}
