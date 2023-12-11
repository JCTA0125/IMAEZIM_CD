using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.Android;
using UnityEngine.InputSystem;
using UnityEngine.Networking;
using UnityEngine.UI;
//using UnityEngine.UIElements;
using static System.Net.WebRequestMethods;

public class MapManager : MonoBehaviour
{
    public RawImage mapRawImage;
    public Text progressText;
    public bool isUpdating;

    LocationService locationService;
    LocationInfo currentGPSPosition;
    //int gps_connect = 0;

    public Button buttonMP, buttonMN;
    public GameObject PopUp_M;

    [Header("Setting")]
    public float desiredAccuracyInMeters;
    public float updateDistanceInMeters;

    [Header("맵 정보 설정")]
    public string strBaseURL = "";
    static public float latitude;
    static public float longitude;
    public float latTmp;
    public float lngTmp;
    //private double altitude;
    public int zoom = 14;
    public int mapWidth;
    public int mapHeight;
    public string strAPIKey = "";

    private int maxWait;

    private void Awake()
    {
        locationService = Input.location;
    }
    // Start is called before the first frame update
    void Start()
    {
        latTmp = 0;
        lngTmp = 0;
        //latitude = 0;
        //longitude = 0;
        maxWait = 10;
        mapRawImage = GetComponent<RawImage>();
        if (!isUpdating)
        {
            StartCoroutine(GetLocation());
            isUpdating = !isUpdating;
        }
        buttonMP.onClick.AddListener(() =>
        {
            PopUp_M.SetActive(false);
            latTmp = 0;
            lngTmp = 0;
            mapRawImage.texture = null;
        });
        buttonMN.onClick.AddListener(() =>
        {
            mapRawImage.texture = null;
            Start();
        });
    }

    IEnumerator GetLocation()
    {
        progressText.text = "getLocation";
        if (!Permission.HasUserAuthorizedPermission(Permission.FineLocation))
        {
            Permission.RequestUserPermission(Permission.FineLocation);
            Permission.RequestUserPermission(Permission.CoarseLocation);
        }
        // First, check if user has location service enabled
        if (!Input.location.isEnabledByUser)
            yield return new WaitForSeconds(10);

        // Start service before querying location
        Input.location.Start();

        // Wait until service initializes

        while (Input.location.status == LocationServiceStatus.Initializing && maxWait > 0)
        {
            yield return new WaitForSeconds(1);
            maxWait--;
        }

        // Service didn't initialize in 20 seconds
        if (maxWait < 1)
        {
            progressText.text = "Timed out";
            yield break;
        }

        // Connection has failed
        if (Input.location.status == LocationServiceStatus.Failed)
        {
            progressText.text = "Unable to determine device location";
            yield break;
        }
        else
        {
            latTmp = Input.location.lastData.latitude;
            lngTmp = Input.location.lastData.longitude;
            progressText.text = "Location: " + Input.location.lastData.latitude + " " + Input.location.lastData.longitude + " " + Input.location.lastData.altitude + 100f + " " + Input.location.lastData.horizontalAccuracy + " " + Input.location.lastData.timestamp;
            // Access granted and location value could be retrieved
        }

        // Stop service if there is no need to query location updates continuously
        isUpdating = !isUpdating;
        Input.location.Stop();
    }


    void RetrieveGPSData()
    {
        currentGPSPosition = Input.location.lastData;

        latitude = currentGPSPosition.latitude;
        longitude = currentGPSPosition.longitude;

        progressText.text = "위도 : " + latitude.ToString() + " / 경도 : " + longitude.ToString();
    }
    IEnumerator LoadMap()
    {
        string url = strBaseURL + "center=" + latTmp + "," + lngTmp
            + "&zoom=" + zoom.ToString() + "&size=" + mapWidth.ToString() + "x" + mapHeight.ToString()
            + "&maptype=roadmap&markers=color:blue" + "%7Clabel:S%7C" + latTmp + "," + lngTmp
            + "&key=" + strAPIKey;

        //Debug.Log("URL : " + url);
        progressText.text = url;
        latitude = latTmp; 
        longitude = lngTmp;
        url = UnityWebRequest.UnEscapeURL(url);
        UnityWebRequest req = UnityWebRequestTexture.GetTexture(url);

        yield return req.SendWebRequest();

        mapRawImage.texture = DownloadHandlerTexture.GetContent(req);
    }

    // Update is called once per frame
    void Update()
    {
        if (latTmp == 0)
        {
            Start();
        }
        if (latTmp != 0)
        {
            StartCoroutine(LoadMap());
        }
    }
}