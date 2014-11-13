using UnityEngine;
using System.Collections;

public sealed class TouchInput : IInput
{
    public bool Primary()
    {
        return Input.touchCount > 1;
    }
    public bool PrimaryAlt()
    {
        // never true for mobile
        return false;
    }
    public bool Secondary()
    {
        return Input.touchCount > 2;
    }

    /// <summary>
    /// Unity's touch implementation requires that you keep state on which finger is doing what
    /// </summary>
    //struct FingerStatus
    //{
    //    // could just use a stack or array, but I guess I'm prematurely optimizing away from anything that touches GC
    //    int nFingers;
    //    int[] fingerIDs = new int[2];

        
    //}
    public Vector2 CurrentPointer { get { return Vector2.zero; } }
    public Vector2 CurrentInputVector
    {
        get 
        { 
            if (Input.touchCount == 0)
            {
                return Vector2.zero;
            }
            //KAI: unreliable, we need to use fingerID to identify touches
            var touch0 = Input.GetTouch(0);
            Vector2 playerInScreen = Camera.main.WorldToScreenPoint(Main.Instance.game.player.transform.position);

            return (touch0.position - playerInScreen).normalized; 
        }
    }
}
