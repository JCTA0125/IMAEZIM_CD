using Google.XR.ARCoreExtensions.Samples.Geospatial;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Networking;

public class GetServer : MonoBehaviour
{

#nullable enable
    [System.Serializable]
    public struct MemoContent
    {
        public int id;
        public string text;
        public string picture;
        public string video; //이것도 string으로 바꿔야 할 듯
    }
    [System.Serializable]
    public struct ServerHistory
    {
        public float longitude; //double
        public float latitude;
        public float altitude;
        public float eunRotationX;
        public float eunRotationY;
        public float eunRotationZ;
        public float eunRotationW;
        public string memoType;
        public string nickname;
        public int userId;
        public int id;
        public MemoContent memo_content;
    }

    [Serializable]
    public class Wrapper
    {
        public ServerHistory[] data;
    }

    public GeospatialController geospatialController;
    // Start is called before the first frame update
    void Start()
    {
        StartCoroutine(CompareGet());
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    IEnumerator CompareGet() //url 요청 코루틴
    {
       // while (true) // 계속해서 반복되는 무한 루프
        //{
            string url = "http://34.22.102.33:8000/outside/memoInfo/";

            UnityWebRequest www = UnityWebRequest.Get(url); // get 방식으로 요청을 보냄.

            yield return www.SendWebRequest(); //응답이 올 때까지 기다림.

            if (www.error == null) //잘 도착했으면
            {
                Debug.Log(www.downloadHandler.text); //응답 다운받은 걸 text 로 볼래
                geospatialController._shouldResolvingHistory = true;
            }
            else
            {
                Debug.Log("ERROR");
            }


            string jsonString = "{\"data\":" + www.downloadHandler.text + "}"; // "data"는 임의의 키로 변경 가능
            Wrapper wrapper = JsonUtility.FromJson<Wrapper>(jsonString);
            List<ServerHistory> historyList = new List<ServerHistory>(wrapper.data);
            /*
            foreach (ServerHistory item in history)
            {
                Debug.Log($"Memo ID: {item.id}, Text: {item.memoType}");
            }
            */
            if (geospatialController._historyCollection.Collection.Count != historyList.Count)
            {
                geospatialController._historyCollection.Collection.Clear();

                foreach (ServerHistory historyItem in historyList)
                {
                    Debug.Log("Picture URL: " + historyItem.memo_content.picture);
                    GeospatialAnchorHistory clonedItem = new GeospatialAnchorHistory
                    {
                        SerializedTime = DateTime.Now.ToString(),
                        Longitude = historyItem.longitude,
                        Latitude = historyItem.latitude,
                        Altitude = historyItem.altitude,
                        EunRotation = new Quaternion(historyItem.eunRotationX, historyItem.eunRotationY, historyItem.eunRotationZ, historyItem.eunRotationW),
                        MemoType = historyItem.memoType,
                        Writer = historyItem.nickname,  //nickname을 가져옴
                        PostId = historyItem.id,
                        AnchorType = AnchorType.Geospatial,
                        Picturebyte = null,
                        Videobyte = null,

                };

                    switch (historyItem.memoType)
                    {
                        case "A":

                            clonedItem.MemoType = "Text";
                            clonedItem.Text = historyItem.memo_content.text; ;
                            clonedItem.Video = "";
                            clonedItem.Picture = "";
                            break;

                        case "B":
                            clonedItem.MemoType = "Picture";
                            clonedItem.Picture = historyItem.memo_content.picture;
                           // clonedItem.Picture = Encoding.UTF8.GetBytes(historyItem.memo_content.picture);
                            clonedItem.Text = "";
                            clonedItem.Video = "";
                            break;

                        case "D":
                            clonedItem.MemoType = "Video";
                            clonedItem.Text = "";
                            clonedItem.Video = historyItem.memo_content.video;
                            clonedItem.Picture = "";
                            break;

                        default:
                            // 예외 처리 또는 기본 동작 설정
                            break;
                    }

                    geospatialController._historyCollection.Collection.Add(clonedItem);
                }
                //geospatialController._shouldResolvingHistory = geospatialController._historyCollection.Collection.Count > 0;
                geospatialController.ResolveHistory();
        //    }
            PrintList(geospatialController._historyCollection.Collection);

           // yield return new WaitForSeconds(30f);
        }
    }
    
    void PrintList(List<GeospatialAnchorHistory> list)
    {
        foreach (var item in list)
        {
            Debug.Log($"Text: {item.Text}, Picture: {item.Picture}, Video: {item.Video}, Longitude: {item.Longitude}, Latitude: {item.Latitude}, Altitude: {item.Altitude}, EunRotation: {item.EunRotation}, MemoType: {item.MemoType}, Writer: {item.Writer}, PostId: {item.PostId}");

        }
    }

    public void resetClicked()
    {

        foreach (var anchor in geospatialController._anchorObjects)
        {
            Destroy(anchor);
        }

        geospatialController._anchorObjects.Clear();
        geospatialController. _historyCollection.Collection.Clear();
        geospatialController.SaveGeospatialAnchorHistory();
        StartCoroutine(CompareGet());

    }

}

