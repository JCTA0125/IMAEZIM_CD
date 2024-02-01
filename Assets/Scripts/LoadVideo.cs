using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.IO;
using UnityEngine.Video;


public class LoadViedo : MonoBehaviour
{
    public GameObject plane;
    //    public RawImage img;
    private VideoPlayer videoPlayer;

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
        /*
        NativeGallery.GetVideoFromGallery((file) =>
        {
            //file 정보 불러옴
            FileInfo selected = new FileInfo(file);

            //용량 제한 50mb
            if (selected.Length > 400000000)
            {
                return;
            }

            //불러오기
            if (!string.IsNullOrEmpty(file))
            {
                StartCoroutine(LoadV(file));
            }
        });
        */
    }

    /*
    IEnumerator LoadV(string path)
    {
        
        yield return null;
        //핸드폰에 저장한 후 다시 가지고 옴.
        byte[] fileData = File.ReadAllBytes(path);
        string fileName = Path.GetFileName(path).Split('.')[0];
        string savePath = Application.persistentDataPath + "/Video";

        if (!Directory.Exists(savePath))
        {
            Directory.CreateDirectory(savePath); //저장된게 없으면 경로 생성
        }

        File.WriteAllBytes(savePath + fileName + ".mp4", fileData); //무조건 png로 데이터 저장

        videoPlayer = plane.GetComponentInChildren<VideoPlayer>();
        videoPlayer.url = savePath + fileName + ".mp4";
        videoPlayer.Prepare();

        //videoPlayer.SetDirectAudioMute(0, true); //소리 mute
        videoPlayer.Play();

        //       img.texture = videoPlayer.targetTexture;
        //       cube.GetComponentInChildren<RawImage>().texture = videoPlayer.targetTexture;
        

    }
    */

}
