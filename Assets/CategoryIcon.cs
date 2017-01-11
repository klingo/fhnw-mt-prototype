using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CategoryIcon : VRTK.VRTK_InteractableObject {

    [Header("Prototype Options", order = 4)]

    [Tooltip("The colour when the object is enabled")]
    public Color enabledColor = Color.clear;

    // Use this for initialization
    protected void Start () {
	}


    public override void StartUsing(GameObject currentUsingObject) {
        Debug.Log(currentUsingObject.name);
        Debug.Log("hello");

        GetComponent<Renderer>().material.color = enabledColor;

        //base.StartUsing(currentUsingObject);
    }

    public override void StopUsing(GameObject previousUsingObject) {
        //base.StopUsing(previousUsingObject);
    }


    // Update is called once per frame
    protected override void Update () {
        base.Update();
	}
}