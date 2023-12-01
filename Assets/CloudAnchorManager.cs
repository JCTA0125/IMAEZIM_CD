using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;
using UnityEngine.Networking;

using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;

using Google.XR.ARCoreExtensions;
using System;
using System.Linq;
using System.IO;
using System.Collections;

using NAudio;
using NAudio.Wave;
using static CloudAnchorManager;

public class CloudAnchorManager : MonoBehaviour
{
    MapManager mapmanager;
    public enum Mode { READY, HOST, HOST_PENDING, RESOLVE, RESOLVE_PENDING };   // 상태 변수

    public Button hostButton;       // 클라우드 앵커 등록
    //public Button resolveButton;    // 클라우드 앵커 조회
    public Button resetButton;      // 리셋
    public Button cancelButton;      // 호스팅(인식) 중 취소

    public Text messageText;    // 메세지 출력 텍스트

    public Mode mode = Mode.READY;
    public ARAnchorManager anchorManager;
    public ARRaycastManager raycastManager;

    public GameObject anchorPrefab; // 증강시킬 객체 프리팹
    public GameObject textPrefab, imagePrefab, videoPrefab, audioPrefab;
    private GameObject anchorGameObject;    // 저장 객체 변수(삭제하기 위한 용도)

    private ARAnchor localAnchor;   // 로컬앵커 저장 변수
    private ARCloudAnchor cloudAnchor;  // 클라우드 앵커 변수

    private List<ARRaycastHit> hits = new List<ARRaycastHit>(); // Raycast Hit

    public struct Memo
    {
        public string anchorID;
        public object memo;
        public int type;  //글1 이미지2 비디오3 오디오4
        public string nickname;
        //public double latitude;
        //public double longitude;

        public Memo(string anchorID, object memo, int type, string nickname) //double latitude, double longitude
        {
            this.anchorID = anchorID;
            this.memo = memo;
            this.type = type;
            this.nickname = nickname;
            //this.latitude = latitude;
            //this.longitude = longitude;
        }
    }
    public List<Memo> memoList = new List<Memo>();

    public struct Anchor
    {
        public object memo;
        public int type;  //글1 이미지2 비디오3 오디오4
        public string nickname;

        public Anchor(object memo, int type, string nickname)
        {
            this.memo = memo;
            this.type = type;
            this.nickname = nickname;
        }
    }
    public Dictionary<ARCloudAnchor, Anchor> cloudAnchors = new Dictionary<ARCloudAnchor, Anchor>();
    public Dictionary<GameObject, Anchor> anchorGameObjects = new Dictionary<GameObject, Anchor>();

    public GameObject PopUp_H, PopUp_T, PopUp_I, PopUp_R, PopUp_V, PopUp_A, PopUp_M; // CM;
    public Button buttonT, buttonI, buttonV, buttonA, buttonX; // buttonCM;
    public Button buttonTC, buttonIS, buttonIC, buttonVS, buttonVC, buttonAS, buttonAC, buttonP, buttonS, buttonRP, buttonRS;
    public Button buttonTL, buttonIL, buttonVL, buttonAL;
    public InputField inputT; //inputCM;
    public RawImage img;
    public RawImage video;
    public VideoPlayer videoPlayer;
    public AudioSource audioSource;

    private Texture2D texture;
    //private RenderTexture renderTexture;
    private string vPath;
    private string aPath;

    public Text MEMO;
    public RawImage pop_img;
    public VideoPlayer vp;
    public AudioSource aSource;
    private string myNickname;

    [SerializeField] private Camera arCamera;

