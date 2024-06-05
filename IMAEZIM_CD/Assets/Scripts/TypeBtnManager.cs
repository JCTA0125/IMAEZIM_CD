using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;
using UnityEngine.XR.ARFoundation;
using Google.XR.ARCoreExtensions.Samples.Geospatial;

public class TypeBtnManager : MonoBehaviour
{
    public GameObject TextPanel;
    public GameObject imagePanel;
    public GameObject VideoPanel;
    public GameObject TypePanel;
    public Button TypeBtn;
    public GameObject endBtn;
    public GeospatialController geospatialController;

    //public ARRaycastManager RaycastManager;


    public void BtnType()
    {

        TypePanel.SetActive(true);
    }

    public void BtnText()  
    {
        PlayerPrefs.SetString("MemoType", "Text");
        TypePanel.SetActive(false);
        TextPanel.SetActive(true);
        geospatialController.isRay = true;
        endBtn.SetActive(true);
        //SceneManager.LoadScene(sceneName, LoadSceneMode.Single);
    }

    public void BtnImage()
    {
        PlayerPrefs.SetString("MemoType", "Picture"); //geo를 위한 모드 저장
        TypePanel.SetActive(false);
        imagePanel.SetActive(true);
        //RaycastManager.enabled = true;
        geospatialController.isRay = true;
        endBtn.SetActive(true);
    }

    public void BtnVideo()
    {
        PlayerPrefs.SetString("MemoType", "Video");
        TypePanel.SetActive(false);
        VideoPanel.SetActive(true);
        //RaycastManager.enabled = true;
        geospatialController.isRay = true;
        endBtn.SetActive(true);


    }
    public void BtnImageBack()
    {
        imagePanel.SetActive(false);

    }

    public void BtnVideoBack()
    {
        VideoPanel.SetActive(false);

    }


    public void endBtnClicked()
    {
        geospatialController.isRay = false;
        endBtn.SetActive(false );

    }

    public TMP_InputField inputText;
    public void BtnTextBack()
    {
        PlayerPrefs.SetString("MemoText", inputText.text);
        TextPanel.SetActive(false);
    }


}
