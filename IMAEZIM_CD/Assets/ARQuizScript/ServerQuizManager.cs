using Google.XR.ARCoreExtensions;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Networking;
using static QuizPlacer;
using static ServerQuizManager;

public class ServerQuizManager : MonoBehaviour
{
    [Header("User")]
    //awake 함수에서 가져올 예정
    public int userId = 5;

    [Header("ForFirstSetting")]
    public List<GameObject> quizObjects = new List<GameObject>();
    private QuizList getGuizList;

    public GameObject quizPrefab;

    [Header("AnotherScript")]
    public ForQuizUI forQuizUI;
    public ARController arController;
    public QuizPlacer QuizPlacer;

    [System.Serializable]
    public class Quiz
    {
        public int quizId;
        //퀴즈위치
        public float latitude;
        public float longitude;
        public float altitude;
        //퀴즈회전
        public float eunRotationX;
        public float eunRotationY;
        public float eunRotationZ;
        public float eunRotationW;
        //퀴즈정보
        public string content;
        public string answer;
    }

    [System.Serializable]
    public class QuizList
    {
        public Quiz[] quizzes;
    }

    [System.Serializable]
    public class CorrectQuizList
    {
        public int[] quizIds;
    }

    void Start()
    {
        StartCoroutine(WaitingGetQuizInfo());
    }



    #region GETQUIZINFO

    IEnumerator WaitingGetQuizInfo()
    {
        Debug.Log("GetQuizInfo Start");
        yield return StartCoroutine(GetQuizInfo());
        Debug.Log("GetQuizInfo Done");


        if (getGuizList == null)
        {
            Debug.Log("Making Anchorcant Start");
            yield break;
        }
        Debug.Log("MakingQuizzesAnchor Start");
        yield return StartCoroutine(arController.MakingQuizzesAnchor(getGuizList.quizzes));

        Debug.Log("GetCorrectCount Start");
        StartCoroutine(GetCorrectCount());
        Debug.Log("GetCorrectCount Done");
    }

    IEnumerator GetQuizInfo()
    {
        string url = "http://34.22.102.33:8000/quiz/quiz_api/";
        UnityWebRequest www = UnityWebRequest.Get(url);
        yield return www.SendWebRequest(); //올 때까지 기다림.

        if (www.error == null) //잘 도착했으면
        {
            string jsonResponse = www.downloadHandler.text;
            string wrappedJsonResponse = "{\"quizzes\":" + jsonResponse + "}";
            QuizList quizList = JsonUtility.FromJson<QuizList>(wrappedJsonResponse);
            if(quizList != null && quizList.quizzes.Length > 0)
            {
                getGuizList = quizList;
                InstatiateQuiz(quizList.quizzes);
                Debug.Log("GetQuizInfo - InstatiateQuiz Start");
            }
            else
            {
                Debug.Log("GetQuizInfo - InstatiateQuiz Can't Start");
            }

        }
        else
        {
            Debug.Log("Quiz Get ERROR");
        }
    }
    IEnumerator GetCorrectCount()
    {
        string url = $"http://34.22.102.33:8000/quiz/correct_quiz_api/{userId}/";
        UnityWebRequest www = UnityWebRequest.Get(url);
        yield return www.SendWebRequest();

        if (www.error == null)
        {
            string jsonResponse = www.downloadHandler.text;
            CorrectQuizList correctQuizList = JsonUtility.FromJson<CorrectQuizList>(jsonResponse);
            if(correctQuizList != null)
            {
                forQuizUI.correctCount = correctQuizList.quizIds.Count();
                forQuizUI.UpdateCount();
                ChangingQuizStatus(correctQuizList.quizIds);
            }
        }
    }

    #endregion

    #region DELETEQUIZ

    public IEnumerator deleteQuiz(GameObject delQuiz)
    {
        if (delQuiz != null)
        {

            //가장 부모를 찾는 일
            Debug.Log("delQuiz name is" + delQuiz.name);
            Transform currentParent = delQuiz.transform;
            while (currentParent.parent != null)
            {
                if (currentParent.name.Contains("YourQuiz"))
                {
                    // "YourQuiz"를 포함하는 이름을 찾은 경우 종료
                    break;
                }
                currentParent = currentParent.parent;
            }
            Transform quizObject = currentParent;
            //앵커를 찾아서 없애야 함
            if(quizObject == null) {
                Debug.Log("we don't have quizobject in deleteQuiz");
                yield break;
            }
            QuizMovement quizMovement = quizObject.GetComponent<QuizMovement>();

            if (quizMovement != null)
            {
                string url = $"http://34.22.102.33:8000/quiz/quiz_api/{quizMovement.quizId}/";
                UnityWebRequest www = UnityWebRequest.Delete(url);
                yield return www.SendWebRequest();

                if (www.error == null) //잘 도착했으면
                {
                    if (arController.allAnchors.Contains(currentParent.gameObject))
                    {
                        arController.allAnchors.Remove(currentParent.gameObject);
                    }
                    Destroy(currentParent.gameObject);
                    Debug.Log("Delete Completed");

                }
                else
                {

                    Debug.Log("Delete Error :" + www.error);
                }
            }



        }
        else
        {
            Debug.Log("delete Quiz method dont have delquiz");
      
        }
       
    }

    #endregion

    #region POST

