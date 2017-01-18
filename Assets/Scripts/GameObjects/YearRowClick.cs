using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using VRTK;

public class YearRowClick : VRTK_InteractableObject {

    public override void StartUsing(GameObject currentUsingObject) {
        base.StartUsing(currentUsingObject);
    }

    public override void StartTouching(GameObject currentTouchingObject) {
        base.StartTouching(currentTouchingObject);
    }

    public override void StopTouching(GameObject previousTouchingObject) {
        base.StopTouching(previousTouchingObject);
    }
}
