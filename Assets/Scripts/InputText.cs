using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class InputText : MonoBehaviour
{
    public TMP_InputField inputText;

    public void SaveText()
    {
        PlayerPrefs.SetString("MemoText", inputText.text);
    }

}
