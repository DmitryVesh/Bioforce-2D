
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
        int randomNum = Random.Range(0, 2);
        if (randomNum == 0)
            return false;
        else
            return true;
    }
}
