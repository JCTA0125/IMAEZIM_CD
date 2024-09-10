// <copyright file="GeospatialController.cs" company="Google LLC">
//
// Copyright 2022 Google LLC
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
// http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
//
// </copyright>
//-----------------------------------------------------------------------
using TMPro;

namespace Google.XR.ARCoreExtensions.Samples.Geospatial
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.Linq;
    using UnityEngine;
    using UnityEngine.EventSystems;
    using UnityEngine.UI;
    using TMPro;
    using System.Threading.Tasks;
    using UnityEngine.XR.ARFoundation;
    using UnityEngine.XR.ARSubsystems;
    using Unity.XR.CoreUtils;
    using UnityEngine.Networking;
#if UNITY_ANDROID

    using UnityEngine.Android;

#endif

    [System.Serializable]
    public class ResponseId
    {
        public int id;
    }

    [System.Serializable]
    public class ResponseImage
    {
        public string picture;
    }

    [System.Serializable]
    public class ResponseVideo
    {
        public string video;
    }
    /// <summary>
    /// Controller for Geospatial sample.
    /// </summary>
    [SuppressMessage("StyleCop.CSharp.ReadabilityRules", "SA1118:ParameterMustNotSpanMultipleLines",
        Justification = "Bypass source check.")]
    public class GeospatialController : MonoBehaviour
    {
        [Header("AR Components")]

        public Boolean isRay = false;

        /// <summary>
        /// The ARSessionOrigin used in the sample.
        /// </summary>
        public XROrigin SessionOrigin;

        /// <summary>
        /// The ARSession used in the sample.
        /// </summary>
        public ARSession Session;

        /// <summary>
        /// The ARAnchorManager used in the sample.
        /// </summary>
        public ARAnchorManager AnchorManager;

        /// <summary>
        /// The ARRaycastManager used in the sample.
        /// </summary>
        public ARRaycastManager RaycastManager;

        /// <summary>
        /// The AREarthManager used in the sample.
        /// </summary>
        public AREarthManager EarthManager;

        /// <summary>
        /// The ARStreetscapeGeometryManager used in the sample.
        /// </summary>
        public ARStreetscapeGeometryManager StreetscapeGeometryManager;

        /// <summary>
        /// The ARCoreExtensions used in the sample.
        /// </summary>
        public ARCoreExtensions ARCoreExtensions;

        /// <summary>
        /// The StreetscapeGeometry materials for rendering geometry building meshes.
        /// </summary>
        public List<Material> StreetscapeGeometryMaterialBuilding;

        /// <summary>
        /// The StreetscapeGeometry material for rendering geometry terrain meshes.
        /// </summary>
        public Material StreetscapeGeometryMaterialTerrain;

        [Header("UI Elements")]

        /// <summary>
        /// A 3D object that presents a Geospatial Anchor.
        /// </summary>
        public GameObject GeospatialPrefab;  //Text
        public GameObject GeospatialPrefab2;  //Image
        public GameObject GeospatialPrefab3;  //Video
        public GameObject GeospatialPrefabArrow;
        public GameObject GeospatialPrefabGoal;
        public GameObject GeospatialPrefabArrow_game;
        public GameObject GeospatialPrefabGoal_game;
        public Text info;
        /// <summary>
        /// A 3D object that presents a Geospatial Terrain anchor.
        /// </summary>
        public GameObject TerrainPrefab;

        /// <summary>
        /// UI element showing privacy prompt.
        /// </summary>
        public GameObject PrivacyPromptCanvas;

        /// <summary>ClearAllButton
        /// UI element showing VPS availability notification.
        /// </summary>
        public GameObject VPSCheckCanvas;

        /// <summary>
        /// UI element containing all AR view contents.
        /// </summary>
        public GameObject ARViewCanvas;

        /// <summary>
        /// UI element for clearing all anchors, including history.
        /// </summary>
        public Button ClearAllButton;

        /// <summary>
        /// UI element that enables streetscape geometry visibility.
        /// </summary>
        public Toggle GeometryToggle;

        /// <summary>
        /// UI element to display or hide the Anchor Settings panel.
        /// </summary>
        public Button AnchorSettingButton;

        /// <summary>
        /// UI element for the Anchor Settings panel.
        /// </summary>
        public GameObject AnchorSettingPanel;

        /// <summary>
        /// UI element that toggles anchor type to Geometry.
        /// </summary>
        public Toggle GeospatialAnchorToggle;

        /// <summary>
        /// UI element that toggles anchor type to Terrain.
        /// </summary>
        public Toggle TerrainAnchorToggle;

        /// <summary>
        /// UI element that toggles anchor type to Rooftop.
        /// </summary>
        public Toggle RooftopAnchorToggle;

        /// <summary>
        /// UI element to display information at runtime.
        /// </summary>
        public GameObject InfoPanel;

        /// <summary>
        /// Text displaying <see cref="GeospatialPose"/> information at runtime.
        /// </summary>
        public Text InfoText;

        /// <summary>
        /// Text displaying in a snack bar at the bottom of the screen.
        /// </summary>
        public Text SnackBarText;

        /// <summary>
        /// Text displaying debug information, only activated in debug build.
        /// </summary>
        public Text DebugText;

        //3d object
        //public TextMeshPro Text_3d;
        //public TextMeshPro TextWriter_3d;
        //public TextMeshPro testText;  //
        public Text test;  //

        /// <summary>
        /// Help message shown while localizing.
        /// </summary>
        private const string _localizingMessage = "Localizing your device to set anchor.";

        /// <summary>
        /// Help message shown while initializing Geospatial functionalities.
        /// </summary>
        private const string _localizationInitializingMessage =
            "Initializing Geospatial functionalities.";

        /// <summary>
        /// Help message shown when <see cref="AREarthManager.EarthTrackingState"/> is not tracking
        /// or the pose accuracies are beyond thresholds.
        /// </summary>
        private const string _localizationInstructionMessage = "현재 위치 확인중";
        // "Point your camera at buildings, stores, and signs near you.";

        /// <summary>
        /// Help message shown when location fails or hits timeout.
        /// </summary>
        private const string _localizationFailureMessage = "gps 정보가 없습니다";
        //"Localization not possible.\n" +
        //  "Close and open the app to restart the session.";

        /// <summary>
        /// Help message shown when localization is completed.
        /// </summary>
        private const string _localizationSuccessMessage = "Localization completed.";

        /// <summary>
        /// The timeout period waiting for localization to be completed.
        /// </summary>
        private const float _timeoutSeconds = 180;

        /// <summary>
        /// Indicates how long a information text will display on the screen before terminating.
        /// </summary>
        private const float _errorDisplaySeconds = 3;

        /// <summary>
        /// The key name used in PlayerPrefs which indicates whether the privacy prompt has
        /// displayed at least one time.
        /// </summary>
        private const string _hasDisplayedPrivacyPromptKey = "HasDisplayedGeospatialPrivacyPrompt";

        /// <summary>
        /// The key name used in PlayerPrefs which stores geospatial anchor history data.
        /// The earliest one will be deleted once it hits storage limit.
        /// </summary>
        private const string _persistentGeospatialAnchorsStorageKey = "PersistentGeospatialAnchors";
        //길찾기
        private const string _persistentGeospatialAnchorsStorageKey_nav = "PersistentGeospatialAnchors_nav";

        /// <summary>
        /// The limitation of how many Geospatial Anchors can be stored in local storage.
        /// </summary>
        private const int _storageLimit = 200;

        /// <summary>
        /// Accuracy threshold for orientation yaw accuracy in degrees that can be treated as
        /// localization completed.
        /// </summary>
        private const double _orientationYawAccuracyThreshold = 25;

        /// <summary>
        /// Accuracy threshold for heading degree that can be treated as localization completed.
        /// </summary>
        private const double _headingAccuracyThreshold = 25;

        /// <summary>
        /// Accuracy threshold for altitude and longitude that can be treated as localization
        /// completed.
        /// </summary>
        private const double _horizontalAccuracyThreshold = 20;

        /// <summary>
        /// Determines if the anchor settings panel is visible in the UI.
        /// </summary>
        private bool _showAnchorSettingsPanel = false;

        /// <summary>
        /// Represents the current anchor type of the anchor being placed in the scene.
        /// </summary>
        private AnchorType _anchorType = AnchorType.Geospatial;

        /// <summary>
        /// Determines if streetscape geometry is rendered in the scene.
        /// </summary>
        private bool _streetscapeGeometryVisibility = false;

        //3d 오브젝트 터치 활성화 여부
        private bool _3dObjTouch = false;

        /// <summary>
        /// Determines which building material will be used for the current building mesh.
        /// </summary>
        private int _buildingMatIndex = 0;

        /// <summary>
        /// Dictionary of streetscapegeometry handles to render objects for rendering
        /// streetscapegeometry meshes.
        /// </summary>
        private Dictionary<TrackableId, GameObject> _streetscapegeometryGOs =
            new Dictionary<TrackableId, GameObject>();

        /// <summary>
        /// ARStreetscapeGeometries added in the last Unity Update.
        /// </summary>
        List<ARStreetscapeGeometry> _addedStreetscapeGeometries =
            new List<ARStreetscapeGeometry>();

        /// <summary>
        /// ARStreetscapeGeometries updated in the last Unity Update.
        /// </summary>
        List<ARStreetscapeGeometry> _updatedStreetscapeGeometries =
            new List<ARStreetscapeGeometry>();

        /// <summary>
        /// ARStreetscapeGeometries removed in the last Unity Update.
        /// </summary>
        List<ARStreetscapeGeometry> _removedStreetscapeGeometries =
            new List<ARStreetscapeGeometry>();

        /// <summary>
        /// Determines if streetscape geometry should be removed from the scene.
        /// </summary>
        private bool _clearStreetscapeGeometryRenderObjects = false;

        private bool _waitingForLocationService = false;
        private bool _isInARView = false;
        private bool _isReturning = false;
        private bool _isLocalizing = false;
        private bool _enablingGeospatial = false;
        public bool _shouldResolvingHistory = false;
        public bool _shouldResolvingHistory_nav = false;
        private float _localizationPassedTime = 0f;
        private float _configurePrepareTime = 3f;
        public GeospatialAnchorHistoryCollection _historyCollection = null;
        public GeospatialAnchorHistoryCollection_nav _historyCollection_nav = null;
        public List<GameObject> _anchorObjects = new List<GameObject>();
        public List<GameObject> _anchorObjects_nav = new List<GameObject>();

        private IEnumerator _startLocationService = null;
        private IEnumerator _asyncCheck = null;

        //3d 오브젝트 터치
        [SerializeField] private Camera arCamera;

        private string AndUserId = "a@a.com";

        public void AndUserInfo(string id)  //안드로이드에서 호출할 함수
        {
            Debug.Log("AndUserInfo 실행 id = " + id);
            AndUserId = id;
        }

        private string UserNickName = "Kim";

        public void AndUserNick(string nickname)  //안드로이드에서 호출할 함수
        {
            Debug.Log("AndUserNick 실행 nickname = " + nickname);
            UserNickName = nickname;
        }

        /// <summary>
        /// Callback handling "Get Started" button click event in Privacy Prompt.
        /// </summary>
        public void OnGetStartedClicked()
        {
            PlayerPrefs.SetInt(_hasDisplayedPrivacyPromptKey, 1);
            PlayerPrefs.Save();
            SwitchToARView(true);
        }

        /// <summary>
        /// Callback handling "Learn More" Button click event in Privacy Prompt.
        /// </summary>
        public void OnLearnMoreClicked()
        {
            Application.OpenURL(
                "https://developers.google.com/ar/data-privacy");
        }

        /// <summary>
        /// Callback handling "Clear All" button click event in AR View.
        /// </summary>
        public void OnClearAllClicked()
        {
            foreach (var anchor in _anchorObjects)
            {
                Destroy(anchor);
            }

            _anchorObjects.Clear();
            _historyCollection.Collection.Clear();
            SnackBarText.text = "Anchor(s) cleared!";
            ClearAllButton.gameObject.SetActive(false);
            SaveGeospatialAnchorHistory();
        }

        public void OnClearAllClicked_nav()  //길찾기
        {
            foreach (var anchor in _anchorObjects_nav)
            {
                Destroy(anchor);
            }

            _anchorObjects_nav.Clear();
            _historyCollection_nav.Collection.Clear();
            //SnackBarText.text = "Anchor(s) cleared!";
            //ClearAllButton.gameObject.SetActive(false);
            SaveGeospatialAnchorHistory_nav();

            gpsLine.Clear();
            gpsPoint.Clear();
            lineList.Clear();

            PlayerPrefs.SetString("memoLatitudeKey", "0.0");
            PlayerPrefs.SetString("memoLongitudeKey", "0.0");
        }


        /// <summary>
        /// Callback handling "Continue" button click event in AR View.
        /// </summary>
        public void OnContinueClicked()
        {
            VPSCheckCanvas.SetActive(false);
        }

        /// <summary>
        /// Callback handling "Geometry" toggle event in AR View.
        /// </summary>
        /// <param name="enabled">Whether to enable Streetscape Geometry visibility.</param>
        public void OnGeometryToggled(bool enabled)
        {
            _streetscapeGeometryVisibility = enabled;
            if (!_streetscapeGeometryVisibility)
            {
                _clearStreetscapeGeometryRenderObjects = true;
            }
            //3d 오브젝트 터치 활성화 or 비활성화
            _3dObjTouch = enabled;
        }

        /// <summary>
        /// Callback handling the  "Anchor Setting" panel display or hide event in AR View.
        /// </summary>
        public void OnAnchorSettingButtonClicked()
        {
            _showAnchorSettingsPanel = !_showAnchorSettingsPanel;
            if (_showAnchorSettingsPanel)
            {
                SetAnchorPanelState(false); //
            }
            else
            {
                SetAnchorPanelState(false);
            }
        }

        /// <summary>
        /// Callback handling Geospatial anchor toggle event in AR View.
        /// </summary>
        /// <param name="enabled">Whether to enable Geospatial anchors.</param>
        public void OnGeospatialAnchorToggled(bool enabled)
        {
            // GeospatialAnchorToggle.GetComponent<Toggle>().isOn = true;;
            _anchorType = AnchorType.Geospatial;
            SetAnchorPanelState(false);
        }

        /// <summary>
        /// Callback handling Terrain anchor toggle event in AR View.
        /// </summary>
        /// <param name="enabled">Whether to enable Terrain anchors.</param>
        public void OnTerrainAnchorToggled(bool enabled)
        {
            // TerrainAnchorToggle.GetComponent<Toggle>().isOn = true;
            _anchorType = AnchorType.Terrain;
            SetAnchorPanelState(false);
        }

        /// <summary>
        /// Callback handling Rooftop anchor toggle event in AR View.
        /// </summary>
        /// <param name="enabled">Whether to enable Rooftop anchors.</param>
        public void OnRooftopAnchorToggled(bool enabled)
        {
            // RooftopAnchorToggle.GetComponent<Toggle>().isOn = true;
            _anchorType = AnchorType.Rooftop;
            SetAnchorPanelState(false);
        }

        /// <summary>
        /// Unity's Awake() method.
        /// </summary>
        public void Awake()
        {
            // Lock screen to portrait.
            Screen.autorotateToLandscapeLeft = false;
            Screen.autorotateToLandscapeRight = false;
            Screen.autorotateToPortraitUpsideDown = false;
            Screen.orientation = ScreenOrientation.Portrait;

            // Enable geospatial sample to target 60fps camera capture frame rate
            // on supported devices.
            // Note, Application.targetFrameRate is ignored when QualitySettings.vSyncCount != 0.
            Application.targetFrameRate = 60;

            if (SessionOrigin == null)
            {
                Debug.LogError("Cannot find ARSessionOrigin.");
            }

            if (Session == null)
            {
                Debug.LogError("Cannot find ARSession.");
            }

            if (ARCoreExtensions == null)
            {
                Debug.LogError("Cannot find ARCoreExtensions.");
            }
        }
        string navMode = ""; //길찾기 모드
        public Button altitudeBtn;
        public Button altitudeBtn_down;
        public Button altitudeBtn_up;
        public Button clearNavBtn;
        public Text distanceText;
        public GameObject navPanel;

        public void Start()
        {
            //PlayerPrefs.SetString("NavigationMode", "gameNav");  //나중에 주석 -> 게임장 찾기 버튼 클릭시 호출 
            //PlayerPrefs.SetString("NavigationMode", "memoNav");  // -> 길찾기 기본 모드
            navMode = PlayerPrefs.GetString("NavigationMode"); 

            PlayerPrefs.SetString("startLatitudeKey", "0.0");
            PlayerPrefs.SetString("startLongitudeKey", "0.0");
            Debug.Log("nav : Start");
            Start_nav();  //길찾기
        }

        /// <summary>
        /// Unity's OnEnable() method.
        /// </summary>
        public void OnEnable()
        {
            _startLocationService = StartLocationService();
            StartCoroutine(_startLocationService);

            _isReturning = false;
            _enablingGeospatial = false;
            InfoPanel.SetActive(false);
            GeometryToggle.gameObject.SetActive(false);
            AnchorSettingButton.gameObject.SetActive(false);
            AnchorSettingPanel.gameObject.SetActive(false);
            GeospatialAnchorToggle.gameObject.SetActive(false);
            TerrainAnchorToggle.gameObject.SetActive(false);
            RooftopAnchorToggle.gameObject.SetActive(false);
            ClearAllButton.gameObject.SetActive(false);
            DebugText.gameObject.SetActive(Debug.isDebugBuild && EarthManager != null);
            GeometryToggle.onValueChanged.AddListener(OnGeometryToggled);
            AnchorSettingButton.onClick.AddListener(OnAnchorSettingButtonClicked);
            GeospatialAnchorToggle.onValueChanged.AddListener(OnGeospatialAnchorToggled);
            TerrainAnchorToggle.onValueChanged.AddListener(OnTerrainAnchorToggled);
            RooftopAnchorToggle.onValueChanged.AddListener(OnRooftopAnchorToggled);

            _localizationPassedTime = 0f;
            _isLocalizing = true;
            SnackBarText.text = _localizingMessage;

            LoadGeospatialAnchorHistory();
            _shouldResolvingHistory = _historyCollection.Collection.Count > 0;
            //길찾기
            LoadGeospatialAnchorHistory_nav();
            _shouldResolvingHistory_nav = _historyCollection_nav.Collection.Count > 0;

            SwitchToARView(PlayerPrefs.HasKey(_hasDisplayedPrivacyPromptKey));

            if (StreetscapeGeometryManager == null)
            {
                Debug.LogWarning("StreetscapeGeometryManager must be set in the " +
                    "GeospatialController Inspector to render StreetscapeGeometry.");
            }

            if (StreetscapeGeometryMaterialBuilding.Count == 0)
            {
                Debug.LogWarning("StreetscapeGeometryMaterialBuilding in the " +
                    "GeospatialController Inspector must contain at least one material " +
                    "to render StreetscapeGeometry.");
                return;
            }

            if (StreetscapeGeometryMaterialTerrain == null)
            {
                Debug.LogWarning("StreetscapeGeometryMaterialTerrain must be set in the " +
                    "GeospatialController Inspector to render StreetscapeGeometry.");
                return;
            }

            // get access to ARstreetscapeGeometries in ARStreetscapeGeometryManager
            if (StreetscapeGeometryManager)
            {
                StreetscapeGeometryManager.StreetscapeGeometriesChanged += GetStreetscapeGeometry;
            }
        }

        /// <summary>
        /// Unity's OnDisable() method.
        /// </summary>
        public void OnDisable()
        {
            StopCoroutine(_asyncCheck);
            _asyncCheck = null;
            StopCoroutine(_startLocationService);
            _startLocationService = null;
            Debug.Log("Stop location services.");
            Input.location.Stop();

            foreach (var anchor in _anchorObjects)
            {
                Destroy(anchor);
            }
            _anchorObjects.Clear();
            SaveGeospatialAnchorHistory();

            //길찾기
            foreach (var anchor in _anchorObjects_nav)
            {
                Destroy(anchor);
            }
            _anchorObjects_nav.Clear();
            SaveGeospatialAnchorHistory_nav();


            if (StreetscapeGeometryManager)
            {
                StreetscapeGeometryManager.StreetscapeGeometriesChanged -=
                    GetStreetscapeGeometry;
            }
        }

        /// <summary>
        /// Unity's Update() method.
        /// </summary>
        public void Update()
        {
            if (isRay)
            {
                RaycastManager.enabled = true;
            }
            if (memoLatitude != 0.0)
            {
                altitudeBtn.gameObject.SetActive(true);
                altitudeBtn_down.gameObject.SetActive(true);
                altitudeBtn_up.gameObject.SetActive(true);
                clearNavBtn.gameObject.SetActive(true);
                distanceText.gameObject.SetActive(true);
                navPanel.gameObject.SetActive(true);
            }
            else
            {
                altitudeBtn.gameObject.SetActive(false);
                altitudeBtn_down.gameObject.SetActive(false);
                altitudeBtn_up.gameObject.SetActive(false);
                clearNavBtn.gameObject.SetActive(false);
                distanceText.gameObject.SetActive(false);
                navPanel.gameObject.SetActive(false);

            }

            //길찾기
            if (lineList.Count == gpsLine.Count)
            {
                UpdateLine();
            }

            if (!_isInARView)
            {
                return;
            }

            UpdateDebugInfo();

            // Check session error status.
            LifecycleUpdate();
            if (_isReturning)
            {
                return;
            }

            if (ARSession.state != ARSessionState.SessionInitializing &&
                ARSession.state != ARSessionState.SessionTracking)
            {
                return;
            }

            // Check feature support and enable Geospatial API when it's supported.
            var featureSupport = EarthManager.IsGeospatialModeSupported(GeospatialMode.Enabled);
            switch (featureSupport)
            {
                case FeatureSupported.Unknown:
                    return;
                case FeatureSupported.Unsupported:
                    ReturnWithReason("The Geospatial API is not supported by this device.");
                    return;
                case FeatureSupported.Supported:
                    if (ARCoreExtensions.ARCoreExtensionsConfig.GeospatialMode ==
                        GeospatialMode.Disabled)
                    {
                        Debug.Log("Geospatial sample switched to GeospatialMode.Enabled.");
                        ARCoreExtensions.ARCoreExtensionsConfig.GeospatialMode =
                            GeospatialMode.Enabled;
                        ARCoreExtensions.ARCoreExtensionsConfig.StreetscapeGeometryMode =
                            StreetscapeGeometryMode.Enabled;
                        _configurePrepareTime = 3.0f;
                        _enablingGeospatial = true;
                        return;
                    }

                    break;
            }

            // Waiting for new configuration to take effect.
            if (_enablingGeospatial)
            {
                _configurePrepareTime -= Time.deltaTime;
                if (_configurePrepareTime < 0)
                {
                    _enablingGeospatial = false;
                }
                else
                {
                    return;
                }
            }

            // Check earth state.
            var earthState = EarthManager.EarthState;
            if (earthState == EarthState.ErrorEarthNotReady)
            {
                SnackBarText.text = _localizationInitializingMessage;
                return;
            }
            else if (earthState != EarthState.Enabled)
            {
                string errorMessage =
                    "Geospatial sample encountered an EarthState error: " + earthState;
                Debug.LogWarning(errorMessage);
                SnackBarText.text = errorMessage;
                return;
            }

            // Check earth localization.
            bool isSessionReady = ARSession.state == ARSessionState.SessionTracking &&
                Input.location.status == LocationServiceStatus.Running;
            var earthTrackingState = EarthManager.EarthTrackingState;
            var pose = earthTrackingState == TrackingState.Tracking ?
                EarthManager.CameraGeospatialPose : new GeospatialPose();
            if (!isSessionReady || earthTrackingState != TrackingState.Tracking ||
                pose.OrientationYawAccuracy > _orientationYawAccuracyThreshold ||
                pose.HorizontalAccuracy > _horizontalAccuracyThreshold)
            {
                // Lost localization during the session.
                if (!_isLocalizing)
                {
                    _isLocalizing = true;
                    _localizationPassedTime = 0f;
                    GeometryToggle.gameObject.SetActive(false);
                    AnchorSettingButton.gameObject.SetActive(false);
                    AnchorSettingPanel.gameObject.SetActive(false);
                    GeospatialAnchorToggle.gameObject.SetActive(false);
                    TerrainAnchorToggle.gameObject.SetActive(false);
                    RooftopAnchorToggle.gameObject.SetActive(false);
                    ClearAllButton.gameObject.SetActive(false);
                    foreach (var go in _anchorObjects)
                    {
                        go.SetActive(false);
                    }
                    foreach (var go in _anchorObjects_nav)
                    {
                        go.SetActive(false);
                    }
                }

                if (_localizationPassedTime > _timeoutSeconds)
                {
                    Debug.LogError("Geospatial sample localization timed out.");
                    ReturnWithReason(_localizationFailureMessage);
                }
                else
                {
                    _localizationPassedTime += Time.deltaTime;
                    SnackBarText.text = _localizationInstructionMessage;
                }
            }
            else if (_isLocalizing)
            {
                // Finished localization.
                _isLocalizing = false;
                _localizationPassedTime = 0f;
                GeometryToggle.gameObject.SetActive(true);
                AnchorSettingButton.gameObject.SetActive(true);
                ClearAllButton.gameObject.SetActive(_anchorObjects.Count > 0);
                SnackBarText.text = _localizationSuccessMessage;

                //길찾기
                //gps 안정화 -> 현재 위치 업데이트
                startLatitude = pose.Latitude;
                startLongitude = pose.Longitude;
                startAltitude = pose.Altitude;
                PlayerPrefs.SetString("startLatitudeKey", startLatitude.ToString());
                PlayerPrefs.SetString("startLongitudeKey", startLongitude.ToString());
                PlayerPrefs.SetString("startAltitudeKey", startAltitude.ToString());
                //Debug.Log("출발 위치 GPS" + startLatitude + " " + startLongitude + " " + startAltitude);

                foreach (var go in _anchorObjects)
                {
                    go.SetActive(true);
                }
                ResolveHistory();

                //길찾기
                foreach (var go in _anchorObjects_nav)
                {
                    go.SetActive(true);
                }
                ResolveHistory_nav();
            }
            else
            {
                if (_streetscapeGeometryVisibility)
                {
                    foreach (
                        ARStreetscapeGeometry streetscapegeometry in _addedStreetscapeGeometries)
                    {
                        InstantiateRenderObject(streetscapegeometry);
                    }

                    foreach (
                        ARStreetscapeGeometry streetscapegeometry in _updatedStreetscapeGeometries)
                    {
                        // This second call to instantiate is required if geometry is toggled on
                        // or off after the app has started.
                        InstantiateRenderObject(streetscapegeometry);
                        UpdateRenderObject(streetscapegeometry);
                    }

                    foreach (
                        ARStreetscapeGeometry streetscapegeometry in _removedStreetscapeGeometries)
                    {
                        DestroyRenderObject(streetscapegeometry);
                    }
                }
                else if (_clearStreetscapeGeometryRenderObjects)
                {
                    DestroyAllRenderObjects();
                    _clearStreetscapeGeometryRenderObjects = false;
                }

                if (Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Began
                    && !EventSystem.current.IsPointerOverGameObject(Input.GetTouch(0).fingerId)
                    && _anchorObjects.Count < _storageLimit)
                {
                    // Set anchor on screen tap.
                    PlaceAnchorByScreenTap(Input.GetTouch(0).position);
                }

                // Hide anchor settings and toggles if the storage limit has been reached.
                if (_anchorObjects.Count >= _storageLimit)
                {
                    AnchorSettingButton.gameObject.SetActive(false);
                    AnchorSettingPanel.gameObject.SetActive(false);
                    GeospatialAnchorToggle.gameObject.SetActive(false);
                    TerrainAnchorToggle.gameObject.SetActive(false);
                    RooftopAnchorToggle.gameObject.SetActive(false);
                }
                else
                {
                    AnchorSettingButton.gameObject.SetActive(true);
                }
            }

            InfoPanel.SetActive(true);
            if (earthTrackingState == TrackingState.Tracking)
            {
                /*
                InfoText.text = string.Format(
                "Latitude/Longitude: {1}°, {2}°{0}" +
                "Horizontal Accuracy: {3}m{0}" +
                "Altitude: {4}m{0}" +
                "Vertical Accuracy: {5}m{0}" +
                "Eun Rotation: {6}{0}" +
                "Orientation Yaw Accuracy: {7}°",
                Environment.NewLine,
                pose.Latitude.ToString("F6"),
                pose.Longitude.ToString("F6"),
                pose.HorizontalAccuracy.ToString("F6"),
                pose.Altitude.ToString("F2"),
                pose.VerticalAccuracy.ToString("F2"),
                pose.EunRotation.ToString("F1"),
                pose.OrientationYawAccuracy.ToString("F1"));
                */
                //길찾기
                //현재 위치, 고도 업데이트
                currentLatitude = pose.Latitude;
                currentLongitude = pose.Longitude;
                currentAltitude = pose.Altitude;
                //Debug.Log("update tDistance");
                //Debug.Log(string.Format("총거리 update : {0}", api.tDistance));
                //Debug.Log("총거리 : " + api.tDistance);
                //Debug.Log("총거리 : ");
                if (api.tDistance == 0)
                {
                    if (api.ApiResult == 0) {
                        info.text = "경로 정보가 존재하지 않습니다";
                    }
                }
                else
                {
                    info.text = string.Format(
                    "총 거리: {1}m{0}남은 거리: {2}m",
                    Environment.NewLine, api.tDistance, (int)(memoDistance * 1000));
                }
            }
            else
            {
                //InfoText.text = "GEOSPATIAL POSE: not tracking";
            }

            //3d 오브젝트 터치
            //Checking();
        }

        /// <summary>
        /// Connects the <c>ARStreetscapeGeometry</c> to the specified lists for access.
        /// </summary>
        /// <param name="eventArgs">The
        /// <c><see cref="ARStreetscapeGeometriesChangedEventArgs"/></c> containing the
        /// <c>ARStreetscapeGeometry</c>.
        /// </param>
        private void GetStreetscapeGeometry(ARStreetscapeGeometriesChangedEventArgs eventArgs)
        {
            _addedStreetscapeGeometries = eventArgs.Added;
            _updatedStreetscapeGeometries = eventArgs.Updated;
            _removedStreetscapeGeometries = eventArgs.Removed;
        }

        /// <summary>
        /// Sets up a render object for this <c>ARStreetscapeGeometry</c>.
        /// </summary>
        /// <param name="streetscapegeometry">The
        /// <c><see cref="ARStreetscapeGeometry"/></c> object containing the mesh
        /// to be rendered.</param>
        private void InstantiateRenderObject(ARStreetscapeGeometry streetscapegeometry)
        {
            if (streetscapegeometry.mesh == null)
            {
                return;
            }

            // Check if a render object already exists for this streetscapegeometry and
            // create one if not.
            if (_streetscapegeometryGOs.ContainsKey(streetscapegeometry.trackableId))
            {
                return;
            }

            GameObject renderObject = new GameObject(
                "StreetscapeGeometryMesh", typeof(MeshFilter), typeof(MeshRenderer));

            if (renderObject)
            {
                renderObject.transform.position = new Vector3(0, 0.5f, 0);
                renderObject.GetComponent<MeshFilter>().mesh = streetscapegeometry.mesh;

                // Add a material with transparent diffuse shader.
                if (streetscapegeometry.streetscapeGeometryType ==
                    StreetscapeGeometryType.Building)
                {
                    renderObject.GetComponent<MeshRenderer>().material =
                        StreetscapeGeometryMaterialBuilding[_buildingMatIndex];
                    _buildingMatIndex =
                        (_buildingMatIndex + 1) % StreetscapeGeometryMaterialBuilding.Count;
                }
                else
                {
                    renderObject.GetComponent<MeshRenderer>().material =
                        StreetscapeGeometryMaterialTerrain;
                }

                renderObject.transform.position = streetscapegeometry.pose.position;
                renderObject.transform.rotation = streetscapegeometry.pose.rotation;

                _streetscapegeometryGOs.Add(streetscapegeometry.trackableId, renderObject);
            }
        }

        /// <summary>
        /// Updates the render object transform based on this StreetscapeGeometries pose.
        /// It must be called every frame to update the mesh.
        /// </summary>
        /// <param name="streetscapegeometry">The <c><see cref="ARStreetscapeGeometry"/></c>
        /// object containing the mesh to be rendered.</param>
        private void UpdateRenderObject(ARStreetscapeGeometry streetscapegeometry)
        {
            if (_streetscapegeometryGOs.ContainsKey(streetscapegeometry.trackableId))
            {
                GameObject renderObject = _streetscapegeometryGOs[streetscapegeometry.trackableId];
                renderObject.transform.position = streetscapegeometry.pose.position;
                renderObject.transform.rotation = streetscapegeometry.pose.rotation;
            }
        }

        /// <summary>
        /// Destroys the render object associated with the
        /// <c><see cref="ARStreetscapeGeometry"/></c>.
        /// </summary>
        /// <param name="streetscapegeometry">The <c><see cref="ARStreetscapeGeometry"/></c>
        /// containing the render object to be destroyed.</param>
        private void DestroyRenderObject(ARStreetscapeGeometry streetscapegeometry)
        {
            if (_streetscapegeometryGOs.ContainsKey(streetscapegeometry.trackableId))
            {
                var geometry = _streetscapegeometryGOs[streetscapegeometry.trackableId];
                _streetscapegeometryGOs.Remove(streetscapegeometry.trackableId);
                Destroy(geometry);
            }
        }

        /// <summary>
        /// Destroys all stored <c><see cref="ARStreetscapeGeometry"/></c> render objects.
        /// </summary>
        private void DestroyAllRenderObjects()
        {
            var keys = _streetscapegeometryGOs.Keys;
            foreach (var key in keys)
            {
                var renderObject = _streetscapegeometryGOs[key];
                Destroy(renderObject);
            }

            _streetscapegeometryGOs.Clear();
        }

        /// <summary>
        /// Activate or deactivate all UI elements on the anchor setting Panel.
        /// </summary>
        /// <param name="state">A bool value to determine if the anchor settings panel is visible.
        private void SetAnchorPanelState(bool state)
        {
            AnchorSettingPanel.gameObject.SetActive(state);
            GeospatialAnchorToggle.gameObject.SetActive(state);
            TerrainAnchorToggle.gameObject.SetActive(state);
            RooftopAnchorToggle.gameObject.SetActive(state);
        }

        private IEnumerator CheckRooftopPromise(ResolveAnchorOnRooftopPromise promise,
            GeospatialAnchorHistory history)
        {
            yield return promise;

            var result = promise.Result;
            if (result.RooftopAnchorState == RooftopAnchorState.Success &&
                result.Anchor != null)
            {
                // Adjust the scale of the prefab anchor object to maintain visibility when it is
                // far away.
                result.Anchor.gameObject.transform.localScale *= GetRooftopAnchorScale(
                    result.Anchor.gameObject.transform.position,
                    Camera.main.transform.position);
                GameObject anchorGO = Instantiate(TerrainPrefab,
                    result.Anchor.gameObject.transform);
                anchorGO.transform.parent = result.Anchor.gameObject.transform;

                _anchorObjects.Add(result.Anchor.gameObject);
                _historyCollection.Collection.Add(history);

                SnackBarText.text = GetDisplayStringForAnchorPlacedSuccess();

                ClearAllButton.gameObject.SetActive(_anchorObjects.Count > 0);
                SaveGeospatialAnchorHistory();
            }
            else
            {
                SnackBarText.text = GetDisplayStringForAnchorPlacedFailure();
            }

            yield break;
        }

        private IEnumerator CheckTerrainPromise(ResolveAnchorOnTerrainPromise promise,
            GeospatialAnchorHistory history)
        {
            yield return promise;

            var result = promise.Result;
            if (result.TerrainAnchorState == TerrainAnchorState.Success &&
                result.Anchor != null)
            {
                GameObject anchorGO = Instantiate(TerrainPrefab,
                    result.Anchor.gameObject.transform);
                anchorGO.transform.parent = result.Anchor.gameObject.transform;

                _anchorObjects.Add(result.Anchor.gameObject);
                _historyCollection.Collection.Add(history);

                SnackBarText.text = GetDisplayStringForAnchorPlacedSuccess();

                ClearAllButton.gameObject.SetActive(_anchorObjects.Count > 0);
                SaveGeospatialAnchorHistory();
            }
            else
            {
                SnackBarText.text = GetDisplayStringForAnchorPlacedFailure();
            }

            yield break;
        }

        private float GetRooftopAnchorScale(Vector3 anchor, Vector3 camera)
        {
            // Return the scale in range [1, 2] after mapping a distance between camera and anchor
            // to [2, 20].
            float distance =
                Mathf.Sqrt(
                    Mathf.Pow(anchor.x - camera.x, 2.0f)
                    + Mathf.Pow(anchor.y - camera.y, 2.0f)
                    + Mathf.Pow(anchor.z - camera.z, 2.0f));
            float mapDistance = Mathf.Min(Mathf.Max(2.0f, distance), 20.0f);
            return (mapDistance - 2.0f) / (20.0f - 2.0f) + 1.0f;
        }

        private void PlaceAnchorByScreenTap(Vector2 position)
        {
            if (_streetscapeGeometryVisibility)
            {
                // Raycast against streetscapeGeometry.
                List<XRRaycastHit> hitResults = new List<XRRaycastHit>();
                if (RaycastManager.RaycastStreetscapeGeometry(position, ref hitResults))
                {
                    if (_anchorType == AnchorType.Rooftop || _anchorType == AnchorType.Terrain)
                    {
                        var streetscapeGeometry =
                            StreetscapeGeometryManager.GetStreetscapeGeometry(
                                hitResults[0].trackableId);
                        if (streetscapeGeometry == null)
                        {
                            return;
                        }

                        if (_streetscapegeometryGOs.ContainsKey(streetscapeGeometry.trackableId))
                        {
                            Pose modifiedPose = new Pose(hitResults[0].pose.position,
                                Quaternion.LookRotation(Vector3.right, Vector3.up));

                            GeospatialAnchorHistory history =
                                CreateHistory(modifiedPose, _anchorType);

                            // Anchor returned will be null, the coroutine will handle creating
                            // the anchor when the promise is done.
                            PlaceARAnchor(history, modifiedPose, hitResults[0].trackableId);
                        }
                    }
                    else
                    {
                        GeospatialAnchorHistory history = CreateHistory(hitResults[0].pose,
                            _anchorType);

                        var anchor = PlaceARAnchor(history, hitResults[0].pose,      //**
                            hitResults[0].trackableId);
                        if (anchor != null)
                        {
                            _historyCollection.Collection.Add(history);
                        }

                        ClearAllButton.gameObject.SetActive(_anchorObjects.Count > 0);
                        SaveGeospatialAnchorHistory();
                    }
                }

                return;
            }

            // Raycast against detected planes.
            List<ARRaycastHit> planeHitResults = new List<ARRaycastHit>();
            RaycastManager.Raycast(
                position, planeHitResults, TrackableType.Planes | TrackableType.FeaturePoint);
            if (planeHitResults.Count > 0)
            {
                GeospatialAnchorHistory history = CreateHistory(planeHitResults[0].pose,
                    _anchorType);

                if (_anchorType == AnchorType.Rooftop)
                {
                    // The coroutine will create the anchor when the promise is done.
                    Quaternion eunRotation = CreateRotation(history);
                    ResolveAnchorOnRooftopPromise rooftopPromise =
                        AnchorManager.ResolveAnchorOnRooftopAsync(
                            history.Latitude, history.Longitude,
                            0, eunRotation);

                    StartCoroutine(CheckRooftopPromise(rooftopPromise, history));
                    return;
                }
                //수정할 부분 -> 화면 클릭시 이부분 호출됨
                var anchor = PlaceGeospatialAnchor(history);       //새로 만든 obj 띄우기
                if (anchor != null)
                {
                    _historyCollection.Collection.Add(history);
                }

                ClearAllButton.gameObject.SetActive(_anchorObjects.Count > 0);
                SaveGeospatialAnchorHistory();
            }
        }


        private GeospatialAnchorHistory CreateHistory(Pose pose, AnchorType anchorType)
        {
            GeospatialPose geospatialPose = EarthManager.Convert(pose);
            string memoType = PlayerPrefs.GetString("MemoType"); // 추가
            string memoTypeForServer = "";
            //string userId = AndUserId;//"111@111.com"; //일단 user id
            //string userId = "222@222.com";
            string userId = MainMenu.UserEmail;
            //string writer = UserNickName;
            //string writer = "Oda";
            string writer = MainMenu.UserNickname;
            int postId = 0;         //일단 0으로  //서버 만들어지면 수정 -> postId가 저장되려면 새로 만든 앵커가 생기면 다시 다 불러오는 방법이 필요함
            string text = null;
            byte[] picture = null; //for server
            string pictureurl = "";//for anchor
            byte[] video = null;
            string videourl = "";


            if (memoType == "Text")  //Text일 경우만 text 저장(애초에 url이 아닌 byte로 변경)
            {
                text = PlayerPrefs.GetString("MemoText");
                memoTypeForServer = "A";
            }
            else if (memoType == "Picture")
            {
                // string storedBase64String = PlayerPrefs.GetString("MemoPicture");
                string picturebyte = PlayerPrefs.GetString("MemoPicture");
                picture = Convert.FromBase64String(picturebyte);  //64로 인코딩 된 걸 다시 변환
                memoTypeForServer = "B";
                //test.text = picture;
            } //사진은 url 이 아닌 byte
            else if (memoType == "Video")
            {
                // string storedBase64String = PlayerPrefs.GetString("MemoPicture");
                string picturebyte = PlayerPrefs.GetString("MemoVideo");
                video = Convert.FromBase64String(picturebyte);  //64로 인코딩 된 걸 다시 변환
                memoTypeForServer = "D";
                //test.text = picture;
            } //사진은 url 이 아닌 byte
            // 서버에 넣고(1번) -> 확인후 postid, url 가져옴
            StartCoroutine(AddMemoPost());


            //    GeospatialAnchorHistory history = new GeospatialAnchorHistory(); //참조값 변환

            //delay(); //1초 delay
            GeospatialAnchorHistory history = new GeospatialAnchorHistory(
                    geospatialPose.Latitude, geospatialPose.Longitude, geospatialPose.Altitude,
                    anchorType, geospatialPose.EunRotation, memoType, writer, postId, text, pictureurl, videourl, picture, video); //memoType 추가
                                                                                                                                   //picture이 url이 됨.
            Debug.Log($"Geo : ancor Text: {history.Text}, Picture: {history.Picture}, Video: {history.Video}, MemoType: {history.MemoType}, PostId: {history.PostId}");
            //geo는 memoType, server는 memoTypeforServer



            return history;
            /*
            async void delay()
            {
                await Task.Delay(3000); // 1초 대기


            }
            */
            IEnumerator AddMemoPost()
            {
                string url = "http://34.22.102.33:8000/outside/addMemo/";
                WWWForm form = new WWWForm();

                if (memoType == "Text")
                {
                    form.AddField("memo_content", text);
                }
                else if (memoType == "Picture") //쓰려면 메모타입 꼭 바꾸기
                {
                    //byte[] byteTexture = System.IO.File.ReadAllBytes(picture);
                    form.AddBinaryData("memo_content", picture, "ImageFromUnity.png", "image/png"); //ImageFromUnity는 이름
                }
                else if (memoType == "Video") //쓰려면 메모타입 꼭 바꾸기
                {
                    //byte[] byteTexture = System.IO.File.ReadAllBytes(picture);
                    form.AddBinaryData("memo_content", video, "ImageFromUnity.mp4", "video/mp4"); //ImageFromUnity는 이름
                }

                //값 넣기

                form.AddField("userId", userId);
                form.AddField("memoType", memoTypeForServer); //일단 id에 저장
                form.AddField("latitude", geospatialPose.Latitude.ToString());
                form.AddField("longitude", geospatialPose.Longitude.ToString());
                form.AddField("altitude", geospatialPose.Altitude.ToString());
                form.AddField("eunRotationX", geospatialPose.EunRotation.x.ToString());
                form.AddField("eunRotationY", geospatialPose.EunRotation.y.ToString());
                form.AddField("eunRotationZ", geospatialPose.EunRotation.z.ToString());
                form.AddField("eunRotationW", geospatialPose.EunRotation.w.ToString());//nickname은 어떻게 저장하지?
                UnityWebRequest www = UnityWebRequest.Post(url, form);
                yield return www.SendWebRequest();

                if (www.error == null)
                {
                    Debug.Log(www.downloadHandler.text);
                    /*
                     if (memoType == "Picture")
                     {
                         yield return StartCoroutine(GetImageURL());
                     } //사진은 url 이 아닌 byte
                     else if (memoType == "Video")
                     {   yield return StartCoroutine(GetVideoURL()); }

                     */
                }
                else
                {
                    Debug.Log("AddMemoPost_error");
                }

            }

            /*
        IEnumerator GetPostId()
        {

                string url = "http://34.22.102.33:8000/outside/get_last_Postid/" + UnityWebRequest.EscapeURL(userId) + "/";

                UnityWebRequest www = UnityWebRequest.Get(url);

                yield return www.SendWebRequest();

                if (www.error == null)
                {
                    string jsonResponse = www.downloadHandler.text;
                    ResponseId ResponseId = JsonUtility.FromJson<ResponseId>(jsonResponse);
                    postId = ResponseId.id;
                    Debug.Log("PostId 성공!"+postId);

                }
                else
                {
                    Debug.Log("PostId 오류");
                }

        }


        IEnumerator GetImageURL() //url 를 가져오는 코드
        {

            // string userId = "1"; //id 받아오면 변경s

            //user id 를 안전한 url 형태로 변형
            string url = "http://34.22.102.33:8000/outside/get_last_Image/" + UnityWebRequest.EscapeURL(userId) + "/";

            UnityWebRequest www = UnityWebRequest.Get(url); // get 방식으로 요청을 보냄.

            yield return www.SendWebRequest(); //응답이 올 때까지 기다림.

            if (www.error == null) //잘 도착했으면
            {
                string jsonResponse = www.downloadHandler.text;
                ResponseImage ResponseImage = JsonUtility.FromJson<ResponseImage>(jsonResponse);
                pictureurl = ResponseImage.picture;
                Debug.Log("pictureur 성공!" + pictureurl);

            }
            else
            {
                Debug.Log("pictureur_error");

            }

        }
        IEnumerator GetVideoURL() //url 를 가져오는 코드
        {

            // string userId = "1"; //id 받아오면 변경s

            //user id 를 안전한 url 형태로 변형
            string url = "http://34.22.102.33:8000/outside/get_last_Video/" + UnityWebRequest.EscapeURL(userId) + "/";

            UnityWebRequest www = UnityWebRequest.Get(url); // get 방식으로 요청을 보냄.

            yield return www.SendWebRequest(); //응답이 올 때까지 기다림.

            if (www.error == null) //잘 도착했으면
            {
                string jsonResponse = www.downloadHandler.text;
                ResponseVideo ResponseVideo = JsonUtility.FromJson<ResponseVideo>(jsonResponse);
                videourl = ResponseVideo.video;
                Debug.Log("videourl성공!" + videourl);


            }
            else
            {
                Debug.Log("Getvideourl_ERROR");

            }

        }
        */
        }

        private Quaternion CreateRotation(GeospatialAnchorHistory history)
        {
            Quaternion eunRotation = history.EunRotation;
            if (eunRotation == Quaternion.identity)
            {
                // This history is from a previous app version and EunRotation was not used.
                eunRotation =
                    Quaternion.AngleAxis(180f - (float)history.Heading, Vector3.up);
            }

            return eunRotation;
        }

        private Quaternion CreateRotation_nav(GeospatialAnchorHistory_nav history)  //길찾기
        {
            Debug.Log("nav : CreateRotation_nav");
            Quaternion eunRotation = history.EunRotation;
            if (eunRotation == Quaternion.identity)
            {
                // This history is from a previous app version and EunRotation was not used.
                eunRotation =
                    Quaternion.AngleAxis(180f - (float)history.Heading, Vector3.up);
            }

            return eunRotation;
        }

        private ARAnchor PlaceARAnchor(GeospatialAnchorHistory history, Pose pose = new Pose(),
            TrackableId trackableId = new TrackableId())
        {
            Quaternion eunRotation = CreateRotation(history);
            ARAnchor anchor = null;
            switch (history.AnchorType)
            {
                case AnchorType.Rooftop:
                    ResolveAnchorOnRooftopPromise rooftopPromise =
                        AnchorManager.ResolveAnchorOnRooftopAsync(
                            history.Latitude, history.Longitude,
                            0, eunRotation);

                    StartCoroutine(CheckRooftopPromise(rooftopPromise, history));
                    return null;

                case AnchorType.Terrain:
                    ResolveAnchorOnTerrainPromise terrainPromise =
                        AnchorManager.ResolveAnchorOnTerrainAsync(
                            history.Latitude, history.Longitude,
                            0, eunRotation);

                    StartCoroutine(CheckTerrainPromise(terrainPromise, history));
                    return null;

                case AnchorType.Geospatial:
                    ARStreetscapeGeometry streetscapegeometry =
                        StreetscapeGeometryManager.GetStreetscapeGeometry(trackableId);
                    if (streetscapegeometry != null)
                    {
                        anchor = StreetscapeGeometryManager.AttachAnchor(
                            streetscapegeometry, pose);
                    }

                    if (anchor != null)
                    {
                        _anchorObjects.Add(anchor.gameObject);
                        _historyCollection.Collection.Add(history);
                        ClearAllButton.gameObject.SetActive(_anchorObjects.Count > 0);
                        SaveGeospatialAnchorHistory();

                        SnackBarText.text = GetDisplayStringForAnchorPlacedSuccess();
                    }
                    else
                    {
                        SnackBarText.text = GetDisplayStringForAnchorPlacedFailure();
                    }

                    break;
            }

            return anchor;
        }

        private ARGeospatialAnchor PlaceGeospatialAnchor(
            GeospatialAnchorHistory history)
        {
            bool terrain = history.AnchorType == AnchorType.Terrain;
            Quaternion eunRotation = CreateRotation(history);
            ARGeospatialAnchor anchor = null;

            if (terrain)
            {
                // Anchor returned will be null, the coroutine will handle creating the
                // anchor when the promise is done.
                ResolveAnchorOnTerrainPromise promise =
                    AnchorManager.ResolveAnchorOnTerrainAsync(
                        history.Latitude, history.Longitude,
                        0, eunRotation);

                StartCoroutine(CheckTerrainPromise(promise, history));
                return null;
            }
            else
            {
                anchor = AnchorManager.AddAnchor(
                    history.Latitude, history.Longitude, history.Altitude + 0.2, eunRotation);
            }

            if (anchor != null)
            {
                //DebugText.text +=  $"place ";
                GameObject anchorGO;
                if (history.MemoType == "Text")    //새로운 obj 만들때마다 호출되지는 않음
                {
                    Debug.Log("success text");
                    anchorGO = history.AnchorType == AnchorType.Geospatial ?
                    Instantiate(GeospatialPrefab, anchor.transform) :
                    Instantiate(TerrainPrefab, anchor.transform);
                    //ARMemoSetText 스크립트에 접근 -> 3d 오브젝트에 정보 전달
                    ARMemoSetText textObjScript = anchorGO.GetComponent<ARMemoSetText>();
                    if (textObjScript != null)
                    {
                        // Debug.Log("text success");
                        textObjScript.ReceiveData(history.Text, history.Writer);
                    }

                }
                else if (history.MemoType == "Picture")
                {
                    anchorGO = history.AnchorType == AnchorType.Geospatial ?
                    Instantiate(GeospatialPrefab2, anchor.transform) :
                    Instantiate(TerrainPrefab, anchor.transform);

                    //ARMemoSetPicture 스크립트에 접근 -> 3d 오브젝트에 정보 전달
                    ARMemoSetPicture pictureObjScript = anchorGO.GetComponent<ARMemoSetPicture>();
                    if (pictureObjScript != null)
                    {
                        pictureObjScript.cube = anchorGO; //video에서도 살펴라~ 
                        if (history.Picturebyte != null)
                        {
                            pictureObjScript.ReceiveDataByte(history.Picturebyte, history.Writer);
                        }
                        else
                        {
                            pictureObjScript.ReceiveDataUrl(history.Picture, history.Writer);
                        }
                    }

                    //test.text = history.Picture;
                }
                else if (history.MemoType == "Video")
                {
                    anchorGO = history.AnchorType == AnchorType.Geospatial ?
                    Instantiate(GeospatialPrefab3, anchor.transform) :
                    Instantiate(TerrainPrefab, anchor.transform);
                    //ARMemoSetVideo 스크립트에 접근 -> 3d 오브젝트에 정보 전달
                    ARMemoSetVideo videoObjScript = anchorGO.GetComponent<ARMemoSetVideo>();
                    if (videoObjScript != null)
                    {
                        videoObjScript.plane = anchorGO;
                        if (history.Videobyte != null)
                        {
                            videoObjScript.ReceiveDataByte(history.Videobyte, history.Writer);
                        }
                        else
                        {
                            videoObjScript.ReceiveDataUrl(history.Video, history.Writer);
                        }
                    }
                }
                else { anchorGO = null; }

                anchor.gameObject.SetActive(!terrain);
                anchorGO.transform.parent = anchor.gameObject.transform;
                _anchorObjects.Add(anchor.gameObject);
                SnackBarText.text = GetDisplayStringForAnchorPlacedSuccess();
            }
            else
            {
                SnackBarText.text = GetDisplayStringForAnchorPlacedFailure();
            }

            return anchor;
        }

        //private LineRenderer lineRenderer;
        //private List<GameObject> lineList = new();
        private ARGeospatialAnchor PlaceGeospatialAnchor_nav(      //길찾기                    
    GeospatialAnchorHistory_nav history)
        {
            //Debug.Log("nav : PlaceGeospatialAnchor_nav");
            bool terrain = history.AnchorType == AnchorType.Terrain;
            Quaternion eunRotation = CreateRotation_nav(history);
            ARGeospatialAnchor anchor = null;

            if (terrain)
            {
                /*
                // Anchor returned will be null, the coroutine will handle creating the
                // anchor when the promise is done.
                ResolveAnchorOnTerrainPromise promise =
                    AnchorManager.ResolveAnchorOnTerrainAsync(
                        history.Latitude, history.Longitude,
                        0, eunRotation);

                StartCoroutine(CheckTerrainPromise(promise, history));
                */
                return null;
            }
            else
            {
                anchor = AnchorManager.AddAnchor(
                    history.Latitude, history.Longitude, history.Altitude + 0.2, eunRotation);
            }

            if (anchor != null)
            {
                Debug.Log("nav : PlaceGeospatialAnchor_nav anchor");
                //DebugText.text +=  $"place ";
                GameObject anchorGO;
                if (history.ObjType == "point")
                {
                    Debug.Log("point");
                    anchorGO = history.AnchorType == AnchorType.Geospatial ?
                    Instantiate(GeospatialPrefab, anchor.transform) :
                    Instantiate(TerrainPrefab, anchor.transform);
                }
                else if (history.ObjType == "arrow") //화살표 obj 놓기
                {
                    if (navMode == "gameNav")
                    {
                        anchorGO = history.AnchorType == AnchorType.Geospatial ?
                        Instantiate(GeospatialPrefabArrow_game, anchor.transform) :
                        Instantiate(TerrainPrefab, anchor.transform);
                    }
                    else
                    {
                        //Debug.Log("nav : PlaceGeospatialAnchor_nav arrow");
                        anchorGO = history.AnchorType == AnchorType.Geospatial ?
                        Instantiate(GeospatialPrefabArrow, anchor.transform) :
                        Instantiate(TerrainPrefab, anchor.transform);
                    }
                }
                else if (history.ObjType == "Line")
                {
                    anchorGO = null;
                    lineList.Add(anchor.gameObject);
                }
                else if (history.ObjType == "Goal") //도착점 obj 놓기
                {
                    if (navMode == "gameNav")
                    {
                        anchorGO = history.AnchorType == AnchorType.Geospatial ?
                        Instantiate(GeospatialPrefabGoal_game, anchor.transform) :
                        Instantiate(TerrainPrefab, anchor.transform);
                    }
                    else
                    {
                        anchorGO = history.AnchorType == AnchorType.Geospatial ?
                        Instantiate(GeospatialPrefabGoal, anchor.transform) :
                        Instantiate(TerrainPrefab, anchor.transform);
                    }

                }
                else { anchorGO = null; }

                anchor.gameObject.SetActive(!terrain);
                if (anchorGO != null) anchorGO.transform.parent = anchor.gameObject.transform;
                _anchorObjects_nav.Add(anchor.gameObject);
                SnackBarText.text = GetDisplayStringForAnchorPlacedSuccess();
            }
            else
            {
                SnackBarText.text = GetDisplayStringForAnchorPlacedFailure();
            }
            //Debug.Log("nav : PlaceGeospatialAnchor_nav _5");

            return anchor;
        }

        public void ResolveHistory()
        {
            Debug.Log("nav : ResolveHistory");
            if (!_shouldResolvingHistory)
            {
                return;
            }

            _shouldResolvingHistory = false;
            foreach (var history in _historyCollection.Collection)
            {
                switch (history.AnchorType)
                {
                    case AnchorType.Rooftop:
                        PlaceARAnchor(history);
                        break;
                    case AnchorType.Terrain:
                        PlaceARAnchor(history);
                        break;
                    default:
                        PlaceGeospatialAnchor(history);
                        break;
                }
            }

            ClearAllButton.gameObject.SetActive(_anchorObjects.Count > 0);
            SnackBarText.text = string.Format("{0} anchor(s) set from history.",
                _anchorObjects.Count);
        }

        public void ResolveHistory_nav() //길찾기
        {
            Debug.Log("nav : ResolveHistory_nav");
            if (!_shouldResolvingHistory_nav)
            {
                Debug.Log("nav : ResolveHistory_nav if !_shouldResolvingHistory_nav");
                return;
            }

            Debug.Log("nav2 : " + _historyCollection_nav.Collection);
            _shouldResolvingHistory_nav = false;
            foreach (var history in _historyCollection_nav.Collection)
            {
                //Debug.Log("nav : ResolveHistory_nav for");
                switch (history.AnchorType)
                {
                    /*
                    case AnchorType.Rooftop:
                        PlaceARAnchor(history);
                        break;
                    case AnchorType.Terrain:
                        PlaceARAnchor(history);
                        break;
                    */
                    case AnchorType.Geospatial:
                        Debug.Log("nav : ResolveHistory_nav case");
                        PlaceGeospatialAnchor_nav(history);
                        break;
                    default:
                        break;
                }
            }

            //ClearAllButton.gameObject.SetActive(_anchorObjects.Count > 0);
            //SnackBarText.text = string.Format("{0} anchor(s) set from history.",
            //     _anchorObjects.Count);
        }

        private void LoadGeospatialAnchorHistory()
        {
            if (PlayerPrefs.HasKey(_persistentGeospatialAnchorsStorageKey))
            {
                _historyCollection = JsonUtility.FromJson<GeospatialAnchorHistoryCollection>(
                    PlayerPrefs.GetString(_persistentGeospatialAnchorsStorageKey));

                // Remove all records created more than 24 hours and update stored history.
                /*
                DateTime current = DateTime.Now;
                _historyCollection.Collection.RemoveAll(
                    data => current.Subtract(data.CreatedTime).Days > 0);
                */
                PlayerPrefs.SetString(_persistentGeospatialAnchorsStorageKey,
                    JsonUtility.ToJson(_historyCollection));
                PlayerPrefs.Save();
            }
            else
            {
                _historyCollection = new GeospatialAnchorHistoryCollection();
            }
        }

        private void LoadGeospatialAnchorHistory_nav()
        {
            if (PlayerPrefs.HasKey(_persistentGeospatialAnchorsStorageKey_nav))
            {
                _historyCollection_nav = JsonUtility.FromJson<GeospatialAnchorHistoryCollection_nav>(
                    PlayerPrefs.GetString(_persistentGeospatialAnchorsStorageKey_nav));

                // Remove all records created more than 24 hours and update stored history.
                /*
                DateTime current = DateTime.Now;
                _historyCollection_nav.Collection.RemoveAll(
                    data => current.Subtract(data.CreatedTime).Days > 0);
                */
                PlayerPrefs.SetString(_persistentGeospatialAnchorsStorageKey_nav,
                    JsonUtility.ToJson(_historyCollection_nav));
                PlayerPrefs.Save();
            }
            else
            {
                _historyCollection_nav = new GeospatialAnchorHistoryCollection_nav();
            }
        }

        public void SaveGeospatialAnchorHistory()
        {
            // Sort the data from latest record to earliest record.
            _historyCollection.Collection.Sort((left, right) =>
                right.CreatedTime.CompareTo(left.CreatedTime));

            // Remove the earliest data if the capacity exceeds storage limit.
            if (_historyCollection.Collection.Count > _storageLimit)
            {
                _historyCollection.Collection.RemoveRange(
                    _storageLimit, _historyCollection.Collection.Count - _storageLimit);
            }

            PlayerPrefs.SetString(
                _persistentGeospatialAnchorsStorageKey, JsonUtility.ToJson(_historyCollection));
            PlayerPrefs.Save();
        }

        public void SaveGeospatialAnchorHistory_nav()
        {
            //Debug.Log("nav : SaveGeospatialAnchorHistory_nav");
            // Sort the data from latest record to earliest record.
            //_historyCollection.Collection.Sort((left, right) =>
            //    right.CreatedTime.CompareTo(left.CreatedTime));

            // Remove the earliest data if the capacity exceeds storage limit.
            if (_historyCollection_nav.Collection.Count > _storageLimit)
            {
                _historyCollection_nav.Collection.RemoveRange(
                    _storageLimit, _historyCollection_nav.Collection.Count - _storageLimit);
            }

            PlayerPrefs.SetString(
                _persistentGeospatialAnchorsStorageKey_nav, JsonUtility.ToJson(_historyCollection_nav));
            PlayerPrefs.Save();
        }

        private void SwitchToARView(bool enable)
        {
            _isInARView = enable;
            SessionOrigin.gameObject.SetActive(enable);
            Session.gameObject.SetActive(enable);
            ARCoreExtensions.gameObject.SetActive(enable);
            ARViewCanvas.SetActive(enable);
            PrivacyPromptCanvas.SetActive(!enable);
            VPSCheckCanvas.SetActive(false);
            if (enable && _asyncCheck == null)
            {
                _asyncCheck = AvailabilityCheck();
                StartCoroutine(_asyncCheck);
            }
        }

        private IEnumerator AvailabilityCheck()
        {
            if (ARSession.state == ARSessionState.None)
            {
                yield return ARSession.CheckAvailability();
            }

            // Waiting for ARSessionState.CheckingAvailability.
            yield return null;

            if (ARSession.state == ARSessionState.NeedsInstall)
            {
                yield return ARSession.Install();
            }

            // Waiting for ARSessionState.Installing.
            yield return null;
#if UNITY_ANDROID

            if (!Permission.HasUserAuthorizedPermission(Permission.Camera))
            {
                Debug.Log("Requesting camera permission.");
                Permission.RequestUserPermission(Permission.Camera);
                yield return new WaitForSeconds(3.0f);
            }

            if (!Permission.HasUserAuthorizedPermission(Permission.Camera))
            {
                // User has denied the request.
                Debug.LogWarning(
                    "Failed to get the camera permission. VPS availability check isn't available.");
                yield break;
            }
#endif

            while (_waitingForLocationService)
            {
                yield return null;
            }

            if (Input.location.status != LocationServiceStatus.Running)
            {
                Debug.LogWarning(
                    "Location services aren't running. VPS availability check is not available.");
                yield break;
            }

            // Update event is executed before coroutines so it checks the latest error states.
            if (_isReturning)
            {
                yield break;
            }

            var location = Input.location.lastData;
            var vpsAvailabilityPromise =
                AREarthManager.CheckVpsAvailabilityAsync(location.latitude, location.longitude);
            yield return vpsAvailabilityPromise;

            Debug.LogFormat("VPS Availability at ({0}, {1}): {2}",
                location.latitude, location.longitude, vpsAvailabilityPromise.Result);
            VPSCheckCanvas.SetActive(vpsAvailabilityPromise.Result != VpsAvailability.Available);
        }

        private IEnumerator StartLocationService()
        {
            _waitingForLocationService = true;
#if UNITY_ANDROID
            if (!Permission.HasUserAuthorizedPermission(Permission.FineLocation))
            {
                Debug.Log("Requesting the fine location permission.");
                Permission.RequestUserPermission(Permission.FineLocation);
                yield return new WaitForSeconds(3.0f);
            }
#endif

            if (!Input.location.isEnabledByUser)
            {
                Debug.Log("Location service is disabled by the user.");
                _waitingForLocationService = false;
                yield break;
            }

            Debug.Log("Starting location service.");
            Input.location.Start();

            while (Input.location.status == LocationServiceStatus.Initializing)
            {
                yield return null;
            }

            _waitingForLocationService = false;
            if (Input.location.status != LocationServiceStatus.Running)
            {
                Debug.LogWarningFormat(
                    "Location service ended with {0} status.", Input.location.status);
                Input.location.Stop();
            }
        }

        private void LifecycleUpdate()
        {
            // Pressing 'back' button quits the app.
            if (Input.GetKeyUp(KeyCode.Escape))
            {
                Application.Quit();
            }

            if (_isReturning)
            {
                return;
            }

            // Only allow the screen to sleep when not tracking.
            var sleepTimeout = SleepTimeout.NeverSleep;
            if (ARSession.state != ARSessionState.SessionTracking)
            {
                sleepTimeout = SleepTimeout.SystemSetting;
            }

            Screen.sleepTimeout = sleepTimeout;

            // Quit the app if ARSession is in an error status.
            string returningReason = string.Empty;
            if (ARSession.state != ARSessionState.CheckingAvailability &&
                ARSession.state != ARSessionState.Ready &&
                ARSession.state != ARSessionState.SessionInitializing &&
                ARSession.state != ARSessionState.SessionTracking)
            {
                returningReason = string.Format(
                    "Geospatial sample encountered an ARSession error state {0}.\n" +
                    "Please restart the app.",
                    ARSession.state);
            }
            else if (Input.location.status == LocationServiceStatus.Failed)
            {
                returningReason =
                    "Geospatial sample failed to start location service.\n" +
                    "Please restart the app and grant the fine location permission.";
            }
            else if (SessionOrigin == null || Session == null || ARCoreExtensions == null)
            {
                returningReason = string.Format(
                    "Geospatial sample failed due to missing AR Components.");
            }

            ReturnWithReason(returningReason);
        }

        private void ReturnWithReason(string reason)
        {
            if (string.IsNullOrEmpty(reason))
            {
                return;
            }

            GeometryToggle.gameObject.SetActive(false);
            AnchorSettingButton.gameObject.SetActive(false);
            AnchorSettingPanel.gameObject.SetActive(false);
            GeospatialAnchorToggle.gameObject.SetActive(false);
            TerrainAnchorToggle.gameObject.SetActive(false);
            RooftopAnchorToggle.gameObject.SetActive(false);
            ClearAllButton.gameObject.SetActive(false);
            InfoPanel.SetActive(false);

            Debug.LogError(reason);
            SnackBarText.text = reason;
            _isReturning = true;
            Invoke(nameof(QuitApplication), _errorDisplaySeconds);
        }

        private void QuitApplication()
        {
            Application.Quit();
        }

        private void UpdateDebugInfo()
        {
            if (!Debug.isDebugBuild || EarthManager == null)
            {
                return;
            }

            var pose = EarthManager.EarthState == EarthState.Enabled &&
                EarthManager.EarthTrackingState == TrackingState.Tracking ?
                EarthManager.CameraGeospatialPose : new GeospatialPose();
            var supported = EarthManager.IsGeospatialModeSupported(GeospatialMode.Enabled);
            /*
            DebugText.text =
                $"IsReturning: {_isReturning}\n" +
                $"IsLocalizing: {_isLocalizing}\n" +
                $"SessionState: {ARSession.state}\n" +
                $"LocationServiceStatus: {Input.location.status}\n" +
                $"FeatureSupported: {supported}\n" +
                $"EarthState: {EarthManager.EarthState}\n" +
                $"EarthTrackingState: {EarthManager.EarthTrackingState}\n" +
                $"  LAT/LNG: {pose.Latitude:F6}, {pose.Longitude:F6}\n" +
                $"  HorizontalAcc: {pose.HorizontalAccuracy:F6}\n" +
                $"  ALT: {pose.Altitude:F2}\n" +
                $"  VerticalAcc: {pose.VerticalAccuracy:F2}\n" +
                $". EunRotation: {pose.EunRotation:F2}\n" +
                $"  OrientationYawAcc: {pose.OrientationYawAccuracy:F2}";
            */
        }

        /// <summary>
        /// Generates the placed anchor success string for the UI display.
        /// </summary>
        /// <returns> The string for the UI display for successful anchor placement.</returns>
        private string GetDisplayStringForAnchorPlacedSuccess()
        {
            return string.Format(
                    "{0} / {1} Anchor(s) Set!", _anchorObjects.Count, _storageLimit);
        }

        /// <summary>
        /// Generates the placed anchor failure string for the UI display.
        /// </summary>
        /// <returns> The string for the UI display for a failed anchor placement.</returns>
        private string GetDisplayStringForAnchorPlacedFailure()
        {
            return string.Format(
                    "Failed to set a {0} anchor!", _anchorType);
        }

        /*
        //3d 오브젝트 터치가 true일 경우만 
        if(_3dObjTouch == flase) return;
        */


        /*
        //3d 오브젝트 터치
        void Checking()
        {
            //3d 오브젝트 터치가 true일 경우만 
            if (_3dObjTouch == false) return;

            if (Input.touchCount == 0) return;
            Touch touch = Input.GetTouch(0);

            //터치 시작시
            if (touch.phase == TouchPhase.Began)
            {
                Ray ray;
                RaycastHit hitobj;

                ray = arCamera.ScreenPointToRay(touch.position);  //

                //Ray를 통한 오브젝트 인식
                int layerMask = 1 << LayerMask.NameToLayer("Cube");
                if (Physics.Raycast(ray, out hitobj, 500f, layerMask) && _anchorObjects.Count > 0) 
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
        */
        //길찾기
        //출발점 위치 받아오기  
        public double startLatitude = 0.0;  //gps 안정화 되기전까지 0.0
        public double startLongitude = 0.0;
        public double startAltitude = 0.0;
        //현재 위치 -> 계속 업데이트
        public double currentLatitude = 0.0;
        public double currentLongitude = 0.0;
        public double currentAltitude = 0.0;
        //메모 위치 -> 안드에서 받아오기
        public double memoLatitude = 0.0;
        public double memoLongitude = 0.0;
        //public double memoAltitude = 0.0;

        public List<api.GPSPoint> gpsPoint = new();
        public List<api.GPSPoint> gpsLine = new();

        public api api;
        public Button OutsideButton;
        public Button NavClearButton;

        public bool NavState;
        private LineRenderer lineRenderer;
        private List<GameObject> lineList = new();


        public void Start_nav()
        {
            api = GameObject.FindObjectOfType<api>();
            if (api == null)
            {
                Debug.Log("API 객체를 찾을 수 없습니다.");
            }
            //SnackBarText.text = "현재 위치 확인 중";
            //OnGetStartedClicked(); 
            //history 지우기
            foreach (var anchor in _anchorObjects_nav)
            {
                Destroy(anchor);
            }

            _anchorObjects_nav.Clear();
            _historyCollection_nav.Collection.Clear();
            //SnackBarText.text = "Anchor(s) cleared!";
            //ClearAllButton.gameObject.SetActive(false);
            SaveGeospatialAnchorHistory_nav();

            gpsLine.Clear();
            gpsPoint.Clear();
            lineList.Clear();

            api.gpsCallback(getGpsData); //gps 리스트 저장된 후에 실행
            LineRenderer();
            //SaveGeospatialAnchorHistory();

            //3d 오브젝트 크기 조정
            ScaleObject(GeospatialPrefabArrow, 0.2f);
            ScaleObject(GeospatialPrefabArrow_game, 0.2f);
            ScaleObject(GeospatialPrefabGoal_game, 0.1f);
            //메모 & 현재 위치 거리 확인
            InvokeRepeating("checkMemoDistance", 0, 2.0f);  //2초마다 비교

            //memoLatitude = double.Parse(PlayerPrefs.GetString("memoLatitudeKey"));
            //memoLongitude = double.Parse(PlayerPrefs.GetString("memoLongitudeKey"));
        }

        public void addHistory(double latitude, double longitude, double altitude, Quaternion eunRotation, string objType)  //_historyCollection 리스트에 history 추가
        {
            //Quaternion eunRotation = Quaternion.identity; // 단위 쿼터니언으로 초기화

            GeospatialAnchorHistory_nav history = new GeospatialAnchorHistory_nav(
                   latitude, longitude, altitude,
                   AnchorType.Geospatial, eunRotation, objType);  // Quaternion eunRotation
            _historyCollection_nav.Collection.Add(history);
            Debug.Log("nav : addHistory " + objType + "  "+ latitude + "  " + longitude);
        }

        public Quaternion arrowDirection(double startLatitude, double startLongitude, double endLatitude, double endLongitude)
        {
            // 각도를 라디안으로 변환
            float angle = Mathf.Atan2((float)(endLongitude - startLongitude), (float)(endLatitude - startLatitude)) * Mathf.Rad2Deg;

            if (angle > 0 && angle < 1)
            {
                angle = 1;
            }
            else if (angle < 0 && angle > -1)
            {
                angle = -1;
            }
            else { }
            Quaternion rotation = Quaternion.Euler(0, angle, 90);

            return rotation;
        }

        //화살표 앵커 추가
        public void addArrowAnchor()
        {
            Debug.Log("nav : addArrowAnchor");
            Quaternion direction = Quaternion.identity; // 단위 쿼터니언으로 초기화
                                                        //getGps(); //나중에 바꿀 부분

            //for (int i = 0; i < gpsPoint.Count - 2; i++)
            for (int i = 0; i < gpsLine.Count -1; i += 2) //i < gpsLine.Count -2 ; i += 3)
            {
                //addHistory(gpsLine[i].latitude, gpsLine[i].longitude, startAltitude, eunRotation, "Line");

                //Debug.Log("gpsPoint : "+i);
                // i번째와 i+1번째 GPS 좌표 가져오기
                api.GPSPoint startPoint = gpsLine[i];
                api.GPSPoint endPoint = gpsLine[i + 1];
                Debug.Log("start Point2 : " + startPoint.latitude + " " + startPoint.longitude);
                Debug.Log("end Point2 : " + endPoint.latitude + " " + endPoint.longitude);

                //화살표 방향 계산
                direction = arrowDirection(startPoint.latitude, startPoint.longitude, endPoint.latitude, endPoint.longitude);
                Debug.Log("Point2 Direction : " + direction);
                //addHistory(startPoint.latitude, startPoint.longitude, startAltitude + 1, direction, "arrow");
                addHistory(startPoint.latitude, startPoint.longitude, startPoint.elevation+3, direction, "arrow");
            }
            SaveGeospatialAnchorHistory_nav();
        }

        public void addPointAnchor()
        {
            Debug.Log("nav : addPointAnchor");
            for (int i = 0; i < gpsPoint.Count; i++)
            {
                Quaternion eunRotation = Quaternion.identity; // 단위 쿼터니언으로 초기화

                //point 앵커 추가
                //addHistory(gpsPoint[i].latitude, gpsPoint[i].longitude, startAltitude + 2, eunRotation, "point");
                addHistory(gpsPoint[i].latitude, gpsPoint[i].longitude, gpsPoint[i].elevation, eunRotation, "point");
            }
            SaveGeospatialAnchorHistory_nav();
        }

        public void addLineAnchor()
        {
            Debug.Log("nav : addLineAnchor");
            Quaternion eunRotation = Quaternion.identity;
            for (int i = 0; i < gpsLine.Count; i++)
            {
                //addHistory(gpsLine[i].latitude, gpsLine[i].longitude, startAltitude, eunRotation, "Line");
                addHistory(gpsLine[i].latitude, gpsLine[i].longitude, gpsLine[i].elevation, eunRotation, "Line");
                Debug.Log("gpsLine" + i + "번 : "+ gpsLine[i].latitude + "  " + gpsLine[i].longitude);
                if (i == gpsLine.Count-1)
                {
                    //addHistory(gpsLine[i].latitude, gpsLine[i].longitude, startAltitude + 1, eunRotation, "Goal");
                    addHistory(gpsLine[i].latitude, gpsLine[i].longitude, gpsLine[i].elevation+3, eunRotation, "Goal");
                }
            }
            SaveGeospatialAnchorHistory_nav();
        }

        public void LineRenderer()
        {
            lineRenderer = gameObject.AddComponent<LineRenderer>();
            lineRenderer.positionCount = 0;
            lineRenderer.startWidth = 1f;
            lineRenderer.endWidth = 1f;
            lineRenderer.material = new Material(Shader.Find("Standard"));
            if (navMode == "gameNav")
            {
                lineRenderer.material.color = Color.green;
            }
            else
            {
                lineRenderer.material.color = Color.blue;
            }
        }

        public void UpdateLine()
        {
            lineRenderer.positionCount = lineList.Count;

            for (int i = 0; i < lineList.Count; i++)
            {
                lineRenderer.SetPosition(i, lineList[i].transform.position);
            }
        }

        public void getGpsData()
        {
            //Debug.Log("nav : getGpsData");
            gpsPoint = api.gpsPointList;
            gpsLine = api.gpsLinestringList;
            //Debug.Log("nav : " + gpsPoint);
            //Debug.Log("nav : " + gpsLine);
            addArrowAnchor();
            //addPointAnchor();
            addLineAnchor();
            /*
            foreach (var history in _historyCollection_nav.Collection)
            {
                Debug.Log("nav : getGpsData : " + history.ObjType +"  "+history.Latitude + " " + history.Longitude);

            }
            */
            _shouldResolvingHistory_nav = true;
            ResolveHistory_nav();
        }

        // 크기 조절하는 함수
        public void ScaleObject(GameObject targetObject, float scaleFactor)
        {
            if (targetObject != null)
            {
                //현재 크기를 가져옴 -> scaleFactor만큼 크기를 조절
                Vector3 currentScale = targetObject.transform.localScale;
                Vector3 newScale = currentScale * scaleFactor;
                //새로운 크기로 설정
                targetObject.transform.localScale = newScale;
            }
            else
            {
                Debug.LogError("대상 오브젝트가 설정되지 않았습니다.");
            }
        }

        //gps 거리계산
        public static double CalculateDistance(double lat1, double lon1, double lat2, double lon2)
        {
            double earthRadius = 6371; // 지구 반지름 (단위: 킬로미터)

            // 라디안 변환
            double lat1Rad = DegreesToRadians(lat1);
            double lon1Rad = DegreesToRadians(lon1);
            double lat2Rad = DegreesToRadians(lat2);
            double lon2Rad = DegreesToRadians(lon2);

            // 위도, 경도 간의 차이
            double deltaLat = lat2Rad - lat1Rad;
            double deltaLon = lon2Rad - lon1Rad;

            // 위도, 경도 간의 거리 계산
            double a = Math.Pow(Math.Sin(deltaLat / 2), 2) +
                       Math.Cos(lat1Rad) * Math.Cos(lat2Rad) *
                       Math.Pow(Math.Sin(deltaLon / 2), 2);
            double c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));

            // 두 지점 간의 직선 거리 계산
            double distance = earthRadius * c;

            return distance;  //km단위
        }
        public static double DegreesToRadians(double degrees)
        {
            return degrees * Math.PI / 180;
        }

        //메모 현재 사이의 거리
        public double memoDistance = 0.0f;
        //메모와 현재위치 거리 계산 함수
        void checkMemoDistance()
        {
            //memoLatitude = 37.524421; 
            //memoLongitude = 127.031531;
            memoLatitude = double.Parse(PlayerPrefs.GetString("memoLatitudeKey"));
            memoLongitude = double.Parse(PlayerPrefs.GetString("memoLongitudeKey"));

            if (memoLatitude == 0 || currentLatitude == 0)
            {
                //OutsideButton.gameObject.SetActive(false);

            }
            else
            {
                memoDistance = CalculateDistance(memoLatitude, memoLongitude, currentLatitude, currentLongitude);  //거리 계산
                //Debug.Log("거리" + memoDistance);
                //30m 안에 메모 있는지 확인
                if (memoDistance <= 0.03)  //0.03km
                {
                    //Debug.Log("30m 반경 내에 메모!" + memoDistance);
                    //OutsideButton.gameObject.SetActive(true);
                    //버튼 보이게
                }
                else
                {
                    //Debug.Log("30m 반경 내에 메모 없음!" + memoDistance);
                    //OutsideButton.gameObject.SetActive(false);
                }
            }
        }

        //using UnityEngine.XR.ARCore;

        public void AltitudeBtnClicked()
        {
            lineList.Clear();

            for (int i = 0; i < _historyCollection_nav.Collection.Count; i++)
            {
                Debug.Log("nav : AltitudeBtnClicked");
                var history = _historyCollection_nav.Collection[i];
                if (history.ObjType == "arrow") { history.Altitude = startAltitude - 5; } 
                else if (history.ObjType == "Line") { history.Altitude = startAltitude - 8; }
                else if (history.ObjType == "Goal") { history.Altitude = startAltitude - 5; }
                else { }
                _historyCollection_nav.Collection[i] = history;
            }

            foreach (var anchor in _anchorObjects_nav)
            {
                Destroy(anchor);
            }
            _anchorObjects_nav.Clear();
            SaveGeospatialAnchorHistory_nav();

            //_shouldResolvingHistory_nav = _historyCollection_nav.Collection.Count > 0;
            _shouldResolvingHistory_nav = true;
            ResolveHistory_nav();
        }

        public void AltitudeBtn_down()
        {
            lineList.Clear();

            for (int i = 0; i < _historyCollection_nav.Collection.Count; i++)
            {
                //Debug.Log("nav : AltitudeBtnClicked");
                var history = _historyCollection_nav.Collection[i];
                if (history.ObjType == "arrow") { history.Altitude = history.Altitude - 1; }
                else if (history.ObjType == "Line") { history.Altitude = history.Altitude - 1; }
                else if (history.ObjType == "Goal") { history.Altitude =  history.Altitude - 1; }
                else { }
                _historyCollection_nav.Collection[i] = history;
            }

            foreach (var anchor in _anchorObjects_nav)
            {
                Destroy(anchor);
            }
            _anchorObjects_nav.Clear();
            SaveGeospatialAnchorHistory_nav();

            //_shouldResolvingHistory_nav = _historyCollection_nav.Collection.Count > 0;
            _shouldResolvingHistory_nav = true;
            ResolveHistory_nav();
        }

        public void AltitudeBtn_up()
        {
            lineList.Clear();

            for (int i = 0; i < _historyCollection_nav.Collection.Count; i++)
            {
                //Debug.Log("nav : AltitudeBtnClicked");
                var history = _historyCollection_nav.Collection[i];
                if (history.ObjType == "arrow") { history.Altitude = history.Altitude + 1; }
                else if (history.ObjType == "Line") { history.Altitude = history.Altitude + 1; }
                else if (history.ObjType == "Goal") { history.Altitude =  history.Altitude + 1; }
                else { }
                _historyCollection_nav.Collection[i] = history;
            }

            foreach (var anchor in _anchorObjects_nav)
            {
                Destroy(anchor);
            }
            _anchorObjects_nav.Clear();
            SaveGeospatialAnchorHistory_nav();

            //_shouldResolvingHistory_nav = _historyCollection_nav.Collection.Count > 0;
            _shouldResolvingHistory_nav = true;
            ResolveHistory_nav();
        }

        public void OnApplicationFocus()
        {
            Debug.Log("갤러리에서 돌아옴. AR 세션 재시작.");
            RestartARSessionAndLocation();
        }

        void RestartARSessionAndLocation()
        {
            if (ARSession.state != ARSessionState.SessionTracking)
            {
                Debug.Log("AR 세션이 다시 시작되지 않았음. 재설정 시도.");
                Session.Reset();  // AR 세션 재설정
            }

            if (Input.location.status != LocationServiceStatus.Running)
            {
                Debug.Log("위치 서비스가 중단됨. 다시 시작.");
                Input.location.Start();  // 위치 서비스 다시 시작
            }
        }

    }
}
