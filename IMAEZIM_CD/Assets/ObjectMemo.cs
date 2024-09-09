using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.Android;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class ObjectMemo : MonoBehaviour
{
    WebCamTexture camTexture;
    public RawImage cameraViewImage, capturedImage, checkImg, askImg;
    public Button B_Check, B_Plus, B_Yes, B_No, B_Post, B_X, B_back;
    public Text messageText, scrollT;
    public GameObject P_Ask, P_Write, P_Check;
    public InputField memoText;
    private List<Texture2D> Img10 = new();
    private int objId, del_objId;

    public void CameraOn()
    {
        if (!Permission.HasUserAuthorizedPermission(Permission.Camera))
        {
            Permission.RequestUserPermission(Permission.Camera);
        }

        WebCamDevice[] devices = WebCamTexture.devices;
        int selectedCameraIndex = -1;

        for (int i = 0; i < devices.Length; i++)
        {
            if (devices[i].isFrontFacing == false)
            {
                selectedCameraIndex = i;
                break;
            }
        }

        if (selectedCameraIndex >= 0)
        {
            camTexture = new WebCamTexture(devices[selectedCameraIndex].name);
            camTexture.requestedFPS = 30;
            cameraViewImage.texture = camTexture;
            camTexture.Play();
            cameraViewImage.rectTransform.Rotate(0, 0, -90);
        }
    }

    public void CameraOff()
    {
        if (camTexture != null)
        {
            camTexture.Stop();
            WebCamTexture.Destroy(camTexture);
            camTexture = null;
        }
    }

    public Texture2D Capture()
    {
        if (camTexture != null && camTexture.isPlaying)
        {
            Texture2D snapshot = new(camTexture.height, camTexture.width, TextureFormat.RGB24, false);
            Color[] originalPixels = camTexture.GetPixels();
            for (int x = 0; x < camTexture.width; x++)
            {
                for (int y = 0; y < camTexture.height; y++)
                {
                    snapshot.SetPixel(y, camTexture.width - 1 - x, originalPixels[x + y * camTexture.width]);
                }
            }
            snapshot.Apply();
            capturedImage.texture = snapshot;

            return snapshot;
        }

        return null;
    }

    [System.Serializable]
    public class TextInfo
    {
        public string text;
        public string userNickname;
        public string date;
    }

    [System.Serializable]
    public class CheckData
    {
        public string status;
        public string result_obj_img;
        public TextInfo[] text_info_list;
    }

    [System.Serializable]
    public class AskData
    {
        public string status;
        public int new_obj_id;
        public int old_obj_id;
        public string old_obj_img;
    }

    IEnumerator LoadImage(string imageUrl)  //물건 대표 이미지
    {
        int index = imageUrl.IndexOf("/media");
        using UnityWebRequest www = UnityWebRequestTexture.GetTexture("http://34.22.102.33:8000" + imageUrl[index..]);
        yield return www.SendWebRequest();
        messageText.text = "";
        if (www.result == UnityWebRequest.Result.Success)
        {
            Texture2D texture = DownloadHandlerTexture.GetContent(www);
            checkImg.texture = texture;
            P_Check.SetActive(true);
        }
        else
        {
            Debug.Log("Failed to load image: " + www.error);
        }
    }

    IEnumerator CheckMemo(byte[] img)   //메모 확인
    {
        string url = "http://34.22.102.33:8000/object/searchObj/";
        WWWForm form = new();
        form.AddBinaryData("obj_img", img, "image.jpg", "image/jpg");

        using UnityWebRequest www = UnityWebRequest.Post(url, form);
        yield return www.SendWebRequest();

        if (www.result != UnityWebRequest.Result.Success)
        {
            Debug.Log("Error: " + www.error);
            messageText.text = "";
        }
        else
        {
            var response = JsonUtility.FromJson<CheckData>(www.downloadHandler.text);
            if (response.status == "success")
            {
                StartCoroutine(LoadImage(response.result_obj_img));
                foreach (var textItem in response.text_info_list)
                {
                    scrollT.text += $"{textItem.userNickname} : {textItem.text}\n";
                }
            }
            else
            {
                messageText.text = "메모가 존재하지 않습니다";
            }
        }
    }

    IEnumerator PostMemo()  //메모 작성
    {
        string url = "http://34.22.102.33:8000/object/addText/";
        WWWForm form = new();
        string objid = objId.ToString();
        string del_objid = del_objId.ToString();
        string userId = MainMenu.UserId;
        string text = memoText.text;
        string open = "public";
        yield return null;
        form.AddField("objId", objid);
        form.AddField("del_objId", del_objid);
        form.AddField("userId", userId);
        form.AddField("text", text);
        form.AddField("open", open);

        using UnityWebRequest www = UnityWebRequest.Post(url, form);
        yield return www.SendWebRequest();

        if (www.result == UnityWebRequest.Result.Success)
        {
            messageText.text = "메모 등록 완료";
        }
        else
        {
            Debug.LogError("Error sending additional request: " + www.error);
            messageText.text = "메모 등록 실패";
        }
        memoText.text = "";
    }

    IEnumerator LoadOldImage(string imageUrl)   //물건 확인용 이미지
    {
        int index = imageUrl.IndexOf("/media");
        using UnityWebRequest www = UnityWebRequestTexture.GetTexture("http://34.22.102.33:8000" + imageUrl[index..]);
        yield return www.SendWebRequest();
        messageText.text = "";
        if (www.result == UnityWebRequest.Result.Success)
        {
            Texture2D texture = DownloadHandlerTexture.GetContent(www);
            askImg.texture = texture;
            P_Ask.SetActive(true);
        }
        else
        {
            Debug.LogError("Error loading old object image: " + www.error);
        }
    }

    IEnumerator UploadImages()  //물건 10장 보내기
    {
        string url = "http://34.22.102.33:8000/object/addObj/";
        WWWForm form = new();
        for (int i = 0; i < 10; i++)
        {
            byte[] imageData = Img10[i].EncodeToJPG();
            form.AddBinaryData($"obj_img{i + 1}", imageData, $"image{i + 1}.jpg", "image/jpg");
        }

        using UnityWebRequest www = UnityWebRequest.Post(url, form);
        yield return www.SendWebRequest();

        if (www.result != UnityWebRequest.Result.Success)
        {
            Debug.LogError("Error: " + www.error);
            messageText.text = "이미지 업로드 실패";
        }
        else
        {
            var response = JsonUtility.FromJson<AskData>(www.downloadHandler.text);
            if (response.status == "Few feature points")
            {
                messageText.text = "특징점이 부족합니다";
            }
            else if (response.status == "success")
            {
                StartCoroutine(LoadOldImage(response.old_obj_img));
                ClearButtonListeners();
                B_Yes.onClick.AddListener(() =>
                {
                    P_Ask.SetActive(false);
                    P_Write.SetActive(true);
                    objId = response.old_obj_id;
                    del_objId = response.new_obj_id;
                });
                B_No.onClick.AddListener(() =>
                {
                    P_Ask.SetActive(false);
                    P_Write.SetActive(true);
                    objId = response.new_obj_id;
                    del_objId = response.old_obj_id;
                });
            }
            else
            {
                Debug.LogError("Unknown response status.");
            }
        }
        foreach (var texture in Img10)
        {
            if (texture != null)
            {
                Object.Destroy(texture);
            }
        }
        Img10.Clear();
    }

    private void ClearButtonListeners()
    {
        B_Yes.onClick.RemoveAllListeners();
        B_No.onClick.RemoveAllListeners();
    }

    public void CreateMemo()    //물건 10장 캡쳐
    {
        if (Img10.Count < 10)
        {
            Img10.Add(Capture());
            messageText.text = $"{Img10.Count}/10";
            if (Img10.Count == 10)
            {
                messageText.text = "물건 확인 중...";
                StartCoroutine(UploadImages());
            }
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        CameraOn();
        B_Check.onClick.AddListener(() =>
        {
            messageText.text = "메모 확인 중...";
            StartCoroutine(CheckMemo(Capture().EncodeToJPG()));
        });
        B_Plus.onClick.AddListener(CreateMemo);
        B_Post.onClick.AddListener(() => {
            P_Write.SetActive(false);
            StartCoroutine(PostMemo());
        });
        B_X.onClick.AddListener(() => {
            P_Check.SetActive(false);
            scrollT.text = "";
        });
        B_back.onClick.AddListener(() =>
        {
            SceneManager.LoadScene("MainTitleScene");
        });
    }
}
