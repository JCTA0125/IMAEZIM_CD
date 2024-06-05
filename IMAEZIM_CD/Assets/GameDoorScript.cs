using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameDoorScript : MonoBehaviour
{

    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;
            if (Physics.Raycast(ray, out hit))
            {
                if (hit.collider.CompareTag("Door")) //Door 이란 씬을 만든다. 
                {
                    // 여기서 "YourSceneName"을 로드할 씬의 이름으로 바꿔주세요
                    SceneManager.LoadScene("Scene_Lobby");
                }
            }
        }
    }
}
