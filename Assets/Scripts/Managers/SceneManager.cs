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
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Text.RegularExpressions;
using System.Threading;
using UnityEngine;
using UnityEngine.UI;
using VRTK;

public class SceneManager : Singleton<SceneManager> {

    // Singleton
    protected SceneManager() { }

    //==========================================================================

    // Constants for names of the different charts
    public const string CHART_NAME_YEAR_OVERVIEW = "BarChart-YearOverview";
    public const string CHART_NAME_MONTH_OVERVIEW = "BarChart-MonthOverview";

    // Time in seconds after which the controller tooltips shall disapper
    private const float CONTROLLER_TOOLTIP_TIMEOUT = 15f;

    // Whether certain debug statements should be printed or not (0 = none; 1 = some, 2 = all)
    public const int DEBUG_LEVEL = 0;

    //==========================================================================

    // A mapping of the GameObject name of a category, and itsuser-friendly name
    public Dictionary<string, string> gameObjectCategoryMap { get; private set; }

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

    // The highlight Colours for the table rows
    public Color highlightRowColor = new Color(0.9f, 0.9f, 1);
    public Color selectedRowColor = new Color(0.7f, 0.7f, 1);

    //--------------------------------------------------------------------------

    // array values for charts
    float[] yearOverviewValues = new float[12];
    float[] monthOverviewValues = new float[31];

    // array labels for charts
    string[] yearOverviewLabels = new string[12];
    string[] monthOverviewLabels = new string[31];

    // max and current threshold level for the bar charts
    float globalMaxThreshold = 0f;
    float globalThreshold = 0f;

    // list with financial transactions for table
    List<string[]> tableOverviewValues = new List<string[]>();

    //--------------------------------------------------------------------------

    // selected values
    int selectedYear;
    int selectedMonth;
    int selectedDay;

    //--------------------------------------------------------------------------

    // Image reference of the currently selected table rows and chart bars
    public Image selectedTableRowImage;
    public Image selectedYearRowImage;
    public Bar selectedMonthBar;
    public Bar selectedDayBar;

    //--------------------------------------------------------------------------

    // Blockers for Category processing
    public bool isCategoryBeingProcessed = false;
    CategoryIconClick[] categoryIconClickers;

    //--------------------------------------------------------------------------

    // Multithreading information
    bool _threadRunning;
    Thread _thread;

    //==========================================================================


    /// <summary>
    /// Awake is always called before any Start functions
    /// Makes sure the Singleton instance is created
    /// </summary>
    void Awake() {
        // Debug to inform that Singleton was created!
        Logger.Log(0, "Awoke Singleton Instance: " + gameObject.GetInstanceID());
    }


    /// <summary>
    /// Start is directly called after the Awake function
    /// Makes sure all initialisation tasks are completed and reads in the (first) CSF file from
    /// the projects root folder, mapps it into DataTables and stores the in the DataViewManager class.
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
        string rootDir = Directory.GetCurrentDirectory();
        string csvFilePath = "N/A";
        // get all CSV files under that path
        string[] files = Directory.GetFiles(rootDir, "*.csv");

