using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using System;
using UnityEngine.UI;

public class Damage : MonoBehaviourPun
{
    HealthBarScript healthBarScript;
    int damageAmount;
    public Vector3 firstPos;

    private GameObject enemy;

    [Header("ResultPanel")]
    [SerializeField]public GameObject resultPanelUIPrefab;
    private GameObject resultPanelUIGameobject;

    //        photonView.RPC("ReBorn", RpcTarget.AllBuffered); 어디에? , 위치 리스폰
    private void Awake()
    {
        healthBarScript = transform.Find("HCanvas").Find("HP").GetComponent<HealthBarScript>();
    }
    private void Start()
    {
        if(gameObject.name == "Attacker(Clone)")
        {
            damageAmount = 15;
        }
        else
        {
            damageAmount = 10;
        }
    }

    [PunRPC]
    public void GetDamage()
    {
        if (healthBarScript.curHp >= 10)
        {
            healthBarScript.curHp -= damageAmount;


        }
        else
        {
            Die();
        }

    }

    void Die()
    {
        GameObject player = transform.gameObject;
        player.transform.Find("JCanvas").gameObject.SetActive(false); //조이스틱 없앰


        if (photonView.IsMine)
        {
            //게임 캐릭터를 없앤 후, 상대가 죽은 사실을 알리고 판넬도 띄우자.
            FindOpponentCharacter(); //알릴 상대를 찾음

            if (enemy != null)
            {
                enemy.GetComponent<PhotonView>().RPC("YouWin", RpcTarget.OthersBuffered); //나빼고 상대방만
            }
            else
            {
                Debug.Log("I can't find the enemy so youwin x");
            }
            YouLose();
            
        }
    }

    void FindOpponentCharacter()
    {
        // 자신이 로컬 플레이어이면 상대방 캐릭터를 찾아서 할당
        if (photonView.IsMine)
        {
            GameObject[] attackers = GameObject.FindGameObjectsWithTag("Attacker");
            GameObject[] defenders = GameObject.FindGameObjectsWithTag("Defender");

            if(attackers.Length > 0)
            {
                foreach (GameObject player in attackers)
                {
                    if (!player.GetComponent<PhotonView>().IsMine)
                    {
                        enemy = player;
                        break;
                    }
                }
            }

            if (defenders.Length > 0)
            {
                foreach (GameObject player in defenders)
                {
                    if (!player.GetComponent<PhotonView>().IsMine)
                    {
                        enemy = player;
                        break;
                    }
                }
            }
        }
    }


    void YouLose()
    {
        StartCoroutine(ResultPanel(false));
    }

    [PunRPC]
    void YouWin()
    {
        GameObject player = transform.gameObject;
        Transform jCanvas = player.transform.Find("JCanvas"); //조이스틱만 없애도록 바꾸자.
        if(jCanvas != null)
        {
            jCanvas.gameObject.SetActive(false);
        }
        else
        {
            Debug.Log("can't find the jCanvas");
        }

        StartCoroutine(ResultPanel(true));
    }

    IEnumerator ResultPanel(bool result)
    {
        //그럼 판넬 만들어보자
        GameObject canvasGameobject = GameObject.Find("Canvas");

        if (resultPanelUIGameobject == null) //처음에만 만듦.
        {
            resultPanelUIGameobject = Instantiate(resultPanelUIPrefab, canvasGameobject.transform);
        }
        else
        {
            resultPanelUIGameobject.SetActive(true);
        }
        Text DeathInfoText = resultPanelUIGameobject.transform.Find("DeathInfoText").GetComponent<Text>();
        if (DeathInfoText != null)
        {
            if (result)
            {
                DeathInfoText.text = "You Win";
            }
            else
            {
                DeathInfoText.text = "You Lose";
            }
        }
        else {
            Debug.Log("We can't find a Deathinfo");
        }


        Text respawnTimeText = resultPanelUIGameobject.transform.Find("RespawnTimeText").GetComponent<Text>();
        if (respawnTimeText != null)
        {
            float respawnTime = 5.0f;

            respawnTimeText.text = respawnTime.ToString(".00");

            while (respawnTime > 0.0f)
            {
                yield return new WaitForSeconds(1.0f);
                respawnTime -= 1.0f;
                respawnTimeText.text = respawnTime.ToString(".00");
            }

            resultPanelUIGameobject.SetActive(false);
            SceneLoader.Instance.LoadScene("Scene_Lobby");
            /*
            if (result)
            {
                photonView.RPC("ReBorn", RpcTarget.AllBuffered);
                Debug.Log("ReBorn 1");
            }
            */
        }
        else
        {
            Debug.Log("We can't find a respawnTimeText");

        }
    }

    [PunRPC] //일단 reborn pass
    public void ReBorn()
    {
        if (photonView.IsMine)
        {
            healthBarScript.curHp = 100; 
            GameObject player = transform.gameObject;
            if(firstPos != Vector3.zero) { //null 이 아닌 zero 임
                player.transform.position = firstPos;
            }

            player.transform.Find("JCanvas").gameObject.SetActive(true);

            Debug.Log("ReBorn 2");
        }

    }
}
