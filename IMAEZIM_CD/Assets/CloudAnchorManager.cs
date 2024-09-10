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

using NAudio;
using NAudio.Wave;
using static CloudAnchorManager;
using Google.XR.ARCoreExtensions.Samples.PersistentCloudAnchors;
using UnityEngine.SceneManagement;
using Unity.XR.CoreUtils;
//using static UnityEditor.Progress;

public class CloudAnchorManager : MonoBehaviour
{

    public enum Mode { READY, HOST, HOST_PENDING, RESOLVE, RESOLVE_PENDING };   // 상태 변수

    public Button hostButton, resetButton, cancelButton, backButton;
    public Text messageText;    // 메세지 출력 텍스트
    public Mode mode = Mode.READY;
    public ARAnchorManager anchorManager;
    //public CloudAnchorManager anchorManager;
    public ARRaycastManager raycastManager;
    public ARPlaneManager PlaneManager;

    public GameObject anchorPrefab; // 증강시킬 객체 프리팹
    public GameObject MapQualityIndicatorPrefab;
    public GameObject textPrefab, imagePrefab, videoPrefab, audioPrefab;
    private GameObject anchorGameObject;    // 저장 객체 변수(삭제하기 위한 용도)
    private GameObject indicatorGO;
    private MapQualityIndicator _qualityIndicator = null;
    public ARPlane plane;

    private ARAnchor localAnchor;  // 로컬앵커 저장 변수
    private ARCloudAnchor cloudAnchor;  // 클라우드 앵커 변수
    private List<ARRaycastHit> hits = new List<ARRaycastHit>(); // Raycast Hit

    public XROrigin xrOrigin;
    public struct Memo
    {
        public string anchorId;
        public string memoType;  //글A 사진B 음성C 영상D
        public string nickname;
        public string memo_content;

        public Memo(string anchorId, string memoType, string nickname, string memo_content)
        {
            this.anchorId = anchorId;
            this.memoType = memoType;
            this.nickname = nickname;
            this.memo_content = memo_content;
        }
    }
    public List<Memo> memoList = new List<Memo>();
#nullable enable
    [System.Serializable]
    public struct MemoContent
    {
        public int id;
        public string text;
        public string picture; // 이미지 경로
        public string video; // 비디오 경로
        public string record; // 오디오 경로
    }
    [System.Serializable]
    public struct ServerHistory
    {
        public int id;
        public string anchorId;
        public int userId;
        public string date;
        public string memoType;
        public int objectNumber;
        public float latitude;
        public float longitude;
        public string open;
        public string detailAddr;
        public MemoContent memo_content;
        public string nickname;
    }

    [Serializable]
    public class Wrapper
    {
        public List<ServerHistory> data;
    }
    public List<ServerHistory> historyList;

    public Dictionary<ARCloudAnchor, Memo> cloudAnchors = new Dictionary<ARCloudAnchor, Memo>();
    public Dictionary<GameObject, Memo> anchorGameObjects = new Dictionary<GameObject, Memo>();

    public GameObject PopUp_H, PopUp_T, PopUp_I, PopUp_R, PopUp_V, PopUp_A, PopUp_M;
    public Button buttonT, buttonI, buttonV, buttonA, buttonX;
    public Button buttonTC, buttonIS, buttonIC, buttonVS, buttonVC, buttonAS, buttonAC, buttonP, buttonS, buttonRP, buttonRS;
    public Button buttonTL, buttonIL, buttonVL, buttonAL;
    public InputField inputT;
    public RawImage img;
    public RawImage video;
    public VideoPlayer videoPlayer;
    public AudioSource audioSource;

    private Texture2D texture;
    private string iPath;
    private string vPath;
    private string aPath;

    public Text MEMO;
    public RawImage pop_img;
    public VideoPlayer vp;
    public AudioSource aSource;
    private string myNickname = "CodeDuck";

    [SerializeField] private Camera arCamera;

    public InputField inputAddress;
    public byte[] postByte;

