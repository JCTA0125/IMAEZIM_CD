using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;
using TMPro;

public class api : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        StartCoroutine(MakeRequest());
    }

    // Update is called once per frame
    void Update()
    {

    }

    IEnumerator MakeRequest()
    {
        string url = "https://apis.openapi.sk.com/tmap/routes/pedestrian";

        // 파라미터 설정
        WWWForm form = new WWWForm();
        form.AddField("startX", "127.101394");
        form.AddField("startY", "37.503147");
        //form.AddField("startX", "126.92365493654832");
        //form.AddField("startY", "37.556770374096615");
        form.AddField("speed", "4");
        form.AddField("endX", "127.099765");
        form.AddField("endY", "37.502168");
        //form.AddField("endX", "126.92432158129688");
        //form.AddField("endY", "37.55279861528311");
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
                Debug.LogError("Network error: " + www.responseCode); 

            }
            else
            {
                // 응답을 JSON 문자열로 출력
                string jsonResponse = www.downloadHandler.text;
                Debug.Log(jsonResponse);

                // 텍스트 필드에 JSON 결과 표시
                //responseText.text = jsonResponse;
            }
        }
    }
}
