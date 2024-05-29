using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }

    public void OnClickOutdoor()
    {
        PlayerPrefs.SetString("memoLatitudeKey", "0.0");
        PlayerPrefs.SetString("memoLongitudeKey", "0.0");
        SceneManager.LoadScene("Geospatial");
    }

    public void OnClickIndoor()
    {
        SceneManager.LoadScene("SampleScene");
    }


    public void OnClickGame()
    {
        Debug.Log("Game Clicked");
    }

    public void OnClickGameNav()
    {
        //PlayerPrefs.SetString("memoLatitudeKey", "0.0");
        //PlayerPrefs.SetString("memoLongitudeKey", "0.0");  //게임장 위치
        PlayerPrefs.SetString("NavigationMode", "gameNav");
        SceneManager.LoadScene("Geospatial");
    }

    public void OnClickObject()
    {
        SceneManager.LoadScene("ObjMemoScene");
    }

    public void OnClickBackBtn()
    {
        SceneManager.LoadScene("MainTitleScene");
    }

}

