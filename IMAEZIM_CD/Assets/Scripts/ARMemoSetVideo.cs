using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.IO;
using System;
using UnityEngine.Networking;
using UnityEngine.Video;

public class ARMemoSetVideo : MonoBehaviour
{
    //public TextMeshPro _writer;
    public GameObject plane;
    //    public RawImage img;

    // Start is called before the first frame update
    public void ReceiveDataByte(byte[] recvVideo, string recvWriter)
    {
        ServerVideoByte(recvVideo, recvWriter);
    }

    public void ReceiveDataUrl(string recvVideo, string recvWriter)
    {
        StartCoroutine(RunFirst(recvVideo, recvWriter));
    }

    IEnumerator RunFirst(string recvVideo, string recvWriter)

    {

        yield return StartCoroutine(ServerVideoUrl(recvVideo, recvWriter));

    }

    IEnumerator ServerVideoUrl(string recvVideo, string recvWriter)
    {
        string url = "http://34.22.102.33:8000" + recvVideo;
        UnityWebRequest www = UnityWebRequest.Get(url);
        www.downloadHandler = new DownloadHandlerBuffer();
        yield return www.SendWebRequest();


        if (www.result == UnityWebRequest.Result.Success)
        {
            byte[] videoData = www.downloadHandler.data;
            string videoName = Path.GetFileName(url);
            string tempPath = Application.persistentDataPath + "/" + videoName;
            // 동일한 파일이 이미 존재하는지 확인
            if (!File.Exists(tempPath))
            {
                // 파일 저장
                File.WriteAllBytes(tempPath, videoData);
            }
            //비디오 저장된게 없으면 write

            VideoPlayer[] videoPlayers = plane.GetComponentsInChildren<VideoPlayer>();


            // 동일한 비디오 파일을 모든 비디오 플레이어에 설정하고 재생
            foreach (var videoPlayer in videoPlayers)
            {
                videoPlayer.url = tempPath;
                videoPlayer.Prepare();
                videoPlayer.Play();

                videoPlayers[0].SetDirectAudioMute(0, true);
                videoPlayers[1].SetDirectAudioMute(0, true);
            }

            setWriter(recvWriter);
        }
    }

    void ServerVideoByte(byte[] recvVideo, string recvWriter)
    {
        Debug.Log("recieveVideo 도착 시작");
        string tempFilePath = Application.persistentDataPath + "/tempVideo.mp4";
        File.WriteAllBytes(tempFilePath, recvVideo);

        VideoPlayer[] videoPlayers = plane.GetComponentsInChildren<VideoPlayer>();


        // 동일한 비디오 파일을 모든 비디오 플레이어에 설정하고 재생
        foreach (var videoPlayer in videoPlayers)
        {
            videoPlayer.source = VideoSource.VideoClip; //로컬에서 로드
            videoPlayer.url = "file://" + tempFilePath;
            videoPlayer.Prepare();
            videoPlayer.Play();

            videoPlayers[0].SetDirectAudioMute(0, true);
            videoPlayers[1].SetDirectAudioMute(0, true);
        }

        setWriter(recvWriter);
    }

    private void setWriter(string recvWriter)
    {
        TextMeshPro _writer = plane.GetComponentsInChildren<TextMeshPro>()[0];
        _writer.text = recvWriter;
    }
}