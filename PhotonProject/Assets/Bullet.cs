using UnityEngine;
using Photon.Pun;   // 引用 Photon API

public class Bullet : MonoBehaviourPun  // 繼承父類別 - Pun
{
    [Header("速度")]
    public float speed = 10;

    private void Update()
    {
        Move();
    }

    /// <summary>
    /// 當物件碰撞開始時會執行一次。
    /// </summary>
    private void OnCollisionEnter2D(Collision2D collision)
    {
        // 伺服器.刪除(物件); - 用伺服器生成的物件必須透過伺服器刪除。
        PhotonNetwork.Destroy(gameObject);
    }

    private void Start()
    {
        //Destroy(gameObject, 2);   // Unity API 刪除(物件，延遲刪除時間)
        Invoke("DelayDestroy", 2);  // Photon API 刪除必須透過其他方式延遲 - Invoke("方法名稱"，延遲時間);
    }

    private void DelayDestroy()
    {
        PhotonNetwork.Destroy(gameObject);
    }

    /// <summary>
    /// 子彈移動。
    /// </summary>
    private void Move()
    {
        // 變形元件.移動(子彈.右邊 * 1/60 * 速度，空間.世界座標)
        // X 軸 紅 transform.right
        // Y 軸 綠 transform.up
        // Z 軸 藍 transform.forward
        transform.Translate(transform.right * Time.deltaTime * speed, Space.World);
    }
}
