using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using Unity.XR.CoreUtils;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.Windows;
using Button = UnityEngine.UI.Button;

public class ForQuizUI : MonoBehaviour
{
    [Header("CreateBtnClick")]
    public bool createMode = false;
    public GameObject creatingPanel;

    [Header("ForAnswerBtn")]
    public Button oneBtn;
    public Button twoBtn;
    public Button threeBtn;
    public Button fourBtn;
    public Button lastChoice;
    public Sprite afterClickSprite;
    public Sprite beforeClickSprite;

    public TMP_InputField quizInput;
    public TMP_InputField[] inputArray;

    [Header("ForARPlace")]
    public Canvas userCanvas;


    public Dictionary<string, object> dict;

    public QuizPlacer quizPlacer;

    [Header("ForDeleteBtn")]
    public bool deleteMode = false;
    public GameObject deleteConfirmPanel;
    public ServerQuizManager serverQuizManager;
    public RaycastForAnswer raycastForAnswer;
    public GameObject createBtn;

    [Header("ForSocre")]
    public TextMeshProUGUI scoreText;
    public int rewardCount = 1;
    public int correctCount = 0;

    [Header("ForStamp")]
    public GameObject stampPanel;
    public RawImage stampRawImage;
    public RawImage stampListRawImageForGB;
    //경복궁stampimage
    public Texture2D notCompletedImage;
    public Texture2D CompletedImage;
    public GameObject doneBtn;
    //stampList
    public GameObject stampListPanel;

