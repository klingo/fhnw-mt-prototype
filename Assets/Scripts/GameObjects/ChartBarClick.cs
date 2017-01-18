using System.Collections;
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
            stopFlashingBar(SceneManager.Instance.selectedDayBar);

            // Activate blinking for the new one
            barHolder.StartCoroutine(Flash(barHolderImages, 0.5f));
            // store the (new) barHolder of the new bar as the reference
            SceneManager.Instance.selectedDayBar = barHolder;
        }
        else if (parentChartName == SceneManager.CHART_NAME_YEAR_OVERVIEW) {
            // a MONTH was clicked on in the YEAR overview
            previousBarChart = SceneManager.Instance.selectedMonthBar;
            // stop the flashing for that previous bar
            stopFlashingBar(previousBarChart);

            // Activate blinking for the new one
            barHolder.StartCoroutine(Flash(barHolderImages, 0.5f));
            // store the (new) barHolder of the new bar as the reference
            SceneManager.Instance.selectedMonthBar = barHolder;

            // In case of a changed month, also stop the day-coroutine!
            previousBarChart = SceneManager.Instance.selectedDayBar;
            if (previousBarChart != null) {
                // stop the flashing for that previous bar
                stopFlashingBar(previousBarChart);
                previousBarChart = null;
                // and set the reference to null
                SceneManager.Instance.selectedDayBar = null;
            }
        }
        else {
            Debug.LogError("Invalid Bar Chart for Using!");
        }

        SceneManager.Instance.updateSelection(label);
    }


    private void stopFlashingBar(Bar barHolder) {
        if (barHolder != null) {
            // if there was a previous bar, stop the blinking
            barHolder.StopAllCoroutines();
            // In case some images are currently partially faded out, fad them back in!
            List<Image> images = new List<Image>(barHolder.GetComponentsInChildren<Image>());
            foreach (Image image in images) {
                image.CrossFadeAlpha(1f, 0.5f, false);
            }
        }
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


    IEnumerator Flash(List<Image> images, float duration) {
        bool fadeState = false;
        while (true) {
            if (fadeState) {
                //Debug.Log("FADE IN");
                foreach (Image image in images) {
                    // Flashing only goes back to 0.99 alpha value, in order to distinguish it with non-flasing
                    // bar that always has an alpha value of 1.00
                    image.CrossFadeAlpha(0.99f, duration, false);
                }
                yield return new WaitForSeconds(duration);
            }
            else {
                //Debug.Log("FADE OUT");
                foreach (Image image in images) {
                    // Flashing only goes down to 0.51 alpha value, in order to distinguish it with non-flashing
                    // but highlighted bars that always have an alpha value of 0.50
                    image.CrossFadeAlpha(0.51f, duration, false);
                }
                yield return new WaitForSeconds(duration);
            }
            fadeState = !fadeState;
        }
    }

}