    public IEnumerator PostQuiz()
    {

        string url = "http://34.22.102.33:8000/quiz/quiz_api/";
        WWWForm form = new WWWForm(); //데이터를 form 형태로 보낸다. 

        //데이터를 넣는다. 
        ServerQuizManager.Quiz newQuiz = QuizPlacer.lastnewQuizInfo;
        //위치데이터
        form.AddField("longitude", newQuiz.longitude.ToString());
        form.AddField("latitude", newQuiz.latitude.ToString());
        form.AddField("altitude", newQuiz.altitude.ToString());

        //회전각도
        form.AddField("eunRotationX", newQuiz.eunRotationX.ToString());
        form.AddField("eunRotationY", newQuiz.eunRotationY.ToString());
        form.AddField("eunRotationZ", newQuiz.eunRotationZ.ToString());
        form.AddField("eunRotationW", newQuiz.eunRotationW.ToString());

        //퀴즈내용
        form.AddField("content", newQuiz.content);
        form.AddField("answer", newQuiz.answer);

        UnityWebRequest www = UnityWebRequest.Post(url, form);
        yield return www.SendWebRequest();

        if (www.error == null)
        {
            //post 가 제대로 되었다면 quizid 를 저장
            string jsonResponse = www.downloadHandler.text;

            if (jsonResponse != null)
            {
                PostResponse response = JsonUtility.FromJson<PostResponse>(jsonResponse);
                if (response.pk == 0)
                {
                    Debug.Log("Post Failed");
                    yield return null;
                }
                QuizMovement quizMovement = QuizPlacer.newQuiz.GetComponent<QuizMovement>();
                if (quizMovement != null)
                {
                    quizMovement.quizId = response.pk;
                    Debug.Log(response.message + "quizid is" + response.pk);
                }
            }
        }
        else
        {
            Debug.Log("postQuiz error");
        }
    }

    public IEnumerator postSolvedQues(int quizId)
    {
        string url = "http://34.22.102.33:8000/quiz/correct_quiz_api/";
        WWWForm form = new WWWForm();

        form.AddField("quizId", quizId.ToString());
        form.AddField("userId", userId.ToString());

        UnityWebRequest www = UnityWebRequest.Post(url, form);
        yield return www.SendWebRequest();

        if(www.error == null)
        {
            Debug.Log("postSolvedQues is success!");
        }
        else
        {
            Debug.Log("postSolvedQues is not success, check it out");
        }
    }
    #endregion

    #region USEINFO

    private void InstatiateQuiz(Quiz[] quizzes)
    {
        foreach (Quiz quiz in quizzes)
        {
            try
            {
                //회전값 합치기
                Quaternion rotation = new Quaternion(quiz.eunRotationX, quiz.eunRotationY, quiz.eunRotationZ, quiz.eunRotationW);

                //위치 합치기
                Vector3 position = Vector3.zero;
                GameObject newQuiz = Instantiate(quizPrefab, position, rotation);
                newQuiz.SetActive(false);

                //content, answer, quizid 부여
                QuizMovement quizMovement = newQuiz.GetComponent<QuizMovement>();
                if (quizMovement != null)
                {
                    quizMovement.quizId = quiz.quizId;
                }

                //ques 등록
                Transform child = newQuiz.transform.Find("QuizObject");
                Transform childQues = child.transform.Find("Ques");

                if (child != null)
                {
                    TextMeshProUGUI quesContent = childQues.GetComponentInChildren<TextMeshProUGUI>();
                    if (quesContent != null)
                    {
                        quesContent.text = quiz.content;
                    }
                }
                else
                {
                    Debug.Log("You dont have a child Quiz");
                }

                //answer 등록
                quizMovement.realAnswer = quiz.answer;

                quizObjects.Add(newQuiz);
            }
            catch (Exception ex)
            {
                Debug.LogError("An error occurred while instantiating a quiz: " + ex.Message);
            }
        }
    }

    private void ChangingQuizStatus(int[] quizIds)
    {
        if (quizObjects.Count > 0)
        {
            foreach (GameObject quiz in quizObjects)
            {

                QuizMovement quizMovement = quiz.GetComponent<QuizMovement>();
                if (quizMovement != null && System.Array.Exists(quizIds, id => id == quizMovement.quizId))//람다식 quiz id 가 있으면
                {
                    Transform childCheck = quiz.transform.Find("CheckMark");
                    if (childCheck != null)
                    {
                        ForFloating forFloating = childCheck.GetComponentInChildren<ForFloating>();
                        if (forFloating != null)
                        {
                            forFloating.enabled = false;
                            childCheck.gameObject.SetActive(true);
                            StartCoroutine(FloatingEnabledCoroutine(forFloating));
                        }
                    }
                    else
                    {
                        Debug.Log("found not Quiz checkmark- ChangingQuizStatus");
                    }
                }
                else
                {
                    Transform present = quiz.transform.Find("Present");
                    if (present != null)
                    {
                        ForFloating forFloating = present.GetComponentInChildren<ForFloating>();
                        if (forFloating != null)
                        {
                            forFloating.enabled = false;
                            present.gameObject.SetActive(true);
                            StartCoroutine(FloatingEnabledCoroutine(forFloating));
                        }
                    }
                    else
                    {
                        Debug.Log("found not Quiz Present- ChangingQuizStatus");
                    }
                }
            }
        }

    }

    IEnumerator FloatingEnabledCoroutine(ForFloating forFloating)
    {
        yield return new WaitForSeconds(3f); // 3초 대기

        // 3초 후에 enabled 속성을 반대로 변경
        //forFloating.enabled = !forFloating.enabled;
        forFloating.enabled = true;
        Debug.Log("FloatingEnabledCoroutine" + forFloating.enabled);
    }
    #endregion

}
