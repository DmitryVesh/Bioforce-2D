using UnityEngine;

public class SimpleCameraFollow : MonoBehaviour
{
    Transform follow = null;
    private void Awake()
    {
        follow = transform.parent;
        transform.parent.DetachChildren();
    }
    private void FixedUpdate()
    {
        Vector3 cameraFollow = follow.position;
        float offsetY = 0.4f;

        transform.position = new Vector3(cameraFollow.x, cameraFollow.y + offsetY, -10);
    }
}
