using UnityEngine;
using System.Collections;
using System.Collections.Generic;

using PvT.Util;

public sealed class TouchInput : IInput
{
    public bool Primary()
    {
        return false;
    }
    public bool PrimaryAlt()
    {
        // never true for mobile
        return false;
    }
    public bool Secondary()
    {
        // this is true when _state.second.down is true, AND it was initiated close to
        // the hero's location.  "Close" should be defined in real-world inches
        return false;
    }
    public Vector2 CurrentPointer
    {
        get
        {
            return Vector2.zero;
        }
    }
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

    /// <summary>
    /// Unity's touch implementation requires keeping state on each finger.  Part of why this is complicated is that in my control
    /// scheme, the first finger can lift while the second continues to be held.  If the other finger comes back down, it's still
    /// considered to be the first finger, even though it's now technically second.
    /// 
    /// There are asset store products that implement touch joysticks and controls - it's just that those control mechanisms suck,
    /// I want something that's more intuitive and enjoyable to use.
    /// </summary>
    sealed class TouchState
    {
        // keeping track of the currently pressed fingerIDs, and the order in which they were pressed
        public sealed class FingerState
        {
            public readonly int orderPressed;
            public int lastKnownFrame;
            public Vector2 position;

            public FingerState(int orderPressed) { this.orderPressed = orderPressed; }
        }

        // lookup of ID => FingerState
        Dictionary<int, FingerState> _fingerIDToState = new Dictionary<int, FingerState>();

        IList<int> _expiredFingerIDs = new List<int>(8);

        int _currentFrame;
        public void Update()
        {
            // This implementation wouldn't need to be so complicated if I trusted that Unity told us every touch
            // that has expired.  I don't have confidence in this, so I'm looping all the touches every frame, and
            // determining myself whether a touch has ended.
            if (Time.frameCount > _currentFrame)
            {
                _currentFrame = Time.frameCount;

                // loop the current touches to refresh our knowledge of them
                for (int i = 0; i < Input.touchCount; ++i)
                {
                    var touch = Input.GetTouch(i);
                    bool down = touch.phase != TouchPhase.Canceled || touch.phase != TouchPhase.Ended;
                    if (down)
                    {
                        FingerState fingerState = null;
                        if (!_fingerIDToState.TryGetValue(touch.fingerId, out fingerState))
                        {
                            fingerState = new FingerState(_fingerIDToState.Count);
                            _fingerIDToState[touch.fingerId] = fingerState;
                        }
                        fingerState.lastKnownFrame = _currentFrame;
                        fingerState.position = touch.position;
                    }
                }
                // cull the dropped touches
                foreach (var node in _fingerIDToState)
                {
                    if (node.Value.lastKnownFrame < _currentFrame)
                    {
                        _expiredFingerIDs.Add(node.Key);
                    }
                }
                if (_expiredFingerIDs.Count > 0)
                {
                    Debug.Log(string.Format("Removing {0} fingers", _expiredFingerIDs.Count));
                    foreach (var fingerID in _expiredFingerIDs)
                    {
                        _fingerIDToState.Remove(fingerID);
                    }
                }

                DebugUtil.Assert(_fingerIDToState.Count <= Input.touchCount);
            }
        }
    }
    readonly TouchState _state = new TouchState();
}
