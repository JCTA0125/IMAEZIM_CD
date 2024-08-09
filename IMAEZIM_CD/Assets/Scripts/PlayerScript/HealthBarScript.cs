using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class HealthBarScript : MonoBehaviour
{
    [SerializeField] Slider hpbar;
    [SerializeField] TextMeshProUGUI playerNameText;
    [SerializeField] Transform player;

    [SerializeField] public float maxHp = 100;
    [SerializeField] public float curHp = 100;

    // Start is called before the first frame update
    void Start()
    {
        hpbar.value = (float)curHp / (float)maxHp;
    }

    // Update is called once per frame
    void Update()
    {
        //Hp바 위치
        Vector3 newHPPosition = transform.position;
        newHPPosition.x = player.position.x;
        newHPPosition.z = player.position.z;
        transform.position = newHPPosition;

        //name 위치
        Vector3 newNamePosition = playerNameText.transform.position;
        newNamePosition.x = player.position.x;
        newNamePosition.z = player.position.z;
        playerNameText.transform.position = newNamePosition;

        HandleHp();
    }
    private void HandleHp() //Hp 값을 update시켜주는 함수
    {
        hpbar.value = Mathf.Lerp(hpbar.value, (float)curHp / (float)maxHp, Time.deltaTime); 
    }


}
