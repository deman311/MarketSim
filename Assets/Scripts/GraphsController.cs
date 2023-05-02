using Newtonsoft.Json.Bson;
using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class GraphsController : MonoBehaviour
{
    [SerializeField] private Sprite dotSprite;
    private RectTransform graphContainer;
    private RectTransform labelTemplateX;
    private RectTransform labelTemplateY;
    private RectTransform dashTemplateX;
    private RectTransform dashTemplateY;
    private List<GameObject> gameObjectsList;
    

    void Awake()
    {
        graphContainer = transform.Find("Graph").GetComponent<RectTransform>();
        labelTemplateX = graphContainer.Find("labelTemplateX").GetComponent<RectTransform>();
        labelTemplateY = graphContainer.Find("labelTemplateY").GetComponent<RectTransform>();
        dashTemplateX = graphContainer.Find("dashTemplateX").GetComponent<RectTransform>();
        dashTemplateY = graphContainer.Find("dashTemplateY").GetComponent<RectTransform>();
        gameObjectsList = new List<GameObject>();

        List<int> points = new List<int> {0, 5, 98, 49, 33, 17, 16, 15, 20, 30, 40, 35, 60, 80 };
        //CreateCircle(new Vector2(20, 20));
        ShowGraph(points, -1);

    }

    // Update is called once per frame
    void Update()
    {

    }


    private void ShowGraph(List<int> values, int maxVisibleValues = -1 )
    {
        if(maxVisibleValues <= 0)
        {
            maxVisibleValues = values.Count;
        }

        foreach(GameObject gameObject in gameObjectsList)
        {
            Destroy(gameObject);
        }
        gameObjectsList.Clear();

        float graphHeight = graphContainer.sizeDelta.y;
        float graphWidth = graphContainer.sizeDelta.x;

        float yMax = values[0];
        float yMin = values[0];

        for(int i = Math.Max(values.Count - maxVisibleValues, 0); i < values.Count; i++)
        {
            int value = values[i];
            if(value > yMax)
            {
                yMax = value;
            }
            if(value < yMin)
            {
                yMin = value;
            }
        }
        float yDelta = yMax - yMin;
        if(yDelta < 0)
        {
            yDelta = 5f;
        }
        yMax = yMax + (yDelta*0.2f); //make a some space between the top of the graph and the max value
        yMin = yMin - (yDelta*0.2f); 

        float xStep = graphWidth / (maxVisibleValues+1);

        int xIndex = 0;

        LineGraph lineGraph = new LineGraph(graphContainer, dotSprite, new Color(0.5f, 0.5f, 1, 0), Color.green);
        for (int i = Math.Max(values.Count - maxVisibleValues, 0); i < values.Count; i++)
        {
            float x = 5 + xIndex * xStep;
            float y = ((values[i] - yMin) / (yMax-yMin)) * graphHeight;
            gameObjectsList.AddRange(lineGraph.AddDot(new Vector2(x, y)));

            RectTransform xLabel = Instantiate(labelTemplateX);
            xLabel.SetParent(graphContainer, false);
            xLabel.gameObject.SetActive(true);
            xLabel.anchoredPosition = new Vector2(x, 0);
            xLabel.GetComponent<Text>().text = "D"+(i+1).ToString();
            gameObjectsList.Add(xLabel.gameObject);

            RectTransform xDash = Instantiate(dashTemplateX);
            xDash.SetParent(graphContainer, false);
            xDash.gameObject.SetActive(true);
            xDash.anchoredPosition = new Vector2(x, 0);
            gameObjectsList.Add(xDash.gameObject);
            xIndex++;
        }

        int yAxisSeperatorsCount = 10;
        for(int i=0; i<=yAxisSeperatorsCount; i++)
        {
            RectTransform yLabel = Instantiate(labelTemplateY);
            yLabel.SetParent(graphContainer, false);
            yLabel.gameObject.SetActive(true);
            float normalizedY = i * 1f / yAxisSeperatorsCount;
            yLabel.anchoredPosition = new Vector2(5f, normalizedY * graphHeight);
            yLabel.GetComponent<Text>().text = Mathf.RoundToInt(yMin + (normalizedY * (yMax-yMin))).ToString()+"$";
            gameObjectsList.Add(yLabel.gameObject);


            RectTransform yDash = Instantiate(dashTemplateY);
            yDash.SetParent(graphContainer, false);
            yDash.gameObject.SetActive(true);
            yDash.anchoredPosition = new Vector2(5f, normalizedY * graphHeight);
            gameObjectsList.Add(yDash.gameObject);
        }


    }



    private class LineGraph
    {
        /// <summary>
        /// class to visualise data in a line graph in unity.
        /// </summary>
        private RectTransform graphContainer;
        private Sprite dotSprite;
        private GameObject previousDot;
        private Color dotColor;
        private Color dotConnectionColor;

        public LineGraph(RectTransform graphContainer, Sprite dotSprite, Color dotColor, Color dotConnectionColor)
        {
            this.graphContainer = graphContainer;
            this.dotSprite = dotSprite;
            this.dotColor = dotColor;
            this.dotConnectionColor = dotConnectionColor;
            previousDot = null;
        }

        /// <summary>
        /// Adds a dot to the graph at the specified position and connects it to the previous dot, if any.
        /// Returns a list of GameObjects which includes the dot and the dot connection, if any.
        /// </summary>
        /// <param name="graphPosition">The position in the graph where the dot is to be added.</param>
        /// <returns>A list of GameObjects representing the dot and the dot connection, if any.</returns>
        public List<GameObject> AddDot(Vector2 graphPosition)
        {
            List<GameObject> gameObjectsList = new List<GameObject>();
            GameObject dotGameObject = CreateDot(graphPosition);
            gameObjectsList.Add(dotGameObject);
            if (previousDot != null)
            {
                GameObject dotConnectionGameObjec = CreateDotConnection(previousDot.GetComponent<RectTransform>().anchoredPosition,
                    dotGameObject.GetComponent<RectTransform>().anchoredPosition);
                gameObjectsList.Add(dotConnectionGameObjec);
            }
            previousDot = dotGameObject;
            return gameObjectsList;
        }

        /// <summary>
        /// Creates a dot game object with the specified position and size in the graph.
        /// Returns the newly created dot game object.
        /// </summary>
        /// <param name="pos">The position of the dot in the graph.</param>
        /// <returns>The newly created dot game object.</returns>
        private GameObject CreateDot(Vector2 pos)
        {
            GameObject gameObject = new GameObject("dot", typeof(Image));
            gameObject.transform.SetParent(graphContainer, false);
            gameObject.GetComponent<Image>().sprite = dotSprite;
            gameObject.GetComponent<Image>().color = dotColor;
            RectTransform rectTransform = gameObject.GetComponent<RectTransform>();
            rectTransform.anchoredPosition = pos;
            rectTransform.sizeDelta = new Vector2(10, 10);
            rectTransform.anchorMin = new Vector2(0, 0);
            rectTransform.anchorMax = new Vector2(0, 0);
            return gameObject;
        }
        
        /// <summary>
        /// Creates a dot connection game object between the two specified anchored positions in the graph.
        /// Returns the newly created dot connection game object.
        /// </summary>
        /// <param name="anchoredPosition1">The anchored position of the first dot in the connection.</param>
        /// <param name="anchoredPosition2">The anchored position of the second dot in the connection.</param>
        /// <returns>The newly created dot connection game object.</returns>
        private GameObject CreateDotConnection(Vector2 anchoredPosition1, Vector2 anchoredPosition2)
        {
            GameObject gameObject = new GameObject("dotConnection", typeof(Image));
            gameObject.transform.SetParent(graphContainer, false);
            gameObject.GetComponent<Image>().color = dotConnectionColor;
            RectTransform rectTransform = gameObject.GetComponent<RectTransform>();
            Vector2 direction = (anchoredPosition2 - anchoredPosition1).normalized;
            float distance = Vector2.Distance(anchoredPosition1, anchoredPosition2);
            rectTransform.anchorMin = new Vector2(0, 0);
            rectTransform.anchorMax = new Vector2(0, 0);
            rectTransform.sizeDelta = new Vector2(distance, 3f);
            rectTransform.anchoredPosition = anchoredPosition1 + direction * distance * 0.5f;
            rectTransform.localEulerAngles = new Vector3(0, 0, Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg);
            return gameObject;
        }
    }
}
