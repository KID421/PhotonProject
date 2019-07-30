using UnityEngine;
using Photon.Pun;

public class Bullet : MonoBehaviourPun
{
    [Header("速度")]
    public float speed = 10;

    private void Update()
    {
        Move();
    }

    private void Move()
    {
        transform.Translate(transform.right * Time.deltaTime * speed, Space.World);
    }

    private void Start()
    {
        Invoke("DelayDestroy", 2);
    }

    private void DelayDestroy()
    {
        PhotonNetwork.Destroy(gameObject);
    }
}
