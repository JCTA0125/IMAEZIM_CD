using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class ARSceneManager : MonoBehaviour
{ 
    public void GotoMemoInput()
    {
        SceneManager.LoadScene("MemoInput", LoadSceneMode.Additive);
    }

    public void GotoScence(string sceneName)
    {
        SceneManager.LoadScene(sceneName, LoadSceneMode.Additive);
    }
     
    public void GotoScenceSingle(string sceneName)
    {
        SceneManager.LoadScene(sceneName, LoadSceneMode.Single);
    }

    public void GotoScenceText(string sceneName)
    {
        PlayerPrefs.SetString("MemoType", "Text");
        SceneManager.LoadScene(sceneName, LoadSceneMode.Single);
    }

    public void GotoScenceImage(string sceneName)
    {
        PlayerPrefs.SetString("MemoType", "Picture");
        SceneManager.LoadScene(sceneName, LoadSceneMode.Single);
    } 

    public void GotoScenceSound(string sceneName)
    {
        PlayerPrefs.SetString("MemoType", "Record");
        SceneManager.LoadScene(sceneName, LoadSceneMode.Single);
    }


}
