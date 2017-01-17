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
        base.StartUsing(usingObject);

        // change activation state
        isActivated = !isActivated;
        // indicate that loading started
        isLoading = true;

        if (isActivated) {
            SceneManager.Instance.addGameObjectToCategoryFilter(gameObject, monthlyCategoryThreshold);
        }
        else {
            SceneManager.Instance.removeGameObjectFromCategoryFilter(gameObject, monthlyCategoryThreshold);
        }
    }


    public override void StartTouching(GameObject currentTouchingObject) {
        base.StartTouching(currentTouchingObject);

        if (!isActivated) {
            // when NOT activated, allow a highlight color
            gameObject.GetComponent<Renderer>().material.color = highlightColorInactive;
        } else if(!isLoading) {
            // when activated and NOT loading anymore, allow a highlight color as well
            gameObject.GetComponent<Renderer>().material.color = highlightColorActive;
        }
    }


    public override void StopTouching(GameObject previousTouchingObject) {
        base.StopTouching(previousTouchingObject);

        if (!isActivated) {
            // when NOT activated, remove a highlight color
            gameObject.GetComponent<Renderer>().material.color = inactiveColor;
        }
        else if (!isLoading) {
            // when activated and NOT loading anymore, remove a highlight color as well
            gameObject.GetComponent<Renderer>().material.color = activeColor;
        }
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