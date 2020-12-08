using UnityEngine;

public class FallBoundry : MonoBehaviour
{
    private void OnCollisionEnter2D(Collision2D collision)
    {
        IHealth health = collision.transform.GetComponentInParent<IHealth>();
        if (health != null && health.GetOwnerClientID() == Client.Instance.ClientID)
        {
            health.FallDie();
        }
    }
}