        if (files.Length == 0) {
            // no files found, log error
            Logger.LogError("No CSV file could be found! Please make sure you have one available in the projects root folder: [" + rootDir + "\\]");
        } else {
            if (files.Length > 1) {
                // more than one file found, log warning and use first file
                Logger.LogWarning("More than one CSV file found under [" + rootDir + "]. Using the first one.");
            }
            csvFilePath = files[0];

            // If CSV exist, load it into a DataTable/DataView
            if (File.Exists(csvFilePath)) {
                Logger.Log(0, "Reading CSV file from: " + csvFilePath);

                dvManager = DataViewManager.CreateInstance<DataViewManager>();

                dataTable = ConvertCSVtoDataTable(csvFilePath);
                dataView = new DataView(dataTable);
                // sort the DataView by Date
                dataView.Sort = "[Date] ASC";
                // save it back to the DataTable
                dataTable = dataView.ToTable();
                Logger.Log(1, dataView.Count + " entries loaded and sorted to DataView!");

                // Get the first and last Date from the (sorted) DataTable.
                firstDate = (DateTime)dataTable.Rows[0]["Date"];
                lastDate = (DateTime)dataTable.Rows[dataTable.Rows.Count - 1]["Date"];
                // Create all DataViews in the DataViewManager
                for (int currYear = lastDate.Year; currYear >= firstDate.Year; currYear--) {
                    int startMonth = 1;
                    int endMonth = 12;
                    if (currYear == lastDate.Year) {
                        // only if the current year is the same as the last year, loop until that dates month, otherwise to 12
                        endMonth = lastDate.Month;
                    }
                    if (currYear == firstDate.Year) {
                        // only if the current year is th same as the first year, loop from that month onwards, otherwis from 1
                        startMonth = firstDate.Month;
                    }
                    for (int currMonth = startMonth; currMonth <= endMonth; currMonth++) {
                        dvManager.StoreFilteredDataTable(dataTable, currYear, currMonth);
                    }
                    // Also store a table for the year
                    dvManager.StoreFilteredDataTable(dataTable, currYear);

                    // Add the current year to the year selection table
                    YearTable yearTable = GameObject.FindObjectOfType<YearTable>();
                    yearTable.AddYearToTable(currYear);
                }

                // Finally, initialise the application with the latest year, as the selected one.
                selectedYear = lastDate.Year;
            } else {
                Logger.LogError("Could not read CSV file: [" + csvFilePath + "]");
            }
        }

