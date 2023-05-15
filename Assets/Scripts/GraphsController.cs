using Newtonsoft.Json.Bson;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;
using static UnityEditor.PlayerSettings;

public class GraphsController : MonoBehaviour
{
    [SerializeField] private Sprite dotSprite;
    private RectTransform graphContainer;
    private RectTransform labelTemplateX;
    private RectTransform labelTemplateY;
    private RectTransform dashTemplateX;
    private RectTransform dashTemplateY;
    private List<GameObject> gameObjectsList;
    private List<IGraphVisualObject> graphVisualObjectList;
    private List<RectTransform> yLabelsList;
    private List<float> storeValues;
    private List<float> aiValues;


    private interface IGraphVisualObject
    {
        void SetLineGraphVisualObject(Vector2 graphPosition);
        void CleanUp();
    }

  
    public void UpdateLineGraph(float storeValue, float aiValue)
    {
        if (storeValue != null && aiValue != null)
        {
            storeValues.Add(storeValue);
            aiValues.Add(aiValue);
            Debug.Log(storeValue + "-" + aiValue);
            ShowGraph(storeValues, aiValues, -1);
        }
    }

    void Awake()
    {
        graphContainer = transform.Find("Graph").GetComponent<RectTransform>();
        labelTemplateX = graphContainer.Find("labelTemplateX").GetComponent<RectTransform>();
        labelTemplateY = graphContainer.Find("labelTemplateY").GetComponent<RectTransform>();
        dashTemplateX = graphContainer.Find("dashTemplateX").GetComponent<RectTransform>();
        dashTemplateY = graphContainer.Find("dashTemplateY").GetComponent<RectTransform>();
        yLabelsList = new List<RectTransform>();
        gameObjectsList = new List<GameObject>();
        graphVisualObjectList = new List<IGraphVisualObject>();
        storeValues = new List<float>();
        aiValues = new List<float>();

        //List<float> points = new List<float> {0, 5, 98, 49, 33, 17, 16, 15, 20, 30, 40, 35, 60, 80 };
        //List<float> points2 = new List<float> {20,7, 60, 70, 81, 92,100, 15, 20, 30, 28, 26, 24, 25 };
        //CreateCircle(new Vector2(20, 20));
        //ShowGraph(points, points2, - 1);

    }

    // Update is called once per frame
    void Update()
    {

    }


    private void ShowGraph(List<float> storeValues, List<float> aiValues, int maxVisibleValues = -1 )
    {
        if(maxVisibleValues <= 0)
        {
            maxVisibleValues = storeValues.Count;
        }

        foreach(GameObject gameObject in gameObjectsList)
        {
            Destroy(gameObject);
        }
        gameObjectsList.Clear();

        foreach(IGraphVisualObject graphVisualObject in graphVisualObjectList)
        {
            graphVisualObject.CleanUp();
        }
        graphVisualObjectList.Clear();
        yLabelsList.Clear();

        float graphHeight = graphContainer.sizeDelta.y;
        float graphWidth = graphContainer.sizeDelta.x;

        float yMin, yMax;
        CalculateYScale(out yMin, out yMax, storeValues, aiValues, maxVisibleValues);

        float xStep = graphWidth / (maxVisibleValues+1);

        int xIndex = 0;

        LineGraph storeLineGraph = new LineGraph(graphContainer, dotSprite, new Color(0.5f, 0.5f, 1, 0), Color.green);
        LineGraph aiLineGraph = new LineGraph(graphContainer, dotSprite, new Color(0.5f, 0.5f, 1, 0), Color.blue);
        for (int i = Math.Max(storeValues.Count - maxVisibleValues, 0); i < storeValues.Count; i++)
        {
            float x = 5 + xIndex * xStep;
            float y = ((storeValues[i] - yMin) / (yMax-yMin)) * graphHeight;
            float y2 = ((aiValues[i] - yMin) / (yMax-yMin)) * graphHeight;
            graphVisualObjectList.Add(storeLineGraph.AddDot(new Vector2(x, y)));
            graphVisualObjectList.Add(aiLineGraph.AddDot(new Vector2(x, y2)));

            /*RectTransform xLabel = Instantiate(labelTemplateX);
            xLabel.SetParent(graphContainer, false);
            xLabel.gameObject.SetActive(true);
            xLabel.anchoredPosition = new Vector2(x, 0);
            xLabel.GetComponent<Text>().text = (i+1).ToString();
            gameObjectsList.Add(xLabel.gameObject);
            */

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
            int label = Mathf.RoundToInt(yMin + (normalizedY * (yMax - yMin)));
            yLabel.GetComponent<Text>().text = (label > 1000) ? (label%1000).ToString()+"K$" : label.ToString() + "$";
            yLabelsList.Add(yLabel);
            gameObjectsList.Add(yLabel.gameObject);


            RectTransform yDash = Instantiate(dashTemplateY);
            yDash.SetParent(graphContainer, false);
            yDash.gameObject.SetActive(true);
            yDash.anchoredPosition = new Vector2(5f, normalizedY * graphHeight);
            gameObjectsList.Add(yDash.gameObject);
        }


    }

   /* private void CalculateYScale(out float yMin, out float yMax, List<float> values, int maxVisibleValues)
    {
        yMax = values[0];
        yMin = values[0];

        for (int i = Math.Max(values.Count - maxVisibleValues, 0); i < values.Count; i++)
        {
            float value = values[i];
            if (value > yMax)
            {
                yMax = value;
            }
            if (value < yMin)
            {
                yMin = value;
            }
        }
        float yDelta = yMax - yMin;
        if (yDelta < 0)
        {
            yDelta = 5f;
        }
        yMax = yMax + (yDelta * 0.2f); //make a some space between the top of the graph and the max value
        yMin = yMin - (yDelta * 0.2f);
    }*/

