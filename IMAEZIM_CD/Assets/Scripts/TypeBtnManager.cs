using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class TypeBtnManager : MonoBehaviour
{
    public GameObject TextPanel;
    public GameObject imagePanel;
    public GameObject VideoPanel;
    public GameObject TypePanel;
    public Button TypeBtn;


    public void BtnType()
    {

        TypePanel.SetActive(true);
    }

    public void BtnText()  
    {
        PlayerPrefs.SetString("MemoType", "Text");
        TypePanel.SetActive(false);
        TextPanel.SetActive(true);
        //SceneManager.LoadScene(sceneName, LoadSceneMode.Single);
    }

    public void BtnImage()
    {
        PlayerPrefs.SetString("MemoType", "Picture"); //geo를 위한 모드 저장
        TypePanel.SetActive(false);
        imagePanel.SetActive(true);
    }

    public void BtnVideo()
    {
        PlayerPrefs.SetString("MemoType", "Video");
        TypePanel.SetActive(false);
        VideoPanel.SetActive(true);

    }
    public void BtnImageBack()
    {
        imagePanel.SetActive(false);

    }

    public void BtnVideoBack()
    {
        VideoPanel.SetActive(false);

    }

    public void BtnTextBack()
    {
        TextPanel.SetActive(false);
    }


}
