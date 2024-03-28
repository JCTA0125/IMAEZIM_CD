using UnityEngine;
using UnityEngine.Networking;
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

        public GPSPoint(double lat, double lon)
        {
            latitude = lat;
            longitude = lon;
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
        GPSPoint point = new GPSPoint(latitude, longitude);
        gpsPointList.Add(point);
    }

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
    
    private Action dataCallback;

    private void OnDataReceived() //콜백 함수
    {
        dataCallback?.Invoke();
    }

    public void gpsCallback(Action callback) //콜백 추가
    {
        dataCallback += callback;
    }

    //public GeospatialController geospatialController;
    void Start()
    {
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
        memoLatitude = double.Parse(latitude);
        PlayerPrefs.SetString("memoLatitudeKey", latitude);
    }
    public void getLongitude(string longitude)
    {
        Debug.Log("getLongitude 실행 longitude = " + longitude);
        memoLongitude = double.Parse(longitude);
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
        memoLatitude = 37.501192; //37.651681;  //안드에서 받아오게 수정
        memoLongitude = 127.097448; // 127.014791;
        PlayerPrefs.SetString("memoLatitudeKey", memoLatitude.ToString());  //나중에 주석처리
        PlayerPrefs.SetString("memoLongitudeKey", memoLongitude.ToString());
    }
    public void getStratGps()
    {
        startLatitude = 37.503221; //37.653302; //double.Parse(PlayerPrefs.GetString("startLatitudeKey"));
        startLongitude = 127.101494; //127.015870; //double.Parse(PlayerPrefs.GetString("startLongitudeKey"));
    }

    IEnumerator MakeRequest()
    {
        getMemoGps();
        getStratGps();

        while (startLatitude == 0.0) { getStratGps(); } //출발점 gps 안정화 될때까지 대기

        while (memoLatitude == 0.0) { getMemoGps(); } //메모 위치 안드에서 받아올때까지 대기

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
                            AddLinestringGPS(latitude, longitude); // LineString의 GPS 좌표를 리스트에 추가
                            Debug.Log("LineString Point: (" + latitude + ", " + longitude + ")");
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
            OnDataReceived(); //리스트 추가 후 콜백

        }
        //한번이라도 gps 안정화 되야 여기 코드 실행, api 요청도 끝나야 실행 
        if (tDistance == 0) //총거리 없으면 -> api 요청 못받은 상황
        {
            ApiResult = 0;  //"경로 존재하지 않습니다" 띄우는 상황
        }
    }
}
