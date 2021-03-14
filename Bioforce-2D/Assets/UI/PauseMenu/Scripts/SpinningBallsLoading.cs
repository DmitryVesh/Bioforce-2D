using UnityEngine;

public class SpinningBallsLoading : MonoBehaviour
{
    [SerializeField] private int NumBalls = 8;
    [SerializeField] private float TimeForTurn = 0.1f;
    
    private float TimeStart { get; set; }
    private float AngleTurn { get; set; }

    void Start()
    {
        AngleTurn = (float)360 / (float)NumBalls;
        TimeStart = Time.time;
    }
    
    void FixedUpdate()
    {
        if (Time.time - TimeStart >= TimeForTurn)
        {
            Vector3 rotation = transform.localEulerAngles;
            rotation.z += AngleTurn;
            transform.localEulerAngles = rotation;
            TimeStart = Time.time;
        }
    }
}
