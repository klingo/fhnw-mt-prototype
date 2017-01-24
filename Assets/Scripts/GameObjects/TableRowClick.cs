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
