using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Data;
using UnityEngine;
using System.Text.RegularExpressions;
using System;
using System.Threading;

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
    private DataViewManager dvManager;

    // array values for charts
    float[] yearOverviewValues = new float[12];
    float[] monthOverviewValues = new float[31];

    // array labels for charts
    string[] yearOverviewLabels = new string[12];
    string[] monthOverviewLabels = new string[31];

    // selected values
    int selectedYear = 2016;
    int selectedMonth = 9;
    int selectedDay = 15;

    // Get the first and last Date from the (sorted) DataTable.
    private DateTime firstDate;
    private DateTime lastDate;

    // Multithreading
    bool _threadRunning;
    Thread _thread;

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
        InitGameObjectCategoryMap();

        // prepare labels for array
        for (int i = 0; i < monthOverviewLabels.Length; i++) {
            monthOverviewLabels[i] = (i + 1).ToString();
        }

        // Prepare CSV path
        string csvFolderPath = Directory.GetCurrentDirectory() + CSV_REL_PATH;
        string csvFilePath = csvFolderPath + CSV_FILE_NAME;

        // If CSV exist, load it into a DataTable/DataView
        if (File.Exists(csvFilePath)) {
            dvManager = DataViewManager.CreateInstance<DataViewManager>();

            dataTable = ConvertCSVtoDataTable(csvFilePath);
            dataView = new DataView(dataTable);
            // sort the DataView by Date
            dataView.Sort = "[Date] ASC";
            // save it back to the DataTable
            dataTable = dataView.ToTable();
            Debug.Log(dataView.Count + " entries loaded and sorted to DataView!");

            // Get the first and last Date from the (sorted) DataTable.
            firstDate = (DateTime)dataTable.Rows[0]["Date"];
            lastDate = (DateTime)dataTable.Rows[dataTable.Rows.Count - 1]["Date"];
            // Create all DataViews in the DataViewManager
            for (int currYear = firstDate.Year; currYear <= lastDate.Year; currYear++) {
                int endMonth = 12;
                if (currYear == lastDate.Year) {
                    // only if the current year is the same as the last year, loop until that dates month, otherwise to 12
                    endMonth = lastDate.Month;
                }
                for (int currMonth = firstDate.Month; currMonth <= endMonth; currMonth++) {
                    dvManager.StoreFilteredDataTable(dataTable, currYear, currMonth);
                }
                // Also store a table for the year
                dvManager.StoreFilteredDataTable(dataTable, currYear);
            }
        }
        else {
            Debug.LogError(CSV_FILE_NAME + " could not be found!");
        }
    }


    /// <summary>
    /// Adds a category to the categories filter for the graphs
    /// </summary>
    /// <param name="gameObjectName"></param>
    public void addGameObjectToCategoryFilter(string gameObjectName) {
        string categoryName = GetCategoryNameFromGameObjectName(gameObjectName);

        if (categoryName.Trim() != string.Empty) {
            activeCategories.Add(categoryName);
            Debug.Log("Category [" + categoryName + "] added to filter!");
            Debug.Log(activeCategories.Count + " entries");

            // Begin our heavy work in a coroutine.
            StartCoroutine(YieldingWork());
        }
    }


    /// <summary>
    /// Removes a category from the categories filter for the graphs
    /// </summary>
    /// <param name="gameObjectName"></param>
    public void removeGameObjectFromCategoryFilter(string gameObjectName) {
        string categoryName = GetCategoryNameFromGameObjectName(gameObjectName);

        if (categoryName.Trim() != string.Empty) {
            activeCategories.Remove(categoryName);
            Debug.Log("Category [" + categoryName + "] removed from filter!");
            Debug.Log(activeCategories.Count + " entries");

            // Begin our heavy work in a coroutine.
            StartCoroutine(YieldingWork());
        }
    }

    IEnumerator YieldingWork() {
        bool workDone = false;

        // Begin our heavy work on a new thread.
        _thread = new Thread(ThreadedWork);
        _thread.Start();

        while (!workDone) {
            // Let the engine run for a frame.
            yield return null;

            // Do Work...
            if (_threadRunning == false) {
                // Only update the bar charts (in the main thread), when the update-thread is completed
                updateBarCharts();
                workDone = true;
            }
        }
    }

    void ThreadedWork() {
        _threadRunning = true;
        bool workDone = false;

        Debug.Log("START THREAD");
        var watch = System.Diagnostics.Stopwatch.StartNew();

        // This pattern lets us interrupt the work at a safe point if neeeded.
        while (_threadRunning && !workDone) {
            // since there was an update, refresh the DataView
            updateArrayLists();
            //updateBarCharts();

            _threadRunning = false;
        }

        watch.Stop();
        var elapsedMs = watch.ElapsedMilliseconds;
        Debug.Log("END THREAD after " + (elapsedMs / 1000) + " s");
    }


    void OnDisable() {
        // If the thread is still running, we should shut it down,
        // otherwise it can prevent the game from exiting correctly.
        if (_threadRunning) {
            // This forces the while loop in the ThreadedWork function to abort.
            _threadRunning = false;

            // This waits until the thread exits,
            // ensuring any cleanup we do after this is safe. 
            _thread.Join();
        }

        // Thread is guaranteed no longer running. Do other cleanup tasks.
    }


    private string GetCategoryNameFromGameObjectName(string gameObjectName) {
        return gameObjectCategoryMap[gameObjectName];
    }


    private void updateArrayLists() {
        KeyValuePair<string[], float[]> kvp;
        
        // First get the YEAR
        kvp = dvManager.GetBarChartValuesAndLabels(selectedYear);
        yearOverviewLabels = kvp.Key;
        yearOverviewValues = kvp.Value;

        // Then get the MONTH
        kvp = dvManager.GetBarChartValuesAndLabels(selectedYear, selectedMonth);
        monthOverviewLabels = kvp.Key;
        monthOverviewValues = kvp.Value;
    }


    private void updateBarCharts() {
        // Get all Bar Charts
        BarChart[] barCharts = GameObject.FindObjectsOfType<BarChart>();

        for (int i = 0; i < barCharts.Length; i++) {
            // Look for the Year Overview Chart
            if (barCharts[i].name == CHART_NAME_YEAR_OVERVIEW && yearOverviewValues.Length > 0) {
                Debug.Log("chart [" + CHART_NAME_YEAR_OVERVIEW + "] found");

                // update this chart with its corresponding data
                barCharts[i].DisplayGraph(yearOverviewLabels, yearOverviewValues, selectedYear.ToString());

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

    public void updateSelection(string selectedLabelText) {
        if (!String.IsNullOrEmpty(selectedLabelText)) {
            int number;
            bool isNumeric = int.TryParse(selectedLabelText, out number);
            if (isNumeric) {
                // A day was selected, since the label is numeric
                selectedDay = number;
            } else {
                // As the label is not numeric, it must have been a month
                selectedMonth = dvManager.GetMonthNoFromString(selectedLabelText);
            }
            // Finally, update the charts
            StartCoroutine(YieldingWork());

        } else {
            Debug.LogError("Invalid Label selected!");
        }
    }


    public string GetActiveCategoriesBinaryString() {
        string binaryState = String.Empty;
        foreach (string gameObjectName in gameObjectCategoryMap.Values) {
            if (activeCategories.Contains(gameObjectName)) {
                binaryState += "1";
            } else {
                binaryState += "0";
            }
        }
        return binaryState;
    }


    private void InitGameObjectCategoryMap() {
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
