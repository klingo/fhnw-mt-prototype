using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class YearTable : MonoBehaviour {

    [Header("[Year]", order = 0)]
    public Year yearHolderPrefab;

    List<Year> yearHolders = new List<Year>();

    public void AddYearToTable(int year) {

        Year newYearHolder;

        // Instantiate new Year
        newYearHolder = Instantiate(yearHolderPrefab);

        newYearHolder.transform.SetParent(transform);
        // due to the parent transformation, the localScale hs to be reset to 1/1/1, because of Canvas.scale = 0.00234375
        newYearHolder.transform.localScale = Vector3.one;
        // due to parent transformation, also set the z-axis back to 0
        newYearHolder.transform.localPosition = new Vector3(newYearHolder.transform.localPosition.x, newYearHolder.transform.localPosition.y);
        // due to parent transformation, also set the rotation back to 0
        newYearHolder.transform.localRotation = Quaternion.Euler(0f, 0f, 0f);

        // Set the label
        newYearHolder.yearText.text = year.ToString();

        if (yearHolders.Count == 0) {
            // This is the first year we add, and thus also the pre-selected one
            Image firstYearImage = newYearHolder.GetComponent<Image>();
            firstYearImage.color = SceneManager.Instance.selectedRowColor;
            // store it in the scenemanager so it can be properly de-selected
            SceneManager.Instance.selectedYearRowImage = firstYearImage;
        }

        // Finally, add the bar to the list
        yearHolders.Add(newYearHolder);
    }
}