        // Hides the controller tooltips after a given time
        StartCoroutine(HideTooltipsAfterTime(CONTROLLER_TOOLTIP_TIMEOUT));
    }


    /// <summary>
    /// Puts the provided gameObject (Category) to the filter list and updates the global threshold level.
    /// </summary>
    /// <param name="gameObject">The Category Icon Object</param>
    /// <param name="monthlyCategoryThreshold">The corresponding threshold of this object</param>
    public void addGameObjectToCategoryFilter(GameObject gameObject, float monthlyCategoryThreshold) {
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

                // Update the global threshold
                globalThreshold += monthlyCategoryThreshold;

                // Begin our heavy work in a coroutine.
                StartCoroutine(YieldingWork(clickerClass));
            }
        }
    }


    /// <summary>
    /// Removes the provided gameObject (Category) from the filter list and updates the global threshold level.
    /// </summary>
    /// <param name="gameObject">The Category Icon Object</param>
    /// <param name="monthlyCategoryThreshold">The corresponding threshold of this object</param>
    public void removeGameObjectFromCategoryFilter(GameObject gameObject, float monthlyCategoryThreshold) {
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

                // Update the global threshold
                globalThreshold -= monthlyCategoryThreshold;

                // Begin our heavy work in a coroutine.
                StartCoroutine(YieldingWork(clickerClass));
            }
        }
    }


    /// <summary>
    /// Removes all gameObjects (Categories) from the filter and sets the global threshold level to zero.
    /// </summary>
    public void removeAllGameObjectsFromCategoryFilter() {
        // only proceed if no other processing is ongoing
        if (!isCategoryBeingProcessed) {
            // ENABLE the blocker
            isCategoryBeingProcessed = true;
            // Check if we already have a list of all clickers
            if (categoryIconClickers == null || categoryIconClickers.Length == 0) {
                // If not, populate them
                categoryIconClickers = GameObject.FindObjectsOfType<CategoryIconClick>();
            }

            foreach (CategoryIconClick clicker in categoryIconClickers) {
                // Get the category name from the GameObject name
                string categoryName = GetCategoryNameFromGameObjectName(clicker.name);

                // Only for the active ones, change the state
                if (activeCategories.Contains(categoryName)) {
                    // toggles the [isActived] flag, and sets [isLoading] to [true]
                    clicker.SetToLoadingState();

                    // Changes the Color to the loadingColor
                    clicker.GetComponentInParent<Renderer>().material.color = clicker.loadingColor;
                }
            }

            // clear all active Categories
            activeCategories.Clear();

            // Update the global threshold
            globalThreshold = 0f;

            // when we remove all categories, we also have to re-set the day/month selection!
            selectedMonth = 0;
            selectedDay = 0;

            // Begin our heavy work in a coroutine.
            StartCoroutine(YieldingWork(categoryIconClickers));
        }
    }


    /// <summary>
    /// Adds all not yet added gameObjects (Categories) to the filter and sets the globla threshold level
    /// to the maximum.
    /// </summary>
    public void addAllGameObjectsToCategoryFilter() {
        // only proceed if no other processing is ongoing
        if (!isCategoryBeingProcessed) {
            // ENABLE the blocker
            isCategoryBeingProcessed = true;
            // Check if we already have a list of all clickers
            if (categoryIconClickers == null || categoryIconClickers.Length == 0) {
                // If not, populate them
                categoryIconClickers = GameObject.FindObjectsOfType<CategoryIconClick>();
            }

            foreach (CategoryIconClick clicker in categoryIconClickers) {
                // Get the category name from the GameObject name
                string categoryName = GetCategoryNameFromGameObjectName(clicker.name);

                // toggles the [isActived] flag, and sets [isLoading] to [true]
                clicker.SetToLoadingState();

                // Changes the Color to the loadingColor
                clicker.GetComponentInParent<Renderer>().material.color = clicker.loadingColor;

                // Add he individual category to the list of active ones
                activeCategories.Add(categoryName);

                // Update the global threshold
                globalThreshold = globalMaxThreshold;
            }

            // Begin our heavy work in a coroutine.
            StartCoroutine(YieldingWork(categoryIconClickers));
        }
    }


    /// <summary>
    /// The wrapper class for the CoRoutine work that has to be run in parallel to keeping the game
    /// up to date. This method allows to provide a single iconClickerClass (CategoryIconClick.cs) instead
    /// of an array of them.
    /// </summary>
    /// <param name="iconClickerClass">Optional paramters where a specific icon (category) can be provided to be updated</param>
    /// <returns></returns>
    IEnumerator YieldingWork(CategoryIconClick iconClickerClass = null) {
        CategoryIconClick[] clickerAry = new CategoryIconClick[1];
        clickerAry[0] = iconClickerClass;
        return YieldingWork(clickerAry);
    }


    /// <summary>
    /// The main CoRoutine method where a second Thread is started to to the heavy workload on filtering the
    /// DataViews, preparing the data sets for the charts/tables and stores them in the cache. After the thread has
    /// ended, the control goes back to the CoRoutine which then sets the final colour on the Categories to green
    /// and unblocks the whole processing. The CoRoutine is required as the MainThread cannot wait for the processing
    /// to be over but still needs to allow the user to look and move around in the virtual environment.
    /// </summary>
    /// <param name="iconClickerClasses">An array if iconClickerClasses that shall be updated</param>
    /// <returns></returns>
    IEnumerator YieldingWork(CategoryIconClick[] iconClickerClasses) {
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
                // Set the final color!
                if (iconClickerClasses != null) {
                    foreach (CategoryIconClick clicker in iconClickerClasses) {
                        if (clicker != null) {
                            clicker.SetFinalColor();
                        }
                    }
                }

                // RELEASE the blocker
                isCategoryBeingProcessed = false;

                workDone = true;
            }
        } 
    }


    /// <summary>
    /// The actual work that is executed in a separate Thread. It mainly updates all the arrayLists that 
    /// will be cached afterwards. This has to be done in a separate thread as it otherwise would
    /// literally freeze the whole game world for several seconds until the processing has completed.
    /// </summary>
    void ThreadedWork() {
        _threadRunning = true;
        bool workDone = false;

        Logger.Log(2, "START THREAD");
        var watch = System.Diagnostics.Stopwatch.StartNew();

        // This pattern lets us interrupt the work at a safe point if neeeded.
        while (_threadRunning && !workDone) {
            // since there was an update, refresh the DataView
            updateArrayLists();

            _threadRunning = false;
        }

        watch.Stop();
        var elapsedMs = watch.ElapsedMilliseconds;
        Logger.Log(2, "END THREAD after " + elapsedMs + " ms");
    }


    /// <summary>
    /// This method is automatically called when the application is closed. It makes sure that if a 
    /// second Thread was still running, that it gets closed properly.
    /// </summary>
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


    /// <summary>
    /// Returns the Category Name that is used in the CSV based on a provided CategoryIcon name.
    /// </summary>
    /// <param name="gameObjectName"></param>
    /// <returns></returns>
    private string GetCategoryNameFromGameObjectName(string gameObjectName) {
        return gameObjectCategoryMap[gameObjectName];
    }


    /// <summary>
    /// This is the heavy workload task where all the array lists are updated based on the latest filter
    /// settings.
    /// </summary>
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

            if (validDay) {
                // valid day selected, update the tableOverview Values
                tableOverviewValues = dvManager.GetTableRows(selectedYear, selectedMonth, selectedDay);
            }
            else {
                // No valid day selected
                // In this case, also update the tableOverview Values, but without a Day
                tableOverviewValues = dvManager.GetTableRows(selectedYear, selectedMonth);
            }
        }
        else {
            monthOverviewLabels = new string[] { };
            monthOverviewValues = new float[] { };

            if (!validDay) {
                // if also no valid day, then reset the Table
                tableOverviewValues = null;
            }
        }
    }


    /// <summary>
    /// Updates both bar charts with their new title and uses the values and labels available
    /// in the Singleton class that before have been updated by the updateArrayLists() method.
    /// </summary>
    private void updateBarCharts() {
        // Get all Bar Charts
        BarChart[] barCharts = GameObject.FindObjectsOfType<BarChart>();

        string chartTitle = selectedYear.ToString();

        for (int i = 0; i < barCharts.Length; i++) {
            // Look for the Year Overview Chart
            if (barCharts[i].name == CHART_NAME_YEAR_OVERVIEW) {
                Logger.Log(2, "Chart [" + CHART_NAME_YEAR_OVERVIEW + "] found");

                if (yearOverviewValues.Length <= 0) {
                    chartTitle += " (no data found)";
                }

                // update this chart with its corresponding data
                // in this case, [yearOverviewLabels] and [yearOverviewValues] should already be empty because of [GetBarChartValuesAndLabels();]
                barCharts[i].DisplayGraph(yearOverviewLabels, yearOverviewValues, chartTitle, globalThreshold);

                // continue with next chart
                continue;
            }
            // Look for the Month Overview Chart
            else if (barCharts[i].name == CHART_NAME_MONTH_OVERVIEW) {
                Logger.Log(2, "Chart [" + CHART_NAME_MONTH_OVERVIEW + "] found");

                if (monthOverviewValues.Length > 0) {
                    chartTitle = yearOverviewLabels[selectedMonth - 1] + " " + selectedYear.ToString();
                } else {
                    // No month was selected, which is also a valid case
                    chartTitle = "Month Overview (no month selected)";
                }

                // update this chart with its corresponding data
                // in this case, [monthOverviewLabels] and [monthOverviewValues] should already be empty because of [updateArrayLists();]
                barCharts[i].DisplayGraph(monthOverviewLabels, monthOverviewValues, chartTitle, globalThreshold);

                // continue with next chart
                continue;

            } else {
                Logger.LogError("No charts found to be updated!");
            }
        }
    }


    /// <summary>
    /// Updates the table with the values and labels available in the Singleton class that before have 
    /// been updated by the updateArrayLists() method.
    /// </summary>
    private void updateTable() {
        // Get the first (and only) Table
        Table table = GameObject.FindObjectOfType<Table>();

        string tableTitle = "Day Overview";

        if (table != null) {
            // if value list is not empty
            bool validDay = (selectedDay > 0 && selectedDay <= DateTime.DaysInMonth(selectedYear, selectedMonth));
            bool validMonth = (selectedMonth > 0 && selectedMonth <= 12);

            // at least a valid month must be provided for the Table to be updated
            if (validMonth) {
                tableTitle = yearOverviewLabels[selectedMonth - 1] + " ";    // e.g. [January ]
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

            } else {
                // no (valid) month provided
                if (table.HasEntries()) {
                    table.DisplayTable(tableOverviewValues, tableTitle);
                }

                // also update the detail view (i.e. hide it)
                updateDetailView(null);
            }
        } else {
            Logger.LogError("No table found to be updated!");
        }
    }


    /// <summary>
    /// Updates the detail view with the values based on the selected Row in the Table. The provided
    /// Row can also be empty which will hide the detail view.
    /// </summary>
    /// <param name="rowHolder">Optional. If null, then the detail view will be hidden in the world.</param>
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
            Logger.LogError("No detail found to be updated!");
        }
    }


    /// <summary>
    /// A generic method where either a yar, a month name or a day can be handed over. It will then update
    /// the currently selected year/month/day accordingly. This is a core feature to make sure that the
    /// updateArrayLists() works correctly.
    /// </summary>
    /// <param name="selectedLabelText"></param>
    public void updateSelection(string selectedLabelText) {
        if (!String.IsNullOrEmpty(selectedLabelText)) {
            int number;
            bool isNumeric = int.TryParse(selectedLabelText, out number);
            if (isNumeric) {
                // A day or year was selected, since the label is numeric
                if (number > 31) {
                    // since the number is > 31 it must be a year
                    selectedYear = number;
                    selectedMonth = 0;
                    selectedDay = 0;
                } else {
                    selectedDay = number;
                }
            } else {
                // As the label is not numeric, it must have been a month
                selectedMonth = dvManager.GetMonthNoFromString(selectedLabelText);
                // when the selected month changes, reset the selected day!
                selectedDay = 0;
            }

            // Since there was a change of month/day/year, the selected transaction has to be re-set!
            updateDetailView(null);

            // Also make sure to reset the coloured table-selection (if existing)
            if (selectedTableRowImage != null) {
                selectedTableRowImage.CrossFadeColor(Color.white, 0.1f, false, false);
                selectedTableRowImage.color = Color.white;
                selectedTableRowImage = null;
            }

            // Finally, update the charts
            StartCoroutine(YieldingWork());

        } else {
            Logger.LogError("Invalid Label selected!");
        }
    }


    /// <summary>
    /// Returns a binary string from all categories to indicate their current state. A '1' represents that the
    /// category is active whereas a '0' indicates that it is inactive.
    /// </summary>
    /// <returns></returns>
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


    /// <summary>
    /// Initialises the GameObjectCategoryMap which is used as a mapping between the category names from the CSV
    /// and the names of the GameObjects that represent them.
    /// </summary>
    private void InitGameObjectCategoryMap() {
        if (globalMaxThreshold == 0) {
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

            // now loop through all categories, to evaluate the maximum overall threshold (for when all categories are activated
            CategoryIconClick[] clickersAry = GameObject.FindObjectsOfType<CategoryIconClick>();
            foreach (CategoryIconClick clicker in clickersAry) {
                globalMaxThreshold += clicker.monthlyCategoryThreshold;
            }
        } else {
            Logger.LogWarning("GameObject CategoryMap already initialised!");
        }
    }


    /// <summary>
    /// Hides the controller tooltips after a given time
    /// </summary>
    /// <param name="time">Time in seconds until the controller tooltips shall be hidden</param>
    /// <returns></returns>
    IEnumerator HideTooltipsAfterTime(float time) {
        yield return new WaitForSeconds(time);
        VRTK_ControllerTooltips[] tooltips = GameObject.FindObjectsOfType<VRTK_ControllerTooltips>();
        foreach (VRTK_ControllerTooltips tooltip in tooltips) {
            tooltip.ToggleTips(false);
        }
        yield break;
    }


    /// <summary>
    /// Based on: https://immortalcoder.blogspot.ch/2013/12/convert-csv-file-to-datatable-in-c.html
    /// This method reads in a CSV file and creates a DataTable based on it. The first row will be used
    /// for the Header information.
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