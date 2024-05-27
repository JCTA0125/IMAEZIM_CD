using UnityEngine;
using UnityEngine.Networking;
//using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Collections;
using System.Collections.Generic;
using System;
//using Google.XR.ARCoreExtensions.Samples.Geospatial2;

public class api : MonoBehaviour
{
    // GPS 좌표를 담는 구조체
    public struct GPSPoint
    {
        public double latitude;
        public double longitude;
        public double elevation;

        public GPSPoint(double lat, double lon, double elev)
        {
            latitude = lat;
            longitude = lon;
            elevation = elev;
        }
    }

    // Point의 GPS 좌표를 담는 리스트
    public List<GPSPoint> gpsPointList = new List<GPSPoint>();

    // LineString의 GPS 좌표를 담는 리스트
    public List<GPSPoint> gpsLinestringList = new List<GPSPoint>();

    public int tDistance = 0; //총거리
    public int ApiResult = 1;  //"경로 정보 존재하지 않습니다" 띄우는 상황 : 0   띄우면 안되는 상황 : 1

    // GPS 좌표를 리스트에 추가
    public void AddPointGPS(double latitude, double longitude)
    {
        GPSPoint point = new GPSPoint(latitude, longitude, 40);
        gpsPointList.Add(point);
    }
    /*
    public void AddLinestringGPS(double latitude, double longitude)
    {
        GPSPoint point = new GPSPoint(latitude, longitude);

        // 리스트 내 GPS 좌표 존재유무 확인
        if (!gpsLinestringList.Contains(point))
        {
            gpsLinestringList.Add(point);
        }
        else
        {
            Debug.Log("GPS already exists in the Linestring list! -> (" + latitude + ", " + longitude + ")");
        }
    }
    */
    public void AddLinestringGPS(double latitude, double longitude, double elevation)
    {
        Debug.Log("api : AddLinestringGPS");
        GPSPoint point = new GPSPoint(latitude, longitude, elevation);

        if (!gpsLinestringList.Contains(point))
        {
            gpsLinestringList.Add(point);
        }
        else
        {
            Debug.Log("GPS already exists in the Linestring list! -> (" + latitude + ", " + longitude + ", " + elevation + ")");
        }
    }

    private Action dataCallback;

    private void OnDataReceived() //콜백 함수
    {
        //Debug.Log("api : OnDataReceived");
        dataCallback?.Invoke();
    }

    public void gpsCallback(Action callback) //콜백 추가
    {
        //Debug.Log("api : gpsCallback");
        if (dataCallback == null)
        {
            dataCallback = () => { }; // 빈 동작으로 초기화
        }
        dataCallback += callback;
    }

    //public GeospatialController geospatialController;
    void Start()
    {
        PlayerPrefs.SetString("startLatitudeKey", "0.0");
        PlayerPrefs.SetString("startLongitudeKey", "0.0");
        //Debug.Log("api : Start");
        StartCoroutine(MakeRequest());
    }

    public double startLatitude = 0.0;
    public double startLongitude = 0.0;
    public double memoLatitude = 0.0;
    public double memoLongitude = 0.0;
    //안드 -> 유니티
    public void getLatitude(string latitude)
    {
        Debug.Log("getLatitude 실행 latitude = " + latitude);
        //memoLatitude = double.Parse(latitude);
        PlayerPrefs.SetString("memoLatitudeKey", latitude);
    }
    public void getLongitude(string longitude)
    {
        Debug.Log("getLongitude 실행 longitude = " + longitude);
        //memoLongitude = double.Parse(longitude);
        PlayerPrefs.SetString("memoLongitudeKey", longitude);
    }
    public void getInOut(string inout)
    {
        Debug.Log("getInOut 실행  실내 or 실외 = " + inout);
        PlayerPrefs.SetString("inout", inout);
    }

    //메모 위치 받아오는 함수
    public void getMemoGps()
    {
        //memoLatitude = 37.651634;   //안드에서 받아오게 수정
        //memoLongitude = 127.016865; 
        //PlayerPrefs.SetString("memoLatitudeKey", memoLatitude.ToString());  //나중에 주석처리
        //PlayerPrefs.SetString("memoLongitudeKey", memoLongitude.ToString());

        memoLatitude = double.Parse(PlayerPrefs.GetString("memoLatitudeKey"));
        memoLongitude = double.Parse(PlayerPrefs.GetString("memoLongitudeKey"));
    }
    public void getStratGps()
    {
        //startLatitude = 37.652387;  //double.Parse(PlayerPrefs.GetString("startLatitudeKey"));
        //startLongitude = 127.016292;  //double.Parse(PlayerPrefs.GetString("startLongitudeKey"));

        startLatitude = double.Parse(PlayerPrefs.GetString("startLatitudeKey"));
        startLongitude = double.Parse(PlayerPrefs.GetString("startLongitudeKey"));
    }

