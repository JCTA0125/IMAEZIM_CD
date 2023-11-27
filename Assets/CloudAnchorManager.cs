using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;

using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;

using Google.XR.ARCoreExtensions;
using System;
using System.Linq;
using System.IO;

public class CloudAnchorManager : MonoBehaviour
{
    // 상태 변수
    public enum Mode { READY, HOST, HOST_PENDING, RESOLVE, RESOLVE_PENDING };

    // 버튼
    public Button hostButton;       // 클라우드 앵커 등록
    public Button resolveButton;    // 클라우드 앵커 조회
    public Button resetButton;      // 리셋

    // 메세지 출력 텍스트
    public Text messageText;

    // 상태변수
    public Mode mode = Mode.READY;
    // AnchorManager    // 로컬 앵커를 생성하기 위한 클래스
    public ARAnchorManager anchorManager;
    // ArRaycastManager
    public ARRaycastManager raycastManager;

    // 증강시킬 객체 프리팹
    public GameObject anchorPrefab;
    // 저장 객체 변수 (삭제하기 위한 용도)
    private GameObject anchorGameObject;
    //private List<GameObject> anchorGameObjects = new List<GameObject>();
    private Dictionary<GameObject, object> anchorGameObjects = new Dictionary<GameObject, object>();

    // 로컬앵커 저장 변수
    private ARAnchor localAnchor;
    // 클라우드 앵커 변수
    private ARCloudAnchor cloudAnchor;
   // private List<ARCloudAnchor> cloudAnchors = new List<ARCloudAnchor>();
    private Dictionary<ARCloudAnchor, object> cloudAnchors = new Dictionary<ARCloudAnchor, object>();


    // Raycast Hit
    private List<ARRaycastHit> hits = new List<ARRaycastHit>();

    //private List<string> cloudAnchorIds = new List<string>();
    private Dictionary<string, object> cloudAnchorIds = new Dictionary<string, object>();


    public GameObject PopUp_H, PopUp_T, PopUp_I, PopUp_V, PopUp_R;
    public Button buttonT, buttonI, buttonTC, buttonIS, buttonIC, buttonVS, buttonVC, buttonX;
    public InputField inputT;
    public Text MEMO;
    public RawImage img;
    public Texture2D texture;
    public RawImage pop_img;
    //public VideoPlayer vp;
    //public RawImage vd;
    //public RenderTexture renderTexture;

    [SerializeField] private Camera arCamera;

