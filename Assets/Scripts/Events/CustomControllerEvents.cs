using UnityEngine;
using VRTK;

public class CustomControllerEvents : MonoBehaviour {

	// Use this for initialization
	void Start () {
        if (GetComponent<VRTK_ControllerEvents>() == null) {
            Debug.LogError("CustomControllerEvents is required to be attached to a Controller that has the VRTK_ControllerEvents script attached to it");
            return;
        }

        // Assign custom methods to the button one events
        GetComponent<VRTK_ControllerEvents>().ButtonOnePressed += new ControllerInteractionEventHandler(DoButtonOnePressed);
        GetComponent<VRTK_ControllerEvents>().ButtonOneReleased += new ControllerInteractionEventHandler(DoButtonOneReleased);

    }

    private void DoButtonOnePressed(object sender, ControllerInteractionEventArgs e) {
        
        // Only proceed, if no processing is currently ongoing
        if (!SceneManager.Instance.isCategoryBeingProcessed) {
            if (SceneManager.Instance.activeCategories.Count == 0) {
                // No categories are active at the moment
                // This means, we want to activate all of them
                SceneManager.Instance.addAllGameObjectsToCategoryFilter();
            } else {
                // One or more (or all) categories are active at the moment
                // This means, we want to DEactivate all of them
                SceneManager.Instance.removeAllGameObjectsFromCategoryFilter();
            }
        }
    }

    private void DoButtonOneReleased(object sender, ControllerInteractionEventArgs e) {
        // Do nothing on release
    }
}