    void Start()
    {
        myNickname = "myName";

        hostButton.onClick.AddListener(() => {
            cancelButton.gameObject.SetActive(true);
            hostButton.gameObject.SetActive(false);
            OnHostClick();
        });
        //resolveButton.onClick.AddListener(() => OnResolveClick());
        resetButton.onClick.AddListener(() => OnResetClick());
        cancelButton.onClick.AddListener(() => OnCancelClick());
        buttonT.onClick.AddListener(() =>
        {
            PopUp_H.SetActive(false);
            PopUp_T.SetActive(true);
        });
        buttonTL.onClick.AddListener(() =>
        {
            PopUp_M.SetActive(true);
            buttonTL.gameObject.SetActive(false);
            buttonTC.gameObject.SetActive(true);
        });
        buttonTC.onClick.AddListener(() => {
            PopUp_T.SetActive(false);
            //cloudAnchors.Add(cloudAnchor, new Anchor() { memo = inputT.text, type = 1, nickname = myNickname });
            textPrefab.transform.Find("nickname").GetComponent<TextMesh>().text = myNickname;
            anchorGameObjects.Add(Instantiate(textPrefab, cloudAnchor.transform), new Anchor() { memo = inputT.text, type = 1, nickname = myNickname });
            memoList.Add(new Memo() { anchorID = cloudAnchor.cloudAnchorId, memo = inputT.text, type = 1, nickname = myNickname });
            localAnchor = null; cloudAnchor = null; Destroy(anchorGameObject);
            inputT.text = "";
            buttonTL.gameObject.SetActive(true);
            buttonTC.gameObject.SetActive(false);
            mode = Mode.RESOLVE_PENDING;
        });
        buttonI.onClick.AddListener(() =>
        {
            PopUp_H.SetActive(false);
            PopUp_I.SetActive(true);
        });
        buttonIS.onClick.AddListener(() =>
        {
            getImage();
        });
        buttonIL.onClick.AddListener(() =>
        {
            PopUp_M.SetActive(true);
            buttonIL.gameObject.SetActive(false);
            buttonIC.gameObject.SetActive(true);
        });
        buttonIC.onClick.AddListener(() => {
            PopUp_I.SetActive(false);
            imagePrefab.transform.Find("nickname").GetComponent<TextMesh>().text = myNickname;
            anchorGameObjects.Add(Instantiate(imagePrefab, cloudAnchor.transform), new Anchor() { memo = texture, type = 2, nickname = myNickname });
            memoList.Add(new Memo() { anchorID = cloudAnchor.cloudAnchorId, memo = texture, type = 2, nickname = myNickname });
            localAnchor = null; cloudAnchor = null; Destroy(anchorGameObject);
            texture = null; img.texture = null;
            ImageSizeReturn(img, 300, 250);
            buttonIL.gameObject.SetActive(true);
            buttonIC.gameObject.SetActive(false);
            mode = Mode.RESOLVE_PENDING;
        });
        buttonV.onClick.AddListener(() =>
        {
            PopUp_H.SetActive(false);
            PopUp_V.SetActive(true);
            videoPlayer.gameObject.SetActive(true);
        });
        buttonVS.onClick.AddListener(() =>
        {
            getVideo();
        });
        buttonVL.onClick.AddListener(() =>
        {
            PopUp_M.SetActive(true);
            buttonVL.gameObject.SetActive(false);
            buttonVC.gameObject.SetActive(true);
            videoPlayer.Stop();
        });
        buttonVC.onClick.AddListener(() => {
            PopUp_V.SetActive(false);
            videoPrefab.transform.Find("nickname").GetComponent<TextMesh>().text = myNickname;
            anchorGameObjects.Add(Instantiate(videoPrefab, cloudAnchor.transform), new Anchor() { memo = vPath, type = 3, nickname = myNickname });
            memoList.Add(new Memo() { anchorID = cloudAnchor.cloudAnchorId, memo = vPath, type = 3, nickname = myNickname });
            localAnchor = null; cloudAnchor = null; Destroy(anchorGameObject);
            //renderTexture = null;
            video.texture = null;
            ImageSizeReturn(img, 300, 250);
            buttonVL.gameObject.SetActive(true);
            buttonVC.gameObject.SetActive(false);
            videoPlayer.gameObject.SetActive(false);
            videoPlayer.url = null;
            mode = Mode.RESOLVE_PENDING;
        });
        buttonA.onClick.AddListener(() =>
        {
            PopUp_H.SetActive(false);
            PopUp_A.SetActive(true);
        });
        buttonAS.onClick.AddListener(() =>
        {
            getAudio();
        });
        buttonAL.onClick.AddListener(() =>
        {
            PopUp_M.SetActive(true);
            buttonAL.gameObject.SetActive(false);
            buttonAC.gameObject.SetActive(true);
            audioSource.Stop();
        });
        buttonAC.onClick.AddListener(() => {
            PopUp_A.SetActive(false);
            audioPrefab.transform.Find("nickname").GetComponent<TextMesh>().text = myNickname;
            anchorGameObjects.Add(Instantiate(audioPrefab, cloudAnchor.transform), new Anchor() { memo = aPath, type = 4, nickname = myNickname });
            memoList.Add(new Memo() { anchorID = cloudAnchor.cloudAnchorId, memo = aPath, type = 4, nickname = myNickname });
            localAnchor = null; cloudAnchor = null; Destroy(anchorGameObject);
            buttonAL.gameObject.SetActive(true);
            buttonAC.gameObject.SetActive(false);
            audioSource.gameObject.SetActive(false);
            mode = Mode.RESOLVE_PENDING;
        });
        buttonP.onClick.AddListener(() =>
        {
            audioSource.gameObject.SetActive(true);
            if (!string.IsNullOrEmpty(aPath))
            {
                StartCoroutine(LoadAudio(aPath));
            }
        });
        buttonS.onClick.AddListener(() =>
        {
            audioSource.Stop();
            audioSource.gameObject.SetActive(false);
        });
        buttonRP.onClick.AddListener(() =>
        {
            aSource.gameObject.SetActive(true);
            if (!string.IsNullOrEmpty(aPath))
            {
                StartCoroutine(LoadAudio2(aPath));
            }
        });
        buttonRS.onClick.AddListener(() =>
        {
            aSource.Stop();
            aSource.gameObject.SetActive(false);
        });
        //buttonCM.onClick.AddListener(() =>
        //{
        //    comments.Add(myNickname, inputCM.text);
        //    inputCM.text = "";
        //});
        buttonX.onClick.AddListener(() =>
        {
            PopUp_R.SetActive(false);
            MEMO.text = "";
            pop_img.texture = null;
            ImageSizeReturn(pop_img, 360, 250);
            buttonRP.gameObject.SetActive(false);
            buttonRS.gameObject.SetActive(false);
            pop_img.gameObject.SetActive(true);
            aSource.gameObject.SetActive(false);
            vp.gameObject.SetActive(false);
        });

        StartCoroutine(Resolving());
    }

