using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Data;
using UnityEngine;
using System.Text.RegularExpressions;
using System;

public class SceneManager : Singleton<SceneManager> {

    // Singleton
    protected SceneManager() { }

    // Constants for CSV File location
    private const string CSV_REL_PATH = "\\Assets\\Resources\\CSV\\";
    private const string CSV_FILE_NAME = "Expenses_2016_anonymized.csv";

    // Constants for names of the different charts
    private const string CHART_NAME_YEAR_OVERVIEW = "BarChart-YearOverview";
    private const string CHART_NAME_MONTH_OVERVIEW = "BarChart-MonthOverview";

    private Dictionary<string, string> gameObjectCategoryMap;

    // A hashset that contains all active categories
    public HashSet<string> activeCategories { get; private set; }

    // Data containers
    private DataTable dataTable;
    private DataView dataView;
    private DataView monthlyDataView;

    // array values for charts
    float[] yearOverviewValues = new float[12];
    float[] monthOverviewValues = { };

    // array labels for charts
    string[] yearOverviewLabels = { "January", "February", "March", "April", "May", "June", "July", "August", "September", "October", "November", "December" };
    string[] monthOverviewLabels = new string[31];

    // to be updated!
    private const int CURRENT_YEAR = 2016;


    /// <summary>
    /// Awake is always called before any Start functions
    /// </summary>
    void Awake() {
        // Debug to inform that Singleton was created!
        Debug.Log("Awoke Singleton Instance: " + gameObject.GetInstanceID());
    }


    /// <summary>
    /// 
    /// </summary>
    void Start() {
        // init HashSet
        activeCategories = new HashSet<string>();

        // init mapping between gameObject name (in Unity) and category name (in CSV)
        gameObjectCategoryMap = new Dictionary<string, string>();
        gameObjectCategoryMap.Add("Movie", "Communication & media");
        gameObjectCategoryMap.Add("MedicalBox", "Health");
        gameObjectCategoryMap.Add("ShoppingCart", "Household");
        //gameObjectCategoryMap.Add("tbd", "Income & credits"); // out of scope since it is not an expense, but an income
        gameObjectCategoryMap.Add("Football", "Leisure time, sport & hobby");
        gameObjectCategoryMap.Add("House", "Living & energy");
        gameObjectCategoryMap.Add("Others", "Other expenses");
        gameObjectCategoryMap.Add("Person", "Personal expenditure");
        gameObjectCategoryMap.Add("Taxes", "Taxes & duties");
        gameObjectCategoryMap.Add("Car", "Traffic, car & transport");
        gameObjectCategoryMap.Add("Airplane", "Vacation & travel");
        gameObjectCategoryMap.Add("Cash", "Withdrawals");
        //gameObjectCategoryMap.Add("PiggyBank", "tbd");

        // Prepare CSV path
        string csvFolderPath = Directory.GetCurrentDirectory() + CSV_REL_PATH;
        string csvFilePath = csvFolderPath + CSV_FILE_NAME;

        // If CSV exist, load it into a DataTable/DataView
        if (File.Exists(csvFilePath)) {
            dataTable = ConvertCSVtoDataTable(csvFilePath);
            dataView = new DataView(dataTable);
            Debug.Log(dataView.Count + " entries loaded to DataView!");
        }
        else {
            Debug.LogError(CSV_FILE_NAME + " could not be found!");
        }

        // prepare labels for array
        for (int i = 0; i < monthOverviewLabels.Length; i++) {
            monthOverviewLabels[i] = (i + 1).ToString();
        }
    }


    /// <summary>
    /// 
    /// </summary>
    /// <param name="gameObjectName"></param>
    public void addGameObjectToCategoryFilter(string gameObjectName) {
        string categoryName = gameObjectCategoryMap[gameObjectName];

        if (categoryName.Trim() != string.Empty) {
            activeCategories.Add(categoryName);
            Debug.Log("Category [" + categoryName + "] added to Filter!");
            Debug.Log(activeCategories.Count + " entries");
            // since there was an update, refresh the DataView
            updateDataView(9);
            // and update the graphs
            updateBarCharts(9);
        }
    }


    /// <summary>
    /// 
    /// </summary>
    /// <param name="gameObjectName"></param>
    public void removeGameObjectFromCategoryFilter(string gameObjectName) {
        string categoryName = gameObjectCategoryMap[gameObjectName];

        if (categoryName.Trim() != string.Empty) {
            activeCategories.Remove(categoryName);
            Debug.Log("Category [" + categoryName + "] removed from Filter!");
            Debug.Log(activeCategories.Count + " entries");
            // since there was an update, refresh the DataView
            updateDataView(9);
            // and update the graphs
            updateBarCharts(9);
        }
    }


