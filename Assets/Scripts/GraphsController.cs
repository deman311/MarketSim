using Newtonsoft.Json.Bson;
using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class GraphsController : MonoBehaviour
{
    [SerializeField] private Sprite circleSprite;
    private RectTransform graphContainter;
    private RectTransform labelTemplateX;
    private RectTransform labelTemplateY;
    private RectTransform dashTemplateX;
    private RectTransform dashTemplateY;

    void Awake()
    {
        graphContainter = transform.Find("Graph").GetComponent<RectTransform>();
        labelTemplateX = graphContainter.Find("labelTemplateX").GetComponent<RectTransform>();
        labelTemplateY = graphContainter.Find("labelTemplateY").GetComponent<RectTransform>();
        dashTemplateX = graphContainter.Find("dashTemplateX").GetComponent<RectTransform>();
        dashTemplateY = graphContainter.Find("dashTemplateY").GetComponent<RectTransform>();
        List<int> points = new List<int> {0, 5, 98, 49, 33, 17, 16, 15, 20, 30, 40, 35 };
        //CreateCircle(new Vector2(20, 20));
        ShowGraph(points);
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private GameObject CreateCircle(Vector2 pos)
    {
        GameObject gameObject = new GameObject("circle", typeof(Image));
        gameObject.transform.SetParent(graphContainter, false);
        gameObject.GetComponent<Image>().sprite = circleSprite;
        gameObject.GetComponent<Image>().color = new Color(0.5f, 0.5f, 1, 0); //Make the dot invisible
        RectTransform rectTransform = gameObject.GetComponent<RectTransform>();
        rectTransform.anchoredPosition = pos;
        rectTransform.sizeDelta = new Vector2(10, 10);
        rectTransform.anchorMin = new Vector2(0, 0);
        rectTransform.anchorMax = new Vector2(0, 0);
        return gameObject;
    }

    private void ShowGraph(List<int> values)
    {
        float graphHeight = graphContainter.sizeDelta.y;
        float yMax = values[0];

        foreach(int value in values)
        {
            if(value > yMax)
            {
                yMax = value;
            }
        }
        yMax *= 1.2f; //make a some space between the top of the graph and the max value

        float xStep = 20f;

        GameObject previousCircle = null;
        for (int i = 0; i < values.Count; i++)
        {
            float x = xStep + i * xStep;
            float y = (values[i] / yMax) * graphHeight;
            GameObject circleGameObject = CreateCircle(new Vector2(x, y));
            if (previousCircle != null)
            {
                CreateDotConnection(previousCircle.GetComponent<RectTransform>().anchoredPosition,
                    circleGameObject.GetComponent<RectTransform>().anchoredPosition);
            }
            previousCircle = circleGameObject;

            RectTransform xLabel = Instantiate(labelTemplateX);
            xLabel.SetParent(graphContainter, false);
            xLabel.gameObject.SetActive(true);
            xLabel.anchoredPosition = new Vector2(x, 0);
            xLabel.GetComponent<Text>().text = "Day"+(i+1).ToString();

            RectTransform xDash = Instantiate(dashTemplateX);
            xDash.SetParent(graphContainter, false);
            xDash.gameObject.SetActive(true);
            xDash.anchoredPosition = new Vector2(x, 0);
        }

        int yAxisSeperatorsCount = 10;
        for(int i=0; i<=yAxisSeperatorsCount; i++)
        {
            RectTransform yLabel = Instantiate(labelTemplateY);
            yLabel.SetParent(graphContainter, false);
            yLabel.gameObject.SetActive(true);
            float normalizedY = i * 1f / yAxisSeperatorsCount;
            yLabel.anchoredPosition = new Vector2(7f, normalizedY * graphHeight);
            yLabel.GetComponent<Text>().text = Mathf.RoundToInt(normalizedY * yMax).ToString()+"$";

            RectTransform yDash = Instantiate(dashTemplateY);
            yDash.SetParent(graphContainter, false);
            yDash.gameObject.SetActive(true);
            yDash.anchoredPosition = new Vector2(5f, normalizedY * graphHeight);
        }
        

    }

    private void CreateDotConnection(Vector2 anchoredPosition1, Vector2 anchoredPosition2)
    {
        GameObject gameObject = new GameObject("dotConnection", typeof(Image));
        gameObject.transform.SetParent(graphContainter, false);
        gameObject.GetComponent<Image>().color = new Color(0,1,0,0.5f);
        RectTransform rectTransform= gameObject.GetComponent<RectTransform>();
        Vector2 direction = (anchoredPosition2 - anchoredPosition1).normalized;
        float distance = Vector2.Distance(anchoredPosition1, anchoredPosition2);
        rectTransform.anchorMin= new Vector2(0, 0);
        rectTransform.anchorMax= new Vector2(0, 0);
        rectTransform.sizeDelta = new Vector2(distance, 3f);
        rectTransform.anchoredPosition = anchoredPosition1 + direction * distance * 0.5f;
        rectTransform.localEulerAngles= new Vector3(0,0,Mathf.Atan2(direction.y,direction.x)*Mathf.Rad2Deg);
    }
}
