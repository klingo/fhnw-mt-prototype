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

using UnityEngine;
using UnityEngine.UI;
using VRTK;

public class YearRowClick : VRTK_InteractableObject {


    /// <summary>
    /// Override method from VRTK. Is triggered when a row has been clicked on and thus should load the 
    /// corresponding YearOverview data. Will also higlhight the row as being selected and checks if another row
    /// was selected before to remove its selected-colour.
    /// </summary>
    /// <param name="currentUsingObject">The GameObject to which the script is attached to</param>
    public override void StartUsing(GameObject currentUsingObject) {
        // If category processing is already going on... do NOTHING
        if (!SceneManager.Instance.isCategoryBeingProcessed) {
            // User clicked on a row!
            base.StartUsing(currentUsingObject);

            // Get the Year object of the clicked on year
            Year yearHolder = currentUsingObject.GetComponent<Year>();

            // get the image reference, of the previously selected image (to de-color it)
            Image previousImage = SceneManager.Instance.selectedYearRowImage; ;
            if (previousImage != null) {
                // if there was a previous image, change it back to white (also cross-fade out, to remove the overlay)
                previousImage.CrossFadeColor(Color.white, 0.1f, false, false);
                previousImage.color = Color.white;
            }

            // color it according to the [selectedColor]
            Image newImage = currentUsingObject.GetComponentInChildren<Image>();
            newImage.color = SceneManager.Instance.selectedRowColor;
            // store the (new) image of the new bar as the reference
            SceneManager.Instance.selectedYearRowImage = newImage;

            // Make sure that if a month or day was flashing, that it stops
            Bar previousBarChart = SceneManager.Instance.selectedDayBar;
            if (previousBarChart != null) {
                // stop the flashing for that previous bar
                Flasher.stopFlashingBar(previousBarChart);
                previousBarChart = null;
                // and set the reference to null
                SceneManager.Instance.selectedDayBar = null;
            }
            previousBarChart = SceneManager.Instance.selectedMonthBar;
            if (previousBarChart != null) {
                // stop the flashing for that previous bar
                Flasher.stopFlashingBar(previousBarChart);
                previousBarChart = null;
                // and set the reference to null
                SceneManager.Instance.selectedMonthBar = null;
            }

            // Update all the Tables/Views
            SceneManager.Instance.updateSelection(yearHolder.yearText.text);
        }
    }


    /// <summary>
    /// Override method from VRTK. Inititias a color change to visualise the highlighting effect on the row
    /// where the gesture-controller-pointer is pointing at.
    /// </summary>
    /// <param name="currentTouchingObject">The GameObject to which the script is attached to</param>
    public override void StartTouching(GameObject currentTouchingObject) {
        // If category processing is already going on... do NOTHING
        if (!SceneManager.Instance.isCategoryBeingProcessed) {
            gameObject.GetComponent<Image>().CrossFadeColor(SceneManager.Instance.highlightRowColor, 0.1f, false, false);
            base.StartTouching(currentTouchingObject);
        }
    }


    /// <summary>
    /// Override method from VRTK. Sets the color back to its original value to indicate that the highlighting
    /// is no longer active for this row. This is only done if the row is currently not selected (i.e. not activated)
    /// </summary>
    /// <param name="previousTouchingObject">The GameObject to which the script is attached to</param>
    public override void StopTouching(GameObject previousTouchingObject) {
        // If category processing is already going on... do NOTHING
        if (!SceneManager.Instance.isCategoryBeingProcessed) {
            gameObject.GetComponent<Image>().CrossFadeColor(Color.white, 0.1f, false, false);
            base.StopTouching(previousTouchingObject);
        }
    }
}