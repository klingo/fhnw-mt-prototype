using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class Table : MonoBehaviour {

    [Header("[Row]", order = 0)]
    public Row rowHolderPrefab;

    [Header("[Text]", order = 1)]
    public Text tableTitle;

    List<Row> rowHolders = new List<Row>();


    /// <summary>
    /// 
    /// </summary>
    /// <param name="tableRows"></param>
    /// <param name="tableTitle"></param>
    public void DisplayTable(List<string[]> tableRows, string tableTitle) {

        // set the table title
        this.tableTitle.text = tableTitle;

        for (int currRowIndex = 0; currRowIndex < tableRows.Count; currRowIndex++) {

            string[] currTableRow = tableRows.ElementAt<string[]>(currRowIndex);

            Row newRowHolder;
            bool reuseRow = false;

            // first check if we already have instances of rowHolders
            if (rowHolders.Count > currRowIndex) {
                newRowHolder = rowHolders.ElementAt<Row>(currRowIndex);
                reuseRow = true;
            }
            else {
                // Instantiate new Row
                newRowHolder = Instantiate(rowHolderPrefab) as Row;

                newRowHolder.transform.SetParent(transform);
                // due to the parent transformation, the localScale hs to be reset to 1/1/1, because of Canvas.scale = 0.00234375
                newRowHolder.transform.localScale = Vector3.one;
                // due to parent transformation, also set the z-axis back to 0
                newRowHolder.transform.localPosition = new Vector3(newRowHolder.transform.localPosition.x, newRowHolder.transform.localPosition.y);
                // due to parent transformation, also set the rotation back to 0
                newRowHolder.transform.localRotation = Quaternion.Euler(0f, 0f, 0f);
            }

            // Set the color of the row
            // TODO: newRowHolder.rowImage

            // Set the vlaues of the rown
            newRowHolder.dateText.text = currTableRow[0];
            newRowHolder.recipientText.text = currTableRow[1];
            newRowHolder.currencyText.text = currTableRow[2];
            newRowHolder.amountText.text = currTableRow[3];
            newRowHolder.categoryText.text = currTableRow[4];

            // Finally, add the bar to the list
            if (!reuseRow) {
                rowHolders.Add(newRowHolder);
            }
        }

        // Clean up potentially no longer required rows (e.g. when changing from a month-view to a day-view
        while (rowHolders.Count > tableRows.Count) {
            // First get the Row
            Row rowToRemove = rowHolders.ElementAt(rowHolders.Count - 1);
            // then remove it from the barHolders-List
            rowHolders.Remove(rowToRemove);
            // then detach it from its parent
            rowToRemove.transform.SetParent(null);
            // Finally, destroy the GameObject
            Destroy(rowToRemove);
        }
    }
}
