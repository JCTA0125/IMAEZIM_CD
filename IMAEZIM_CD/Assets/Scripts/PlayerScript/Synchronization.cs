using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class Synchronization : MonoBehaviour, IPunObservable
{
    Rigidbody rb;
    PhotonView photonView;

    Vector3 networkedPosition;
    Quaternion networkedRotation;

    public bool synchronizeVelocity = true;
    public bool synchronizeAngularVelocity = true;
    public bool isTeleportEnabled = true;
    public float teleportIfDistanceGreaterThan = 1.0f;

    private float distance;
    private float angle;

    private GameObject battleArenaGameobject;

    private void Awake()
    {
        Transform parent = transform.Find("Player");
        rb = parent.GetComponent<Rigidbody>();
        photonView = GetComponent<PhotonView>();

        networkedPosition = new Vector3();
        networkedRotation = new Quaternion();

        battleArenaGameobject = GameObject.Find("ArenaStructure");
    }


    private void FixedUpdate()
    {
        if (!photonView.IsMine)
        {
            rb.position = Vector3.MoveTowards(rb.position, networkedPosition, distance * (1.0f / PhotonNetwork.SerializationRate)); //SR 기본 값 10, 1초에 10번 실행됨. 바꾸고 싶으면 start 에서 SR 값을 바꿈.
            rb.rotation = Quaternion.RotateTowards(rb.rotation, networkedRotation, angle * (1.0f / PhotonNetwork.SerializationRate)); //각도 차이가 있으면 rotation을 향해 더 빠르게 회전함.

        }
    }

    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (stream.IsWriting)
        {

            stream.SendNext(rb.position - battleArenaGameobject.transform.position);
            stream.SendNext(rb.rotation);

            if (synchronizeVelocity)
            {
                stream.SendNext(rb.velocity);
            }

            if (synchronizeAngularVelocity)
            {
                stream.SendNext(rb.angularVelocity);
            }

        }
        else
        {
            networkedPosition = (Vector3)stream.ReceiveNext() + battleArenaGameobject.transform.position;
            networkedRotation = (Quaternion)stream.ReceiveNext();

            if (isTeleportEnabled)
            {
                if (Vector3.Distance(rb.position, networkedPosition) > teleportIfDistanceGreaterThan)
                {
                    rb.position = networkedPosition;
                }

            }

            if (synchronizeVelocity || synchronizeAngularVelocity)
            {
                float lag = Mathf.Abs((float)(PhotonNetwork.Time - info.SentServerTime)); //지연시간

                if (synchronizeVelocity) //속도
                {
                    rb.velocity = (Vector3)stream.ReceiveNext();

                    networkedPosition += rb.velocity * lag;

                    distance = Vector3.Distance(rb.position, networkedPosition);
                }

                if (synchronizeAngularVelocity) //각도
                {
                    rb.angularVelocity = (Vector3)stream.ReceiveNext();

                    networkedRotation = Quaternion.Euler(rb.angularVelocity * lag) * networkedRotation;

                    angle = Quaternion.Angle(rb.rotation, networkedRotation);
                }
            }
        }
    }
}