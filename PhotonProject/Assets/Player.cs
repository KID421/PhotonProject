using UnityEngine;

namespace KID
{
    public class Player : MonoBehaviour
    {
        public Rigidbody2D rig;
        public float speed = 10;

        /// <summary>
        /// 固定頻率執行事件
        /// </summary>
        private void FixedUpdate()
        {
            Move();
        }

        private void Move()
        {
            // 剛體.推力(三維向量)
            // 角色.右邊 * ("Horizontal" - A D)
            // 角色.上方 * ("Vertical" - W S)
            rig.AddForce((
                transform.right * Input.GetAxisRaw("Horizontal") +
                transform.up * Input.GetAxisRaw("Vertical")) * speed);
        }
    }
}
