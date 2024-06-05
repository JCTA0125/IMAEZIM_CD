using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;

public class QuizMovement : MonoBehaviour
{

    [Header("Time")]
    public TextMeshProUGUI timeText;
    public TextMeshPro answerText;

    [Header("Answer")]
    public GameObject choosingNumber;
    [SerializeField] public string realAnswer;
    public bool isClicked = false;
    private bool IsOutOFTime = false;

    [Header("QuizObject")]
    public GameObject quizObject;
    public GameObject panelObject;
    public GameObject presentObject;
    public GameObject checkObject;

    [Header("SERVER")]
    public int quizId;

    [Header("AnotherScripts")]
    //답을 복수로 선택하는 경우를 방지하기 위해서 사용한다.
    RaycastForAnswer raycastForAnswer;
    ForQuizUI forQuizUI;
    ServerQuizManager serverQuizManager;

    private Coroutine countCoroutine;

    private void Awake()
    {
        raycastForAnswer = FindObjectOfType<RaycastForAnswer>();
        forQuizUI = FindObjectOfType<ForQuizUI>();
        serverQuizManager = FindAnyObjectByType<ServerQuizManager>();
    }

    void Update()
    {
        //isClicked 버튼이 눌렸는 지 확인하는 변수
        if (isClicked && !IsOutOFTime)
        {
            raycastForAnswer.isProcessing = true;
            isClicked = false;
            TheAnswerIs();

        }
    }




    #region Time

    IEnumerator CountDown()
    {
        Debug.Log("StartCountDown");
        float LeftTime = 15.0f;
        int displayTime = Mathf.FloorToInt(LeftTime); // 소수점 이하를 버림하여 정수로 변환
        timeText.text = displayTime.ToString();

        while (LeftTime >= 0)
        {
            LeftTime -= Time.deltaTime;
            int nextDisplayTime = Mathf.FloorToInt(LeftTime);

            if (nextDisplayTime != displayTime)
            {
                displayTime = nextDisplayTime;
                timeText.text = displayTime.ToString();
            }

            yield return null; // 다음 프레임까지 대기
        }

        timeText.text = "TimeOver";
        IsOutOFTime = true;
        if (!panelObject.activeSelf) //panel 이 setActive(false) 일 때 실행
        {
            StartCoroutine(DelayAndfalse(2.0f, false));
        }

    }

    public void StartCount() {
        countCoroutine = StartCoroutine(CountDown());
    }
    #endregion

    #region TheAnswer

    //Answer 를 표시하고, 다시할 것인지를 물음

    private void TheAnswerIs()
    {
        if (countCoroutine != null)
        {
            StopCoroutine(countCoroutine);
        }
        Color color = Color.blue;
        bool result = false;
        if(choosingNumber.transform.name != realAnswer)
        {
            answerText.text = "X";
            color = Color.red;
        }
        else
        {
            answerText.text = "O";
            result = true;
        }
        answerText.gameObject.SetActive(true);
        MeshRenderer objectColor = choosingNumber.gameObject.GetComponent<MeshRenderer>();
        objectColor.material.color = color;

        if (result)
        {
            //맞은 문제 count & server update
            forQuizUI.correctCount += 1;
            forQuizUI.UpdateCount();
            StartCoroutine(serverQuizManager.postSolvedQues(quizId));
            StartCoroutine(WaitingDelayAndfalse(1.0f, result)); //사용자가 답을 확인할 때까지 기다린다. 

            //stamp 부여자격을 확인한다.
            StartCoroutine(forQuizUI.GetStamp());

        }
        else
        {
            StartCoroutine(DelayAndfalse(2.0f, result));
        }

        
    }

    //Wrapping DelayAndfalse

    IEnumerator WaitingDelayAndfalse(float time, bool result)
    {
        yield return StartCoroutine(DelayAndfalse(time, result));
    }

    //사용자가 맞았는 지 확인할 동안 유지
    IEnumerator DelayAndfalse(float time, bool result)
    {
        yield return new WaitForSeconds(time);

        //quiz obejct 사라짐.
        quizObject.SetActive(false);

        if (result)
        {
            checkObject.SetActive(true);
        }
        else
        {
            panelObject.SetActive(true);
        }
        raycastForAnswer.isProcessing = false;

    }

    public void yesBtnClicked()
    {

        Reset();
        quizObject.SetActive(true);
        StartCount();

    }

    public void noBtnClicked()
    {
        Reset();
        quizObject.SetActive(false);
        presentObject.SetActive(true);

    }
    #endregion

    #region QuizReset

    
    //quizObject 초기화
    public void Reset()
    {

        timeText.text = "Timer";
        IsOutOFTime = false;
        answerText.gameObject.SetActive(false);
        panelObject.SetActive(false);
        if(choosingNumber == null)
        {
            return;
        }
        choosingNumber.GetComponent<MeshRenderer>().material.color = Color.white;
    }
    #endregion
}
