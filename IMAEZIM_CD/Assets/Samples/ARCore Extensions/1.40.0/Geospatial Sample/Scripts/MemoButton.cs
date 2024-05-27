using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MemoButton : MonoBehaviour
{
    public void OnClickMemoButton()
    {
        if (PlayerPrefs.GetString("memoLongitudeKey") == "in")
        {
            SceneManager.LoadScene("SampleScene");
        }
        else if (PlayerPrefs.GetString("memoLongitudeKey") == "out")
        {
            SceneManager.LoadScene("Geospatial");
        }
        else { Debug.Log("OnClickMemoButton : 실내 실외 정보 없음"); }
    }
}