    private void CalculateYScale(out float yMin, out float yMax, List<float> storeValues, List<float> aiValues, int maxVisibleValues)
    {
        int startIndex = Math.Max(storeValues.Count - maxVisibleValues, 0);
        List<float> combinedValues = new List<float>(maxVisibleValues * 2);
        for (int i = startIndex; i < storeValues.Count && i < startIndex + maxVisibleValues; i++)
        {
            combinedValues.Add(storeValues[i]);
        }
        for (int i = startIndex; i < aiValues.Count && i < startIndex + maxVisibleValues; i++)
        {
            combinedValues.Add(aiValues[i]);
        }
        yMax = combinedValues.Max();
        yMin = combinedValues.Min();
        float yDelta = yMax - yMin;
        if (yDelta < 0)
        {
            yDelta = 5f;
        }
        yMax = yMax + (yDelta * 0.2f); //make some space between the top of the graph and the max value
        yMin = yMin - (yDelta * 0.2f);
    }



    private class LineGraph
    {
        /// <summary>
        /// class to visualise data in a line graph in unity.
        /// </summary>
        private RectTransform graphContainer;
        private Sprite dotSprite;
        private LineGraphVisualObject previousLineGraphVisualObject;
        private Color dotColor;
        private Color dotConnectionColor;

        public LineGraph(RectTransform graphContainer, Sprite dotSprite, Color dotColor, Color dotConnectionColor)
        {
            this.graphContainer = graphContainer;
            this.dotSprite = dotSprite;
            this.dotColor = dotColor;
            this.dotConnectionColor = dotConnectionColor;
            previousLineGraphVisualObject = null;
        }

        /// <summary>
        /// Adds a dot to the graph at the specified position and connects it to the previous dot, if any.
        /// Returns a list of GameObjects which includes the dot and the dot connection, if any.
        /// </summary>
        /// <param name="graphPosition">The position in the graph where the dot is to be added.</param>
        /// <returns>A list of GameObjects representing the dot and the dot connection, if any.</returns>
        public IGraphVisualObject AddDot(Vector2 graphPosition)
        {
            //List<GameObject> gameObjectsList = new List<GameObject>();
            GameObject dotGameObject = CreateDot(graphPosition);
            //gameObjectsList.Add(dotGameObject);
            GameObject dotConnectionGameObject = null;
            if (previousLineGraphVisualObject != null)
            {
                dotConnectionGameObject = CreateDotConnection(previousLineGraphVisualObject.GetGraphPosition(),
                    dotGameObject.GetComponent<RectTransform>().anchoredPosition);
                //gameObjectsList.Add(dotConnectionGameObject);
            }

            LineGraphVisualObject lineGraphVisualObject = new LineGraphVisualObject(dotGameObject, dotConnectionGameObject, previousLineGraphVisualObject);
            lineGraphVisualObject.SetLineGraphVisualObject(graphPosition);
            previousLineGraphVisualObject = lineGraphVisualObject;

            return lineGraphVisualObject;
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
            rectTransform.sizeDelta = new Vector2(distance, 2f);
            rectTransform.anchoredPosition = anchoredPosition1 + direction * distance * 0.5f;
            rectTransform.localEulerAngles = new Vector3(0, 0, Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg);
            return gameObject;
        }

        public class LineGraphVisualObject : IGraphVisualObject
        {
            public event EventHandler OnChangedGraphVisualObject;
            private GameObject dotGameObject;
            private GameObject dotConnectionGameObject;
            private LineGraphVisualObject previousLineGraphVisualObject;
            public LineGraphVisualObject(GameObject dotGameObject, GameObject dotConnectionGameObject, LineGraphVisualObject previousLineGraphVisualObject)
            {
                this.dotGameObject = dotGameObject;
                this.dotConnectionGameObject = dotConnectionGameObject;
                this.previousLineGraphVisualObject = previousLineGraphVisualObject;

                if(previousLineGraphVisualObject!= null)
                {
                    previousLineGraphVisualObject.OnChangedGraphVisualObject += PreviousLineGraphVisualObject_OnChangedGraphVisualObject;
                }
            }

            private void PreviousLineGraphVisualObject_OnChangedGraphVisualObject(object sender, EventArgs e)
            {
                UpdateDotConnection();
            }

            public void SetLineGraphVisualObject(Vector2 graphPosition)
            {
                RectTransform rectTransform = dotGameObject.GetComponent<RectTransform>();
                rectTransform.anchoredPosition = graphPosition;

                UpdateDotConnection();

                if (OnChangedGraphVisualObject != null) { OnChangedGraphVisualObject(this, EventArgs.Empty); }
            }
            public void CleanUp()
            {
                Destroy(dotGameObject);
                Destroy(dotConnectionGameObject);
            }

            public Vector2 GetGraphPosition()
            {
                RectTransform rectTransform = dotGameObject.GetComponent<RectTransform>();
                return rectTransform.anchoredPosition;
            }

            private void UpdateDotConnection()
            {
                if (dotConnectionGameObject != null)
                {
                    RectTransform dotConnectionRectTransform = dotConnectionGameObject.GetComponent<RectTransform>();
                    Vector2 direction = (previousLineGraphVisualObject.GetGraphPosition() - GetGraphPosition()).normalized;
                    float distance = Vector2.Distance(GetGraphPosition(), previousLineGraphVisualObject.GetGraphPosition());
                    dotConnectionRectTransform.sizeDelta = new Vector2(distance, 2f);
                    dotConnectionRectTransform.anchoredPosition = GetGraphPosition() + direction * distance * 0.5f;
                    dotConnectionRectTransform.localEulerAngles = new Vector3(0, 0, Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg);
                }
            }

        }
    }
}
