using Google.XR.ARCoreExtensions;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using Unity.XR.CoreUtils;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UIElements;
using UnityEngine.XR.ARFoundation;

//quiz 의 GPS 전 1차배치
public class QuizPlacer : MonoBehaviour
{
    public GameObject quizPrefab;
    public GameObject player;

    public ForQuizUI forQuizUI;

    [Header("PlACEUI")]
    public GameObject adjustBtn;
    public GameObject placeBtn;
    public GameObject checkBtn;
    public GameObject cancelBtn;
    public GameObject replaceBtn;
    public bool btnClicked = false;

    [Header("GEOTRACKING")]
    public ARPlaneManager ARPlaneManager;
    public ARPlacementManagers ARPlacementManager;
    public RaycastForAnswer RaycastForAnswer;
    public XROrigin xrOrigin;

    [Header("ForPostInfo")]
    public ARController ARController;
    Dictionary<string, object> newQuizDict;
    public GameObject newQuiz;
    public ServerQuizManager.Quiz lastnewQuizInfo;
    public ServerQuizManager ServerQuizManager;

    //ForParsing
    [System.Serializable]
    public class PostResponse
    {
        public string message;
        public int pk = 0;
    }

    #region ForPlaceQuiz

    public void PlaceQuiz()
    {

        Vector3 playerLocation = xrOrigin.Camera.transform.position;

        //quiz는 player 앞 1 
        Vector3 quizLocation = new Vector3(playerLocation.x, playerLocation.y, playerLocation.z + 1);

        //Vector3 direction = quizLocation - playerLocation; //마주봐야하니까 quiz - 벡터가 되야한다. 
        // 플레이어를 바라보는 회전 설정, 매개변수는 방향벡터가 들어가야 한다.

        Vector3 direction;
        if (quizLocation.z > playerLocation.z)
        {
            // 퀴즈가 플레이어 앞에 있을 때
            direction = quizLocation - playerLocation;
        }
        else
        {
            // 퀴즈가 플레이어 뒤에 있을 때
            direction = playerLocation - quizLocation;
        }

        Quaternion quizRotation = Quaternion.LookRotation(direction);

        newQuiz = Instantiate(quizPrefab, quizLocation, quizRotation);
        newQuiz.SetActive(false);

        //quiz 의 ques와 answer 입력
        newQuizDict = forQuizUI.dict;

        if (newQuizDict != null)
        {
            QuizMovement quizMovement = newQuiz.GetComponent<QuizMovement>();
            object answer = newQuizDict["answer"];
            object content = newQuizDict["content"];

            //ques 등록
            Transform child = newQuiz.transform.Find("QuizObject");
            Transform childQues = child.transform.Find("Ques");

            if (child != null)
            {
                TextMeshProUGUI quesContent = childQues.GetComponentInChildren<TextMeshProUGUI>();
                if (quesContent != null)
                {
                    quesContent.text = content.ToString();
                }
            }
            else
            {
                Debug.Log("You dont have a child Quiz");
            }
            //answer 등록
            if (answer != null)
            {
                quizMovement.realAnswer = answer.ToString();
            }
            newQuiz.SetActive(true);
        

            SettingQuizInfo(content, answer, quizRotation);
            AblePlaneDetection();
        }
        else
        {
            Debug.Log("You dont have any dict");
        }


    }

    private void SettingQuizInfo(object content, object answer, Quaternion quizRotation)
    {
        ServerQuizManager.Quiz newQuizInfo = new ServerQuizManager.Quiz();
        newQuizInfo.content = content.ToString() ;
        newQuizInfo.answer = answer.ToString() ;

        newQuizInfo.eunRotationX = quizRotation.x;
        newQuizInfo.eunRotationY = quizRotation.y;
        newQuizInfo.eunRotationZ = quizRotation.z;
        newQuizInfo.eunRotationW = quizRotation.w;

        lastnewQuizInfo = newQuizInfo;

    }
    #endregion


    #region ForARUI

    public void WhereistheInstance()
    {
        if(!btnClicked)
        {
            ARPlaneManager.enabled = false;
            ARPlacementManager.enabled = false;

            Vector3 playerLocation = xrOrigin.Camera.transform.position;

            //quiz는 player 앞 1 
            Vector3 quizLocation = new Vector3(playerLocation.x, playerLocation.y, playerLocation.z +4 );
            newQuiz.transform.position = quizLocation;
            btnClicked = true;
        }
        else
        {
            ARPlaneManager.enabled = true;
            ARPlacementManager.enabled = true;
            btnClicked = false;
        }

    }
    public void AblePlaneDetection()
    {
        ARPlaneManager.enabled = true;
        ARPlacementManager.enabled = true;
        RaycastForAnswer.enabled = false;
        SetAllPlaneActiveOrDeactive(true);
        placeBtn.SetActive(true);
        cancelBtn.SetActive(true);
        replaceBtn.SetActive(true);
        forQuizUI.AllUIDisappeared();
    }

    public void DisablePlaneDetection()
    {
        ARPlaneManager.enabled = false;
        ARPlacementManager.enabled = false;
        SetAllPlaneActiveOrDeactive(false);
        placeBtn.SetActive(false);
        adjustBtn.SetActive(true);
        checkBtn.SetActive(true);
        replaceBtn.SetActive(false);
    }

    public void IsCheckBtnClicked()
    {
        ARPlacementManager.ActivePresent();
        StartCoroutine(ARController.WaitingQuizAnchor(newQuiz));
        RaycastForAnswer.enabled=true;
        cancelBtn.SetActive(false);
        checkBtn.SetActive(false);
        adjustBtn.SetActive(false);
        forQuizUI.AllUIAppeared();
        //GPS 정보를 얻어와야 함
    }

    public void CancleBtnClicked()
    {
        DisablePlaneDetection();
        forQuizUI.AllUIAppeared();
        forQuizUI.PanelReset();

    }
    #endregion

    #region FORPLANEMANGERPREFAB
    private void SetAllPlaneActiveOrDeactive(bool value) //모든 plane 객체 false
    {
        if(ARPlaneManager.trackables.count > 0)
        {
            foreach (var plane in ARPlaneManager.trackables) //감지된 plane 을 엑세스 할 수 있다. (RPlaneManager에서 추적 중인 평면들의 목록)
            {
                plane.gameObject.SetActive(value);
            }
        }
    }
    #endregion
}