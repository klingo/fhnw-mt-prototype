using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Data;
using UnityEngine;
using System.Text.RegularExpressions;

public class SceneManager : Singleton<SceneManager> {

    // Singleton
    protected SceneManager() { }

    // Constants for CSV File location
    private const string CSV_REL_PATH = "\\Assets\\Resources\\CSV\\";
    private const string CSV_FILE_NAME = "Expenses_2016_anonymized.csv";

    private Dictionary<string, string> gameObjectCategoryMap;

    // A hashset that contains all active categories
    public HashSet<string> activeCategories { get; private set; }
    public DataTable dataTable { get; private set; }
    public DataView dataView { get; private set; }

    // Awake is always called before any Start functions
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
        gameObjectCategoryMap.Add("tbd", "Income & credits");       // TODO: Add to Unity!!!
        gameObjectCategoryMap.Add("Football", "Leisure time, sport & hobby");
        gameObjectCategoryMap.Add("House", "Living & energy");
        gameObjectCategoryMap.Add("Others", "Other expenses");
        gameObjectCategoryMap.Add("Person", "Personal expenditure");
        gameObjectCategoryMap.Add("tbd2", "Taxes & duties");         // TODO: Add to Unity!!!
        gameObjectCategoryMap.Add("Car", "Traffic, car & transport");
        gameObjectCategoryMap.Add("Airplane", "Vacation & travel");
        gameObjectCategoryMap.Add("tbd3", "Withdrawals");            // TODO: Add to Unity!!!
        gameObjectCategoryMap.Add("PiggyBank", "tbd");               // TODO: Add to CSV!!!

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
    }


    public void addGameObjectToCategoryFilter(string gameObjectName) {
        string categoryName = gameObjectCategoryMap[gameObjectName];

        if (categoryName.Trim() != string.Empty) {
            activeCategories.Add(categoryName);
            Debug.Log("Category [" + categoryName + "] added to Filter!");
            Debug.Log(activeCategories.Count + " entries");
            // since there was an update, refresh the DataView
            updateDataView();
        }

    }

    public void removeGameObjectFromCategoryFilter(string gameObjectName) {
        string categoryName = gameObjectCategoryMap[gameObjectName];

        if (categoryName.Trim() != string.Empty) {
            activeCategories.Remove(categoryName);
            Debug.Log("Category [" + categoryName + "] removed from Filter!");
            Debug.Log(activeCategories.Count + " entries");
            // since there was an update, refresh the DataView
            updateDataView();

        }
    }

    private void updateDataView() {
        string query = "";

        foreach (string hashVal in activeCategories) {
            query += "'" + hashVal + "',";
        }

        // if there is at least one entry (incl. a comma), remove the last character (i.e. the comma) again
        if (query.Length > 0) { 
            query = query.Remove(query.Length - 1);
            dataView.RowFilter = "[Main category] IN(" + query + ")";
        } else {
            // TODO: when nothing is selected, all entries are shown - change to display nothing?
            dataView.RowFilter = "[Main category] = ''";
            //dataView.RowFilter = "";
        }
        Debug.Log("Filtered entries: " + dataView.Count);
    }


    // Update is called once per frame
    void Update() {
    }


    // Source: https://immortalcoder.blogspot.ch/2013/12/convert-csv-file-to-datatable-in-c.html
    // 
    private static DataTable ConvertCSVtoDataTable(string strFilePath) {
        StreamReader sr = new StreamReader(strFilePath);
        string[] headers = sr.ReadLine().Split(';');
        DataTable dt = new DataTable();
        foreach (string header in headers) {
            dt.Columns.Add(header);
        }
        while (!sr.EndOfStream) {
            string[] rows = Regex.Split(sr.ReadLine(), ";(?=(?:[^\"]*\"[^\"]*\")*[^\"]*$)");
            DataRow dr = dt.NewRow();
            for (int i = 0; i < headers.Length; i++) {
                dr[i] = rows[i];
            }
            dt.Rows.Add(dr);
        }
        return dt;
    }
}
