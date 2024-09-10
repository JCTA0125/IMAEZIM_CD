using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.IO;
using System;
using UnityEngine.Networking;

public class ARMemoSetPicture : MonoBehaviour
{
    //public TextMeshPro _text;
    //public TextMeshPro _writer;
    //public RawImage img; 

    public GameObject cube; // cube 오브젝트에 접근하기 위해 필요한 레퍼런스
    public void ReceiveDataByte(byte[] recvPicture, string recvWriter)
    {
        ServerPictureByte(recvPicture, recvWriter);
    }

    public void ReceiveDataUrl(string recvPicture, string recvWriter)
    {
        StartCoroutine(RunFirst(recvPicture, recvWriter));
    }

    IEnumerator RunFirst(string recvPicture, string recvWriter)

    {

        yield return StartCoroutine(ServerPictureUrl(recvPicture, recvWriter));

    }



    private IEnumerator ServerPictureUrl(string url, string recvWriter)
    {
      //  Debug.Log("picture url is " + url);
        UnityWebRequest www = UnityWebRequestTexture.GetTexture("http://34.22.102.33:8000" + url);
        yield return www.SendWebRequest();

        if (www.result == UnityWebRequest.Result.Success)
        {
           
            // 이미지 다운로드 성공
            Texture2D texture = DownloadHandlerTexture.GetContent(www);
            RawImage[] rawImages = cube.GetComponentsInChildren<RawImage>();

            // 2 raw image에 다운로드한 텍스처를 적용
            foreach (RawImage rawImage in rawImages)
            {
                rawImage.texture = texture;
            }

            // 작가 설정
            setWriter(recvWriter);
        }
        else
        {
            Debug.Log("Failed to download image: " + www.error);
        }
    }
    private void ServerPictureByte(byte[] pictureByte, string recvWriter)
    {

        Texture2D tex = new Texture2D(0, 0);
        tex.LoadImage(pictureByte);
        RawImage[] rawImages = cube.GetComponentsInChildren<RawImage>();

            // 2 raw image에 다운로드한 텍스처를 적용
            foreach (RawImage rawImage in rawImages)
            {
                rawImage.texture = tex;
            }

            // 작가 설정
            setWriter(recvWriter);

    }

    private void setWriter(string recvWriter)
    {
        TextMeshPro _writer = cube.GetComponentsInChildren<TextMeshPro>()[0];
        _writer.text = recvWriter;
    }

}