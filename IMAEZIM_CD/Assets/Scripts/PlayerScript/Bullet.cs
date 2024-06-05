using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class Bullet : MonoBehaviour
{
    [SerializeField] float bulletSpeed = 2f;
    Rigidbody rb;

    private void Awake()
    {
       rb = GetComponent<Rigidbody>();
    }
    void Update()
    {
        
    }

    private void OnCollisionEnter(Collision collision)
    {
        
        if (collision.gameObject.tag == "Player")
        {
            Debug.Log("ball just bumped into the Enemy!");

           //원격에서는 적용되지 않는 문제점
            gameObject.SetActive(false);
            //Debug.Log(collision.gameObject.name);

            if (collision.collider.gameObject.transform.parent.GetComponent<PhotonView>().IsMine) 
            {
                
                //bullet의 소유권이 내가 아니라면(구현 필요)

                //나보다 속도가 느린 스피너에게 데미지를 입힘.
                collision.collider.gameObject.transform.parent.GetComponent<PhotonView>().RPC("GetDamage", RpcTarget.AllBuffered); 
            }
            

        }
        else if (collision.gameObject.tag == "Wall")
        {
            //Vector3 newDirection = Quaternion.AngleAxis(90, Vector3.up) * rb.velocity.normalized;
            // rb.velocity = newDirection * bulletSpeed;
            // 벽의 표면 법선 벡터 계산
            Vector3 wallNormal = collision.contacts[0].normal;

            // 공의 속도 벡터를 벽의 표면과 반사
            Vector3 reflectedVelocity = Vector3.Reflect(rb.velocity, wallNormal);

            // 반사된 속도 벡터를 사용하여 속도 업데이트
            rb.velocity = reflectedVelocity.normalized * bulletSpeed;

           
            Debug.Log("ball just bumped into the wall!");
        }
        else if (collision.gameObject.name.Contains("wood"))
        {
            Destroy(rb);
        }
        
    }
}
