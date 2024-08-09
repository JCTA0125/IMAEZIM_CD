/*
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using ExitGames.Client.Photon;
using Photon.Realtime;
using UnityEngine.XR.ARFoundation;
using Google.XR.ARCoreExtensions;
using Unity.XR.CoreUtils;
using UnityEngine.XR.ARSubsystems;
using System;
using Unity.VisualScripting;
using Google.CreativeLab.BalloonPop;
using System.Runtime.CompilerServices;
using static UnityEditor.FilePathAttribute;

public class SpawnManagerForMeter : MonoBehaviourPunCallbacks
{
    
    public GameObject[] playerPrefabs;
    public Transform[] spawnPositions;

    public GameObject battleArenaGameobject;

    [Header("GPS")]
    [SerializeField] XROrigin xrOrigin;
    private AREarthManager _earthManager;
    private ARAnchorManager _anchorManager;
    private bool isAnchorSet = true;
    static public float ESTIMATED_CAM_HEIGHT_FROM_FLOOR = 1.3f;
    [SerializeField] Camera aRCamera; //public Mercator mercator;
    public GeospatialPose aRCameraPose;
    ARPlane plane;

    //private bool isCouroutineRunning = false;



    public enum RaiseEventCodes
    {
        PlayerSpawnEventCode = 0,
        PlayerGeoEventCode = 1,
    }
    private void Awake()
    {
        _anchorManager = xrOrigin.GetComponent<ARAnchorManager>();
        if (xrOrigin == null || _anchorManager == null)
        {
            Debug.Log("no anchor manager");
        }
        GameObject earthManagerGO = new GameObject("AREarthManager", typeof(AREarthManager));
        _earthManager = earthManagerGO.GetComponent<AREarthManager>();
    }

    // Start is called before the first frame update
    void Start()
    {
        PhotonNetwork.NetworkingClient.EventReceived += OnEvent;
    }

    // Update is called once per frame
    void Update()
    {
        if (!isAnchorSet)
        {
            if (_earthManager == null)
                return;


            TrackingState trackingState = _earthManager.EarthTrackingState;
            if (trackingState != TrackingState.Tracking)
            {
                return;
            }
            StartCoroutine(WaitingArenaGps());
        }
    }
    private void OnDestroy()
    {
        PhotonNetwork.NetworkingClient.EventReceived -= OnEvent; //onevent 등록 취소
    }

    #region GeoLocation Methods

    IEnumerator WaitingArenaGps()
    {
        yield return StartCoroutine(arenaGps());
    }
    IEnumerator arenaGps()
    {
        //isCouroutineRunning = true;
        Debug.Log("arenaGps Start");

        if (_earthManager == null || _anchorManager == null)
        {
            Debug.Log("null break");
            //isCouroutineRunning=false;
            yield break;
        }


        // EarthManager가 추적 중인지 확인합니다.
        TrackingState trackingState = _earthManager.EarthTrackingState;
        if (trackingState != TrackingState.Tracking)
        {
            Debug.Log("null break");
            yield break;
        }
        GeospatialPose cameraPose = _earthManager.CameraGeospatialPose;
        raiseGpsEvent(cameraPose);

    }

    private Vector3 getDistance(GeospatialPose cameraPose, GeospatialPose otherPose)
    { 


        Mercator.GeoCoordinate geoCoord = new Mercator.GeoCoordinate(cameraPose.Latitude, cameraPose.Longitude, cameraPose.Altitude);
        Mercator.GeoCoordinate otherCoord = new Mercator.GeoCoordinate(otherPose.Latitude, otherPose.Longitude, otherPose.Altitude);
        float halfmeter = (float)geoCoord.GetDistanceTo(otherCoord) / 2f;
        Debug.Log("halfmeter : " + halfmeter); //벡터 크기

        // 지구의 반지름 (단위: 미터)
        float earthRadius = 6371000f;

        // 위도와 경도의 차이를 라디안으로 변환
        float deltaLatitude = (float)(otherPose.Latitude - cameraPose.Latitude) * Mathf.Deg2Rad;
        float deltaLongitude = (float)(otherPose.Longitude - cameraPose.Longitude) * Mathf.Deg2Rad;

        // 방향 벡터 계산
        Vector3 direction = new Vector3(
            earthRadius * Mathf.Cos((float)cameraPose.Latitude * Mathf.Deg2Rad) * Mathf.Sin((float)deltaLongitude),
            0,
            earthRadius * Mathf.Sin(deltaLatitude)
        ).normalized;

        //방향 + 크기 벡터 

        Vector3 newPosition = xrOrigin.transform.position + direction * halfmeter;
        return newPosition;
    }


    //아레나 위치 송신
    private void raiseGpsEvent(GeospatialPose cameraPose)
    {
        double alt = cameraPose.Altitude - ESTIMATED_CAM_HEIGHT_FROM_FLOOR;
        object[] data = new object[]
        {
            cameraPose.Latitude, cameraPose.Longitude, alt
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

        PhotonNetwork.RaiseEvent((byte)RaiseEventCodes.PlayerGeoEventCode, data, raiseEventOptions, sendOptions);
    }
    #endregion
    #region Photon Callback Methods

    void OnEvent(EventData photonEvent)
    {
        if (photonEvent.Code == (byte)RaiseEventCodes.PlayerSpawnEventCode) //player 위치
        {
            object[] data = (object[])photonEvent.CustomData;
            Vector3 receivedPosition = (Vector3)data[0];
            Quaternion recievedRotation = (Quaternion)data[1];
            int receivedPlayerSelectionData = (int)data[3];

            GameObject player = Instantiate(playerPrefabs[receivedPlayerSelectionData], receivedPosition + battleArenaGameobject.transform.position, recievedRotation); //원격 기준의 배틀 안 player 위치
            PhotonView _photonView = player.GetComponent<PhotonView>();
            _photonView.ViewID = (int)data[2];
        }

        if (photonEvent.Code == (byte)RaiseEventCodes.PlayerGeoEventCode) // 내 gps 위치로 아레나 위치를 변경
        {
            GeospatialPose otherPose = new GeospatialPose();

            object[] data = (object[])photonEvent.CustomData;
            otherPose.Latitude = (double)data[0];
            otherPose.Longitude = (double)data[1];
            otherPose.Altitude = (double)data[2];
            otherPose.EunRotation = battleArenaGameobject.transform.rotation;
            Vector3 newArenaPosition = new Vector3(0,0,0);
            Pose newArenaPose = new Pose();
            newArenaPosition = getDistance(aRCameraPose, otherPose);


            Quaternion anchorRot = Quaternion.AngleAxis(0, new Vector3(0.0f, 1.0f, 0.0f)); //앵커에 회전까지 필요없을꺼라 생각됨.
            newArenaPose = new Pose(newArenaPosition, anchorRot);                                                                            //Gps 기반 ARAnchor
            public ARAnchor newAnchor = _anchorManager.AttachAnchor(newArenaPose);

            if (newAnchor != null)
            {

                float distance = Vector3.Distance(newAnchor.transform.position, aRCamera.transform.position);
                Debug.Log($"me and synchronized arena Distance: {distance}");

                battleArenaGameobject.SetActive(false);
                battleArenaGameobject.transform.SetParent(newAnchor.transform, false);
                battleArenaGameobject.transform.localPosition = Vector3.zero;
                battleArenaGameobject.transform.localScale = new Vector3(0.1f, 0.1f, 0.1f);

                newAnchor.gameObject.SetActive(true);
                battleArenaGameobject.SetActive(true);

                SetPlayer(newAnchor);
            }


        }
    }


    public override void OnJoinedRoom()
    {
        if (PhotonNetwork.CurrentRoom.PlayerCount == 1)
        {
            //앵커생성 시작
            isAnchorSet = false;
        }
    }
    public void SetPlayer(ARAnchor anchor)
    {

        if (PhotonNetwork.IsConnectedAndReady)
        {
            object playerSelectionNumber;
            if (PhotonNetwork.LocalPlayer.CustomProperties.TryGetValue(MultiplayerGame.PLAYER_SELECTION_NUMBER, out playerSelectionNumber))
            {
                Debug.Log("Player selection number is " + (int)playerSelectionNumber);

                Vector3 instantiatePosition = spawnPositions[0].position;
                //Vector3 instantiatePosition = spawnPositions[0].position;
                //instantiatePosition = battleArenaGameobject.transform.Find("MyRespawn").position;

                Transform respawnArena = battleArenaGameobject.transform.Find("MyRespawn");
                Transform enemyspawnArena = battleArenaGameobject.transform.Find("EnemyRespawn");



                //PhotonNetwork.Instantiate(playerPrefabs[(int)playerSelectionNumber].name, instantiatePosition, Quaternion.identity);
                GameObject playerGameobject = Instantiate(playerPrefabs[(int)playerSelectionNumber], battleArenaGameobject.transform.position, Quaternion.identity); //player 객체 생성

                PhotonView _photonView = playerGameobject.GetComponent<PhotonView>();

                playerGameobject.transform.SetParent(anchor.transform, false);
                Debug.Log("anhor has a child called playerGameobject");

                if (respawnArena != null)
                {
                    if (PhotonNetwork.CurrentRoom.PlayerCount == 1)
                    {
                        instantiatePosition = respawnArena.position;
                        Debug.Log("right position");
                    }
                }
                else if (enemyspawnArena != null)
                {
                    if (PhotonNetwork.CurrentRoom.PlayerCount > 1)
                    {
                        instantiatePosition = enemyspawnArena.position;
                        Debug.Log("right position");
                    }
                }

                playerGameobject.transform.position = instantiatePosition;
                Rigidbody rb = playerGameobject.GetComponentInChildren<Rigidbody>();
                rb.useGravity = true;

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
}
    */