    void Update()
    {
        if (mode == Mode.HOST)
        {
            Hosting();
            HostProcessing();
        }
        if (mode == Mode.HOST_PENDING)
        {
            HostPending();
        }
        if (mode == Mode.RESOLVE)
        {
            //Resolving();
        }
        if (mode == Mode.RESOLVE_PENDING)
        {
            ResolvePending();
            Checking();
        }
        if (mode == Mode.READY)
        {
            messageText.text = "Ready";
            //Checking();
        }
    }

    void Hosting()
    {
        if (Input.touchCount < 1) return;

        Touch touch = Input.GetTouch(0);
        if (touch.phase != TouchPhase.Began) return;

        if (localAnchor == null)    // 로컬 앵커가 존재하는지 여부 확인
        {
            if (raycastManager.Raycast(touch.position, hits, TrackableType.PlaneWithinPolygon)) // Raycast 발사
            {
                localAnchor = anchorManager.AddAnchor(hits[0].pose);    // 로컬 앵커 생성
                anchorGameObject = Instantiate(anchorPrefab, localAnchor.transform);    // 로컬 앵커 위치에 객체 증강시키고 변수에 저장
            }
        }
    }

    void HostProcessing()   // 클라우드 앵커 등록
    {
        if (localAnchor == null) return;
        FeatureMapQuality quality = anchorManager.EstimateFeatureMapQualityForHosting(GetCameraPose()); // 피쳐포인트 개수 및 퀄리티 측정

        string mappingText = string.Format("맵핑 품질 = {0}", quality);

        if (quality == FeatureMapQuality.Sufficient || quality == FeatureMapQuality.Good)   // 맵핑 퀄리티가 1 이상일 때 호스팅 요청
        {
            cloudAnchor = anchorManager.HostCloudAnchor(localAnchor, 1);    // 1일짜리 앵커포인트

            if (cloudAnchor == null)
            {
                mappingText = "클라우드 앵커 생성 실패";
            }
            else
            {
                mappingText = "클라우드 앵커 생성 시작";
                mode = Mode.HOST_PENDING;
            }
        }
        messageText.text = mappingText;
    }

