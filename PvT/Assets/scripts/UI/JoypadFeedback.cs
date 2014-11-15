using UnityEngine;
using System.Collections;

public sealed class JoypadFeedback : MonoBehaviour
{
    public GameObject stick;

    static readonly Vector2 OFFSET = new Vector2(50, 50);
    void LateUpdate()
    {
        stick.transform.localPosition = MasterInput.impl.CurrentMovementVector;
        transform.position = Main.Instance.game.player.transform.position;

        //var currentPos = ((TouchInput)(MasterInput.impl)).CurrentMovementPosition;
        //if (currentPos != Vector2.zero)
        //{
        //    stick.transform.localPosition = MasterInput.impl.CurrentMovementVector;
        
        //    var thumbInWorld = Camera.main.ScreenToWorldPoint(currentPos + OFFSET);
        //    thumbInWorld.z = transform.position.z;
        //    transform.position = thumbInWorld;
        //}
    }
}
