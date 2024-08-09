using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.XR.ARFoundation;

public class ARPlacementManagers : MonoBehaviour
{
    static List<ARRaycastHit> raycast_Hits = new List<ARRaycastHit>();
    public Camera aRCamera;
    public ARRaycastManager m_ARRaycastManager;
    public QuizPlacer QuizPlacer;


    private void OnEnable()
    {
        ActivePresent();
        NotFloating();

    }
    private void OnDisable()
    {
        NotFloating();

    }

    void Update()
    {
        if (QuizPlacer.newQuiz == null)
        {
            Debug.Log("QuizPlacer.newQuiz is null");
            return;
        }
        Vector3 centerOfScreen = new Vector3(Screen.width / 2, Screen.height / 2); //화면의 중심
        Ray ray = aRCamera.ScreenPointToRay(centerOfScreen);

        if (m_ARRaycastManager.Raycast(ray, raycast_Hits, UnityEngine.XR.ARSubsystems.TrackableType.PlaneWithinPolygon))
        {
            Pose hitPose = raycast_Hits[0].pose;
            Vector3 positionToBePlaced = hitPose.position;
            QuizPlacer.newQuiz.transform.position = positionToBePlaced;
        }

    }

    private void NotFloating()
    {
        if (QuizPlacer.newQuiz == null)
        {
            Debug.Log("QuizPlacer.newQuiz is null");
            return;
        }

        ForFloating forFloating = QuizPlacer.newQuiz.GetComponentInChildren<ForFloating>();

        if (forFloating != null)
        {
            forFloating.enabled = !forFloating.enabled;
        }

    }

    public void ActivePresent()
    {
        if (QuizPlacer.newQuiz == null)
        {
            Debug.Log("QuizPlacer.newQuiz is null");
            return;
        }

        Transform present= QuizPlacer.newQuiz.transform.Find("Present");

        if( present != null )
        {
            present.gameObject.SetActive(!present.gameObject.activeSelf); //바로 변경 가능함.
        }
        else
        {
            Debug.Log("I couldn't find the present -  ActivePresent");
        }
    }
}
