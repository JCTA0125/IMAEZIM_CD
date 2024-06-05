using Google.CreativeLab.BalloonPop;
using Google.XR.ARCoreExtensions;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.XR.CoreUtils;
using UnityEngine;

using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;
using static ServerQuizManager;


//앵커를 만들고 배치하거나 GPS 정보를 얻어오는 script
public class ARController : MonoBehaviour
{


    [Header("GEOTRACKING")]
    public ARSession Session;
    public AREarthManager EarthManager;
    public XROrigin xrOrigin;
    public GeospatialPose lastPose;
    //public TextMeshProUGUI text;


    [Header("ISGEORIGHT")]
    private const double _horizontalAccuracyThreshold = 20;
    private const double _orientationYawAccuracyThreshold = 25;
    private bool isActive = true;

    [Header("GPSUI")]
    public GameObject availableSign;
    public GameObject disavailableSign;

    [Header("MakingAnchor")]
    public ServerQuizManager ServerQuizManager;
    static public float ESTIMATED_CAM_HEIGHT_FROM_FLOOR = 1.3f;
    private ARAnchorManager AnchorManager;


    [Header("ForAnchorAdministration")]
    public List<GameObject> allAnchors = new List<GameObject>();

    [Header("ForPostGPS")]
    public QuizPlacer QuizPlacer;

    private void Awake()
    {
        AnchorManager = xrOrigin.GetComponent<ARAnchorManager>();
        if (xrOrigin == null || AnchorManager == null)
        {
            Debug.Log("no anchor manager");
        }
        GameObject earthManagerGO = new GameObject("AREarthManager", typeof(AREarthManager));
        EarthManager = earthManagerGO.GetComponent<AREarthManager>();
    }
    private void Update()
    {


        bool status = GeoStatus();
        IsSignChanged(status);

    }

    #region FORGPSINFO

    //gps 상태확인
    private bool GeoStatus()
    {
        if (ARSession.state != ARSessionState.SessionInitializing && ARSession.state != ARSessionState.SessionTracking)
        {
            return false;
        }

        bool isSessionReady = ARSession.state == ARSessionState.SessionTracking;
        var earthTrackingState = EarthManager.EarthTrackingState;
        var pose = earthTrackingState == TrackingState.Tracking ? EarthManager.CameraGeospatialPose : new GeospatialPose();

        if (!isSessionReady || earthTrackingState != TrackingState.Tracking || pose.OrientationYawAccuracy > _orientationYawAccuracyThreshold ||
            pose.HorizontalAccuracy > _horizontalAccuracyThreshold)
        {
            if (isActive)
            {
                foreach (var go in allAnchors)
                {
                    go.SetActive(false);
                }
                isActive = false;
            }
            return false;

        }
        else
        {
            lastPose = pose;
            if (!isActive)
            {
                foreach (var go in allAnchors)
                {
                    go.SetActive(true);
                }
                isActive = true;
            }
            return true;
        }
    }

    //객체의 gps 좌표 얻기

    private Mercator.GeoCoordinate GetDistanceAndGPS(GameObject quiz)
    {

        Vector3 playerLocation = xrOrigin.Camera.transform.position;
        float distance = Vector3.Distance(quiz.transform.position, playerLocation);

        Mercator.GeoCoordinate geoCoord = new Mercator.GeoCoordinate(lastPose.Latitude, lastPose.Longitude, lastPose.Altitude);
        Mercator.GeoCoordinate geoCoordAhead = geoCoord.CalculateDerivedPosition(distance, lastPose.Heading);
        return geoCoordAhead;
    }
    #endregion

    #region PLACEQUIZWITHANCHOR

    public IEnumerator WaitingQuizAnchor(GameObject quizObject)
    {
        yield return StartCoroutine(MakingQuizAnchor(quizObject));
        yield return StartCoroutine(ServerQuizManager.PostQuiz());

        if(allAnchors.Count > 0)
        {
            GameObject lastAnchor = allAnchors[allAnchors.Count - 1];
            Transform quiz = lastAnchor.transform.GetChild(0);

            if (quiz != null)
            {
                Transform present = quiz.transform.Find("Present");
                if (present != null)
                {
                    present.gameObject.SetActive(true);
                }
                else
                {
                    Debug.Log("Couldn't find present - WaitingQuizAnchor");
                }
            }
            else
            {
                Debug.Log("Couldn't find quiz - WaitingQuizAnchor");
            }
        }
    }

    public IEnumerator MakingQuizAnchor(GameObject quizObject)
    {
        while (!GeoStatus())
        {
            yield return null;
        }
        Mercator.GeoCoordinate quizGPS = GetDistanceAndGPS(quizObject);
        SettingGPSInfo(quizGPS);
        SettingAnchor(quizGPS, quizObject);
    }

