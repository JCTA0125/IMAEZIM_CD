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
        ServerVideoUrl(recvVideo, recvWriter);
    }

    void ServerVideoUrl(string recvVideo, string recvWriter)
    {

        string url = "http://34.64.197.160:8000" + recvVideo;

        VideoPlayer[] videoPlayers = plane.GetComponentsInChildren<VideoPlayer>();


        // 동일한 비디오 파일을 모든 비디오 플레이어에 설정하고 재생
        foreach (var videoPlayer in videoPlayers)
        {
            videoPlayer.source = VideoSource.Url;
            videoPlayer.url = url;
            videoPlayer.Prepare();
            videoPlayer.Play();

            videoPlayers[0].SetDirectAudioMute(0, true);
            videoPlayers[1].SetDirectAudioMute(0, true);
        }

        setWriter(recvWriter);
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
