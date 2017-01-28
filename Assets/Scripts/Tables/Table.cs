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

using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class Table : MonoBehaviour {

    // Reference to the prefab of the Row object
    [Header("[Row]", order = 0)]
    public Row rowHolderPrefab;

    // Reference to the Text object of the table title
    [Header("[Text]", order = 1)]
    public Text tableTitle;

    // List of all rows in the table
    List<Row> rowHolders = new List<Row>();

    // Required for proper formatting of Date and Amount values
    System.Globalization.CultureInfo modCulture = new System.Globalization.CultureInfo("de-CH");


    /// <summary>
    /// Displays a list of string arays inside the table as individual rows, with the given
    /// title that is shown alongside the table.
    /// </summary>
    /// <param name="tableRows">Arrays with the contents of the individual rows</param>
    /// <param name="tableTitle">The title of the table</param>
    public void DisplayTable(List<string[]> tableRows, string tableTitle) {

        // set the table title
        this.tableTitle.text = tableTitle;

        // make it null-safe (easier for other methods, to just pass 'null' for clearing the table
        if (tableRows == null) {
            tableRows = new List<string[]>();
        }

        // Loop through all row entries
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

            // Set the values of the rown
            newRowHolder.dateText.text = currTableRow[0];
            newRowHolder.recipientText.text = currTableRow[1];
            newRowHolder.currencyText.text = currTableRow[2];
            newRowHolder.amountText.text = Single.Parse(currTableRow[3]).ToString("#,##0.00", modCulture);
            newRowHolder.categoryText.text = currTableRow[4];

            // Also store the other values, for the detail view later on
            newRowHolder.accountName = currTableRow[5];
            newRowHolder.accountNo = currTableRow[6];
            newRowHolder.bookingText = currTableRow[7];
            newRowHolder.subcategory = currTableRow[8];

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

            // TODO: Implement PoolManager instead of desotrying the objects!
            // then detach it from its parent
            //rowToRemove.transform.SetParent(null);

            // Finally, "destroy" the GameObject
            // Unity does not actually delete the object though from runtime
            rowToRemove.name = "DELETE THIS ROW";
            rowToRemove.gameObject.SetActive(false);
            rowToRemove.enabled = false;
            rowToRemove.transform.localScale = Vector3.zero;
            rowToRemove.transform.localPosition = Vector3.zero;
            Destroy(rowToRemove);
            DestroyImmediate(rowToRemove, true);
            rowToRemove = null;
        }
    }


    /// <summary>
    /// Returns the first [Row] object, if there is only one existing. In all other cases, [null] will be returned.
    /// </summary>
    /// <returns>The first Row object in the table, if there is at least one</returns>
    public Row GetFirstRowIfOnlyOneExists() {
        if (rowHolders.Count == 1) {
            return rowHolders.ElementAt<Row>(0);
        }
        return null;
    }


    /// <summary>
    /// Returns whether there are any entries in the table or not
    /// </summary>
    /// <returns>boolean whether there are any entries in the table</returns>
    public bool HasEntries() {
        return (rowHolders.Count > 0);
    }
}