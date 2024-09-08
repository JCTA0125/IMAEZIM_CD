using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;
using Photon.Pun;
public class PMovement : MonoBehaviour
{
    [Header("Player")]
    [SerializeField] float speed = 1.4f;
    [SerializeField] public Joystick joystick;
    Rigidbody rb;
    Vector3 moveVec;

    [Header("bullet")]
        [SerializeField] float bulletSpeed = 2f;
        //[SerializeField] GameObject bullet;
        [SerializeField] Transform startPosition;
        [SerializeField] float bulletTime;
    bool isButton;
    string bulletName;

    [Header("bullet")]
        [SerializeField] float curTime;
        [SerializeField] float maxTime;








        void Awake()
        {
            rb = GetComponent<Rigidbody>();
            // rb = transform.parent.GetComponent<Rigidbody>();
        }
        private void Start()
        {
        Debug.Log(transform.parent.gameObject.name);
        curTime = 0;
        if (transform.parent.gameObject.name == "Attacker 2(Clone)")
        {
            bulletName = "AttackerBullet";
        }
        else
        {
            bulletName = "DefenderBullet";
        }
    }

        void FixedUpdate()
        {
            // 1. Input Value
            float x = joystick.Horizontal;
            float z = joystick.Vertical;

            // 2. Move Position 
            moveVec = new Vector3(x, 0, z) * speed * Time.fixedDeltaTime;
            rb.MovePosition(rb.position + moveVec);

            if (moveVec.sqrMagnitude == 0)
                return; // #. No input = No Rotation

            // 3. Move Rotation
            Quaternion dirQuat = Quaternion.LookRotation(moveVec); //바라보는 값
            Quaternion moveQuat = Quaternion.Slerp(rb.rotation, dirQuat, 0.3f);
            rb.MoveRotation(moveQuat);

            //ball 


        }

        private void Update()
        {
            curTime -= Time.deltaTime;
            if (isButton) 
            {
                curTime = maxTime;
                StartCoroutine(FireBulletCoroutine());
                isButton = false;

            }
        }

        private IEnumerator FireBulletCoroutine() //총알을 발사하는 함수
        {
            var a = PhotonNetwork.Instantiate(bulletName, startPosition.position, startPosition.rotation);
            rb.constraints = RigidbodyConstraints.FreezePosition;                      
            a.GetComponent<Rigidbody>().AddForce(startPosition.transform.forward * bulletSpeed);
            rb.constraints = ~RigidbodyConstraints.FreezePosition;

            yield return new WaitForSeconds(bulletTime);
            if(a != null) //총알이 player 에게 맞아 없어지는 상황이 아니라면 
            {
            PhotonNetwork.Destroy(a.gameObject);
            }
    }

        public void ButtonClick() //bullet 생성시간을 확인하고, firebullet을 실행하는 함수
        {
            if (curTime <= 0)
            {
                isButton = true;

            }
            else
            {
                isButton = false;
            }
        }





        private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.name.Contains("Item"))
        {
            Debug.Log("Item collision");
            Destroy(collision.gameObject);
            if (collision.gameObject.name.Contains("1"))
            {
                Debug.Log("velocity");
                
                StartCoroutine(OnActivateItem("velocityItem", 5));
            }
            else if (collision.gameObject.name.Contains("2"))
            {
                StartCoroutine(OnActivateItem("reloadItem", 5));
            }
        }
    }

    IEnumerator OnActivateItem(string buffName, int time)
    {
        float bT = bulletTime;
        if (buffName == "velocityItem")
        {
            speed = speed * 3.0f;
        }
        else if(buffName == "reloadItem")
        {
            //reload Item Activate
            bulletTime = bT / 2;
        }
            
        yield return new WaitForSeconds(time);

        speed = 1.4f;
        bulletTime = bT;
        
    }

}
