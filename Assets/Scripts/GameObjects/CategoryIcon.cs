using UnityEngine;
using VRTK;

public class CategoryIcon : VRTK_InteractableObject {

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