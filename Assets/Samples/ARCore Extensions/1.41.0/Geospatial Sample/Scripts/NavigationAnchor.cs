using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Google.XR.ARCoreExtensions.Samples.Geospatial2;

public class NavigationAn : MonoBehaviour
{
    public GeospatialController geospatialController;

    public void addHistory(double latitude, double longitude, double altitude, Quaternion eunRotation, string objType)  //_historyCollection 리스트에 history 추가
    {
        //Quaternion eunRotation = Quaternion.identity; // 단위 쿼터니언으로 초기화

        GeospatialAnchorHistory2 history = new GeospatialAnchorHistory2(
               latitude, longitude, altitude,
               AnchorType.Geospatial, eunRotation, objType);  // Quaternion eunRotation
        geospatialController._historyCollection.Collection.Add(history);
        geospatialController.SaveGeospatialAnchorHistory();

        Debug.Log(geospatialController._historyCollection.Collection.Count);
    }

    public Quaternion arrowDirection(double startLatitude, double startLongitude, double endLatitude, double endLongitude)
    {
        //GPS 좌표
        Vector3 gpsCoordinate1 = new Vector3((float)startLatitude, (float)startLongitude, 0f);
        Vector3 gpsCoordinate2 = new Vector3((float)endLatitude, (float)endLongitude, 0f);

        // 두 GPS 좌표 간의 방향 벡터 계산
        Vector3 direction = gpsCoordinate2 - gpsCoordinate1;
        // 방향 벡터를 Quaternion으로 변환
        Quaternion targetRotation = Quaternion.LookRotation(direction);

        //x, y축을  90도 회전
        targetRotation *= Quaternion.Euler(90, 90, 0); //화살표 방향 반대면 x축 조정

        return targetRotation;
    }

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
    // GPS 좌표를 담는 리스트
    public List<GPSPoint> gpsPoints = new List<GPSPoint>();
    // GPS 좌표를 리스트에 추가
    public void AddGPSPoint(double latitude, double longitude)
    {
        GPSPoint point = new GPSPoint(latitude, longitude);
        gpsPoints.Add(point);
    }
    public void getGps()
    {
        Debug.Log("getGps()");
        AddGPSPoint(37.50308926526337, 127.10142273782344);
        AddGPSPoint(37.50288649852998, 127.10072836230565);
        AddGPSPoint(37.50281150779014, 127.10075891720916);
        AddGPSPoint(37.50281706310067, 127.10078113725221);
        AddGPSPoint(37.50253098796738, 127.10093390924818);
        AddGPSPoint(37.50221156021388, 127.09980068807636);
        AddGPSPoint(37.5021754534965, 127.09981179920104);
    }

    //화살표 앵커 추가
    public void addArrowAnchor()
    {
        Debug.Log("addArrowAnchor()");
        getGps();
        for (int i = 0; i < gpsPoints.Count - 1; i++)
        {
            Debug.Log("gpsPoint : "+i);

            // i번째와 i+1번째 GPS 좌표 가져오기
            GPSPoint startPoint = gpsPoints[i];
            GPSPoint endPoint = gpsPoints[i + 1];

            //화살표 방향 계산
            Quaternion direction = arrowDirection(startPoint.latitude, startPoint.longitude, endPoint.latitude, endPoint.longitude);
            //앵커 추가
            addHistory(startPoint.latitude, startPoint.longitude, 42.01, direction, "arrow");

        }
        geospatialController.SaveGeospatialAnchorHistory();
        //geospatialController.ResolveHistory();
        
    }
    
    // Start is called before the first frame update
    void Start()
    {
        /*
        geospatialController.OnClearAllClicked();
        Quaternion eunRotation = Quaternion.identity; // 단위 쿼터니언으로 초기화

        addHistory(127.10142273782344, 37.50308926526337, 42.01, eunRotation, "arrow");
        geospatialController.ResolveHistory();
        */
        addArrowAnchor();
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
