using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;
using UnityEngine.SceneManagement;
using UnityEngine.Networking;
using TMPro;

public class LobbyManager : MonoBehaviourPunCallbacks
{
    [Header("Login UI")]
    public InputField playerNameInputField;
    public GameObject uI_LoginGameobject;

    [Header("Lobby UI")]
    public GameObject uI_LobbyGameobject;
    //public GameObject uI_3DGameObject;

    [Header("Connection Status UI")]
    public GameObject uI_ConnectionStatusGameobject;
    public Text connectionStatusText;
    public bool showConnectionStatus = false;

    [Header("Ranking UI")]
    public GameObject UI_Ranking;
    public GameObject userInfoPrefab;
    public Transform contentTransform;
    public bool isStadium = false;

    [System.Serializable]
    public class User
    {
        public int id;
        public string username;
        public int score;
    }

    public class UserList
    {
        public User[] users;
    }

    public string rankingUrl = "http://34.64.248.130:8000/stadium/ranking/";

    #region UNITY Methods
    // Start is called before the first frame update
    void Start()
    {
        if (isStadium)
        {
            StartCoroutine(GetRequest(rankingUrl));
        }

        if (PhotonNetwork.IsConnected)
        {
            Debug.Log("Connected");
            uI_ConnectionStatusGameobject.SetActive(false);
            uI_LoginGameobject.SetActive(false);

            //uI_3DGameObject.SetActive(true);
            uI_LobbyGameobject.SetActive(true);
        }
        else
        {
            uI_LobbyGameobject.SetActive(false);
            //uI_3DGameObject.SetActive(false);
            uI_ConnectionStatusGameobject.SetActive(false);

            uI_LoginGameobject.SetActive(true);
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (showConnectionStatus)
        {
            connectionStatusText.text = "Connection Status: " + PhotonNetwork.NetworkClientState;
        }
    }

    #endregion

    #region UI Callback Methods
    public void OnEnterGameButtonClicked()
    {

        string playerName = playerNameInputField.text;
        if (!string.IsNullOrEmpty(playerName))
        {
            uI_LobbyGameobject.SetActive(false);
            //uI_3DGameObject.SetActive(false);
            uI_LoginGameobject.SetActive(false);

            showConnectionStatus = true;
            uI_ConnectionStatusGameobject.SetActive(true);

            if (!PhotonNetwork.IsConnected)
            {
                PhotonNetwork.LocalPlayer.NickName = playerName;
                PhotonNetwork.ConnectUsingSettings();
            }
        }
        else
        {
            Debug.Log("Player name is invalid or empty");
        }
    }

    public void OnQuickMatchButtonClicked()
    {
        PlayerPrefs.SetString("nextScene", "BattleArena_H");
        SceneManager.LoadScene("Scene_PlayerSelection");
        //SceneManager.LoadScene("Scene_Loading");
        //SceneLoader.Instance.LoadScene("Scene_PlayerSelection");
        //SceneLoader.Instance.LoadScene("BattleArena_H");
    }

    public void OnCreateRoomButtonClicked()
    {
        //SceneManager.LoadScene("Scene_Loading");
        //SceneLoader.Instance.LoadScene("Scene_PlayerSelection");
        SceneManager.LoadScene("BattleArena_ForGeoSpatial");
    }

    public void OnJoinRoomButtonClicked()
    {
        SceneManager.LoadScene("BattleArena_ForGeoJoiner");
    }

    public void OnExitButtonClicked()
    {
        SceneManager.LoadScene("MainTitleScene");
    }
    #endregion

    #region PHOTON Callback Methods
    public override void OnConnected()
    {
        Debug.Log("We connected to Internet");

    }
    public override void OnConnectedToMaster()
    {
        Debug.Log(PhotonNetwork.LocalPlayer.NickName + " is connected to Photon Server");

        uI_LoginGameobject.SetActive(false);
        uI_ConnectionStatusGameobject.SetActive(false);

        uI_LobbyGameobject.SetActive(true);
        //uI_3DGameObject.SetActive(true);
    }


    #endregion

    #region RankingUI Methods

    public void OnRankCloseButtonClicked()
    {
        UI_Ranking.gameObject.SetActive(false);
    }
    IEnumerator GetRequest(string uri)
    {
        UnityWebRequest www = UnityWebRequest.Get(uri);
        yield return www.SendWebRequest();
    }
    #endregion
}