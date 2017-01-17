using UnityEngine;
using UnityEngine.UI;
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


    Color highlightColor = new Color(1, 1, 0.8f);
    bool isActivated = false;

    public override void StartUsing(GameObject usingObject) {
        base.StartUsing(usingObject);

        isActivated = !isActivated;

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
            gameObject.GetComponent<Renderer>().material.color = highlightColor;
        }
    }


    public override void StopTouching(GameObject previousTouchingObject) {
        base.StopTouching(previousTouchingObject);

        if (!isActivated) {
            gameObject.GetComponent<Renderer>().material.color = Color.white;
        }
    }


    public void SetFinalColor() {
        if (isActivated) {
            gameObject.GetComponent<Renderer>().material.color = activeColor;
        }
        else {
            gameObject.GetComponent<Renderer>().material.color = inactiveColor;
        }
    }

}