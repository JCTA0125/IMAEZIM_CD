using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class RaycastForAnswer : MonoBehaviour
{
    public bool isProcessing = false;

    [Header("ForDelete")]

    //deleteMode 변수 참조
    public ForQuizUI forQuizUI;
    //quiz delete를 위한 gameobject 저장
    public GameObject finalDelQuiz;


    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        if (!isProcessing)
        {
            if (Input.GetMouseButtonDown(0))
            {
                Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
                RaycastHit hit;
                if (Physics.Raycast(ray, out hit))
                {
                    string name = hit.transform.gameObject.name;

                    //먼저 delete 모드인지 확인
                    if (forQuizUI != null)
                    {
                        if (forQuizUI.deleteMode)
                        {
                            finalDelQuiz = hit.transform.gameObject;
                            forQuizUI.DeleteObjectClicked();
                            forQuizUI.deleteMode = false; //delete Object 감지 모드 종료
                            return;
                        }
                    }
                    if (name == "Present")
                    {
                        Transform parentHit = hit.transform.parent;
                        Transform quizTransform = parentHit.Find("QuizObject");
                        if (quizTransform != null)
                        {
                            quizTransform.gameObject.SetActive(true);
                            hit.transform.gameObject.SetActive(false);
                            QuizMovement quizMovement = parentHit.gameObject.GetComponent<QuizMovement>();
                            quizMovement.StartCount();



                        }
                    }
                    if (name == "One" || name == "Two" || name == "Three" || name == "Four")
                    {

                        Transform parentTransform = hit.transform.parent.parent.parent;
                        QuizMovement quizMovement = parentTransform.GetComponent<QuizMovement>();

                        if (quizMovement != null)
                        {

                            quizMovement.choosingNumber = hit.transform.gameObject;
                            quizMovement.isClicked = true;

                        }
                        else
                        {
                            Debug.Log("We don't have a quizMovement");
                        }
                    }


                }
            }
        }

    }

}
