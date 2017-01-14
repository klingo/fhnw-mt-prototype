using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BarChart : MonoBehaviour {

    public Bar barHolderPrefab;

    List<Bar> bars = new List<Bar>();

    float chartHeight;

    // Use this for initialization
    void Start() {

        Transform tCanvas = this.GetComponent<RectTransform>().parent;
        RectTransform rtCanvas = tCanvas.GetComponent<RectTransform>();
        //chartHeight = Screen.height + GetComponent<RectTransform>().sizeDelta.y;
        chartHeight = rtCanvas.sizeDelta.y;

        float[] values = { 0.1f, 0.2f, 0.7f };
        DisplayGraph(values);
    }

    void DisplayGraph(float[] vals) {

        for (int i = 0; i < vals.Length; i++) {
            Bar newBar = Instantiate(barHolderPrefab) as Bar;

            newBar.transform.SetParent(transform);
            // due to the parent transformation, the localScale hs to be reset to 1/1/1, because of Canvas.scale = 0.00234375
            newBar.transform.localScale = new Vector3(1f, 1f, 1f);
            // due to parent transformation, also set the z-axis back to 0
            newBar.transform.localPosition = new Vector3(newBar.transform.localPosition.x, newBar.transform.localPosition.y);

            RectTransform rt = newBar.bar.GetComponent<RectTransform>();
            rt.sizeDelta = new Vector3(rt.sizeDelta.x, chartHeight * vals[i]);
        }
    }


}
