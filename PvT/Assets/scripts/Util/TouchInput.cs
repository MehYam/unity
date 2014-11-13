using UnityEngine;
using System.Collections;

public sealed class TouchInput : IInput
{
    public bool Primary()
    {
        return Input.GetButton("Fire1");
    }
    public bool PrimaryAlt()
    {
        return Input.GetButton("Jump");
    }
    public bool Secondary()
    {
        return Input.GetButton("Fire2");
    }
    public Vector2 CurrentInputVector
    {
        get 
        { 
            if (Input.touchCount == 0)
            {
                return Vector2.zero;
            }
            //KAI: we need to use fingerID to be more reliable than this
            var touch0 = Input.GetTouch(0);
            Vector2 playerInScreen = Camera.main.WorldToScreenPoint(Main.Instance.game.player.transform.position);

            return (touch0.position - playerInScreen).normalized; 
        }
    }
}
