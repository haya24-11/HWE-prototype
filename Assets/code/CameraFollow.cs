using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    public Transform target;      // 追従する対象 (PlayerBall)
    public float smoothSpeed = 0.125f; // カメラの移動を滑らかにする速度
    public Vector3 offset = new Vector3(0, 0, -10f); // カメラの位置オフセット (Z軸 -10で2D用)

    void LateUpdate()
    {
        if (target == null) return;

        Vector3 desiredPosition = target.position + offset;
        Vector3 smoothedPosition = Vector3.Lerp(transform.position, desiredPosition, smoothSpeed);
        transform.position = smoothedPosition;
    }
}