    /// <summary>
    /// 
    /// </summary>
    private void updateDataView(int selectedMonth = 0) {
        string query = "";
        string filter = "";

        // prepare the category filter
        foreach (string hashVal in activeCategories) {
            query += "'" + hashVal + "',";
        }

        // if there is at least one entry (incl. a comma), remove the last character (i.e. the comma) again
        if (query.Length > 0) { 
            query = query.Remove(query.Length - 1);
            filter = "[Main category] IN(" + query + ")";
        } else {
            // when nothing is selected, nothing is shown
            filter = "[Main category] = ''";
        }

        dataView.RowFilter = filter;

        Debug.Log("Filtered entries: " + dataView.Count);

        if (selectedMonth > 0) {
            monthOverviewValues = new float[DateTime.DaysInMonth(CURRENT_YEAR, selectedMonth)];
        }

        // now create the data-list for the chart
        for (int currMonth = 1; currMonth < 13; currMonth++) {
            int daysInMonth = DateTime.DaysInMonth(CURRENT_YEAR, currMonth);

            // used for the month total
            float totalMonthAmount = 0f;

            for (int currDay = 1; currDay < (daysInMonth + 1); currDay++) {
                // prepare query for single day
                //query = "[Date] >= #" + new DateTime(CURRENT_YEAR, currMonth, 1).ToString("MM/dd/yyyy") + "# AND [Date] <= #" + new DateTime(CURRENT_YEAR, currMonth, DateTime.DaysInMonth(CURRENT_YEAR, currMonth)).ToString("MM/dd/yyyy") + "#";
                query = "[Date] = #" + new DateTime(CURRENT_YEAR, currMonth, currDay).ToString("MM/dd/yyyy") + "#";
                // apply it together with the categories filter
                dataView.RowFilter = filter + " AND " + query;

                if (dataView.Count > 0) {
                    // go through every single transaction of a day
                    foreach (DataRowView rowView in dataView) {
                        DataRow row = rowView.Row;
                        totalMonthAmount += Single.Parse(row["Amount"].ToString());

                        if (selectedMonth > 0 && selectedMonth == currMonth) {
                            // set current total to the day value
                            monthOverviewValues[currDay - 1] = totalMonthAmount;
                        }
                    }
                } else if (selectedMonth > 0 && selectedMonth == currMonth) {
                    // no entries for that date, set the total month amount for current day (if a month is provided)
                    monthOverviewValues[currDay - 1] = totalMonthAmount;
                }
            }

            yearOverviewValues[currMonth - 1] = totalMonthAmount;
        }
    }

    private void updateBarCharts(int selectedMonth = 0) {
        // Get all Bar Charts
        BarChart[] barCharts = GameObject.FindObjectsOfType<BarChart>();

        for (int i = 0; i < barCharts.Length; i++) {
            // Look for the Year Overview Chart
            if (barCharts[i].name == CHART_NAME_YEAR_OVERVIEW && yearOverviewValues.Length > 0) {
                Debug.Log("chart [" + CHART_NAME_YEAR_OVERVIEW + "] found");

                // update this chart with its corresponding data
                barCharts[i].DisplayGraph(yearOverviewLabels, yearOverviewValues, CURRENT_YEAR.ToString());

                // continue with next chart
                continue;
            } else if (barCharts[i].name == CHART_NAME_MONTH_OVERVIEW && monthOverviewValues.Length > 0) {
                // Look for the Year Overview Chart
                if (barCharts[i].name == CHART_NAME_MONTH_OVERVIEW) {
                    Debug.Log("chart [" + CHART_NAME_MONTH_OVERVIEW + "] found");

                    // update this chart with its corresponding data
                    barCharts[i].DisplayGraph(monthOverviewLabels, monthOverviewValues, yearOverviewLabels[selectedMonth - 1]);

                    // continue with next chart
                    continue;
                }
            }

            Debug.LogError("No charts found to be updated!");
        }
    }


    /// <summary>
    /// Update is called once per frame
    /// </summary>
    void Update() {
    }


    /// <summary>
    /// Based on: https://immortalcoder.blogspot.ch/2013/12/convert-csv-file-to-datatable-in-c.html
    /// </summary>
    /// <param name="strFilePath"></param>
    /// <returns></returns>
    private static DataTable ConvertCSVtoDataTable(string strFilePath) {
        StreamReader sr = new StreamReader(strFilePath);
        string[] headers = sr.ReadLine().Split(';');
        DataTable dt = new DataTable();
        int colCounter = 0;

        foreach (string header in headers) {
            dt.Columns.Add(header);

            // format forst column as DateTime
            if (colCounter == 0) {
                dt.Columns[colCounter].DataType = System.Type.GetType("System.DateTime");
            }
            // format sicth column as Single (float)
            else if (colCounter == 5) {
                dt.Columns[colCounter].DataType = System.Type.GetType("System.Single");
            }
            colCounter++;
        }

        while (!sr.EndOfStream) {
            string[] rows = Regex.Split(sr.ReadLine(), ";(?=(?:[^\"]*\"[^\"]*\")*[^\"]*$)");
            DataRow dr = dt.NewRow();
            for (int i = 0; i < headers.Length; i++) {
                if (i == 0) {
                    dr[i] = DateTime.ParseExact(rows[i], "dd.MM.yyyy", null);
                }
                else {
                    dr[i] = rows[i];
                }
            }
            dt.Rows.Add(dr);
        }

        return dt;
    }
}