    private void SettingGPSInfo(Mercator.GeoCoordinate quizGPS)
    {
        if (QuizPlacer.lastnewQuizInfo != null)
        {
            QuizPlacer.lastnewQuizInfo.altitude = (float)quizGPS.altitude;
            QuizPlacer.lastnewQuizInfo.longitude = (float)quizGPS.longitude;
            QuizPlacer.lastnewQuizInfo.latitude = (float)quizGPS.latitude;
        }
        else
        {
            Debug.Log("Dont have lastnewinfo");
        }

    }

    public IEnumerator MakingQuizzesAnchor(ServerQuizManager.Quiz[] quizzes)
    {
        while (!GeoStatus())
        {
            yield return null;
        }

        if(ServerQuizManager.quizObjects != null)
        {
            List<GameObject> quizObjects = ServerQuizManager.quizObjects;

            if (quizObjects == null || quizObjects.Count == 0)
            {
                Debug.Log("quizOBjects got problem - MakingQuizzesAnchor");
                yield break;
            }

            for (int i = 0; i < quizzes.Length; i++)
            {
                ServerQuizManager.Quiz quiz = quizzes[i];
                GameObject quizObject = quizObjects[i];

                Mercator.GeoCoordinate quizGPS = new Mercator.GeoCoordinate(quiz.latitude, quiz.longitude, quiz.altitude);
                SettingAnchor(quizGPS, quizObject);
            }
        }
        else
        {
            Debug.Log("quizOBjects got problem - MakingQuizzesAnchor");
        }



    }

    
    private void SettingAnchor(Mercator.GeoCoordinate quizGPS, GameObject quizObject)
    {

        //float alt = (float)quizGPS.altitude - ESTIMATED_CAM_HEIGHT_FROM_FLOOR;
        Quaternion quizRot = quizObject.transform.rotation;
        Quaternion anchorRot = Quaternion.AngleAxis(0, new Vector3(0.0f, 1.0f, 0.0f));

        double lat = quizGPS.latitude;
        double lng = quizGPS.longitude;
        double alt = quizGPS.altitude;
        ARGeospatialAnchor newAnchor = AnchorManager.AddAnchor(lat, lng, alt, anchorRot);
        Debug.Log("latitude is " + quizGPS.latitude);
        if (newAnchor != null)
        {
            Debug.Log("Anchor Position: " + newAnchor.transform.position);
            //floating 종료
            //Floating(quizObject);

            quizObject.transform.SetParent(newAnchor.transform, false);
            quizObject.transform.localPosition = Vector3.zero;
            quizObject.transform.localScale = new Vector3 (2,2,2);
            quizObject.transform.localRotation = quizRot;
            allAnchors.Add(newAnchor.gameObject);

            newAnchor.gameObject.SetActive(true);
            quizObject.SetActive(true);

            Debug.Log("SettingAnchor done");
            Debug.Log("quiz Position: " + quizObject.transform.position);
            Debug.Log("quiz LocalPosition: " + quizObject.transform.localPosition);

            //floating 복원
            //Floating(quizObject);
        }
        else
        {
            Debug.Log("I can't make Anchor Setting Anchor Problem");
        }
    }

    private void Floating(GameObject quiz)
    {
        if (quiz == null)
        {
            Debug.Log("QuizPlacer.newQuiz is null");
            return;
        }
        Transform present = quiz.transform.Find("Present");
        if (present != null || present.gameObject.activeSelf)
        {
            ForFloating forFloating = present.GetComponentInChildren<ForFloating>();
            if (forFloating != null)
            {
                if (forFloating.enabled)
                {
                    forFloating.enabled = false;
                }
                else
                {
                    forFloating.enabled = true;
                }
            }
        }
        else
        {
            Transform checkmark = quiz.transform.Find("CheckMark");
            if (checkmark != null)
            {
                ForFloating forFloating = present.GetComponentInChildren<ForFloating>();
                if (forFloating != null)
                {
                    if (forFloating.enabled)
                    {
                        forFloating.enabled = false;
                    }
                    else
                    {
                        forFloating.enabled = true;
                    }
                }
            }
            else
            {
                Debug.Log("Can't find child object in floating");
            }
        }



    }
    #endregion
    #region FORGPSUI

    public void IsSignChanged(bool isGpsAvailable)
    {
        if (isGpsAvailable)
        {
            availableSign.SetActive(true);
            disavailableSign.SetActive(false);
        }
        else
        {
            availableSign.SetActive(false);
            disavailableSign.SetActive(true);
        }
    }
    #endregion

}


