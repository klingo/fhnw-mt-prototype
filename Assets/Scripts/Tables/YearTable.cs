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

using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class YearTable : MonoBehaviour {

    // Reference to the Prefab of a Year-Row object for the table
    [Header("[Year]", order = 0)]
    public Year yearHolderPrefab;

    // List of all Years in the table
    List<Year> yearHolders = new List<Year>();


    /// <summary>
    /// Adds the provided year to the table. If this is the first year added, also directly 
    /// highlight it accordingly and set the value to the SceneManager for the initial data load.
    /// </summary>
    /// <param name="year">int value of the year</param>
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