    void getImage() //갤러리 이미지
    {
        if(!NativeGallery.IsMediaPickerBusy())
        {
            NativeGallery.GetImageFromGallery((image) =>
            {
                FileInfo selectedImage = new FileInfo(image);
                if (selectedImage.Length > 50000000) return;

                if (!string.IsNullOrEmpty(image))
                {
                    StartCoroutine(LoadImage(image));
                }
            });
        }
    }         
    IEnumerator LoadImage(string imagePath) //이미지 로드 코루틴   
    {
        yield return null;

        //byte[] imageData = File.ReadAllBytes(imagePath);
        //string imageName = Path.GetFileName(imagePath).Split('.')[0];
        //string saveImagePath = Application.persistentDataPath + "/Image";

        //if (!Directory.Exists(saveImagePath)) Directory.CreateDirectory(saveImagePath);

        //File.WriteAllBytes(saveImagePath + imageName + ".jpg", imageData);
        NativeGallery.ImageProperties imageProperties = NativeGallery.GetImageProperties(imagePath);
        NativeGallery.ImageOrientation orientation = imageProperties.orientation;

        byte[] tempImage = File.ReadAllBytes(imagePath);
        texture = new Texture2D(2, 2);
        texture.LoadImage(tempImage);

        if (orientation == NativeGallery.ImageOrientation.Rotate90)
        {
            //img.transform.Rotate(new Vector3(0, 0, 90));
            texture = RotateTexture(texture, 90);
        }
        Texture2D RotateTexture(Texture2D originalTexture, int rotationAngle)
        {
            Color32[] original = originalTexture.GetPixels32();
            Color32[] rotated = new Color32[original.Length];
            int w = originalTexture.width;
            int h = originalTexture.height;

            int iRotated, iOriginal;

            for (int j = 0; j < h; ++j)
            {
                for (int i = 0; i < w; ++i)
                {
                    if (rotationAngle == 90) iRotated = j + (w - 1 - i) * h;
                    else if (rotationAngle == 180) iRotated = (w - 1 - i) + (h - 1 - j) * w;
                    else if (rotationAngle == 270) iRotated = (h - 1 - j) + i * h;
                    else iRotated = j * w + i;

                    iOriginal = j * w + i;
                    rotated[iRotated] = original[iOriginal];
                }
            }

            Texture2D rotatedTexture = new Texture2D(h, w);
            rotatedTexture.SetPixels32(rotated);
            rotatedTexture.Apply();
            return rotatedTexture;
        }

        img.texture = texture;
        img.SetNativeSize();
        ImageSizeSetting(img, 300, 250);
    }
    void getVideo()
    {
        if (!NativeGallery.IsMediaPickerBusy())
        {
            NativeGallery.GetVideoFromGallery((video) =>
            {
                if (!string.IsNullOrEmpty(video))
                {
                    vPath = video;
                    StartCoroutine(LoadVideo(video));
                }
            });
        }
    }
    IEnumerator LoadVideo(string videoPath)
    {
        yield return null;

        videoPlayer.url = videoPath;    // 비디오 플레이어에 비디오 경로 설정
        videoPlayer.Prepare();

        while (!videoPlayer.isPrepared) // Prepare가 완료될 때까지 대기
        {
            yield return null;
        }

        video.texture = videoPlayer.texture;
        video.SetNativeSize();
        ImageSizeSetting(video, 300, 250);

        videoPlayer.Play(); // 비디오 재생
    }
    IEnumerator LoadVideo2(string videoPath)
    {
        yield return null;

        vp.url = videoPath;
        vp.Prepare();

        while (!vp.isPrepared)
        {
            yield return null;
        }

        pop_img.texture = vp.texture;
        pop_img.SetNativeSize();
        ImageSizeSetting(pop_img, 300, 250);

        vp.Play();
    }
    void getAudio()
    {
        if (!NativeGallery.IsMediaPickerBusy())
        {
            NativeGallery.GetAudioFromGallery((audio) =>
            {
                if (!string.IsNullOrEmpty(audio))
                {
                    aPath = audio;
                }
            });
        }
    }
    IEnumerator LoadAudio(string audioPath)
    {
        using (UnityWebRequest www = UnityWebRequestMultimedia.GetAudioClip("file://" + audioPath, AudioType.MPEG))
        {
            yield return www.SendWebRequest();

            if (www.result == UnityWebRequest.Result.Success)
            {
                AudioClip audioClip = DownloadHandlerAudioClip.GetContent(www);
                audioSource.clip = audioClip;
                audioSource.Play();
            }
            else
            {
                Debug.LogError("Failed to load audio: " + www.error);
            }
            audioSource.volume = 1.0f;
        }
    }
    IEnumerator LoadAudio2(string audioPath)
    {
        using (UnityWebRequest www = UnityWebRequestMultimedia.GetAudioClip("file://" + audioPath, AudioType.MPEG))
        {
            yield return www.SendWebRequest();

            if (www.result == UnityWebRequest.Result.Success)
            {
                AudioClip audioClip = DownloadHandlerAudioClip.GetContent(www);
                aSource.clip = audioClip;
                aSource.Play();
            }
            else
            {
                Debug.LogError("Failed to load audio: " + www.error);
            }
            aSource.volume = 1.0f;
        }
    }

