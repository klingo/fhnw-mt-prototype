﻿using System.Collections;
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

        for (int currBarIndex = 0; currBarIndex < vals.Length; currBarIndex++) {
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
            float bottomVal = vals[currBarIndex];
            float topVal = 0f;
            if (bottomVal > threshold) {
                topVal = bottomVal - threshold;
                bottomVal = threshold;
            }
            float normalizedBottomValue = bottomVal / maxValue;
            float normalizedTopValue = ((bottomVal + topVal) / maxValue) - normalizedThresholdValue;

            rtBottomBar.sizeDelta = new Vector3(rtBottomBar.sizeDelta.x, chartHeight * normalizedBottomValue);
            rtTopBar.sizeDelta = new Vector3(rtTopBar.sizeDelta.x, chartHeight * normalizedTopValue);

            // Set the special paddings
            VerticalLayoutGroup vlg = newBarHolder.GetComponent<VerticalLayoutGroup>();
            if (currBarIndex > 0) {
                // all bars except the first one
                vlg.padding.left = 5;
            } else if (currBarIndex < (vals.Length - 1)) {
                // all bars except the last one
                vlg.padding.right = 5;
            }
            

            // Set the color of the bars
            newBarHolder.bottomBar.color = bottomBarColor;
            newBarHolder.topBar.color = topBarColor;

            // Set the bottom label of the bar
            if (labels.Length <= currBarIndex) {
                newBarHolder.label.text = "UNDEFINED";
            } else {
                newBarHolder.label.text = labels[currBarIndex];
            }

            // due to the special paddings, the label position for the bars has to be adjustet: first +7.5f, all others -7.5f
            Vector3 labelLocalPos = newBarHolder.label.rectTransform.localPosition;
            if (currBarIndex > 0) {
                newBarHolder.label.rectTransform.localPosition = new Vector3(-7.5f, labelLocalPos.y);
            } else {
                newBarHolder.label.rectTransform.localPosition = new Vector3(7.5f, labelLocalPos.y);
            }

            // Set the value label at the top of the bar
            if (topVal > 0f) {
                // put the label at the top bar
                newBarHolder.topBarValue.text = vals[currBarIndex].ToString();
                newBarHolder.bottomBarValue.text = string.Empty;
                // if height is too small, move label to top of bar
                if (rtTopBar.sizeDelta.y < 30f) {
                    newBarHolder.topBarValue.rectTransform.pivot = new Vector2(0.5f, 0f);
                    newBarHolder.topBarValue.rectTransform.anchoredPosition = Vector2.zero;
                }
            } else {
                // put the label at the bottom bar
                newBarHolder.topBarValue.text = string.Empty;
                newBarHolder.bottomBarValue.text = vals[currBarIndex].ToString();
                // if height is too small, move label to top of bar
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
