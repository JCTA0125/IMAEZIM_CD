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

    static public string UserId = "1";
    static public string UserEmail = "a@a.com";
    static public string UserNickname = "CodeDuck";
    public void AndUserId(string id)  //안드로이드에서 호출할 함수
    {
        Debug.Log("AndUserId 실행 id = " + id);
        UserId = id;
    }
    public void AndUserInfo(string email)  //안드로이드에서 호출할 함수
    {
        Debug.Log("AndUserInfo 실행 email = " + email);
        UserEmail = email;
    }
    public void AndUserNick(string nickname)  //안드로이드에서 호출할 함수
    {
        Debug.Log("AndUserNick 실행 nickname = " + nickname);
        UserNickname = nickname;
    }
    public void AndLatitude(string latitude)  //일반 길찾기에서 호출
    {
        PlayerPrefs.SetString("NavigationMode", "memoNav");  //길찾기 일반 모드
        Debug.Log("AndLatitude 실행 latitude = " + latitude);
        PlayerPrefs.SetString("memoLatitudeKey", latitude);
    }
    public void AndLongitude(string longitude)  //안드에서 AndLatitude 먼저 호출
    {
        Debug.Log("AndLongitude 실행 longitude = " + longitude);
        PlayerPrefs.SetString("memoLongitudeKey", longitude);
        SceneManager.LoadScene("Geospatial");
    }
    /*
    public void AndMemoNav(string latitude, string longitude)  //안드로이드에서 호출할 함수
    {
        PlayerPrefs.SetString("memoLatitudeKey", latitude);
        PlayerPrefs.SetString("memoLongitudeKey", longitude);
        SceneManager.LoadScene("Geospatial");
    }
    */

    public void OnClickOutdoor()
    {
        PlayerPrefs.SetString("memoLatitudeKey", "0.0");
        PlayerPrefs.SetString("memoLongitudeKey", "0.0");
        SceneManager.LoadScene("Geospatial");
    }

    public void OnClickIndoor()
    {
        SceneManager.LoadScene("IndoorScene");
    }


    public void OnClickGame()
    {
        SceneManager.LoadScene("Scene_Lobby");
        Debug.Log("Game Clicked");
    }

    public void OnClickGameNav()
    {
        PlayerPrefs.SetString("memoLatitudeKey", "37.652005");
        PlayerPrefs.SetString("memoLongitudeKey", "127.016226"); //게임장 위치 
        PlayerPrefs.SetString("NavigationMode", "gameNav");
        SceneManager.LoadScene("Geospatial");
    }
    public void OnClickMemoNav()
    {
        PlayerPrefs.SetString("memoLatitudeKey", "37.652023");
        PlayerPrefs.SetString("memoLongitudeKey", "127.016296"); 
        PlayerPrefs.SetString("NavigationMode", "memoNav");
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

    public void QuizBtnClicked()
    {
        SceneManager.LoadScene("Quiz3DScene");
    }

}

