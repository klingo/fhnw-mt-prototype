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

public class CustomControllerEvents : MonoBehaviour {

	void Start () {
        if (GetComponent<VRTK_ControllerEvents>() == null) {
            Logger.LogError("CustomControllerEvents has to be attached to a controller with the VRTK_ControllerEvents script");
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
