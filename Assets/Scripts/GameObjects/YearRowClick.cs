using UnityEngine;
using UnityEngine.UI;
using VRTK;

public class YearRowClick : VRTK_InteractableObject {

    public override void StartUsing(GameObject currentUsingObject) {
        // If category processing is already going on... do NOTHING
        if (!SceneManager.Instance.isCategoryBeingProcessed) {
            // User clicked on a row!
            base.StartUsing(currentUsingObject);

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

    public override void StartTouching(GameObject currentTouchingObject) {
        // If category processing is already going on... do NOTHING
        if (!SceneManager.Instance.isCategoryBeingProcessed) {
            gameObject.GetComponent<Image>().CrossFadeColor(SceneManager.Instance.highlightRowColor, 0.1f, false, false);
            base.StartTouching(currentTouchingObject);
        }
    }


    public override void StopTouching(GameObject previousTouchingObject) {
        // If category processing is already going on... do NOTHING
        if (!SceneManager.Instance.isCategoryBeingProcessed) {
            gameObject.GetComponent<Image>().CrossFadeColor(Color.white, 0.1f, false, false);
            base.StopTouching(previousTouchingObject);
        }
    }
}