    [Header("ForObjectRot")]
    public ARController ARController;
    public XROrigin xrOrigin;

    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        
    }

    #region CREATEMODE

    public void CreateBtnClicked()
    {
         createMode = true;
         creatingPanel.SetActive(true);
    }

    public void CancelBtnClicked()
    {
        createMode = false;

        //panel 원상복귀
        PanelReset();

        creatingPanel.SetActive(false);

    }

    //Create모드에 있는 delete 버튼
    public void DeleteBtnClicked()
    {
        deleteMode = true;
        creatingPanel.SetActive(false);
        createBtn.SetActive(false);
    }

    //CreatePanelBtn

    public void OneBtnClicked()
    {
        ChangingAnswer(oneBtn);
    }

    public void TwoBtnClicked()
    {
        ChangingAnswer(twoBtn);
    }

    public void ThreeBtnClicked()
    {
        ChangingAnswer(threeBtn);
    }

    public void FourBtnClicked()
    {
        ChangingAnswer(fourBtn);
    }

    private void ChangingAnswer(Button choice)
    {
        var buttonImage = choice.GetComponent<Image>();


        //text 색도 변경
        var buttonText = choice.GetComponentInChildren<TextMeshProUGUI> ();

        //선택을 한 적이 있다면 이전 선택을 취소
        if (lastChoice != null)
        {
            var lastImage = lastChoice.GetComponent<Image>();
            var lastText = lastChoice.GetComponentInChildren<TextMeshProUGUI>();  
            if (lastImage != null || lastText != null)
            {
                //lastImage.color = Color.white;
                //color 대신 sprite
                lastImage.sprite = beforeClickSprite;
                lastText.color = Color.black;
            }
        }


        if (buttonImage != null)
        {
            //buttonImage.color = Color.blue;
            buttonImage.sprite = afterClickSprite;
            buttonText.color = Color.white;
            lastChoice = choice;

        }
    }

    //OK 버튼을 누른 후 퀴즈의 배치가 가능할 수 있게 딕셔너리에 담아놓는다. 
    public void OKBtnClicked()
    {
        if (!string.IsNullOrEmpty(quizInput.text))
        {
            string content = quizInput.text +"\n";
            Debug.Log(content);
            int count = 1;
            foreach (TMP_InputField input in inputArray)
            {
                if (!string.IsNullOrEmpty(input.text))
                {
                    content += count + ": " + input.text + "\n";
                    count++;
                }
                else
                {
                    StartCoroutine(ChangeInputColor(input, 1));
                    return;
                }
            }



            if (lastChoice != null)
            {

                //딕셔너리에 저장해서 다른 스크립트에서 사용하기 편리하도록 함
                dict = new Dictionary<string, object>();
                dict["content"] = content;
                dict["answer"] = lastChoice.name;
            }
            else
            {
                Debug.Log("I can't find the Answer");
            }
        }
        else
        {
            StartCoroutine(ChangeInputColor(quizInput, 1));
            return;
        }
        




        Debug.Log(dict["content"]);

        //panel 원상복귀
        PanelReset();
        creatingPanel.SetActive(false);
        quizPlacer.PlaceQuiz();

        //배치 함수 부르기
        quizPlacer.AblePlaneDetection();


    }

    public IEnumerator ChangeInputColor(TMP_InputField input, float duration)
    {
        Color  originalColor = input.image.color;
        input.image.color = Color.red;
        yield return new WaitForSeconds(duration);

        input.image.color = originalColor;
    }

    public void PanelReset()
    {
        if (lastChoice != null)
        {
            var lastImage = lastChoice.GetComponent<Image>();
            var lastText = lastChoice.GetComponentInChildren<TextMeshProUGUI>();
            if (lastImage != null || lastText != null)
            {
                //lastImage.color = Color.white;
                lastImage.sprite = beforeClickSprite;
                lastText.color = Color.black;
            }

            quizInput.text = "";


            int count = 1;
            foreach (TMP_InputField input in inputArray)
            {
                input.text = "";
                count++;
            }
        }
    }


    #endregion

    #region FORDELETEPROCESS

 //delete panel 이 시작되고, 여기서 ServerDelete 함수를 호출한다.
    public void DeleteYesBtn()
    {
        GameObject lastChoiceQuiz = raycastForAnswer.finalDelQuiz;
        if (lastChoiceQuiz != null)
        {
            StartCoroutine(serverQuizManager.deleteQuiz(lastChoiceQuiz));
            deleteConfirmPanel.SetActive(false);
        }
        else 
        {
            Debug.Log("Can't find a lastChoiceDeletingQuiz");
        }
        createBtn.SetActive(true);

    }

    public void DeleteNoBtn()
    {
        deleteConfirmPanel.SetActive(false);
        createBtn.SetActive(true);
    }


    public void DeleteObjectClicked()
    {
        deleteConfirmPanel.SetActive(true);
    }

    #endregion

    #region FORCOUNT

    public void UpdateCount()
    {
        scoreText.text = $"{correctCount}/{rewardCount}";
    }


    #endregion

    #region FORSTAMP

    //stamp 자격이 되는 지 확인하는 함수
    public IEnumerator GetStamp()
    {
        if (correctCount == rewardCount)
        {
            stampRawImage.texture = notCompletedImage;
            stampPanel.SetActive(true);
            AudioSource audio = stampPanel.GetComponent<AudioSource>();
            audio.Play();
            yield return new WaitForSeconds(2);
            stampRawImage.texture = CompletedImage;

            yield return new WaitForSeconds(2);
            doneBtn.SetActive(true);

        }
    }

    public void DoneBtnclicked()
    {
        stampPanel.SetActive(false);
        doneBtn.SetActive(false);

    }

    public void StampBtnclicked()
    {
        if (stampListPanel.activeSelf)
        {
            stampListPanel.SetActive(false);    
        }
        else
        {
            //경복궁(나중에는 변수를 리스트에 담아놓고 for문 돌리자.)
            if (correctCount >= rewardCount)
            {
                stampListRawImageForGB.texture = CompletedImage;
            }
            else
            {
                stampListRawImageForGB.texture = notCompletedImage;
            }

            stampListPanel.SetActive(true);
        }

    }
    #endregion

    #region FORPLACE

    public void AllUIDisappeared()
    {
        userCanvas.gameObject.SetActive(false);
    }

    public void AllUIAppeared()
    {
        userCanvas.gameObject.SetActive(true);
    }
    #endregion

    #region FOROBJECTROT

    public void RotBtnClicked()
    {
        if(ARController.allAnchors.Count == 0)
        {
            Debug.Log("no have anchor - RotBtnClicked");
            return;
        }

        foreach (var anchor in ARController.allAnchors)
        {
            Transform quiz = anchor.transform.GetChild(0);
            Debug.Log("quiz name is" + quiz.name + " - RotBtnClicked");
            if (quiz != null && quiz.name.Contains("YourQuiz"))
            {
                Quaternion newRot = RotCaluclated(quiz);
                quiz.localRotation = newRot;
            }
        }

    }

    private Quaternion RotCaluclated(Transform quiz)
    {
        Vector3 playerLocation = xrOrigin.Camera.transform.position;
        Vector3 quizLocation = quiz.position;
        //상황에 따라 안되는 경우가 생김
        //Vector3 direction = quizLocation - playerLocation;

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
        return quizRotation;
    }
    #endregion

    #region BACKBTN

    public void BackBtnClicked()
    {
        SceneManager.LoadScene("MainTitleScene");
    }
    #endregion
}
