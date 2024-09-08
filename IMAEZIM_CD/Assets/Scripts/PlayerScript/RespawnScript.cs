using Photon.Pun;
using Photon.Realtime;
using UnityEngine;

public class RespawnScript : MonoBehaviourPun
{
    public Vector3 respawnPoint;

    void Update()
    {
        if (photonView.IsMine)
        {
            // 캐릭터가 Y 좌표 -10 이하로 떨어졌을 때
            if (transform.parent.position.y < -10f)
            {
                photonView.RPC("Respawn", RpcTarget.All);
            }
        }
    }

    [PunRPC]
    void Respawn()
    {
        transform.parent.position = respawnPoint;
        Rigidbody rb = GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.velocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
        }
    }
}
