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
        [Header("同步平滑速度"), Range(0.1f, 20)]
        public float smoothSpeed = 0.5f;
        [Header("同步平滑旋轉武器"), Range(0.1f, 20)]
        public float smoothRotateSpeed = 15;
        [Header("圖片渲染器")]
        public SpriteRenderer sr;
        [Header("玩家名稱介面")]
        public Text textName;
        [Header("生成子彈位置")]
        public Transform pointBullet;
        [Header("子彈")]
        public GameObject bullet;
        [Header("中心點")]
        public Transform pointCenter;
        [Header("玩家介面")]
        public GameObject uiPlayer;     // 整組介面
        public Image imageHp;           // 血量圖片
        public Text textHp;             // 血量文字
        public float hp = 100;          // 當前血量
        private float maxHp = 100;      // 最大血量

        private Text textCCU;
        private Vector2 direction;          // 二維向量 方向
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
                textName.text = PhotonNetwork.NickName;     // 玩家名稱介面.文字 = 伺服器.暱稱
                uiPlayer.SetActive(true);                   // 整組介面.啟動設定(顯示)
            }

            // 連線人數介面 = 遊戲物件.尋找("物件名稱").取得元件<元件類型>();
            textCCU = GameObject.Find("連線人數").GetComponent<Text>();
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
                RotateWeapon();
            }
            else
            {
                SmoothMove();
                SmoothRotateWeapon();
            }

            // PhotonNetwork.CurrentRoom.PlayerCount 伺服器.當前房間.玩家數
            textCCU.text = "連線人數：" + PhotonNetwork.CurrentRoom.PlayerCount + " / 20";
        }

        /// <summary>
        /// 2D 物件碰撞開始時執行一次
        /// </summary>
        private void OnCollisionEnter2D(Collision2D collision)
        {
            if (collision.gameObject.tag == "子彈")                 // 如果 碰到.遊戲物件.標籤 等於 "子彈"
            {
                hp -= 10;                                           // 扣血
                imageHp.fillAmount = hp / maxHp;                    // 更新血條介面
                textHp.text = "HP - " + hp + " / " + maxHp;         // 更新血量介面

                if (hp <= 0) Dead();                                // 如果 血量 <= 0 死亡
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

        /// <summary>
        /// 旋轉武器
        /// </summary>
        private void RotateWeapon()
        {
            // 取得滑鼠座標 - 屬於螢幕座標
            Vector3 posMouse = Input.mousePosition;
            // 將螢幕座標轉為世界座標
            Vector3 posWorld = Camera.main.ScreenToWorldPoint(posMouse);
            
            // 計算方向 = 滑鼠.x - 中心點.x，滑鼠.y - 中心點.y
            direction = new Vector2(posWorld.x - pointCenter.position.x, posWorld.y - pointCenter.position.y);

            // 中心點.方向 = 計算方向
            // 前方為紅色 X 軸 - right
            // 前方為綠色 Y 軸 - up
            // 前方為藍色 Z 軸 - forward
            pointCenter.right = direction;
        }

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

        /// <summary>
        /// 其他玩家的武器同步平滑旋轉
        /// </summary>
        private void SmoothRotateWeapon()
        {
            // 中心點.前方 = 二維向量.插值(中心點.前方，方向，平滑速度 * 1 / 60)
            pointCenter.right = Vector2.Lerp(pointCenter.right, direction, smoothRotateSpeed * Time.deltaTime);
        }

        /// <summary>
        /// 玩家死亡的方法
        /// </summary>
        private void Dead()
        {
            if (pv.IsMine)                          // 如果 是自己的物件
            {
                PhotonNetwork.LeaveRoom();          // 伺服器.離開房間
                PhotonNetwork.LoadLevel("大廳");    // 伺服器.載入大廳
            }
        }

        // 同步資料方法
        public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
        {
            // 如果 正在寫入資料
            if (stream.IsWriting)
            {
                stream.SendNext(transform.position);                // 傳遞資料 (座標)
                stream.SendNext(direction);                         // 傳遞資料 (武器方向)
            }
            // 如果 正在讀取資料
            else if (stream.IsReading)
            {
                positionNext = (Vector3)stream.ReceiveNext();       // 同步座標資訊 = (轉型) 接收資料
                direction = (Vector2)stream.ReceiveNext();          // 同步武器方向資訊 = (轉型) 接收資料
            }
        }
        #endregion
    }
}
