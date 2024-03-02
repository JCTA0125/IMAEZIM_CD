using UnityEngine;
using UnityEngine.Networking;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Collections;
using System.Collections.Generic;
using System;

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

    // GPS 좌표를 리스트에 추가
    public void AddPointGPS(double latitude, double longitude)
    {
        GPSPoint point = new GPSPoint(latitude, longitude);
        gpsPointList.Add(point);
    }

    public void AddLinestringGPS(double latitude, double longitude)
    {
        GPSPoint point = new GPSPoint(latitude, longitude);
        gpsLinestringList.Add(point);
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
    
    void Start()
    {
        StartCoroutine(MakeRequest());
    }

    IEnumerator MakeRequest()
    {
        string url = "https://apis.openapi.sk.com/tmap/routes/pedestrian";

        WWWForm form = new WWWForm();
        form.AddField("startX", "126.92365493654832");
        form.AddField("startY", "37.556770374096615");
        form.AddField("speed", "4");
        form.AddField("endX", "126.92432158129688");
        form.AddField("endY", "37.55279861528311");
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
            }
            OnDataReceived(); //리스트 추가 후 콜백
        }
    }
}
