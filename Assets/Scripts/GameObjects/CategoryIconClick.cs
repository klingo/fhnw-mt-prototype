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
using VRTK;

public class CategoryIconClick : VRTK_InteractableObject {

    [Header("[Color]", order = 4)]
    [Tooltip("Sets the color of the category icon when NOT activated.")]
    public Color inactiveColor = Color.white;
    [Tooltip("Sets the color of the category icon when the data is currently being loaded.")]
    public Color loadingColor = Color.yellow;
    [Tooltip("Sets the color of the category icon when the data loaded and displayed.")]
    public Color activeColor = Color.green;
    [Tooltip("Sets the color of the category icon when the data loaded and displayed.")]
    public float monthlyCategoryThreshold = 0f;


    Color highlightColorInactive = new Color(1, 1, 0.8f);
    Color highlightColorActive = new Color(0.8f, 1, 0.8f);
    bool isActivated = false;
    bool isLoading = false;


    public override void StartUsing(GameObject usingObject) {
        // If category processing is already going on... do NOTHING
        if (!SceneManager.Instance.isCategoryBeingProcessed) {
            // change activation state
            // indicate that loading started
            SetToLoadingState();

            base.StartUsing(usingObject);

            if (isActivated) {
                SceneManager.Instance.addGameObjectToCategoryFilter(gameObject, monthlyCategoryThreshold);
            }
            else {
                SceneManager.Instance.removeGameObjectFromCategoryFilter(gameObject, monthlyCategoryThreshold);
            }
        }
    }


    public override void StartTouching(GameObject currentTouchingObject) {
        // If category processing is already going on... do NOTHING
        if (!SceneManager.Instance.isCategoryBeingProcessed) {
            if (!isLoading) {
                // Only update the highlight colour, when NOT loading
                if (isActivated) {
                    // when activated, allow a highlight color
                    gameObject.GetComponent<Renderer>().material.color = highlightColorActive;
                }
                else {
                    // when NOT activated, allow a highlight color as well
                    gameObject.GetComponent<Renderer>().material.color = highlightColorInactive;
                }
            }
            base.StartTouching(currentTouchingObject);
        }
    }


    public override void StopTouching(GameObject previousTouchingObject) {
        // If category processing is already going on... do NOTHING
        if (!SceneManager.Instance.isCategoryBeingProcessed) {
            if (!isLoading) {
                // Only update the highlight colour, when NOT loading
                if (isActivated) {
                    // when activated, remove a highlight color
                    gameObject.GetComponent<Renderer>().material.color = activeColor;
                }
                else {
                    // when NOT activated, remove a highlight color as well
                    gameObject.GetComponent<Renderer>().material.color = inactiveColor;
                }
            }
            base.StopTouching(previousTouchingObject);
        }
    }


    public void SetToLoadingState() {
        // change activation state
        isActivated = !isActivated;

        // indicate that loading started
        isLoading = true;
    }

    public void SetFinalColor() {
        if (isActivated) {
            gameObject.GetComponent<Renderer>().material.color = activeColor;
        }
        else {
            gameObject.GetComponent<Renderer>().material.color = inactiveColor;
        }

        // loading is complete
        isLoading = false;
    }

}