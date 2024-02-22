using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.IO;
using System;
using UnityEngine.Video;


public class Load2Viedo : MonoBehaviour
{

    //    public RawImage img;


    private void Start()
    {
        /*
        Debug.Log(Application.persistentDataPath);
        //파일이 하나라도 있으면 다시 앱을 킬 때 load 됨.
        if (File.Exists(Application.persistentDataPath + "/Video"))
        {
            byte[] fileData = File.ReadAllBytes(Application.persistentDataPath + "/Video");

        }
        */
    }

    public void OnClickVideoLoad()
    { //video 도 가능
        NativeGallery.GetVideoFromGallery((file) =>
        {
            //file 정보 불러옴
            FileInfo selected = new FileInfo(file);

            //용량 제한 50mb
            if (selected.Length > 500000000)
            {
                return;
            }

            // 파일 데이터 읽기
            byte[] fileData = File.ReadAllBytes(file);
            // string fileName = Path.GetFileName(file).Split('.')[0];
            // string savePath = Application.persistentDataPath + "/Video/";

            // 저장 경로 확인 및 생성
            /* if (!Directory.Exists(savePath))
             {
                 Directory.CreateDirectory(savePath);
             }

             // 비디오 파일 저장
             File.WriteAllBytes(savePath + fileName + ".mp4", fileData);
            */
            string base64String = Convert.ToBase64String(fileData);
           // PlayerPrefs.SetString("MemoVideo", savePath + fileName + ".mp4");  //사진 경로 geo에 저장 위해서
            PlayerPrefs.SetString("MemoVideo", base64String);
            /*
            //불러오기
            if (!string.IsNullOrEmpty(file))
            {
                StartCoroutine(LoadV(file));
            }
            */
        });

    }
    /*
    IEnumerator LoadV(string path)
    {
        yield return null;


        /*
        // 모든 비디오 플레이어 찾기
        VideoPlayer[] videoPlayers = plane.GetComponentsInChildren<VideoPlayer>();

        // 동일한 비디오 파일을 모든 비디오 플레이어에 설정하고 재생
        foreach (var videoPlayer in videoPlayers)
        {
            videoPlayer.url = savePath + fileName + ".mp4";
            videoPlayer.Prepare();
            videoPlayer.Play();


                videoPlayers[0].SetDirectAudioMute(0, true);
                videoPlayers[0].SetDirectAudioMute(0, true);

        }
        
    }
    */

}
