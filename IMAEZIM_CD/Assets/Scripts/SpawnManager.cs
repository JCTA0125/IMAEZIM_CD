using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using ExitGames.Client.Photon;
using Photon.Realtime;
using TMPro;
using UnityEditor;

public class SpawnManager : MonoBehaviourPunCallbacks
{
    public GameObject[] playerPrefabs;
    public Transform[] spawnPositions;

    public GameObject battleArenaGameobject;
    private GameObject me;
    private GameObject enemy;

    bool twoPlayer = false;

    [Header("UI")]
    public TextMeshProUGUI timeText;

    public enum RaiseEventCodes
    {
        PlayerSpawnEventCode = 0
    }

    // Start is called before the first frame update
    void Start()
    {
        PhotonNetwork.NetworkingClient.EventReceived += OnEvent;
    }

    // Update is called once per frame
    void Update()
    {
        if (!twoPlayer)
        {
            if (PhotonNetwork.CurrentRoom.PlayerCount > 1)
            {
                //만약에 만들기도 전에 실행되면 안되니까
                if ((me && enemy) || Application.isEditor)
                {
                    StartCoroutine(CountDown());
                    twoPlayer = true;

                }
            }
        }
    }
    private void OnDestroy()
    {
        PhotonNetwork.NetworkingClient.EventReceived -= OnEvent; //onevent 등록 취소
    }

    #region Photon Callback Methods

    void OnEvent(EventData photonEvent)
    {
        if (photonEvent.Code == (byte)RaiseEventCodes.PlayerSpawnEventCode)
        {
            object[] data = (object[])photonEvent.CustomData;
            Vector3 receivedPosition = (Vector3)data[0];
            Quaternion recievedRotation = (Quaternion)data[1];
            int receivedPlayerSelectionData = (int)data[3];

            GameObject player = Instantiate(playerPrefabs[receivedPlayerSelectionData], receivedPosition + battleArenaGameobject.transform.position, recievedRotation); //원격 기준의 배틀 안 player 위치
            PhotonView _photonView = player.GetComponent<PhotonView>();
            _photonView.ViewID = (int)data[2];
            enemy = player;
            enemy.SetActive(false);
        }
    }


    public override void OnJoinedRoom()
    {
        if (PhotonNetwork.IsConnectedAndReady)
        {
            object playerSelectionNumber;
            if (PhotonNetwork.LocalPlayer.CustomProperties.TryGetValue(MultiplayerGame.PLAYER_SELECTION_NUMBER, out playerSelectionNumber))
            {
                Debug.Log("Player selection number is " + (int)playerSelectionNumber);


                Vector3 instantiatePosition = spawnPositions[0].position;

                if (PhotonNetwork.CurrentRoom.PlayerCount == 1)
                {
                    instantiatePosition = spawnPositions[0].position;
                }
                else if (PhotonNetwork.CurrentRoom.PlayerCount > 1)
                {
                    instantiatePosition = spawnPositions[1].position;
                }

                //PhotonNetwork.Instantiate(playerPrefabs[(int)playerSelectionNumber].name, instantiatePosition, Quaternion.identity);
                GameObject playerGameobject = Instantiate(playerPrefabs[(int)playerSelectionNumber], instantiatePosition, Quaternion.identity);
                Damage loc = playerGameobject.GetComponent<Damage>();
                if (loc != null) //respawn 을 위한 위치 저장
                {
                    loc.firstPos = instantiatePosition;
                }

                PhotonView _photonView = playerGameobject.GetComponent<PhotonView>();
                //전역변수로 저장
                me = playerGameobject;
                me.SetActive(false);
                //respawn 해봄.
                RespawnScript respawnScript= me.GetComponentInChildren<RespawnScript>();
                if (respawnScript != null)
                {
                    respawnScript.respawnPoint = instantiatePosition;
                }
                //respawn 끝.
                if (PhotonNetwork.AllocateViewID(_photonView)) //viewid 할당 성공
                {

                    object[] data = new object[]// 배열인가?
                    {
                    playerGameobject.transform.position - battleArenaGameobject.transform.position, playerGameobject.transform.rotation, _photonView.ViewID, playerSelectionNumber
                    };

                    RaiseEventOptions raiseEventOptions = new RaiseEventOptions
                    {
                        Receivers = ReceiverGroup.Others,
                        CachingOption = EventCaching.AddToRoomCache
                    };

                    SendOptions sendOptions = new SendOptions
                    {
                        Reliability = true
                    };

                    PhotonNetwork.RaiseEvent((byte)RaiseEventCodes.PlayerSpawnEventCode, data, raiseEventOptions, sendOptions);
                    //raise events!

                }
                else
                {
                    Debug.Log("Failed to allocate a viewID");
                    Destroy(playerGameobject);
                }
            }
        }

    }
    #endregion

    #region UI Methods

    IEnumerator CountDown()
    {
        timeText.gameObject.SetActive(true);
        float LeftTime = 3.0f;
        int displayTime = Mathf.FloorToInt(LeftTime); // 소수점 이하를 버림하여 정수로 변환
        timeText.text = displayTime.ToString();

        while (LeftTime >= 0)
        {
            LeftTime -= Time.deltaTime;
            int nextDisplayTime = Mathf.FloorToInt(LeftTime);

            if (nextDisplayTime != displayTime)
            {
                displayTime = nextDisplayTime;
                timeText.text = displayTime.ToString();
            }

            yield return null; // 다음 프레임까지 대기
        }

        timeText.gameObject.SetActive(false);
        if (me && enemy)
        {
            me.SetActive(true);
            enemy.SetActive(true);
            enemy.transform.Find("JCanvas").gameObject.SetActive(false);

        }


        yield return null;
    }

    #endregion
}