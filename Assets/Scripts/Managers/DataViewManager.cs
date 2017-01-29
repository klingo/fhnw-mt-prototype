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
using System.Data;
using System.Globalization;
using UnityEngine;

public class DataViewManager : ScriptableObject {

    // All the dicitonaries where the data is stored during runtime
    public Dictionary<string, DataTable> dataTableDict { get; private set; }
    private Dictionary<string, float[]> chartValuesDict = new Dictionary<string, float[]>();
    private Dictionary<string, string[]> chartLabelsDict = new Dictionary<string, string[]>();
    private Dictionary<string, List<string[]>> tableRowsDict = new Dictionary<string, List<string[]>>();

    // Set of month names
    private string[] yearOverviewLabels = { "January", "February", "March", "April", "May", "June", "July", "August", "September", "October", "November", "December" };


    /// <summary>
    /// Constructor for the DataViewManager that inititialses the dataTableDict.
    /// </summary>
    public DataViewManager() {
        dataTableDict = new Dictionary<string, DataTable>();
    }


    /// <summary>
    /// Creates a new DataTable Object based on a filter for the given month and year, and stores it in the dataTableDictionary.
    /// </summary>
    /// <param name="year">The year (int) of the provided dataTable</param>
    /// <param name="month">The month (int) of the provided dataTable</param>
    /// <param name="table">The DataTable that shall be stored</param>
    public void StoreFilteredDataTable(DataTable dataTable, int year, int month = 0 ) {
        // Create the unique key
        string key = year.ToString() + "-" + month.ToString();

        // Check if key already exists
        if (dataTableDict.ContainsKey(key)) {
            Logger.LogWarning("DataTable for key [" + key + "] has already been added to DataTableDictionary");
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


    /// <summary>
    /// Returns a DataView based on the provided month and year information. It actually gets the DataTable
    /// from the storage, creates a DataView based on it and then returns its reference.
    /// </summary>
    /// <param name="year">The year (int) of the looked for DataView</param>
    /// <param name="month">The month (int) of the looked for DataView</param>
    /// <returns></returns>
    public DataView GetDataView(int year, int month = 0) {
        // Create the key, such as: 2016-9
        string key = year.ToString() + "-" + month.ToString();

        DataTable dataTable;
        DataView dataView = null;

        // Check if entry for that key exists
        // if no dataView was found, log error
        if (dataTableDict.TryGetValue(key, out dataTable) == false) {
            Logger.LogError("No DataTable found in DataTableManager for key = [" + key + "]");
        } else {
            dataView = new DataView(dataTable);
        }

        return dataView;
    }


    /// <summary>
    /// Preferred method to be execute when a List<string[]> with labels and values for the Table is requested.
    /// Makes sure to get the remaining arguments from the SceneManager Instance.
    /// WARNING: Cannot be executed from a secondary thread (Unity Limitaiton)! In such cases the other method 
    /// has to be called where the values need to be directly passed on.
    /// </summary>
    /// <param name="year">The selected year (int)</param>
    /// <param name="month">The selected month (int)</param>
    /// <param name="day">The selected day (int), default = 0</param>
    /// <returns></returns>
    public List<string[]> GetTableRows(int year, int month, int day = 0) {
        return GetTableRows(SceneManager.Instance.activeCategories, SceneManager.Instance.GetActiveCategoriesBinaryString(), year, month, day);
    }


    /// <summary>
    /// Overrides the above method, where the [activeCategories] and [activeCategoriesBinaryString] must be provided.
    /// This should only be used in Multi-Threaded cases where accessing the [SceneManger.Instance] is not allowed/possible
    /// due to a limitation from Unity itself.
    /// </summary>
    /// <param name="activeCategories">A HashSet<string> with all active categories</param>
    /// <param name="activeCategoriesBinaryString">The BinaryString of the categories</param>
    /// <param name="year">The selected year (int)</param>
    /// <param name="month">The selected month (int)</param>
    /// <param name="day">The selected day (int), default = 0</param>
    /// <returns></returns>
    public List<string[]> GetTableRows(HashSet<string> activeCategories, string activeCategoriesBinaryString, int year, int month, int day = 0) {
        // creates a unique key for year, month, day and selected categories
        //example: 2016-9-21-00111101011
        string key = year.ToString() + "-" + month.ToString() + "-" + day.ToString() + "-" + activeCategoriesBinaryString;

        List<string[]> rows;

        // check if the key already exists in the Dictionary
        if (tableRowsDict.TryGetValue(key, out rows) == false) {
            // Apparently not, so create new entries!

            string query = String.Empty;
            string filter = GetFilterQueryForActiveCategories(activeCategories);

            DataView dataView = GetDataView(year, month);
            dataView.RowFilter = filter;

            // since it was empty, initialise it
            rows = new List<string[]>();

            //-----------------------------------------------------------------------------------------------------------------

            int daysInMonth = DateTime.DaysInMonth(year, month);
            int startDay = 1;
            int endDay = daysInMonth;

            if (day > 0 && day <= daysInMonth) {
                // INDIVIDUAL DAY!
                startDay = day;
                endDay = day;
            }

            CultureInfo provider = CultureInfo.InvariantCulture;

            for (int currDay = startDay; currDay <= endDay; currDay++) {
                // prepare query for single day
                query = "[Date] = #" + new DateTime(year, month, currDay).ToString("MM/dd/yyyy") + "#";
                // apply it together with the categories filter
                dataView.RowFilter = filter + " AND " + query;

                if (dataView.Count > 0) {
                    // go through every single transaction of a day
                    foreach (DataRowView rowView in dataView) {
                        DataRow row = rowView.Row;
                        string[] rowAry = new string[9];
                        // Date
                        rowAry[0] = DateTime.Parse(row["Date"].ToString()).ToString("dd.MM.yyyy");
                        // Recipient
                        rowAry[1] = row["Recipient / Order issuer"].ToString();
                        // Currency
                        rowAry[2] = row["Currency"].ToString();
                        // Amount
                        rowAry[3] = row["Amount"].ToString();
                        //Category
                        rowAry[4] = row["Main category"].ToString();

                        // Account name
                        rowAry[5] = row["Account name"].ToString();
                        // Account no
                        rowAry[6] = row["Account no."].ToString();
                        // BookingText
                        rowAry[7] = row["Booking text"].ToString();
                        // Subcategory
                        rowAry[8] = row["Subcategory"].ToString();

                        rows.Add(rowAry);
                    }
                }
            }

            // add it to the Dictionary
            tableRowsDict.Add(key, rows);

            Logger.Log(1,"GetTableRows returns a NEW List<string[]>   |   KEY = " + key);
        }
        else {
            Logger.Log(1, "GetTableRows returns a CACHED List<string[]>   |   KEY = " + key);
        }

        // Now return it!
        return rows;
    }


    /// <summary>
    /// Preferred method to be execute when a KeyValuePair with labels and values for a Bar Chart is requested.
    /// Makes sure to get the remaining arguments from the SceneManager Instance.
    /// WARNING: Cannot be executed from a secondary thread (Unity Limitaiton)! In such cases the other method 
    /// has to be called where the values need to be directly passed on.
    /// </summary>
    /// <param name="year">The year for which the label and value arrays shall be returned</param>
    /// <param name="month">Optional parameter. If left out, the returned KeyValuePair is for the whole year instead of a single month.</param>
    /// <returns></returns>
    public KeyValuePair<string[], float[]> GetBarChartValuesAndLabels(int year, int month = 0) {
        return GetBarChartValuesAndLabels(SceneManager.Instance.activeCategories, SceneManager.Instance.GetActiveCategoriesBinaryString(), year, month);
    }


    /// <summary>
    /// Overrides the above method, where the [activeCategories] and [activeCategoriesBinaryString] must be provided.
    /// This should only be used in Multi-Threaded cases where accessing the [SceneManger.Instance] is not allowed/possible
    /// due to a limitation from Unity itself.
    /// </summary>
    /// <param name="activeCategories">A HashSet<string> with all active categories</param>
    /// <param name="activeCategoriesBinaryString">The BinaryString of the categories</param>
    /// <param name="year">The year for which the label and value arrays shall be returned</param>
    /// <param name="month">Optional parameter. If left out, the returned KeyValuePair is for the whole year instead of a single month.</param>
    /// <returns></returns>
    public KeyValuePair<string[], float[]> GetBarChartValuesAndLabels(HashSet<string> activeCategories, string activeCategoriesBinaryString, int year, int month = 0) {
        // creates a unique key for year, month and selected categories
        string key = year.ToString() + "-" + month.ToString() + "-" + activeCategoriesBinaryString;

        float[] values = { };
        string[] labels = { };

        // check if the key already exists in the Dictionary
        if ((chartValuesDict.TryGetValue(key, out values) && chartLabelsDict.TryGetValue(key, out labels)) == false) {
            // Apparently not, so create new entries!

            string query = String.Empty;
            string filter = GetFilterQueryForActiveCategories(activeCategories);

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

                float totalYearAmount = 0f;

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
                    // also su up the year total
                    totalYearAmount += totalMonthAmount;
                }

                // all calculations are done, check the year total
                if (totalYearAmount == 0f) {
                    // no data was found at all, resetting values and keys
                    values = new float[] { };
                    labels = new string[] { };
                }
            }

            chartValuesDict.Add(key, values);
            chartLabelsDict.Add(key, labels);

            Logger.Log(1, "GetBarChartValuesAndLabels returns a NEW KeyValuePair   |   KEY = " + key);
        } else {
            Logger.Log(1, "GetBarChartValuesAndLabels returns a CACHED KeyValuePair   |   KEY = " + key);
        }

        // Now return it!
        return new KeyValuePair<string[], float[]>(labels, values);
    }


    /// <summary>
    /// Returns a string query for the categories based on the provided HashSet. The query then can be applied 
    /// on a DataView to only see the transactions that match the activated categories
    /// </summary>
    /// <param name="activeCategories">A HashSet<string> with all active categories</param>
    /// <returns></returns>
    private string GetFilterQueryForActiveCategories(HashSet<string> activeCategories) {
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

        return filter;
    }


    /// <summary>
    /// Returns the numeric value of a month, based on its name.
    /// Example: December => 12
    /// </summary>
    /// <param name="monthName">The English name of the month</param>
    /// <returns></returns>
    public int GetMonthNoFromString(string monthName) {
        for (int i = 1; i <= yearOverviewLabels.Length; i++) {
            if (monthName.Equals(yearOverviewLabels[i - 1], StringComparison.OrdinalIgnoreCase)) {
                return i;
            }
        }
        return -1;
    }
}