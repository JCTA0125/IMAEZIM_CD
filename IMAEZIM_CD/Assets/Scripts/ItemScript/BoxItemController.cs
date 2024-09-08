using ExitGames.Client.Photon;
using JetBrains.Annotations;
using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Xml.Serialization;
using Unity.Profiling;
using UnityEngine;

public class BoxItemController : MonoBehaviourPunCallbacks, IPunObservable
{
    private bool isDefault;
    private bool isWoodenBox;
    private bool isTNT;
    private bool _isExplosioned;
    public bool isExplosioned
    {
        get { return _isExplosioned; }
        set
        {
            if (_isExplosioned != value)
            {
                _isExplosioned = value;
                // isExplosioned 변수가 변경될 때마다 동기화를 위한 RPC 호출
                photonView.RPC("OnExplosionStateChanged", RpcTarget.Others, value);
            }
        }
    }


    [SerializeField] private GameObject _replacement;

    [Header("Item Prefabs")]
    public GameObject[] itemDrops;

    private float boxHP;

    // Start is called before the first frame update
    void Start()
    {
        isExplosioned = false;
        if (gameObject.name.Contains("wood"))
        {
            isDefault = false;
            isWoodenBox = true;
            isTNT = false;
            Debug.Log("wwooden");

            boxHP = 100;
        }
        else if (gameObject.name.Contains("TNT"))
        {
            isDefault = false;
            isWoodenBox = false;
            isTNT = true;

            boxHP = 100;
        }
        else if (gameObject.name.Contains("Default"))
        {
            isDefault = true;
            isWoodenBox = false;
            isTNT = false;
        }
    }
   
    private void OnCollisionEnter(Collision collision)
    {
        Debug.Log(collision.gameObject.name);
        if (collision.gameObject.name.Contains("Bullet") && !isDefault)
        {
            isExplosioned = true;
            Destroy(collision.gameObject);
            Debug.Log("sssucces");
            if(isWoodenBox)
            {
                Debug.Log("wooden");
                if (boxHP > 0)
                {
                    boxHP -= 100;
                }
                else
                {
                    OnBoxExplode((Vector3)gameObject.transform.position, isWoodenBox);
                    //gameObject.GetComponent<Collider>().enabled = false;
                    //var replacement = Instantiate(_replacement, transform.position, transform.rotation);
                    //Destroy(gameObject);
                    //var rbs = replacement.GetComponentsInChildren<Rigidbody>();
                    //foreach (var rb in rbs)
                    //{
                    //    rb.AddExplosionForce(0.02f, collision.contacts[0].point, 0.1f);
                    //}
                    //Destroy(replacement, Random.Range(2, 5));
                    //ItemDrop();
                    
                }
            }
            else if(isTNT)
            {
                OnBoxExplode((Vector3)gameObject.transform.position, isWoodenBox);
                //var rb = gameObject.GetComponent<Rigidbody>();
                //rb.AddExplosionForce(collision.relativeVelocity.magnitude*1000, collision.contacts[0].point, 5);
                //var replacement = Instantiate(_replacement, transform.position, transform.rotation);
                //Destroy (gameObject);
                //Destroy(replacement, Random.Range(3, 5));
            }

        }
    }

    private int Choose(float[] probs)
    {

        float total = 0;

        foreach (float elem in probs)
        {
            total += elem;
        }

        float randomPoint = Random.value * total;

        for (int i = 0; i < probs.Length; i++)
        {
            if (randomPoint < probs[i])
            {
                switch (i)
                {
                    case 0:
                        Debug.Log("item1 drop");
                        break;
                    case 1:
                        Debug.Log("item2 drop");
                        break;
                    case 2:
                        Debug.Log("nothing");
                        break;
                }
                return i;
            }
            else
            {
                randomPoint -= probs[i];
            }
        }
        return probs.Length - 1;
    }


    private void ItemDrop()
    {
        //Item Drop Percentage
        //item1 : 80%
        //item2 : 15%
        //default : 5%
        int choice = Choose(new float[3] { 10f, 85f, 5f });
        if(choice != 2)
        {
            Instantiate(itemDrops[choice], transform.position, Quaternion.identity);
        }
        
    }

    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if(stream.IsWriting)
        {
            stream.SendNext(isExplosioned);
        }
        else
        {
            isExplosioned = (bool)stream.ReceiveNext();
        }
    }

    //[PunRPC]
    private void OnBoxExplode(Vector3 explosionPoint, bool isWoodenBox)
    {
        if(isWoodenBox)
        {
            gameObject.GetComponent<Collider>().enabled = false;
            var replacement = Instantiate(_replacement, transform.position, transform.rotation);
            Destroy(gameObject);
            var rbs = replacement.GetComponentsInChildren<Rigidbody>();
            foreach (var rb in rbs)
            {
                rb.AddExplosionForce(0, explosionPoint, 0);
            }
            Destroy(replacement, Random.Range(1, 2));
            ItemDrop();
        }
        else
        {
            Collider[] collider = Physics.OverlapSphere(gameObject.transform.position, 0.5f);
            foreach(Collider c in collider)
            {
                if (c.gameObject.tag == "Player"){
                    c.gameObject.transform.parent.GetComponent<PhotonView>().RPC("GetDamage", RpcTarget.AllBuffered);
                }
            }
            var replacement = Instantiate(_replacement, transform.position, transform.rotation);
            Destroy(gameObject);
            Destroy(replacement, Random.Range(3, 5));
        }
    }

    // 다른 플레이어에게 폭발 상태를 동기화하는 RPC 메소드
    [PunRPC]
    private void OnExplosionStateChanged(bool explosionState)
    {
        _isExplosioned = explosionState;
    }

    void FixedUpdate()
    {
        if (isExplosioned)
        {
            OnBoxExplode((Vector3)gameObject.transform.position, isWoodenBox);
        }
    }

    // isExplosioned 변수의 값이 변경될 때마다 RPC를 호출하여 다른 플레이어에게 동기화
    private void SetExplosionState(bool explosionState)
    {
        if (_isExplosioned != explosionState)
        {
            _isExplosioned = explosionState;
            photonView.RPC("OnExplosionStateChanged", RpcTarget.Others, explosionState);
        }
    }

}