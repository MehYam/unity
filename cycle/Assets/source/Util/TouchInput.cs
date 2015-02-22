#define TILT_MOVEMENT
#define MOVE_DUMBLY_TO_TOUCH
//#define FIRE_AHEAD

using UnityEngine;
using System.Collections;
using System.Collections.Generic;

using PvT.Util;

public sealed class TouchInput : IInput
{
#if TILT_MOVEMENT
    public TouchInput()
    {
    }
#else
    readonly float maxMovementInPixels;
    public TouchInput()
    {
        maxMovementInPixels = Camera.main.pixelRect.size.x / 5;
    }
#endif
    public bool Primary()
    {
#if TILT_MOVEMENT
        // in tilt movement, you hold down two fingers to indicate the secondary
        _touchState.Update();
        return TouchState.Finger.IsDown(_touchState.first) && !TouchState.Finger.IsDown(_touchState.second);
#else

#if FIRE_AHEAD
        return false;
#else
        _touchState.Update();
        return TouchState.Finger.IsDown(_touchState.second);
#endif
#endif
    }
    public bool PrimaryAlt()
    {
#if FIRE_AHEAD
        _touchState.Update();
        return TouchState.Finger.IsDown(_touchState.second);
#else
        return false; // not applicable for mobile, this is equivalent to hitting the spacebar to fire forward
#endif
    }
    public bool Secondary()
    {
#if TILT_MOVEMENT
        // in tilt movement, you hold down two fingers to indicate the secondary
        _touchState.Update();
        return TouchState.Finger.IsDown(_touchState.second) && TouchState.Finger.IsDown(_touchState.first);
#else
        // this is true when _state.second.down is true, AND it was initiated close to
        // the hero's location.  "Close" should be defined in real-world inches
        return false;
#endif
    }
    Vector2 _lastCursor = Vector2.zero;
    public Vector2 CurrentCursor
    {
        get
        {
#if TILT_MOVEMENT
            _touchState.Update();
            if (TouchState.Finger.IsDown(_touchState.first))
            {
                // KAI: okay, this is obscure, but we need to save the last cursor position, otherwise shield and
                // other charge weapons will have nowhere to shoot during the discharge
                //
                // The fix is establishing a proper event system, and not checking for changed user input
                // in Update/FixedUpdate()'s all over the place
                if (TouchState.Finger.IsDown(_touchState.second))
                {
                    _lastCursor = ((_touchState.first.position - _touchState.second.position) * 0.5f) + _touchState.second.position;
                }
                else
                {
                    _lastCursor = _touchState.first.position;
                }
            }
            return _lastCursor;
#else
            _touchState.Update();
            if (TouchState.Finger.IsDown(_touchState.second))
            {
                // KAI: okay, this is obscure, but we need to save the last cursor position, otherwise shield and
                // other charge weapons will have nowhere to shoot during the discharge
                //
                // The fix is establishing a proper event system, and not checking for changed user input
                // in Update/FixedUpdate()'s all over the place
                _lastCursor = _touchState.second.position;
            }
            return  _lastCursor;
#endif
        }
    }
    static readonly float TILT_SENSITIVITY = 4;
    static readonly float TILT_DEADZONE = 0.05f;
    Vector2 _lastMovementVector;
    int     _lastMovementVectorFrame;
    public Vector2 CurrentMovementVector
    {
        get
        {
            if (Time.frameCount > _lastMovementVectorFrame)
            {
                _touchState.Update();
#if TILT_MOVEMENT
                var currentTilt = (Input.acceleration - _calibratedTilt);

                // deadzone
                if (Mathf.Abs(currentTilt.x) < TILT_DEADZONE)
                {
                    currentTilt.x = 0;
                }
                if (Mathf.Abs(currentTilt.y) < TILT_DEADZONE)
                {
                    currentTilt.y = 0;
                }
                _lastMovementVector = currentTilt * TILT_SENSITIVITY;
                _lastMovementVector = Util.Clamp(_lastMovementVector, -1, 1);

                //Debug.Log(_lastMovementVector);
#else
                if (TouchState.Finger.IsDown(_touchState.first))
                {
    #if MOVE_DUMBLY_TO_TOUCH
                    Vector2 playerInScreen = Camera.main.WorldToScreenPoint(Main.Instance.game.player.transform.position);

                    // provide the feel of analog control by factoring the distance of the touch from the player
                    var distance = _touchState.first.position - playerInScreen;
    #else
                    var distance = _touchState.first.position - _touchState.first.startPosition;
    #endif
                    var magnitude = Mathf.Clamp01(distance.magnitude / maxMovementInPixels);

                    _lastMovementVector = distance.normalized * magnitude;
                }
                else
                {
                    _lastMovementVector = Vector2.zero;
                }
#endif
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
            return TouchState.Finger.IsDown(_touchState.first) ? _touchState.first.position : Vector2.zero;
        }
    }
    Vector3 _calibratedTilt;
    public void CalibrateTilt()
    {
        _calibratedTilt = Input.acceleration;
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

        public Finger first { get; private set; }
        public Finger second { get; private set; }

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
                        if (first == null && !Finger.Matches(second, touch))
                        {
                            first = new Finger(touch);
                        }
                        if (!Finger.Matches(first, touch) && !Finger.Matches(second, touch))
                        {
                            second = new Finger(touch);
                        }

                        // just update the remaining
                        if (first != null)
                        {
                            first.MarkAsDown(touch);
                        }
                        if (second != null)
                        {
                            second.MarkAsDown(touch);
                        }
                    }
                }
                if (!Finger.IsDown(first))
                {
                    first = null;
                }
                if (!Finger.IsDown(second))
                {
                    second = null;
                }
                _lastUpdatedFrame = Time.frameCount;
            }
        }
    }
    readonly TouchState _touchState = new TouchState();
}
