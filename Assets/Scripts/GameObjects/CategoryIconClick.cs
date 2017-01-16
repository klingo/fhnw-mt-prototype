using UnityEngine;
using VRTK;

public class CategoryIconClick : VRTK_InteractableObject {

    [Header("Category Options", order = 4)]
    [Tooltip("Sets the treshold for the planned/expected expenses in this category for a single month. It is used for the visualisation of the bar chart.")]
    public float monthlyCategoryThreshold = 0f;
    [Tooltip("If this is checked then the below defined Annual 'Category Threshold' will be used instead of multiplying the monthly threshold by 12 (months).")]
    public bool overrideAnnualThreshold = false;
    [Tooltip("Overrides the annual threshold for the planned/expected expenses.")]
    public float annualCategoryThreshold = 0f;

    bool isActivated = false;
    bool hasChanged = false;
    Color defaultColor;

    public override void StartUsing(GameObject usingObject) {
        base.StartUsing(usingObject);

        hasChanged = true;
        isActivated = !isActivated;

        if (isActivated) {
            SceneManager.Instance.addGameObjectToCategoryFilter(gameObject.name);
        } else {
            SceneManager.Instance.removeGameObjectFromCategoryFilter(gameObject.name);
        }
    }

    public override void StopUsing(GameObject usingObject) {
        base.StopUsing(usingObject);
    }

    protected void Start() {
        defaultColor = gameObject.GetComponent<Renderer>().material.color;
    }

    protected override void Update() {
        base.Update();

        if (hasChanged) {
            hasChanged = false;

            Debug.Log("gameObject = " + gameObject);

            if (isActivated) {
                gameObject.GetComponent<Renderer>().material.color = Color.green;
            }
            else {
                gameObject.GetComponent<Renderer>().material.color = defaultColor; // Color.clear;
            }
        }
    }
}