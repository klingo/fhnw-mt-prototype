// The MIT License (MIT)

// Copyright(c) 2017 Fabian Schär

// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:

// The above copyright notice and this permission notice shall be included in all
// copies or substantial portions of the Software.

// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
// SOFTWARE.

using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using VRTK;

public class ChartBarClick : VRTK_InteractableObject {

    public override void StartUsing(GameObject currentUsingObject) {

        base.StartUsing(currentUsingObject);

        Bar previousBarChart;

        // User clicked on a bar!
        Bar barHolder = currentUsingObject.GetComponent<Bar>();
        List<Image> barHolderImages = new List<Image>(barHolder.GetComponentsInChildren<Image>());
        string label = barHolder.label.text;

        // Check if the selected bar is from the month or year chart
        string parentChartName = gameObject.GetComponentInParent<BarChart>().name;
        if (parentChartName == SceneManager.CHART_NAME_MONTH_OVERVIEW) {
            // a DAY was clicked on in the MONTH overview
            // stop the flashing for that previous bar
            Flasher.stopFlashingBar(SceneManager.Instance.selectedDayBar);

            // Activate blinking for the new one
            barHolder.StartCoroutine(Flasher.Flash(barHolderImages, 0.5f));
            // store the (new) barHolder of the new bar as the reference
            SceneManager.Instance.selectedDayBar = barHolder;
        }
        else if (parentChartName == SceneManager.CHART_NAME_YEAR_OVERVIEW) {
            // a MONTH was clicked on in the YEAR overview
            previousBarChart = SceneManager.Instance.selectedMonthBar;
            // stop the flashing for that previous bar
            Flasher.stopFlashingBar(previousBarChart);

            // Activate blinking for the new one
            barHolder.StartCoroutine(Flasher.Flash(barHolderImages, 0.5f));
            // store the (new) barHolder of the new bar as the reference
            SceneManager.Instance.selectedMonthBar = barHolder;

            // In case of a changed month, also stop the day-coroutine!
            previousBarChart = SceneManager.Instance.selectedDayBar;
            if (previousBarChart != null) {
                // stop the flashing for that previous bar
                Flasher.stopFlashingBar(previousBarChart);
                previousBarChart = null;
                // and set the reference to null
                SceneManager.Instance.selectedDayBar = null;
            }
        }
        else {
            Logger.LogError("Invalid Bar Chart for Using!");
        }

        SceneManager.Instance.updateSelection(label);
    }

    public override void StartTouching(GameObject currentTouchingObject) {
        base.StartTouching(currentTouchingObject);

        List<Image> images = new List<Image>(currentTouchingObject.GetComponentsInChildren<Image>());
        foreach (Image image in images) {
            // Only start to highlight it, if it currently has 1.00 alpha value
            if (image.color.a == 1f) {
                // Then highlight it to 0.5f alpha, to distinguish it from flashing that does not go below 0.51f
                image.color = new Color(image.color.r, image.color.g, image.color.b, 0.5f);
            }
        }
    }


    public override void StopTouching(GameObject previousTouchingObject) {
        base.StopTouching(previousTouchingObject);

        // In case some images are currently partially faded out, fade them back in!
        List<Image> images = new List<Image>(previousTouchingObject.GetComponentsInChildren<Image>());
        foreach (Image image in images) {
            // Only revert the highlight, if it currently is being highlighted, but NOT flashing
            if (image.color.a == 0.5f) {
                image.color = new Color(image.color.r, image.color.g, image.color.b, 1);
            }
        }
    }
}
