﻿using UnityEngine;
using VRTK;

public class CategoryIconClick : VRTK_InteractableObject {

    [Header("Category Options", order = 4)]
    [Tooltip("Sets the color of the category icon when NOT activated.")]
    public Color inactiveColor = Color.white;
    [Tooltip("Sets the color of the category icon when the data is currently being loaded.")]
    public Color loadingColor = Color.yellow;
    [Tooltip("Sets the color of the category icon when the data loaded and displayed.")]
    public Color activeColor = Color.green;
    [Tooltip("Sets the treshold for the planned/expected expenses in this category for a single month. It is used for the visualisation of the bar chart.")]
    public float monthlyCategoryThreshold = 0f;

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

    public void SetFinalColor() {
        if (isActivated) {
            gameObject.GetComponent<Renderer>().material.color = activeColor;
        }
        else {
            gameObject.GetComponent<Renderer>().material.color = inactiveColor;
        }
    }

}