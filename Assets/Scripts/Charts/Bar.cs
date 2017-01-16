using UnityEngine;
using UnityEngine.UI;

public class Bar : MonoBehaviour {

    [Header("[Image]", order = 0)]
    public Image topBar;
    public Image bottomBar;

    [Header("[Text]", order = 1)]
    public Text label;
    public Text topBarValue;
    public Text bottomBarValue;
}