    void ImageSizeSetting(RawImage img, float x, float y)
    {
        var imgX = img.rectTransform.sizeDelta.x;
        var imgY = img.rectTransform.sizeDelta.y;
        if (x / y > imgX / imgY)
        {
            img.rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, y);
            img.rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, imgX * (y / imgY));
        }
        else
        {
            img.rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, x);
            img.rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, imgY * (x / imgX));
        }
    }
    void ImageSizeReturn(RawImage img, float x, float y)
    {
        img.rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, y);
        img.rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, x);
    }

    void HostPending()
    {
        string mappingText = "";
        if (cloudAnchor.cloudAnchorState == CloudAnchorState.Success)
        {
            PopUp_H.SetActive(true);
            cancelButton.gameObject.SetActive(false);
            hostButton.gameObject.SetActive(true);
            mappingText = $"클라우드 앵커 생성 성공, CloudAnchor ID = {cloudAnchor.cloudAnchorId}";

            mode = Mode.READY;
        }
        else
        {
            mappingText = $"클라우드 앵커 생성 진행중...{cloudAnchor.cloudAnchorState}";
        }
        messageText.text = mappingText;
    }

    IEnumerator Resolving()
    {
        if (memoList.Count == 0)
        {
            mode = Mode.RESOLVE_PENDING;
            yield break;
        }
        messageText.text = "";

        foreach (var item in memoList)
        {
            ARCloudAnchor anchor = anchorManager.ResolveCloudAnchorId(item.anchorID);
            yield return new WaitUntil(() => anchor != null); // 기다림
            cloudAnchors.Add(anchor, new Anchor() { memo = item.memo, type = item.type, nickname = item.nickname });
        }

        if (memoList.Count == cloudAnchors.Count)
        {
            mode = Mode.RESOLVE_PENDING;
        }
    }

    void ResolvePending()
    {
        if (cloudAnchors.Count == 0) return;
        bool allAnchorsResolved = true;
        foreach (KeyValuePair<ARCloudAnchor, Anchor> item in cloudAnchors)
        {
            if (item.Key.cloudAnchorState == CloudAnchorState.Success)
            {
                // 객체 증강
                if(item.Value.type == 1)
                {
                    textPrefab.transform.Find("nickname").GetComponent<TextMesh>().text = item.Value.nickname;
                    anchorGameObjects.Add(Instantiate(textPrefab, item.Key.transform), item.Value);
                }
                else if(item.Value.type == 2)
                {
                    imagePrefab.transform.Find("nickname").GetComponent<TextMesh>().text = item.Value.nickname;
                    anchorGameObjects.Add(Instantiate(imagePrefab, item.Key.transform), item.Value);
                }
                else if(item.Value.type == 3)
                {
                    videoPrefab.transform.Find("nickname").GetComponent<TextMesh>().text = item.Value.nickname;
                    anchorGameObjects.Add(Instantiate(videoPrefab, item.Key.transform), item.Value);
                }
                else if (item.Value.type == 4)
                {
                    audioPrefab.transform.Find("nickname").GetComponent<TextMesh>().text = item.Value.nickname;
                    anchorGameObjects.Add(Instantiate(audioPrefab, item.Key.transform), item.Value);
                }
                cloudAnchors.Remove(item.Key);
            }
            else
            {
                allAnchorsResolved = false;
                messageText.text = $"리졸빙 진행 중...{item.Key.cloudAnchorState}";
            }
        }
        if (allAnchorsResolved)
        {
            messageText.text = "리졸브 성공";
        }
    }

    void Checking()
    {
        if (Input.touchCount == 0) return;
        Touch touch = Input.GetTouch(0);

        if (touch.phase == TouchPhase.Began)    //터치 시작시
        {
            Ray ray;
            RaycastHit hitobj;

            ray = arCamera.ScreenPointToRay(touch.position);

            //Ray를 통한 오브젝트 인식
            int layerMask = 1 << LayerMask.NameToLayer("Cube");
            if (Physics.Raycast(ray, out hitobj, 500f, layerMask) && anchorGameObjects.Count > 0)
            {
                PopUp_R.SetActive(true);
                if (anchorGameObjects[hitobj.collider.gameObject].type == 1)
                {
                    MEMO.text = (string)anchorGameObjects[hitobj.collider.gameObject].memo;
                }
                else if (anchorGameObjects[hitobj.collider.gameObject].type == 2)
                {
                    pop_img.texture = (Texture2D)anchorGameObjects[hitobj.collider.gameObject].memo;
                    pop_img.SetNativeSize();
                    ImageSizeSetting(pop_img, 300, 250);
                }
                else if (anchorGameObjects[hitobj.collider.gameObject].type == 3)
                {
                    vp.gameObject.SetActive(true);
                    StartCoroutine(LoadVideo2((string)anchorGameObjects[hitobj.collider.gameObject].memo));
                }
                else if (anchorGameObjects[hitobj.collider.gameObject].type == 4)
                {
                    aPath = (string)anchorGameObjects[hitobj.collider.gameObject].memo;
                    buttonRP.gameObject.SetActive(true);
                    buttonRS.gameObject.SetActive(true);
                    pop_img.gameObject.SetActive(false);
                }
            }
        }
    }

    // MainCamera 태그로 지정된 카메라의 위치와 각도를 Pose 데이터 타입으로 반환
    public Pose GetCameraPose()
    {
        return new Pose(Camera.main.transform.position, Camera.main.transform.rotation);
    }

    private void OnHostClick()
    {
        mode = Mode.HOST;
    }

    //private void OnResolveClick()
    //{
    //    //mode = Mode.RESOLVE;
    //    //StartCoroutine(Resolving());
    //}

    private void OnResetClick()
    {
        if (anchorGameObject != null)
        {
            Destroy(anchorGameObject);
        }
        foreach (var obj in anchorGameObjects.Keys)
        {
            Destroy(obj);
        }
        anchorGameObjects.Clear();
        cloudAnchor = null;
        localAnchor = null;
        memoList.Clear();
        cloudAnchors.Clear();
        mode = Mode.READY;
    }

    private void OnCancelClick()
    {
        if (anchorGameObject != null)
        {
            Destroy(anchorGameObject);
        }
        cloudAnchor = null;
        localAnchor = null;
        mode = Mode.RESOLVE_PENDING;
        cancelButton.gameObject.SetActive(false);
        hostButton.gameObject.SetActive(true);
        messageText.text = "";
    }
}