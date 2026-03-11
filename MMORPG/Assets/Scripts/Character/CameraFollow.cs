using UnityEngine;

// 摄像机跟随脚本，挂载在摄像机上
public class CameraFollow : MonoBehaviour
{

    private Transform _target;//跟随目标
    public float distance = 3f;//摄像机与目标的距离
    public float mouseSensitivity = 2f;//鼠标灵敏度

    float rotX;//摄像机绕Y轴旋转的角度
    float rotY;//摄像机绕X轴旋转的角度
    public void SetTarget(Transform target)
    {
        _target = target;//设置跟随目标
    }

    private void LateUpdate()
    {
        if (_target == null)
        {
            return;
        }

        rotX += Input.GetAxis("Mouse X") * mouseSensitivity;//根据鼠标输入调整摄像机绕Y轴旋转的角度
        rotY -= Input.GetAxis("Mouse Y") * mouseSensitivity;//根据鼠标输入调整摄像机绕X轴旋转的角度

        rotY = Mathf.Clamp(rotY, -40, 60);//限制摄像机绕X轴旋转的角度在-40到60度之间

        Quaternion rotation = Quaternion.Euler(rotY, rotX, 0);//根据上下左右角度，生成一个旋转

        Vector3 position = _target.position + Vector3.up - (rotation * Vector3.forward * distance);//根据旋转方向，把摄像机放到目标后方 distance 的位置

        transform.position = position;//设置摄像机位置
        transform.rotation = rotation;//设置摄像机旋转
    }
}