    private double GetElevationFromGoogleAPI(double latitude, double longitude)
    {
        string requestUrl = "https://maps.googleapis.com/maps/api/elevation/json?locations=" + latitude + "," + longitude + "&key=AIzaSyB0c06V_2K6FVuXLdr-zeTh2HFfNIHnCTg";

        using (UnityWebRequest www = UnityWebRequest.Get(requestUrl))
        {
            www.SendWebRequest();
            while (!www.isDone) { } // 요청 완료 대기

            if (www.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError("Network error: " + www.error);
                return 0;
            }
            else
            {
                string jsonResponse = www.downloadHandler.text;
                //Debug.Log("api : " + jsonResponse);
                JObject jsonObject = JObject.Parse(jsonResponse);
                double elevation = (double)jsonObject["results"][0]["elevation"];
                return elevation + 22;
                //Debug.Log("api2 : " + elevation.ToString());
            }
        }
    }

    IEnumerator MakeRequest()
    {
        getMemoGps();
        getStratGps();

        while (memoLatitude == 0.0)
        {
            getMemoGps();
            yield return null;
        } //메모 위치 안드에서 받아올때까지 대기

        while (startLatitude == 0.0) 
        { 
            getStratGps();
            yield return null;
        } //출발점 gps 안정화 될때까지 대기

        string url = "https://apis.openapi.sk.com/tmap/routes/pedestrian";

        WWWForm form = new WWWForm();
        form.AddField("startX", startLongitude.ToString()); //"126.92365493654832"
        form.AddField("startY", startLatitude.ToString());//"37.556770374096615")
        form.AddField("speed", "4");
        form.AddField("endX", memoLongitude.ToString());//"126.92432158129688"
        form.AddField("endY", memoLatitude.ToString()); //37.55279861528311")
        form.AddField("startName", "출발");
        form.AddField("endName", "도착");

        using (UnityWebRequest www = UnityWebRequest.Post(url, form))
        {
            www.SetRequestHeader("appKey", "rviraw9oy947oGbzBZtVs3jFOEtLxOinaxcrLgRy");
            www.SetRequestHeader("Accept-Language", "ko");

            yield return www.SendWebRequest();

            if (www.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError("Network error: " + www.error);
            }
            else
            {
                string jsonResponse = www.downloadHandler.text;
                Debug.Log(jsonResponse);

                // JSON 파싱
                JObject jsonObject = JObject.Parse(jsonResponse);
                JArray features = (JArray)jsonObject["features"];

                foreach (JToken feature in features)
                {
                    string type = feature["geometry"]["type"].ToString();

                    if (type == "Point")
                    {
                        JArray coordinates = (JArray)feature["geometry"]["coordinates"];
                        double longitude = coordinates[0].Value<double>();
                        double latitude = coordinates[1].Value<double>();
                        AddPointGPS(latitude, longitude); // Point의 GPS 좌표를 리스트에 추가
                        Debug.Log("Point: (" + latitude + ", " + longitude + ")");
                    }
                    else if (type == "LineString")
                    {
                        JArray coordinates = (JArray)feature["geometry"]["coordinates"];
                        foreach (JToken coordinate in coordinates)
                        {
                            double longitude = coordinate[0].Value<double>();
                            double latitude = coordinate[1].Value<double>();
                            // Google Elevation API로 고도 값 가져오기
                            double elevation = GetElevationFromGoogleAPI(latitude, longitude);

                            // 고도 값 포함하여 GPS 좌표를 리스트에 추가
                            AddLinestringGPS(latitude, longitude, elevation);
                            Debug.Log("LineString Point: (" + latitude + ", " + longitude + "), Elevation: " + elevation);

                            //AddLinestringGPS(latitude, longitude); // LineString의 GPS 좌표를 리스트에 추가
                            //Debug.Log("LineString Point: (" + latitude + ", " + longitude + ")");
                        }
                    }
                }

                // 총거리 계산
                int totalDistances = 0;
                foreach (JToken feature in features)
                {
                    // "properties" 필드가 있는지 확인
                    if (feature["properties"] != null)
                    {
                        // "totalDistance" 필드가 있는지 확인
                        if (feature["properties"]["totalDistance"] != null)
                        {
                            int distance = feature["properties"]["totalDistance"].Value<int>();
                            totalDistances += distance;
                        }
                    }
                }

                float totalDistance = 0;
                string unit = "";

                // 총거리가 1000 이상인 경우 km(킬로미터)로
                if (totalDistances >= 1000)
                {
                    totalDistance = totalDistances / 1000.0f;
                    unit = "km";
                }
                // 나머지는 m(미터)로
                else
                {
                    totalDistance = totalDistances;
                    unit = "m";
                }
                Debug.Log("총거리 : " + totalDistance.ToString("F1") + " " + unit);
                tDistance = totalDistances;
            }
            Debug.Log("api : before OnDataReceived");
            OnDataReceived(); //리스트 추가 후 콜백

        }
        //한번이라도 gps 안정화 되야 여기 코드 실행, api 요청도 끝나야 실행 
        if (tDistance == 0) //총거리 없으면 -> api 요청 못받은 상황
        {
            ApiResult = 0;  //"경로 존재하지 않습니다" 띄우는 상황
        }
    }
}
