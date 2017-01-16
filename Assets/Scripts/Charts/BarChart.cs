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
    public Color bottomBarColor;
    public Color topBarColor;
    public Image thresholdLine;
    public Text thresholdValueLabel;
    public Text chartTitle;

    List<Bar> barHolders = new List<Bar>();

    float chartHeight;
    float thresholdLineOffsetY = 0f;

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
    }


    /// <summary>
    /// 
    /// </summary>
    public void DisplayGraph(string[] labels, float[] inputValues, string title) {

        float maxValue = inputValues.Max();
        float normalizedThresholdValue = threshold / maxValue;

        string previousBarValueLabel = string.Empty;

        // set chart title
        chartTitle.text = title;

        for (int currBarIndex = 0; currBarIndex < inputValues.Length; currBarIndex++) {

            Bar newBarHolder;
            bool reuseBar = false;

            // first check if we already have instances of barHolders
            if (barHolders.Count > currBarIndex) {
                newBarHolder = barHolders.ElementAt<Bar>(currBarIndex);
                reuseBar = true;
            }
            else {
                // Instantiate new Bar
                newBarHolder = Instantiate(barHolderPrefab) as Bar;

                newBarHolder.transform.SetParent(transform);
                // due to the parent transformation, the localScale hs to be reset to 1/1/1, because of Canvas.scale = 0.00234375
                newBarHolder.transform.localScale = Vector3.one;
                // due to parent transformation, also set the z-axis back to 0
                newBarHolder.transform.localPosition = new Vector3(newBarHolder.transform.localPosition.x, newBarHolder.transform.localPosition.y);
                // due to parent transformation, also set the rotation back to 0
                newBarHolder.transform.localRotation = Quaternion.Euler(0f, 0f, 0f);
            }

            // Set the size of the bars
            RectTransform rtBottomBar = newBarHolder.bottomBar.GetComponent<RectTransform>();
            RectTransform rtTopBar = newBarHolder.topBar.GetComponent<RectTransform>();
            float bottomVal = inputValues[currBarIndex];
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
            }
            if (currBarIndex < (inputValues.Length - 1)) {
                // all bars except the last one
                vlg.padding.right = 5;
            }

            // Set the color of the bars
            newBarHolder.bottomBar.color = bottomBarColor;
            newBarHolder.topBar.color = topBarColor;

            // Set the bottom label of the bar
            if (labels.Length <= currBarIndex) {
                newBarHolder.label.text = "UNDEFINED";
            }
            else {
                newBarHolder.label.text = labels[currBarIndex];
            }

            // due to the special paddings, the label position for the bars has to be adjustet: first +7.5f, all others -7.5f
            Vector3 labelLocalPos = newBarHolder.label.rectTransform.localPosition;
            if (currBarIndex > 0) {
                newBarHolder.label.rectTransform.localPosition = new Vector3(-7.5f, labelLocalPos.y);
            }
            else {
                newBarHolder.label.rectTransform.localPosition = new Vector3(7.5f, labelLocalPos.y);
            }

            // Set the value label at the top of the bar
            if (inputValues[currBarIndex].ToString() == previousBarValueLabel) {
                // if the label value is the same like for the previous bar, dont show it.
                newBarHolder.topBarValue.text = string.Empty;
                newBarHolder.bottomBarValue.text = string.Empty;
            }
            else {
                if (topVal > 0f) {
                    // put the label at the top bar
                    newBarHolder.topBarValue.text = inputValues[currBarIndex].ToString();
                    newBarHolder.bottomBarValue.text = string.Empty;
                    // if height is too small, move label to top of bar
                    if (rtTopBar.sizeDelta.y < 30f) {
                        newBarHolder.topBarValue.rectTransform.pivot = new Vector2(0.5f, 0f);
                        newBarHolder.topBarValue.rectTransform.anchoredPosition = Vector2.zero;
                    }
                }
                else {
                    // put the label at the bottom bar
                    newBarHolder.topBarValue.text = string.Empty;
                    newBarHolder.bottomBarValue.text = inputValues[currBarIndex].ToString();
                    // if height is too small, move label to top of bar
                    if (rtBottomBar.sizeDelta.y < 30f) {
                        newBarHolder.bottomBarValue.rectTransform.pivot = new Vector2(0.5f, 0f);
                        newBarHolder.bottomBarValue.rectTransform.anchoredPosition = Vector2.zero;
                    }
                }
            }
            previousBarValueLabel = inputValues[currBarIndex].ToString();

            // Finally, add the bar to the list
            if (!reuseBar) {
                barHolders.Add(newBarHolder);
            }
        }


        // Clean up potentially no longer required bars (e.g. when changing from December to November (31 -> 30 bars)
        while(barHolders.Count > labels.Length) {
            // First get the Bar
            Bar barToRemove = barHolders.ElementAt(barHolders.Count - 1);
            // then remove it from the barHolders-List
            barHolders.Remove(barToRemove);
            // then detach it from its parent
            barToRemove.transform.SetParent(null);
            // Finally, destroy the GameObject
            Destroy(barToRemove);
        }


        // set the threshold line
        RectTransform rtThresholdLine = thresholdLine.GetComponent<RectTransform>();

        if (thresholdLineOffsetY == 0f) {
            thresholdLineOffsetY = rtThresholdLine.transform.localPosition.y;
        }

        if (normalizedThresholdValue > 1) {
            thresholdLine.enabled = false;
            thresholdValueLabel.enabled = false;
        } else {
            thresholdLine.enabled = true;
            rtThresholdLine.transform.localPosition = new Vector3(0f, thresholdLineOffsetY + (AXES_GRAPH_OFFSET / 2) + (chartHeight * normalizedThresholdValue));

            // set the threshold value
            thresholdValueLabel.enabled = true; ;
            thresholdValueLabel.text = threshold.ToString();
        }


    }


}
