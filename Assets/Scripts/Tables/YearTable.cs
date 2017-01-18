using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class YearTable : MonoBehaviour {

    [Header("[Year]", order = 0)]
    public Year yearHolderPrefab;


    List<Year> yearHolders = new List<Year>();
}
