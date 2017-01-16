using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Table : MonoBehaviour {

    [Header("[Row]", order = 0)]
    public Row rowHolderPrefab;

    [Header("[Text]", order = 1)]
    public Text chartTitle;

    List<Row> rowHolders = new List<Row>();


}
