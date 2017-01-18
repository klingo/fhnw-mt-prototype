using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using UnityEngine;

public class DataViewManager : ScriptableObject {

    public Dictionary<string, DataTable> dataTableDict { get; private set; }
    private Dictionary<string, float[]> chartValuesDict = new Dictionary<string, float[]>();
    private Dictionary<string, string[]> chartLabelsDict = new Dictionary<string, string[]>();
    private Dictionary<string, List<string[]>> tableRowsDict = new Dictionary<string, List<string[]>>();

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


    /// <summary>
    /// Preferred method to be execute when a List<string[]> with labels and values for the Table is requested.
    /// </summary>
    /// <param name="year"></param>
    /// <param name="month"></param>
    /// <param name="day"></param>
    /// <returns></returns>
    public List<string[]> GetTableRows(int year, int month, int day = 0) {
        return GetTableRows(SceneManager.Instance.activeCategories, SceneManager.Instance.GetActiveCategoriesBinaryString(), year, month, day);
    }


    /// <summary>
    /// Overrides the above method, where the [activeCategories] and [activeCategoriesBinaryString] must be provided.
    /// This should only be used in Multi-Threaded cases where accessing the [SceneManger.Instance] is not allowed/possible.
    /// </summary>
    /// <param name="activeCategories"></param>
    /// <param name="activeCategoriesBinaryString"></param>
    /// <param name="year"></param>
    /// <param name="month"></param>
    /// <param name="day"></param>
    /// <returns></returns>
    public List<string[]> GetTableRows(HashSet<string> activeCategories, string activeCategoriesBinaryString, int year, int month, int day = 0) {
        // creates a unique key for year, month, day and selected categories
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

            Debug.Log("GetTableRows returns a NEW List<string[]>");
        }
        else {
            Debug.Log("GetTableRows returns a CACHED List<string[]>");
        }

        // Now return it!
        return rows;
    }


    /// <summary>
    /// Preferred method to be execute when a KeyValuePair with labels and values for a Bar Chart is requested.
    /// </summary>
    /// <param name="year"></param>
    /// <param name="month">Optional parameter. If left out, the returned KeyValuePair is for the whole year instead of a single month.</param>
    /// <returns></returns>
    public KeyValuePair<string[], float[]> GetBarChartValuesAndLabels(int year, int month = 0) {
        return GetBarChartValuesAndLabels(SceneManager.Instance.activeCategories, SceneManager.Instance.GetActiveCategoriesBinaryString(), year, month);
    }


    /// <summary>
    /// Overrides the above method, where the [activeCategories] and [activeCategoriesBinaryString] must be provided.
    /// This should only be used in Multi-Threaded cases where accessing the [SceneManger.Instance] is not allowed/possible.
    /// </summary>
    /// <param name="activeCategories"></param>
    /// <param name="activeCategoriesBinaryString"></param>
    /// <param name="year"></param>
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

            chartValuesDict.Add(key, values);
            chartLabelsDict.Add(key, labels);

            Debug.Log("GetBarChartValuesAndLabels returns a NEW KeyValuePair");
        } else {
            Debug.Log("GetBarChartValuesAndLabels returns a CACHED KeyValuePair");
        }

        // Now return it!
        return new KeyValuePair<string[], float[]>(labels, values);
    }


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


    public int GetMonthNoFromString(string monthName) {
        for (int i = 1; i <= yearOverviewLabels.Length; i++) {
            if (yearOverviewLabels[i - 1] == monthName) {
                return i;
            }
        }
        return -1;
    }

}
