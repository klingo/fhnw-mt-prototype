using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using UnityEngine;

public class DataViewManager : ScriptableObject {

    public Dictionary<string, DataTable> dataTableDict { get; private set; }
    private Dictionary<string, float[]> chartValues = new Dictionary<string, float[]>();
    private Dictionary<string, string[]> chartLabels = new Dictionary<string, string[]>();

    private string[] yearOverviewLabels = { "January", "February", "March", "April", "May", "June", "July", "August", "September", "October", "November", "December" };

    public DataViewManager() {
        dataTableDict = new Dictionary<string, DataTable>();
    }

    /// <summary>
    /// Creates a new DataTable Object based on a filter for the given month and year, and stores it in the dataTableDictionary.
    /// </summary>
    /// <param name="year"></param>
    /// <param name="month"></param>
    /// <param name="table"></param>
    public void StoreFilteredDataTable(DataTable dataTable, int year, int month = 0 ) {
        // Create the unique key
        string key = year.ToString() + "-" + month.ToString();

        // Check if key already exists
        if (dataTableDict.ContainsKey(key)) {
            Debug.LogError("DataTable for key [" + key + "] has already been added to DataTableDictionary");
        } else {
            string query = String.Empty;
            if (month > 0) {
                // Create query for a month (e.g. from 01.09.2016 until 30.09.2016)
                query = "[Date] >= #" + new DateTime(year, month, 1).ToString("MM/dd/yyyy") + "# AND [Date] <= #" + new DateTime(year, month, DateTime.DaysInMonth(year, month)).ToString("MM/dd/yyyy") + "#";
            } else {
                // Create query for a month (e.g. from 01.09.2016 until 30.09.2016)
                query = "[Date] >= #" + new DateTime(year, 1, 1).ToString("MM/dd/yyyy") + "# AND [Date] <= #" + new DateTime(year, 12, 31).ToString("MM/dd/yyyy") + "#";
            }

            // Apply filter
            DataView dataView = new DataView(dataTable);
            dataView.RowFilter = query;

            // Create a table beased on the filtered view and store it in the dictionary
            DataTable filteredTable = dataView.ToTable();
            dataTableDict.Add(key, filteredTable);
        }
    }



    public DataView GetDataView(int year, int month = 0) {
        // Create the key, such as: 2016-9
        string key = year.ToString() + "-" + month.ToString();

        DataTable dataTable;
        DataView dataView = null;

        // Check if entry for that key exists
        // if no dataView was found, log error
        if (dataTableDict.TryGetValue(key, out dataTable) == false) {
            Debug.LogError("No DataTable found in DataTableManager for key = [" + key + "]");
        } else {
            dataView = new DataView(dataTable);
        }

        return dataView;
    }


    public KeyValuePair<string[], float[]> GetBarChartValuesAndLabels(int year, int month = 0) {
        return GetBarChartValuesAndLabels(SceneManager.Instance.activeCategories, SceneManager.Instance.GetActiveCategoriesBinaryString(), year, month);
    }

    public KeyValuePair<string[], float[]> GetBarChartValuesAndLabels(HashSet<string> activeCategories, string activeCategoriesBinaryString, int year, int month = 0) {
        // creates a unique key for year, month and selected categories
        string key = year.ToString() + "-" + month.ToString() + "-" + activeCategoriesBinaryString;

        float[] values = { };
        string[] labels = { };

        // check if the key already exists in the Dictionary
        if ((chartValues.TryGetValue(key, out values) && chartLabels.TryGetValue(key, out labels)) == false) {
            // Apparently not, so create new entries!

            string query = String.Empty;
            string filter = String.Empty;

            // prepare the category filter
            foreach (string hashVal in activeCategories) {
                query += "'" + hashVal + "',";
            }

            // if there is at least one entry (incl. a comma), remove the last character (i.e. the comma) again
            if (query.Length > 0) {
                query = query.Remove(query.Length - 1);
                filter = "[Main category] IN(" + query + ")";
            }
            else {
                // when nothing is selected, nothing is shown
                filter = "[Main category] = ''";
            }

            DataView dataView = GetDataView(year, month);
            dataView.RowFilter = filter;

            //-----------------------------------------------------------------------------------------------------------------

            // used for the month total
            float totalMonthAmount = 0f;

            if (month > 0) {
                // INDIVIDUAL MONTH!
                int daysInMonth = DateTime.DaysInMonth(year, month);
                values = new float[daysInMonth];
                labels = new string[daysInMonth];

                for (int currDay = 1; currDay <= daysInMonth; currDay++) {
                    // prepare query for single day
                    query = "[Date] = #" + new DateTime(year, month, currDay).ToString("MM/dd/yyyy") + "#";
                    // apply it together with the categories filter
                    dataView.RowFilter = filter + " AND " + query;

                    if (dataView.Count > 0) {
                        // go through every single transaction of a day
                        foreach (DataRowView rowView in dataView) {
                            DataRow row = rowView.Row;
                            totalMonthAmount += Single.Parse(row["Amount"].ToString());
                        }
                    }

                    // set current total to the day value
                    values[currDay - 1] = totalMonthAmount;
                    // also prepare the labels array
                    labels[currDay - 1] = currDay.ToString();
                }
                
            } else {
                // WHOLE YEAR!
                values = new float[12];
                labels = new string[12];

                // now create the data-list for the chart
                for (int currMonth = 1; currMonth < 13; currMonth++) {
                    int daysInMonth = DateTime.DaysInMonth(year, currMonth);

                    // reset month total
                    totalMonthAmount = 0f;

                    for (int currDay = 1; currDay < (daysInMonth + 1); currDay++) {
                        // prepare query for single day
                        query = "[Date] = #" + new DateTime(year, currMonth, currDay).ToString("MM/dd/yyyy") + "#";
                        // apply it together with the categories filter
                        dataView.RowFilter = filter + " AND " + query;

                        if (dataView.Count > 0) {
                            // go through every single transaction of a day
                            foreach (DataRowView rowView in dataView) {
                                DataRow row = rowView.Row;
                                totalMonthAmount += Single.Parse(row["Amount"].ToString());
                            }
                        }
                    }

                    // set current total to the month value
                    values[currMonth - 1] = totalMonthAmount;
                    // also prepare the labels array
                    labels[currMonth - 1] = yearOverviewLabels[currMonth - 1];
                }
            }

            chartValues.Add(key, values);
            chartLabels.Add(key, labels);

            Debug.Log("GetBarChartValuesAndLabels returns a NEW KeyValuePair");
        } else {
            Debug.Log("GetBarChartValuesAndLabels returns a CACHED KeyValuePair");
        }

        // Now return it!
        return new KeyValuePair<string[], float[]>(labels, values);
    }

    public int GetMonthNoFromString(string monthName) {
        for (int i = 1; i <= yearOverviewLabels.Length; i++) {
            if (yearOverviewLabels[i - 1] == monthName) {
                return i;
            }
        }
        return -1;
    }

}
