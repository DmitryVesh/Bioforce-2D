
using UnityEngine;

public static class PhysicsHelper
{
    public static bool GoingRight(Transform target, Transform collider)
    {
        bool goingRight;
        if (target.position.x > collider.position.x) 
            goingRight = false;
        else 
            goingRight = true;
        return goingRight;
    }

    internal static bool RandomBool()
    {
        return Random.Range(0, 2) == 0;
    }
}
