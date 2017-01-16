using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using VRTK;
using VRTK.Highlighters;

public class ChartBarClick : VRTK_InteractableObject {

    public override void StartUsing(GameObject currentUsingObject) {
        base.StartUsing(currentUsingObject);

        // User clicked on a bar!
        Bar barholder = currentUsingObject.GetComponent<Bar>();
        string label = barholder.label.text;

        Debug.Log("Currently Using: " + currentUsingObject.name + " --> " + barholder.label.text);
        SceneManager.Instance.updateSelection(label);



    }



    public override void StartTouching(GameObject currentTouchingObject) {
        base.StartTouching(currentTouchingObject);

        List<Image> images = new List<Image>(currentTouchingObject.GetComponentsInChildren<Image>());

        StopAllCoroutines();
        StartCoroutine(Flash(images, 0.5f));
    }


    public override void StopTouching(GameObject previousTouchingObject) {
        base.StopTouching(previousTouchingObject);

        // Stop All Coroutines (or specifically, the flashing)
        StopAllCoroutines();

        // In case some images are currently partially faded out, fad them back in!
        List<Image> images = new List<Image>(previousTouchingObject.GetComponentsInChildren<Image>());
        foreach (Image image in images) {
            image.CrossFadeAlpha(1f, 0.5f, false);
        }
    }


    IEnumerator Flash(List<Image> images, float duration) {
        bool fadeState = false;
        while (true) {
            if (fadeState) {
                //Debug.Log("FADE IN");
                foreach (Image image in images) {
                    image.CrossFadeAlpha(1f, duration, false);
                }
                yield return new WaitForSeconds(duration);
            }
            else {
                //Debug.Log("FADE OUT");
                foreach (Image image in images) {
                    image.CrossFadeAlpha(0.5f, duration, false);
                }
                yield return new WaitForSeconds(duration);
            }
            fadeState = !fadeState;
        }
    }


    //// Use this for initialization
    //void Start () {

    //}

    //// Update is called once per frame
    //void Update () {

    //}
}
