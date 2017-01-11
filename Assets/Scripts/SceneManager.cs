using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SceneManager : Singleton<SceneManager> {

    protected SceneManager() { }

    public HashSet<string> activeCategories = new HashSet<string>();

    // Awake is always called before any Start functions
    void Awake() {
        Debug.Log("Awoke Singleton Instance: " + gameObject.GetInstanceID());
    }

    // Update is called once per frame
    void Update() {

    }
}
