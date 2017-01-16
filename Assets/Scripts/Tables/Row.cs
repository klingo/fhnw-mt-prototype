using UnityEngine;
using UnityEngine.UI;

public class Row : MonoBehaviour {

    [Header("[Image]", order = 0)]
    public Image rowImage;

    [Header("[Text]", order = 1)]
    public Text dateText;
    public Text recipientText;
    public Text currencyText;
    public Text amountText;
    public Text categoryText;
}
