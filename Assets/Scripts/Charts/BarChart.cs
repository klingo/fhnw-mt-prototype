using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/// <summary>
/// Based on "Unity Tutorial: Bar Chart" (https://www.youtube.com/watch?v=oDX8MiKMorU)
/// </summary>
public class BarChart : MonoBehaviour {

    public Bar barHolderPrefab;
    public float[] inputValues;
    public string[] labels;
    public Color[] colors;

    List<Bar> bars = new List<Bar>();

    float chartHeight;
    const float AXES_GRAPH_OFFSET = 6;  // the offset of a bar compared to the axes-graph
    const float BAR_SCALE_FACTOR = 0.95f; // the scaling of the max bar height

    // Use this for initialization
    void Start() {
        // Prepare Chart-Size first
        Transform tCanvas = this.GetComponent<RectTransform>().parent;
        RectTransform rtCanvas = tCanvas.GetComponent<RectTransform>();
        chartHeight = (rtCanvas.sizeDelta.y - AXES_GRAPH_OFFSET) * BAR_SCALE_FACTOR;



        DisplayGraph(inputValues);
    }

    void DisplayGraph(float[] vals) {
        float maxValue = vals.Max();

        for (int i = 0; i < vals.Length; i++) {
            // Instantiate new Bar
            Bar newBar = Instantiate(barHolderPrefab) as Bar;

            newBar.transform.SetParent(transform);
            // due to the parent transformation, the localScale hs to be reset to 1/1/1, because of Canvas.scale = 0.00234375
            newBar.transform.localScale = Vector3.one;
            // due to parent transformation, also set the z-axis back to 0
            newBar.transform.localPosition = new Vector3(newBar.transform.localPosition.x, newBar.transform.localPosition.y);

            // Set the size of the bar
            RectTransform rt = newBar.bar.GetComponent<RectTransform>();
            float normalizedValue = vals[i] / maxValue;
            rt.sizeDelta = new Vector3(rt.sizeDelta.x, chartHeight * normalizedValue);

            // Set the color of the bar
            newBar.bar.color = colors[i % colors.Length];

            // Set the bottom label of the bar
            if (labels.Length <= i) {
                newBar.label.text = "UNDEFINED";
            } else {
                newBar.label.text = labels[i];
            }

            // Set the value label at the top of the bar
            newBar.barValue.text = vals[i].ToString();
            // if height is too small, move labelt ot op of bar
            if (rt.sizeDelta.y < 30f) {
                newBar.barValue.rectTransform.pivot = new Vector2(0.5f, 0f);
                newBar.barValue.rectTransform.anchoredPosition = Vector2.zero;
            }

            // Finally, add the bar to the list
            bars.Add(newBar);
        }
    }


}
