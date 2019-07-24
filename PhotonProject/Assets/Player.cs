using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;

namespace KID
{
    public class Player : MonoBehaviourPun, IPunObservable
    {
        #region 欄位
        [Header("剛體")]
        public Rigidbody2D rig;
        [Header("速度")]
        public float speed = 10;
        [Header("Photon 元件")]
        public PhotonView pv;
        [Header("Player 腳本")]
        public Player player;
        [Header("攝影機")]
        public GameObject obj;
        [Header("同步座標資訊")]
        public Vector3 positionNext;
        [Header("同步平滑速度"), Range(0.1f, 5)]
        public float smoothSpeed = 0.5f;
        [Header("圖片渲染器")]
        public SpriteRenderer sr;
        [Header("玩家名稱介面")]
        public Text textName;
        #endregion

        #region 事件
        private void Start()
        {
            // 如果 不是自己的物件
            if (!pv.IsMine)
            {
                //player.enabled = false;           // 玩家 元件 = 關閉
                obj.SetActive(false);               // 攝影機 物件 (關閉)
                textName.text = pv.Owner.NickName;  // 玩家名稱介面.文字 = Photon 元件.擁有者.暱稱
            }
            // 否則 是自己的物件
            else
            {
                // 玩家名稱介面.文字 = 伺服器.暱稱
                textName.text = PhotonNetwork.NickName;
            }
        }

        /// <summary>
        /// 固定頻率執行事件
        /// </summary>
        private void FixedUpdate()
        {
            // 如果 是自己的物件 執行 移動
            if (pv.IsMine)
            {
                Move();
                FlipSprite();
                Shoot();
            }
            else
            {
                SmoothMove();
            }
        }
        #endregion

        #region 方法
        /// <summary>
        /// 其他玩家的物件同步平滑移動
        /// </summary>
        private void SmoothMove()
        {
            // 其他玩家的座標 = 插值(原本的座標，同步座標資訊，百分比 - 同步平滑速度 * 一個影格的時間)
            transform.position = Vector3.Lerp(transform.position, positionNext, smoothSpeed * Time.deltaTime);
        }

        /// <summary>
        /// 自己的物件移動方式
        /// </summary>
        private void Move()
        {

            // 剛體.推力(三維向量)
            // 角色.右邊 * ("Horizontal" - A D 左 右)
            // 角色.上方 * ("Vertical" - W S 上 下)
            rig.AddForce((
                transform.right * Input.GetAxisRaw("Horizontal") +
                transform.up * Input.GetAxisRaw("Vertical")) * speed);
        }

        [PunRPC]
        private void RPCFlipSprite(bool flip)
        {
            sr.flipX = flip;
        }

        private void FlipSprite()
        {
            if (Input.GetKeyDown(KeyCode.D) || Input.GetKeyDown(KeyCode.RightArrow))
            {
                sr.flipX = false;
                pv.RPC("RPCFlipSprite", RpcTarget.All, false);
            }
            if (Input.GetKeyDown(KeyCode.A) || Input.GetKeyDown(KeyCode.LeftArrow))
            {
                sr.flipX = true;
                pv.RPC("RPCFlipSprite", RpcTarget.All, true);
            }
        }

        // 同步資料方法
        public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
        {
            // 如果 正在寫入資料
            if (stream.IsWriting)
            {
                stream.SendNext(transform.position);                // 傳遞資料 (座標)
            }
            // 如果 正在讀取資料
            else if (stream.IsReading)
            {
                positionNext = (Vector3)stream.ReceiveNext();       // 同步座標資訊 = (轉型) 接收資料
            }
        }
        #endregion

        [Header("生成子彈位置")]
        public Transform pointBullet;
        [Header("子彈")]
        public GameObject bullet;

        /// <summary>
        /// 發射子彈
        /// </summary>
        private void Shoot()
        {
            if (Input.GetKeyDown(KeyCode.Mouse0))
            {
                // 伺服器.實例化(物件名稱，座標，角度)
                PhotonNetwork.Instantiate(bullet.name, pointBullet.position, pointBullet.rotation);
            }
        }
    }
}
