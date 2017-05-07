using UnityEngine;
using System.Collections;

using PvT3D.Util;

namespace PvT3D.Util
{
    public static class InputUtil
    {
        static Vector3 _movementVector;
        static float _lastMovementTime;
        public static Vector3 MovementVector
        {
            get
            {
                // we do a little processing here to 1) eliminate the Descent double-chording effect when moving in two axes, and
                // 2) (prematurely?) optimize so we only calculate the vector once per frame
                if (Time.fixedTime != _lastMovementTime)
                {
                    _movementVector = new Vector3(Input.GetAxis("Horizontal"), 0, Input.GetAxis("Vertical"));
                    if (_movementVector.sqrMagnitude >= 2)
                    {
                        _movementVector.Normalize();
                    }
                    _lastMovementTime = Time.fixedTime;
                }
                return _movementVector;
            }
        }
        public static Vector3 GetFiringVector(Vector3 relativeTo)
        {
            // test the mouse first
            if (Input.GetMouseButton(0))
            {
                return Util.MouseVector(relativeTo);
            }
            // next, the right thumbstick
            return new Vector3(Input.GetAxis("RightStickHorz"), 0, -Input.GetAxis("RightStickVert"));
        }
    }
}
