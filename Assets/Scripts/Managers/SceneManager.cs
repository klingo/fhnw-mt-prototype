using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Data;
using UnityEngine;
using System.Text.RegularExpressions;
using System;
using System.Threading;
using UnityEngine.UI;

public class SceneManager : Singleton<SceneManager> {

    // Singleton
    protected SceneManager() { }

    //--------------------------------------------------------------------------

    // Constants for CSV File location
    private const string CSV_REL_PATH = "\\Assets\\Resources\\CSV\\";
    private const string CSV_FILE_NAME = "Expenses_2016_anonymized.csv";

    // Constants for names of the different charts
    private const string CHART_NAME_YEAR_OVERVIEW = "BarChart-YearOverview";
    private const string CHART_NAME_MONTH_OVERVIEW = "BarChart-MonthOverview";

    //--------------------------------------------------------------------------

    // A mapping of the GameObject name of a category, and itsuser-friendly name
    private Dictionary<string, string> gameObjectCategoryMap;

    //--------------------------------------------------------------------------

    // A hashset that contains all active categories
    public HashSet<string> activeCategories { get; private set; }

    //--------------------------------------------------------------------------

    // Data containers
    private DataTable dataTable;
    private DataView dataView;
    private DataViewManager dvManager;
    private Detail detailPanel;

    // The first and last Date from the (sorted) DataTable.
    private DateTime firstDate;
    private DateTime lastDate;

    //--------------------------------------------------------------------------

    // array values for charts
    float[] yearOverviewValues = new float[12];
    float[] monthOverviewValues = new float[31];

    // array labels for charts
    string[] yearOverviewLabels = new string[12];
    string[] monthOverviewLabels = new string[31];

    // list with financial transactions for table
    List<string[]> tableOverviewValues = new List<string[]>();

    //--------------------------------------------------------------------------

    // selected values
    int selectedYear = 2016;
    int selectedMonth;
    int selectedDay;

    //--------------------------------------------------------------------------

    public Image selectedTableRowImage;

    //--------------------------------------------------------------------------

    // Blockers for Category processing
    private bool isCategoryBeingProcessed = false;

    //--------------------------------------------------------------------------

    // Multithreading
    bool _threadRunning;
    Thread _thread;

    //==========================================================================


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

        // Get the first (and only) Detail View, and store it (so it can be hidden)
        detailPanel = GameObject.FindObjectOfType<Detail>();
        detailPanel.gameObject.SetActive(false);

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


    public void addGameObjectToCategoryFilter(GameObject gameObject) {
        // only proceed if no other processing is ongoing
        if (!isCategoryBeingProcessed) {
            // ENABLE the blocker
            isCategoryBeingProcessed = true;

            string gameObjectName = gameObject.name;
            string categoryName = GetCategoryNameFromGameObjectName(gameObjectName);

            // Only proceed if no other processing is currently running
            if (categoryName.Trim() != string.Empty) {
                // Set color to loading color
                CategoryIconClick clickerClass = gameObject.GetComponent<CategoryIconClick>();
                gameObject.GetComponent<Renderer>().material.color = clickerClass.loadingColor;

                activeCategories.Add(categoryName);
                //Debug.Log("Category [" + categoryName + "] added to filter!");
                //Debug.Log(activeCategories.Count + " entries");

                // Begin our heavy work in a coroutine.
                StartCoroutine(YieldingWork(clickerClass));
            }
        }
    }

    public void removeGameObjectFromCategoryFilter(GameObject gameObject) {
        // only proceed if no other processing is ongoing
        if (!isCategoryBeingProcessed) {
            // ENABLE the blocker
            isCategoryBeingProcessed = true;

            string gameObjectName = gameObject.name;
            string categoryName = GetCategoryNameFromGameObjectName(gameObjectName);

            // Only proceed if no other processing is currently running
            if (categoryName.Trim() != string.Empty) {
                // Set color to loading color
                CategoryIconClick clickerClass = gameObject.GetComponent<CategoryIconClick>();
                gameObject.GetComponent<Renderer>().material.color = clickerClass.loadingColor;

                activeCategories.Remove(categoryName);
                //Debug.Log("Category [" + categoryName + "] removed from filter!");
                //Debug.Log(activeCategories.Count + " entries");

                // Begin our heavy work in a coroutine.
                StartCoroutine(YieldingWork(clickerClass));
            }
        }
    }


