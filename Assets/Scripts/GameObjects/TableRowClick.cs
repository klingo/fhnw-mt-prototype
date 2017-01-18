using UnityEngine;
using UnityEngine.UI;
using VRTK;

public class TableRowClick : VRTK_InteractableObject {


    public override void StartUsing(GameObject currentUsingObject) {
        // User clicked on a row!
        base.StartUsing(currentUsingObject);

        // get the image reference, of the previously selected image (to de-color it)
        Image previousImage = SceneManager.Instance.selectedTableRowImage;
        if (previousImage != null) {
            // if there was a previous image, change it back to white (also cross-fade out, to remove the overlay)
            previousImage.CrossFadeColor(Color.white, 0.1f, false, false);
            previousImage.color = Color.white;
        }

        // color it according to the [selectedColor]
        Image newImage = currentUsingObject.GetComponentInChildren<Image>();
        newImage.color = SceneManager.Instance.selectedRowColor;
        // store the (new) image of the new bar as the reference
        SceneManager.Instance.selectedTableRowImage = newImage;

        // Update the Detail View
        Row rowHolder = currentUsingObject.GetComponent<Row>();
        SceneManager.Instance.updateDetailView(rowHolder);
    }



    public override void StartTouching(GameObject currentTouchingObject) {
        base.StartTouching(currentTouchingObject);

        Image image = currentTouchingObject.GetComponentInChildren<Image>();
        if (image.color != SceneManager.Instance.selectedRowColor) {
            image.CrossFadeColor(SceneManager.Instance.highlightRowColor, 0.1f, false, false);
        }
    }

    public override void StopTouching(GameObject previousTouchingObject) {
        base.StopTouching(previousTouchingObject);

        Image image = previousTouchingObject.GetComponentInChildren<Image>();
        if (image.color != SceneManager.Instance.selectedRowColor) {
            image.CrossFadeColor(Color.white, 0.1f, false, false);
        }
    }

}
