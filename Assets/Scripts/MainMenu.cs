using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }

    public void OnClickOutdoor()
    {
        SceneManager.LoadScene("Geospatial");
    }

    public void OnClickIndoor()
    {
        SceneManager.LoadScene("SampleScene");
    }


    public void OnClickGame()
    {
        Debug.Log("Game Clicked");
    }

    public void OnClickNav()
    {
        SceneManager.LoadScene("GeospatialArf4");
        //Debug.Log("Nav Clicked");
    }

}