    IEnumerator YieldingWork(CategoryIconClick iconClickerClass = null) {
        bool workDone = false;

        // Begin our heavy work on a new thread.
        _thread = new Thread(ThreadedWork);
        _thread.Start();

        while (!workDone) {
            // Let the engine run for a frame.
            yield return null;

            // Do Work...
            if (_threadRunning == false) {
                // Only update the bar charts and table (in the main thread), when the update-thread is completed
                updateBarCharts();
                updateTable();

                if (iconClickerClass != null) {
                    iconClickerClass.SetFinalColor();

                    // RELEASE the blocker
                    isCategoryBeingProcessed = false;
                }
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
        bool validMonth = (selectedMonth > 0 && selectedMonth <= 12);
        bool validDay = (selectedDay > 0 && selectedDay <= DateTime.DaysInMonth(selectedYear, selectedMonth));

        KeyValuePair<string[], float[]> kvp;
        
        // First get the YEAR
        kvp = dvManager.GetBarChartValuesAndLabels(selectedYear);
        yearOverviewLabels = kvp.Key;
        yearOverviewValues = kvp.Value;

        // Then get the MONTH
        if (validMonth) {
            kvp = dvManager.GetBarChartValuesAndLabels(selectedYear, selectedMonth);
            monthOverviewLabels = kvp.Key;
            monthOverviewValues = kvp.Value;

            if (!validDay) {
                // No valid day selected
                // In this case, also update the tableOverview Values
                tableOverviewValues = dvManager.GetTableRows(selectedYear, selectedMonth);
            }
        } else {
            monthOverviewLabels = new string[] { };
            monthOverviewValues = new float[] { };
        }

        // Finally check the DAY
        if (validDay) {
            // valid day selected, update the tableOverview Values
            tableOverviewValues = dvManager.GetTableRows(selectedYear, selectedMonth, selectedDay);
        }

        Debug.Log("complete");
    }


    private void updateBarCharts() {
        // Get all Bar Charts
        BarChart[] barCharts = GameObject.FindObjectsOfType<BarChart>();

        for (int i = 0; i < barCharts.Length; i++) {
            // Look for the Year Overview Chart
            if (barCharts[i].name == CHART_NAME_YEAR_OVERVIEW && yearOverviewValues.Length > 0) {
                //Debug.Log("chart [" + CHART_NAME_YEAR_OVERVIEW + "] found");

                // update this chart with its corresponding data
                barCharts[i].DisplayGraph(yearOverviewLabels, yearOverviewValues, selectedYear.ToString());

                // continue with next chart
                continue;
            } else if (barCharts[i].name == CHART_NAME_MONTH_OVERVIEW && monthOverviewValues.Length > 0) {
                // Look for the Year Overview Chart
                if (barCharts[i].name == CHART_NAME_MONTH_OVERVIEW) {
                    //Debug.Log("chart [" + CHART_NAME_MONTH_OVERVIEW + "] found");

                    // update this chart with its corresponding data
                    barCharts[i].DisplayGraph(monthOverviewLabels, monthOverviewValues, yearOverviewLabels[selectedMonth - 1] + " " + selectedYear.ToString());

                    // continue with next chart
                    continue;
                }
            } else if (selectedMonth == 0) {
                // No month was selected, which is also a valid case

                // continue with next chart
                continue;
            } else {
                Debug.LogError("No charts found to be updated!");
            }
        }
    }


    private void updateTable() {
        // Get the first (and only) Table
        Table table = GameObject.FindObjectOfType<Table>();

        if (table != null) {
            // if value list is not empty
            bool validDay = (selectedDay > 0 && selectedDay <= DateTime.DaysInMonth(selectedYear, selectedMonth));
            bool validMonth = (selectedMonth > 0 && selectedMonth <= 12);

            // at least a valid month must be provided for the Table to be updated
            if (validMonth) {
                string tableTitle = yearOverviewLabels[selectedMonth - 1] + " ";    // e.g. [January ]
                if (validDay) {
                    tableTitle += selectedDay.ToString() + ", ";                    // e.g. [January 10, ]
                }
                tableTitle += selectedYear.ToString();                              // e.g. [January 10, 2016] or [January 2016]

                table.DisplayTable(tableOverviewValues, tableTitle);

                // if there is exactly one result, also show the detail view
                Row firstRow = table.GetFirstRowIfOnlyOneExists();
                if (firstRow != null) {
                    updateDetailView(firstRow);
                }
            }
        } else {
            Debug.LogError("No table found to be updated!");
        }

    }

    public void updateDetailView(Row rowHolder) {

        // Check the Detail View Panel
        if (detailPanel != null) {
            // if selected transaction is not empty
            if (rowHolder != null) {
                detailPanel.gameObject.SetActive(true);

                // Date
                detailPanel.dateText.text = rowHolder.dateText.text;
                // Recipient / Order issue
                detailPanel.recipientText.text = rowHolder.recipientText.text;
                // Account no. / Account name
                detailPanel.accountNoNameText.text = rowHolder.accountNo;
                string accountName = rowHolder.accountName;
                if (!String.IsNullOrEmpty(accountName)) {
                    detailPanel.accountNoNameText.text += " / " + accountName;
                }
                // Currency / Amount
                detailPanel.currencyAmountText.text = rowHolder.currencyText.text + " " + rowHolder.amountText.text;
                // Booking text
                detailPanel.bookingTextText.text = rowHolder.bookingText;
                // Main category
                detailPanel.mainCategoryText.text = rowHolder.categoryText.text;
                // Subcategory
                detailPanel.subcategoryText.text = rowHolder.subcategory;

            } else {
                detailPanel.gameObject.SetActive(false);
            }
        }
        else {
            Debug.LogError("No detail found to be updated!");
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
                // when the selected month changes, reset the selected day!
                selectedDay = 0;
            }

            // Since there was a change of month/day, the selected transaction has to be re-set!
            updateDetailView(null);

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
