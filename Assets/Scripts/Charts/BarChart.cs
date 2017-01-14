using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Based on "Unity Tutorial: Bar Chart" (https://www.youtube.com/watch?v=oDX8MiKMorU)
/// </summary>
public class BarChart : MonoBehaviour {

    public Bar barHolderPrefab;
    public float threshold;
    public float[] inputValues;
    public string[] labels;
    public Color bottomBarColor;
    public Color topBarColor;

    List<Bar> barHolders = new List<Bar>();

    float chartHeight;

    const float AXES_GRAPH_OFFSET = 6;  // the offset of a bar compared to the axes-graph
    const float BAR_SCALE_FACTOR = 0.95f; // the scaling of the max bar height


    /// <summary>
    /// Use this for initialization
    /// </summary>
    void Start() {
        // Prepare Chart-Size first
        Transform tCanvas = this.GetComponent<RectTransform>().parent;
        RectTransform rtCanvas = tCanvas.GetComponent<RectTransform>();
        chartHeight = (rtCanvas.sizeDelta.y - AXES_GRAPH_OFFSET) * BAR_SCALE_FACTOR;

        DisplayGraph(inputValues);
    }


    /// <summary>
    /// 
    /// </summary>
    /// <param name="vals"></param>
    void DisplayGraph(float[] vals) {
        float maxValue = vals.Max();
        float normalizedThresholdValue = threshold / maxValue;

        for (int i = 0; i < vals.Length; i++) {
            // Instantiate new Bar
            Bar newBarHolder = Instantiate(barHolderPrefab) as Bar;

            newBarHolder.transform.SetParent(transform);
            // due to the parent transformation, the localScale hs to be reset to 1/1/1, because of Canvas.scale = 0.00234375
            newBarHolder.transform.localScale = Vector3.one;
            // due to parent transformation, also set the z-axis back to 0
            newBarHolder.transform.localPosition = new Vector3(newBarHolder.transform.localPosition.x, newBarHolder.transform.localPosition.y);

            // Set the size of the bars
            RectTransform rtBottomBar = newBarHolder.bottomBar.GetComponent<RectTransform>();
            RectTransform rtTopBar = newBarHolder.topBar.GetComponent<RectTransform>();
            float bottomVal = vals[i];
            float topVal = 0f;
            if (bottomVal > threshold) {
                topVal = bottomVal - threshold;
                bottomVal = threshold;
            }
            float normalizedBottomValue = bottomVal / maxValue;
            float normalizedTopValue = ((bottomVal + topVal) / maxValue) - normalizedThresholdValue;

            rtBottomBar.sizeDelta = new Vector3(rtBottomBar.sizeDelta.x, chartHeight * normalizedBottomValue);
            rtTopBar.sizeDelta = new Vector3(rtTopBar.sizeDelta.x, chartHeight * normalizedTopValue);

            // Set the color of the bars
            newBarHolder.bottomBar.color = bottomBarColor;
            newBarHolder.topBar.color = topBarColor;

            // Set the bottom label of the bar
            if (labels.Length <= i) {
                newBarHolder.label.text = "UNDEFINED";
            } else {
                newBarHolder.label.text = labels[i];
            }

            // Set the value label at the top of the bar
            if (topVal > 0f) {
                // put the label at the top bar
                newBarHolder.topBarValue.text = vals[i].ToString();
                newBarHolder.bottomBarValue.text = string.Empty;
            } else {
                // put the label at the bottom bar
                newBarHolder.topBarValue.text = string.Empty;
                newBarHolder.bottomBarValue.text = vals[i].ToString();
                // if height is too small, move labelt ot op of bar
                if (rtBottomBar.sizeDelta.y < 30f) {
                    newBarHolder.bottomBarValue.rectTransform.pivot = new Vector2(0.5f, 0f);
                    newBarHolder.bottomBarValue.rectTransform.anchoredPosition = Vector2.zero;
                }
            }

            // Finally, add the bar to the list
            barHolders.Add(newBarHolder);
        }
    }


}
