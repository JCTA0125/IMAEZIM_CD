using Photon.Pun;
using Photon.Realtime;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class FindPlayer : MonoBehaviourPunCallbacks
{

    [Header("FindRoom UI")]
    public InputField roomNameInputField;
    public GameObject uI_RoomInfoPanel;
    public GameObject uI_SearchTheRoom;

    [Header("ManagerForTheGame")]
    public GameObject gameManger;
    public GameObject spawnManager;

    [Header("GameInfo UI")]
    public GameObject uI_InformPanelGameObject;
    public TextMeshProUGUI uI_InformText;

    private string roomName;

    private void Awake()
    {
        uI_RoomInfoPanel.SetActive(false);
        uI_SearchTheRoom.SetActive(true);

        // spawnManager.SetActive(false);
        //  gameManger.SetActive(false);
        uI_InformPanelGameObject.SetActive(false);
    }



    #region UI Callback Methods

    public void OnEnterSearchRoom()
    {
        roomName = roomNameInputField.text;
        joinRoom(roomName);
    }

    private void joinRoom(string roomName)
    {
        PhotonNetwork.JoinRoom(roomName);
    }




    #endregion

    #region PHOTON Callback Methods

    public override void OnJoinedRoom()
    {
        //search 캔버스를 없애야 함.
        uI_SearchTheRoom.SetActive(false);
        //gameManger.SetActive(true);
        //spawnManager.SetActive(true);
        uI_InformPanelGameObject.SetActive(true);

        uI_InformText.text = "Game Start!";
        StartCoroutine(DeactivateAfterSeconds(uI_InformPanelGameObject, 1.0f));
    }

    public override void OnJoinRoomFailed(short returnCode, string message)
    {
        uI_RoomInfoPanel.SetActive(true);
        //안되었다고 말해야 함.
        TextMeshProUGUI roomInfoText = uI_RoomInfoPanel.transform.GetChild(0).GetComponent<TextMeshProUGUI>();
        roomInfoText.text = "Can't find the Room. Did you write the RoomName Correctly?";
        //panel이 조금 있다 삭제 되야 함. (코루틴 game manager85코드)
        StartCoroutine(DeactivateAfterSeconds(uI_RoomInfoPanel, 2.0f));

    }
    #endregion

    #region PRIVATE Methods

    IEnumerator DeactivateAfterSeconds(GameObject _gameObject, float _seconds)
    {
        yield return new WaitForSeconds(_seconds);
        _gameObject.SetActive(false);
    }


    #endregion
}
