using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using VRTK;

public class TableRowClick : VRTK_InteractableObject {


    public override void StartUsing(GameObject currentUsingObject) {
        base.StartUsing(currentUsingObject);

        // User clicked on a row!
        Row rowHolder = currentUsingObject.GetComponent<Row>();

        SceneManager.Instance.updateDetailView(rowHolder);
    }



    public override void StartTouching(GameObject currentTouchingObject) {
        base.StartTouching(currentTouchingObject);
    }

    public override void StopTouching(GameObject previousTouchingObject) {
        base.StopTouching(previousTouchingObject);

    }

}
