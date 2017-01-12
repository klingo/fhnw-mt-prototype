using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Data;
using UnityEngine;
using System.Text.RegularExpressions;

public class SceneManager : Singleton<SceneManager> {

    private const string CSV_REL_PATH = "\\Assets\\Resources\\CSV\\";
    private const string CSV_FILE_NAME = "Expenses_2016_anonymized.csv";

    // Singleton
    protected SceneManager() { }

    // A hashset that contains all active categories
    public HashSet<string> activeCategories = new HashSet<string>();

    // Awake is always called before any Start functions
    void Awake() {
        // Debug to inform that Singleton was created!
        Debug.Log("Awoke Singleton Instance: " + gameObject.GetInstanceID());
    }


    void Start() {
        // Load CSV dataset
        string csvFolderPath = Directory.GetCurrentDirectory() + CSV_REL_PATH;
        string csvFilePath = csvFolderPath + CSV_FILE_NAME;

        if (File.Exists(csvFilePath)) {
            Debug.Log("FILE FOUND! :-)");

            DataTable dataTable = ConvertCSVtoDataTable(csvFilePath);
            DataView dataView = new DataView(dataTable);

            Debug.Log("count 1: " + dataView.Count);

            Debug.Log("test = " + dataTable.Rows[0].Table.Columns[6]);

            dataView.RowFilter = "[Booking text] = 'INTERNET/PHONE'";
            Debug.Log("count 2: " + dataView.Count);
        }
        else {
            Debug.LogError(CSV_FILE_NAME + " could not be found!");
        }



        //DataView dv = new DataView(yourDatatable);
        //dv.RowFilter = "query"; // query example = "id = 10"
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
