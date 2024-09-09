using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.IO;
using System;
using UnityEngine.Video;
using Google.XR.ARCoreExtensions.Samples.Geospatial;
public class Load2Viedo : MonoBehaviour
{

    public GeospatialController controllerScript;



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
        });
        controllerScript.OnApplicationFocus();
    }


}
