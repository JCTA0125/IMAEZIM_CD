using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.XR.ARFoundation;

public class AReplacementAndPlaneDetection : MonoBehaviour
{
    ARPlaneManager m_ARPlaneManager;
    ARPlacementManager m_ARPlacementManager; //배치를 위한 스크립트

    public GameObject placeButton;
    public GameObject adjustButton;
    public GameObject searchForGameButton;
    public GameObject scaleSlider;


    [Header("Floor Clear")]
    public GameObject clearButton;
    public GameObject floor;
    MeshRenderer floorRenderer;
    private bool isClear = true;

    public TextMeshProUGUI informUIPanel_Text;

    private void Awake()
    {
        m_ARPlaneManager = GetComponent<ARPlaneManager>();
        m_ARPlacementManager = GetComponent<ARPlacementManager>(); 
        floorRenderer = floor.GetComponent<MeshRenderer>();
    }

    // Start is called before the first frame update
    void Start()
    {
        placeButton.SetActive(true);
        scaleSlider.SetActive(true);
        clearButton.SetActive(true);
        adjustButton.SetActive(false);
        searchForGameButton.SetActive(false);

        informUIPanel_Text.text = "폰을 움직여 배틀 아레나의 위치를 변경해주세요!";
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void ClearButtonOnClicked()
    {
        if(isClear)
        {
            isClear = false;
        }
        else
        {
            isClear=true;
        }

        floorRenderer.enabled = isClear;

    }

    public void DisableARPlacementAndPlaneDetection()
    {
        m_ARPlaneManager.enabled = false;
        m_ARPlacementManager.enabled = false; //아레나를 움직이지 못하게 한다.
        SetAllPlaneActiveOrDeactive(false);

        scaleSlider.SetActive(false);
        clearButton.SetActive(false);

        placeButton.SetActive(false);
        adjustButton.SetActive(true);
        searchForGameButton.SetActive(true);

        informUIPanel_Text.text = "장소를 정했군요! Search for the Game 버튼을 눌러주세요! ";
    }

    public void EnableARPlacementAndPlaneDetection()
    {
        m_ARPlaneManager.enabled = true;
        m_ARPlacementManager.enabled = true;
        SetAllPlaneActiveOrDeactive(true);

        scaleSlider.SetActive(true);
        clearButton.SetActive(true);

        placeButton.SetActive(true);
        adjustButton.SetActive(false);
        searchForGameButton.SetActive(false);
    }

    private void SetAllPlaneActiveOrDeactive(bool value) //모든 plane 객체 false
    {
        foreach (var plane in m_ARPlaneManager.trackables) //감지된 plane 을 엑세스 할 수 있다. (RPlaneManager에서 추적 중인 평면들의 목록)
        {
            plane.gameObject.SetActive(value);
        }
    }
}
