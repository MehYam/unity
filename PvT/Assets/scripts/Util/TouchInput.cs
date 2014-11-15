//#define DRAG_PLAYER_FOR_MOVEMENT
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

using PvT.Util;

public sealed class TouchInput : IInput
{
    readonly float maxMovementInPixels;
    public TouchInput()
    {
        maxMovementInPixels = Camera.main.pixelRect.size.x / 5;
    }

    public bool Primary()
    {
        _touchState.Update();
        return TouchState.Finger.IsDown(_touchState.firing);
    }
    public bool PrimaryAlt()
    {
        return false; // not applicable for mobile, this is equivalent to hitting the spacebar to fire forward
    }
    public bool Secondary()
    {
        // this is true when _state.second.down is true, AND it was initiated close to
        // the hero's location.  "Close" should be defined in real-world inches
        return false;
    }
    public Vector2 CurrentCursor
    {
        get
        {
            _touchState.Update();
            return TouchState.Finger.IsDown(_touchState.firing) ? _touchState.firing.position : Vector2.zero;
        }
    }
    Vector2 _lastMovementVector;
    int     _lastMovementVectorFrame;
    public Vector2 CurrentMovementVector
    {
        get
        {
            if (Time.frameCount > _lastMovementVectorFrame)
            {
                _touchState.Update();
                if (TouchState.Finger.IsDown(_touchState.movement))
                {
    #if DRAG_PLAYER_FOR_MOVEMENT
                    Vector2 playerInScreen = Camera.main.WorldToScreenPoint(Main.Instance.game.player.transform.position);

                    // provide the feel of analog control by factoring the distance of the touch from the player
                    var distance = _touchState.movement.position - playerInScreen;
    #else
                    var distance = _touchState.movement.position - _touchState.movement.startPosition;
    #endif
                    var magnitude = Mathf.Clamp01(distance.magnitude / maxMovementInPixels);

                    _lastMovementVector = distance.normalized * magnitude;
                }
                else
                {
                    _lastMovementVector = Vector2.zero;
                }
                _lastMovementVectorFrame = Time.frameCount;
            }
            return _lastMovementVector;
        }
    }
    public Vector2 CurrentMovementPosition
    {
        get
        {
            _touchState.Update();
            return TouchState.Finger.IsDown(_touchState.movement) ? _touchState.movement.position : Vector2.zero;
        }
    }

    /// <summary>
    /// Unity's touch implementation requires keeping state on each finger.  Part of why this is complicated is that in my control
    /// scheme, the first finger can lift while the second continues to be held.  If the other finger comes back down, it's still
    /// considered to be the first finger, even though it's now technically second.
    /// 
    /// There are asset store products that implement touch joysticks and controls - it's just that those control mechanisms suck,
    /// I want something that's more intuitive and enjoyable to use.
    /// </summary>
    public sealed class TouchState
    {
        // keeping track of the currently pressed fingerIDs, and the order in which they were pressed
        public sealed class Finger
        {
            public readonly int id;
            public readonly Vector2 startPosition;
            public Vector2 position;

            public Finger(Touch touch) 
            { 
                this.id = touch.fingerId;
                this.startPosition = touch.position;

                MarkAsDown(touch);
            }
            int _lastDownFrame;
            public void MarkAsDown(Touch touch)
            {
                if (id == touch.fingerId)
                {
                    _lastDownFrame = Time.frameCount;
                    position = touch.position;
                }
            }
            static public bool IsDown(Finger fs)
            {
                return fs != null && fs._lastDownFrame == Time.frameCount;
            }
            static public bool Matches(Finger fs, Touch touch)
            {
                return fs != null && fs.id == touch.fingerId;
            }
        }

        public Finger movement { get; private set; }
        public Finger firing { get; private set; }

        int _lastUpdatedFrame;
        public void Update()
        {
            if (Time.frameCount > _lastUpdatedFrame)
            {
                // loop the current touches to refresh our knowledge of them
                for (int i = 0; i < Input.touchCount; ++i)
                {
                    var touch = Input.GetTouch(i);
                    bool down = touch.phase != TouchPhase.Canceled || touch.phase != TouchPhase.Ended;
                    if (down)
                    {
                        // This logic is unbelievably convoluted, but I can't find a simpler way to express
                        // the desired input behavior without creating new Dictionaries every frame.
                        if (movement == null && !Finger.Matches(firing, touch))
                        {
                            movement = new Finger(touch);
                        }
                        if (!Finger.Matches(movement, touch) && !Finger.Matches(firing, touch))
                        {
                            firing = new Finger(touch);
                        }

                        // just update the remaining
                        if (movement != null)
                        {
                            movement.MarkAsDown(touch);
                        }
                        if (firing != null)
                        {
                            firing.MarkAsDown(touch);
                        }
                    }
                }
                if (!Finger.IsDown(movement))
                {
                    movement = null;
                }
                if (!Finger.IsDown(firing))
                {
                    firing = null;
                }
                _lastUpdatedFrame = Time.frameCount;
            }
        }
    }
    readonly TouchState _touchState = new TouchState();
}
