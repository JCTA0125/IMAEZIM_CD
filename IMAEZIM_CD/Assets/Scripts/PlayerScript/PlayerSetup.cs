using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using TMPro;
using UnityEngine.UI;

public class PlayerSetup : MonoBehaviourPun
{
    //public TextMeshPro playerNameText;
    public TextMeshProUGUI playerNameText;

    // Start is called before the first frame update
    void Start()
    {
        if (photonView.IsMine) //joystick
        {
            Transform jcanvas = transform.Find("JCanvas");
            transform.Find("Player").GetComponent<Movement>().enabled = true;

            if (jcanvas != null)
            {
                jcanvas.gameObject.SetActive(true);
            }
            else
            {
                Debug.Log("We can't find ur JCanvas");
            }


        }
        else
        {
            Transform jcanvas = transform.Find("JCanvas");

            if (jcanvas != null)
            {
                jcanvas.gameObject.SetActive(false);
            }
            else
            {
                Debug.Log("We can't find ur JCanvas");
            }
            transform.Find("Player").GetComponent<Movement>().enabled = false; 
        }
        SetPlayerName();
    }

    void SetPlayerName()
    {
        if (playerNameText != null)
        {
            if (photonView.IsMine)
            {
                playerNameText.text = "YOU";
                playerNameText.color = Color.red;
            }
            else
            {
                playerNameText.text = photonView.Owner.NickName;
            }
        }
    }
}