    void Start()
    {
        // 버튼 이벤트 연결
        hostButton.onClick.AddListener(() => OnHostClick());
        resolveButton.onClick.AddListener(() => OnResolveClick());
        resetButton.onClick.AddListener(() => OnResetClick());
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
            Checking();
        }
    }

    void Hosting()
    {
        if (Input.touchCount < 1) return;

        Touch touch = Input.GetTouch(0);
        if (touch.phase != TouchPhase.Began) return;


        // 로컬 앵커가 존재하는지 여부를 확인
        if (localAnchor == null)
        {
            // Raycast 발사
            if (raycastManager.Raycast(touch.position, hits, TrackableType.PlaneWithinPolygon))
            {
                // 로컬 앵커 생성
                localAnchor = anchorManager.AddAnchor(hits[0].pose);
                // 로컬 앵커 위치에 객체 증강시키고 변수에 저장
                anchorGameObject = Instantiate(anchorPrefab, localAnchor.transform);
            }
        }
    }

    // 클라우드 앵커 등록
    void HostProcessing()
    {
        if (localAnchor == null) return;

        // 피쳐포인트의 갯수 및 퀄리티 측정
        FeatureMapQuality quality = anchorManager.EstimateFeatureMapQualityForHosting(GetCameraPose());

        string mappingText = string.Format("맵핑 품질 = {0}", quality);

        // 맵핑 퀄리티가 1 이상일 때 호스팅 요청
        if (quality == FeatureMapQuality.Sufficient || quality == FeatureMapQuality.Good)
        {
            // 1일짜리 앵커포인트
            cloudAnchor = anchorManager.HostCloudAnchor(localAnchor, 1);

            if (cloudAnchor == null)
            {
                mappingText = "클라우드 앵커 생성 실패";
            }
            else
            {
                // 여기서 클라우드 앵커 ID 찍어도 안나옴
                // 서버에서 작업하는 시간이 있기 때문에
                mappingText = "클라우드 앵커 생성 시작";
                mode = Mode.HOST_PENDING;
            }
        }

        messageText.text = mappingText;
    }

    //갤러리 이미지
    void getImage()
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
    //이미지 로드 코루틴            
    IEnumerator LoadImage(string imagePath)
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
    //void getVideo()
    //{
    //    NativeGallery.GetVideoFromGallery((video) => {
    //        FileInfo selectedVideo = new FileInfo(video);

    //        if (!string.IsNullOrEmpty(video))
    //        {
    //            StartCoroutine(LoadVideo(video));
    //        }
    //    });
    //}
    //IEnumerator LoadVideo(string videoPath)
    //{
    //    yield return null;

    //    var tempVideo = File.ReadAllBytes(videoPath);
    //    //renderTexture = new RenderTexture((int)vp.width, (int)vp.height, 0);
    //    //vp.targetTexture = renderTexture;

    //    if (vp == null)
    //        vp = gameObject.AddComponent<VideoPlayer>();

    //    //VideoPlayer에 소스 비디오 지정
    //    vp.url = videoPath;
    //    vp.source = VideoSource.Url;

    //    //비디오 화면을 표시할 videoDisplay의 texture를 VideoPlayer의 targetTexture로 지정
    //    vd.texture = vp.targetTexture;

    //    vp.Prepare();

    //    while (!vp.isPrepared)
    //        yield return null;

    //    //재생
    //    vp.Play();
    //}

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

    void createMemo()
    {
        PopUp_H.SetActive(true);
        buttonT.onClick.AddListener(() =>
        {
            PopUp_H.SetActive(false);
            PopUp_T.SetActive(true);
        });
        buttonTC.onClick.AddListener(() => {
            PopUp_T.SetActive(false);
            cloudAnchorIds.Add(cloudAnchor.cloudAnchorId, inputT.text);
            localAnchor = null; cloudAnchor = null; Destroy(anchorGameObject);
            inputT.text = "";
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
        buttonIC.onClick.AddListener(() => {
            PopUp_I.SetActive(false);
            cloudAnchorIds.Add(cloudAnchor.cloudAnchorId, texture);
            localAnchor = null; cloudAnchor = null; Destroy(anchorGameObject);
            texture = null; img.texture = null;
            ImageSizeReturn(img, 300, 250);
        });
    }

    void HostPending()
    {
        string mappingText = "";
        if (cloudAnchor.cloudAnchorState == CloudAnchorState.Success)
        {
            createMemo();
            mappingText = $"클라우드 앵커 생성 성공, CloudAnchor ID = {cloudAnchor.cloudAnchorId}";

            mode = Mode.READY;
        }
        else
        {
            mappingText = $"클라우드 앵커 생성 진행중...{cloudAnchor.cloudAnchorState}";
        }

        messageText.text = mappingText;
    }

    //void Resolving()
    //{
    //    if (cloudAnchorIds.Count == 0) return;
    //    messageText.text = "";

    //    //foreach (string cloudId in cloudAnchorIds.Keys)
    //    foreach (KeyValuePair<string, object> item in cloudAnchorIds)
    //    {
    //        // 클라우드 앵커 ID로 CloudAnchor 로드
    //        cloudAnchors.Add(anchorManager.ResolveCloudAnchorId(item.Key), item.Value);
    //    }
    //    if (cloudAnchorIds.Count == cloudAnchors.Count)
    //    {
    //        mode = Mode.RESOLVE_PENDING;
    //    }
    //}
    IEnumerator Resolving()
    {
        if (cloudAnchorIds.Count == 0) yield break;
        messageText.text = "";

        var enumerator = cloudAnchorIds.GetEnumerator();
        while (enumerator.MoveNext())
        {
            var item = enumerator.Current;
            ARCloudAnchor anchor = anchorManager.ResolveCloudAnchorId(item.Key);
            yield return new WaitUntil(() => anchor != null); // 기다림
            cloudAnchors.Add(anchor, item.Value);
        }

        if (cloudAnchorIds.Count == cloudAnchors.Count)
        {
            mode = Mode.RESOLVE_PENDING;
        }
    }

    void ResolvePending()
    {
        bool allAnchorsResolved = true;
        foreach (KeyValuePair<ARCloudAnchor, object> item in cloudAnchors)
        {
            if (item.Key.cloudAnchorState == CloudAnchorState.Success)
            {
                // 객체 증강
                anchorGameObjects.Add(Instantiate(anchorPrefab, item.Key.transform), item.Value);
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
            mode = Mode.READY;
        }
        //if (cloudAnchors.Count == anchorGameObjects.Count)
        //{
        //    mode = Mode.READY;
        //}
    }

    void Checking()
    {
        if (Input.touchCount == 0) return;
        Touch touch = Input.GetTouch(0);

        //터치 시작시
        if (touch.phase == TouchPhase.Began)
        {
            Ray ray;
            RaycastHit hitobj;

            ray = arCamera.ScreenPointToRay(touch.position);

            //Ray를 통한 오브젝트 인식
            int layerMask = 1 << LayerMask.NameToLayer("Cube");
            if (Physics.Raycast(ray, out hitobj, 500f, layerMask) && anchorGameObjects.Count > 0)
            {
                PopUp_R.SetActive(true);
                if (anchorGameObjects[hitobj.collider.gameObject] is string)
                {
                    MEMO.text = (string)anchorGameObjects[hitobj.collider.gameObject];
                }
                else if (anchorGameObjects[hitobj.collider.gameObject] is Texture2D)
                {
                    pop_img.texture = (Texture2D)anchorGameObjects[hitobj.collider.gameObject];
                    pop_img.SetNativeSize();
                    ImageSizeSetting(pop_img, 300, 250);
                }
                buttonX.onClick.AddListener(() =>
                {
                    PopUp_R.SetActive(false);
                    MEMO.text = "";
                    pop_img.texture = null;
                    ImageSizeReturn(pop_img, 360, 250);
                });
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

    private void OnResolveClick()
    {
        //mode = Mode.RESOLVE;
        StartCoroutine(Resolving());
    }

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
        cloudAnchorIds.Clear();
        cloudAnchors.Clear();
        mode = Mode.READY;
    }
}