    void Start()
    {
        //MapManager MapInstance = new();
        myNickname = MainMenu.UserNickname;
        //myId = "2";

        hostButton.onClick.AddListener(() => {
            cancelButton.gameObject.SetActive(true);
            hostButton.gameObject.SetActive(false);
            OnHostClick();
        });
        resetButton.onClick.AddListener(() => OnResetClick());
        cancelButton.onClick.AddListener(() => OnCancelClick());
        backButton.onClick.AddListener(() =>
        {
            SceneManager.LoadScene("MainTitleScene");
        });
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
            //cloudAnchors.Add(cloudAnchor, new Anchor() { memo = inputT.text, type = "A", nickname = myNickname });
            textPrefab.transform.Find("nickname").GetComponent<TextMesh>().text = myNickname;
            anchorGameObjects.Add(Instantiate(textPrefab, cloudAnchor.transform), new Memo() { anchorId = "id", memoType = "A", nickname = myNickname, memo_content = inputT.text });
            StartCoroutine(PostMemo("A"));
            localAnchor = null; Destroy(anchorGameObject); Destroy(indicatorGO);
            //cloudAnchor = null;
            //inputT.text = "";
            buttonTL.gameObject.SetActive(true);
            buttonTC.gameObject.SetActive(false);
            //inputAddress.text = "";
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
            buttonIS.gameObject.SetActive(false);
            buttonIL.gameObject.SetActive(true);
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
            anchorGameObjects.Add(Instantiate(imagePrefab, cloudAnchor.transform), new Memo() { anchorId = "id", memoType = "B", nickname = myNickname, memo_content = iPath });
            StartCoroutine(PostMemo("B"));
            localAnchor = null; Destroy(anchorGameObject); Destroy(indicatorGO);
            img.texture = null;
            ImageSizeReturn(img, 500, 400);
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
            buttonVS.gameObject.SetActive(false);
            buttonVL.gameObject.SetActive(true);
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
            anchorGameObjects.Add(Instantiate(videoPrefab, cloudAnchor.transform), new Memo() { anchorId = "id", memoType = "D", nickname = myNickname, memo_content = vPath });
            StartCoroutine(PostMemo("D"));
            localAnchor = null; Destroy(anchorGameObject); Destroy(indicatorGO);
            video.texture = null;
            ImageSizeReturn(video, 500, 400);
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
            buttonAS.gameObject.SetActive(false);
            buttonAL.gameObject.SetActive(true);
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
            anchorGameObjects.Add(Instantiate(audioPrefab, cloudAnchor.transform), new Memo() { anchorId = "id", memoType = "C", nickname = myNickname, memo_content = aPath });
            StartCoroutine(PostMemo("C"));
            localAnchor = null; Destroy(anchorGameObject); Destroy(indicatorGO);
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
                StartCoroutine(LoadAudio(aPath, audioSource));
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
                if (aPath.Contains("media/record"))
                {
                    StartCoroutine(LoadAudio2(aPath));
                }
                else
                {
                    StartCoroutine(LoadAudio(aPath, aSource));
                }
            }
        });
        buttonRS.onClick.AddListener(() =>
        {
            aSource.Stop();
            aSource.gameObject.SetActive(false);
        });
        buttonX.onClick.AddListener(() =>
        {
            PopUp_R.SetActive(false);
            MEMO.text = "";
            pop_img.texture = null;
            ImageSizeReturn(pop_img, 550, 450);
            buttonRP.gameObject.SetActive(false);
            buttonRS.gameObject.SetActive(false);
            pop_img.gameObject.SetActive(true);
            aSource.gameObject.SetActive(false);
            vp.gameObject.SetActive(false);
        });

        IEnumerator PostMemo(string typem)
        {
            string url = "http://34.22.102.33:8000/inside/addMemo/";
            WWWForm form = new WWWForm();

            string userId = MainMenu.UserId;
            string anchorId = cloudAnchor.cloudAnchorId;
            string memoType = typem;
            string latitude = MapManager.latitude.ToString();
            string longitude = MapManager.longitude.ToString();
            string open = "public";
            string detailAddr = inputAddress.text;
            string input = inputT.text;
            byte[] Byte = postByte;

            yield return null;
            //값 넣기
            form.AddField("userId", userId);
            form.AddField("anchorId", anchorId);
            form.AddField("memoType", memoType);
            form.AddField("latitude", latitude);
            form.AddField("longitude", longitude);
            form.AddField("open", open);
            form.AddField("detailAddr", detailAddr);
            switch (memoType)
            {
                case "A": form.AddField("memo_content", input); break;
                case "B": form.AddBinaryData("memo_content", Byte, "image.jpg", "image/jpg"); break;
                case "C": form.AddBinaryData("memo_content", Byte, "audio.mp3", "audio/mp3"); break;
                case "D": form.AddBinaryData("memo_content", Byte, "video.mp4", "video/mp4"); break;
            }

            UnityWebRequest www = UnityWebRequest.Post(url, form);
            yield return www.SendWebRequest();

            if (www.error == null)
            {
                //messageText.text = "post success";
                cloudAnchor = null;
                inputT.text = "";
                inputAddress.text = "";
            }
            else messageText.text = "error for post";
        }

        IEnumerator MemoInfoGet() //url 요청 코루틴
        {
            string url = "http://34.22.102.33:8000/inside/memoInfo/";
            UnityWebRequest www = UnityWebRequest.Get(url); // get 방식으로 요청을 보냄.
            yield return www.SendWebRequest(); //응답이 올 때까지 기다림.

            if (www.error == null) //잘 도착했으면
            {
                string jsonString = "{\"data\":" + www.downloadHandler.text + "}";
                Wrapper wrapper = JsonUtility.FromJson<Wrapper>(jsonString);
                historyList = new List<ServerHistory>(wrapper.data);
                foreach (var memo in historyList)
                {
                    switch (memo.memoType)
                    {
                        case "A":
                            memoList.Add(new Memo() { anchorId = memo.anchorId, memoType = memo.memoType, nickname = memo.nickname, memo_content = memo.memo_content.text }); break;
                        case "B":
                            memoList.Add(new Memo() { anchorId = memo.anchorId, memoType = memo.memoType, nickname = memo.nickname, memo_content = memo.memo_content.picture }); break;
                        case "C":
                            memoList.Add(new Memo() { anchorId = memo.anchorId, memoType = memo.memoType, nickname = memo.nickname, memo_content = memo.memo_content.record }); break;
                        case "D":
                            memoList.Add(new Memo() { anchorId = memo.anchorId, memoType = memo.memoType, nickname = memo.nickname, memo_content = memo.memo_content.video }); break;
                    }
                }
                //Resolving();
                mode = Mode.RESOLVE;
            }
            else messageText.text = "ERROR";
        }
        StartCoroutine(MemoInfoGet());
    }

    void Update()
    {
        if (mode == Mode.HOST)
        {
            //textPrefab.GetComponent<MeshCollider>().enabled = false;
            //textPrefab.GetComponent<BoxCollider>().enabled = true;
            Hosting();
            HostProcessing();
        }
        if (mode == Mode.HOST_PENDING)
        {
            HostPending();
        }
        if (mode == Mode.RESOLVE)
        {
            Resolving();
        }
        if (mode == Mode.RESOLVE_PENDING)
        {
            //textPrefab.GetComponent<MeshCollider>().enabled = true;
            //textPrefab.GetComponent<BoxCollider>().enabled = false;
            ResolvePending();
            Checking();
        }
        if (mode == Mode.READY)
        {
            //messageText.text = "Ready";
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
                Ray ray;
                RaycastHit hitobj;
                ray = arCamera.ScreenPointToRay(touch.position);
                int layerMask = 1 << LayerMask.NameToLayer("Cube");
                if (Physics.Raycast(ray, out hitobj, 500f, layerMask))
                {

                }
                else
                {
                    ARPlane plane = PlaneManager.GetPlane(hits[0].trackableId);
                    var planeType = PlaneAlignment.HorizontalUp;
                    planeType = plane.alignment;
                    //localAnchor = ARAnchorManagerExtensions.AddAnchor(anchorManager,hits[0].pose);    // 로컬 앵커 생성

                    Pose hitPose = hits[0].pose;
                    Vector3 planeNormal = hitPose.rotation * Vector3.up;

                    if (Vector3.Dot(planeNormal, Vector3.up) < 0.1f)
                    {
                        // z축을 천장으로 향하게 하고 y축을 평면에 수직으로 설정
                        hitPose.rotation = Quaternion.LookRotation(Vector3.up, planeNormal);
                    }
                    else
                    {
                        hitPose.rotation = Quaternion.LookRotation(Vector3.Cross(Vector3.up, planeNormal), Vector3.up);
                    }

                    localAnchor = anchorManager.AttachAnchor(plane, hitPose);

                    localAnchor = anchorManager.AttachAnchor(plane, hits[0].pose);    // 로컬 앵커 생성
                    anchorGameObject = Instantiate(anchorPrefab, localAnchor.transform);    // 로컬 앵커 위치에 객체 증강시키고 변수에 저장
                    indicatorGO = Instantiate(MapQualityIndicatorPrefab, localAnchor.transform);
                    _qualityIndicator = indicatorGO.GetComponent<MapQualityIndicator>();
                    _qualityIndicator.DrawIndicator(planeType, arCamera);
                }

            }
        }
    }

    void HostProcessing()   // 클라우드 앵커 등록
    {
        if (localAnchor == null) return;

        int qualityState = 2;
        FeatureMapQuality quality = anchorManager.EstimateFeatureMapQualityForHosting(GetCameraPose()); // 피쳐포인트 개수 및 퀄리티 측정
        qualityState = (int)quality;
        _qualityIndicator.UpdateQualityState(qualityState);
        string mappingText = string.Format("맵핑 품질 = {0}", quality);

        if (quality == FeatureMapQuality.Sufficient || quality == FeatureMapQuality.Good || quality == FeatureMapQuality.Insufficient)   // 맵핑 퀄리티가 1 이상일 때 호스팅 요청
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

    void getImage() //갤러리 이미지
    {
        if (!NativeGallery.IsMediaPickerBusy())
        {
            NativeGallery.GetImageFromGallery((image) =>
            {
                FileInfo selectedImage = new FileInfo(image);
                if (selectedImage.Length > 50000000) return;

                if (!string.IsNullOrEmpty(image))
                {
                    iPath = image;
                    StartCoroutine(LoadImage(image, img));
                }
            });
        }
    }
    IEnumerator LoadImage(string imagePath, RawImage imgr) //이미지 로드 코루틴   
    {
        yield return null;
        NativeGallery.ImageProperties imageProperties = NativeGallery.GetImageProperties(imagePath);
        NativeGallery.ImageOrientation orientation = imageProperties.orientation;

        postByte = File.ReadAllBytes(imagePath);
        texture = new Texture2D(2, 2);
        texture.LoadImage(postByte);

        if (orientation == NativeGallery.ImageOrientation.Rotate90)
        {
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
        imgr.texture = texture;
        imgr.SetNativeSize();
        ImageSizeSetting(img, 500, 400);
        ImageSizeSetting(pop_img, 550, 450);
    }
    IEnumerator ServerImage(string url)
    {
        UnityWebRequest www = UnityWebRequestTexture.GetTexture("http://34.22.102.33:8000" + url);
        yield return www.SendWebRequest();
        if (www.result == UnityWebRequest.Result.Success)
        {
            // 이미지 다운로드 성공
            Texture2D texture = DownloadHandlerTexture.GetContent(www);
            pop_img.texture = texture;
            pop_img.SetNativeSize();
            ImageSizeSetting(pop_img, 550, 450);
        }
        else messageText.text = "Failed to download image: " + www.error;
    }
    void getVideo()
    {
        if (!NativeGallery.IsMediaPickerBusy())
        {
            NativeGallery.GetVideoFromGallery((Video) =>
            {
                if (!string.IsNullOrEmpty(Video))
                {
                    vPath = Video;
                    postByte = File.ReadAllBytes(Video);
                    StartCoroutine(LoadVideo(Video, videoPlayer, video));
                }
            });
        }
    }
    IEnumerator LoadVideo(string videoPath, VideoPlayer vp, RawImage v)
    {
        yield return null;

        vp.url = videoPath;    // 비디오 플레이어에 비디오 경로 설정
        vp.Prepare();

        while (!vp.isPrepared) // Prepare가 완료될 때까지 대기
        {
            yield return null;
        }

        v.texture = vp.texture;
        v.SetNativeSize();
        ImageSizeSetting(video, 500, 400);
        ImageSizeSetting(pop_img, 550, 450);

        vp.Play(); // 비디오 재생
    }
    IEnumerator LoadVideo2(string videoURL)
    {
        UnityWebRequest www = UnityWebRequest.Get("http://34.22.102.33:8000" + videoURL);
        www.downloadHandler = new DownloadHandlerBuffer();
        yield return www.SendWebRequest();

        if (www.result == UnityWebRequest.Result.Success)
        {
            byte[] videoData = www.downloadHandler.data;
            string videoName = Path.GetFileName(videoURL);
            string tempPath = Application.persistentDataPath + "/" + videoName;
            File.WriteAllBytes(tempPath, videoData);

            vp.url = tempPath;
            vp.Prepare();
            while (!vp.isPrepared)
            {
                yield return null;
            }
            pop_img.texture = vp.texture;
            pop_img.SetNativeSize();
            ImageSizeSetting(pop_img, 550, 450);
            vp.Play();
        }
        else messageText.text = "Failed to download video: " + www.error;
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
                    postByte = File.ReadAllBytes(audio);
                }
            });
        }
    }
    IEnumerator LoadAudio(string audioPath, AudioSource audioSource)
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
            else Debug.LogError("Failed to load audio: " + www.error);
            audioSource.volume = 1.0f;
        }
    }
    IEnumerator LoadAudio2(string url)
    {
        UnityWebRequest www = UnityWebRequestMultimedia.GetAudioClip("http://34.22.102.33:8000" + url, AudioType.MPEG);
        yield return www.SendWebRequest();

        if (www.result == UnityWebRequest.Result.Success)
        {
            AudioClip audioClip = DownloadHandlerAudioClip.GetContent(www);
            aSource.clip = audioClip;
            aSource.Play();
        }
        else messageText.text = "Failed to download audio: " + www.error;
        aSource.volume = 1.0f;
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

    void Resolving()
    {
        if (memoList.Count == 0)
        {
            mode = Mode.RESOLVE_PENDING;
            return;
        }
        messageText.text = "";

        foreach (var item in memoList)
        {
            string id = item.anchorId;
            ARCloudAnchor cloudAnchor = anchorManager.ResolveCloudAnchorId(id);
            if (cloudAnchor != null)
            {
                cloudAnchors.Add(cloudAnchor, item);
                messageText.text += cloudAnchors.Count.ToString();
                memoList.Remove(item);
            }
            else messageText.text += "실패";
        }
        if (memoList.Count == 0) mode = Mode.RESOLVE_PENDING;
    }

    void ResolvePending()
    {
        messageText.text = "ResolvePending";
        if (cloudAnchors.Count == 0) return;
        bool allAnchorsResolved = true;
        foreach (KeyValuePair<ARCloudAnchor, Memo> item in cloudAnchors)
        {
            if (item.Key.cloudAnchorState == CloudAnchorState.Success)
            {
                // 객체 증강
                if (item.Value.memoType == "A")
                {
                    textPrefab.transform.Find("nickname").GetComponent<TextMesh>().text = item.Value.nickname;
                    anchorGameObjects.Add(Instantiate(textPrefab, item.Key.transform), item.Value);
                }
                else if (item.Value.memoType == "B")
                {
                    imagePrefab.transform.Find("nickname").GetComponent<TextMesh>().text = item.Value.nickname;
                    anchorGameObjects.Add(Instantiate(imagePrefab, item.Key.transform), item.Value);
                }
                else if (item.Value.memoType == "D")
                {
                    videoPrefab.transform.Find("nickname").GetComponent<TextMesh>().text = item.Value.nickname;
                    anchorGameObjects.Add(Instantiate(videoPrefab, item.Key.transform), item.Value);
                }
                else if (item.Value.memoType == "C")
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
        if (allAnchorsResolved) messageText.text = "리졸브 성공";
    }

    void Checking()
    {
        //messageText.text = "checking";
        if (Input.touchCount == 0) return;
        Touch touch = Input.GetTouch(0);

        if (touch.phase == TouchPhase.Began)    //터치 시작시
        {
            Ray ray;
            RaycastHit hitobj;
            ray = arCamera.ScreenPointToRay(touch.position);

            //Ray를 통한 오브젝트 인식
            int layerMask = 1 << LayerMask.NameToLayer("Cube");
            if (Physics.Raycast(ray, out hitobj, 500f, layerMask))
            {
                Memo anchor = anchorGameObjects[hitobj.collider.gameObject];
                if (anchor.memoType == "A")
                {
                    MEMO.text = (string)anchorGameObjects[hitobj.collider.gameObject].memo_content;
                }
                else if (anchor.memoType == "B")
                {
                    if (anchor.memo_content.Contains("media/picture")) StartCoroutine(ServerImage(anchor.memo_content));
                    else StartCoroutine(LoadImage(anchor.memo_content, pop_img));
                }
                else if (anchor.memoType == "D")
                {
                    vp.gameObject.SetActive(true);
                    if (anchor.memo_content.Contains("media/video")) StartCoroutine(LoadVideo2(anchor.memo_content));
                    else StartCoroutine(LoadVideo(anchor.memo_content, vp, pop_img));
                }
                else if (anchor.memoType == "C")
                {
                    aPath = anchor.memo_content;
                    buttonRP.gameObject.SetActive(true);
                    buttonRS.gameObject.SetActive(true);
                    pop_img.gameObject.SetActive(false);
                }
                PopUp_R.SetActive(true);
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

    private void OnResetClick()
    {
        if (anchorGameObject != null)
        {
            Destroy(anchorGameObject);
            Destroy(indicatorGO);
        }
        foreach (var obj in anchorGameObjects.Keys)
        {
            Destroy(obj);
        }
        anchorGameObjects.Clear();
        cloudAnchor = null; localAnchor = null;
        memoList.Clear(); cloudAnchors.Clear();
        mode = Mode.READY;
        messageText.text = "Ready";
    }

    private void OnCancelClick()
    {
        if (anchorGameObject != null)
        {
            Destroy(anchorGameObject);
            Destroy(indicatorGO);
        }
        cloudAnchor = null; localAnchor = null;
        mode = Mode.RESOLVE_PENDING;
        cancelButton.gameObject.SetActive(false);
        hostButton.gameObject.SetActive(true);
        messageText.text = "";
    }


}