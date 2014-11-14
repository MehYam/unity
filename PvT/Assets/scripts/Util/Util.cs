#define DEBUG

using UnityEngine;
using UnityEngine.UI;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Linq;

using Random = UnityEngine.Random;

namespace PvT.Util
{
    public static class Util
    {
        static public int FastLog2Floor(int n)
        {
            // Overkill, but fun to investigate.  This is exactly twice as fast as (int)Math.Log in mono,
            // slightly less advantageous in VS 2010
            int bits = 0;
            while (n > 1)
            {
                ++bits;
                n >>= 1;
            }
            return bits;
        }
        /// <summary>
        /// Flips a coin.  You can set the odds
        /// </summary>
        /// <param name="oddsOfTrue">The % of chance of heads</param>
        /// <returns>Returns true if heads</returns>
        static public bool CoinFlip(float oddsOfTrue = 0.5f)
        {
            return Random.value <= oddsOfTrue;
        }

        static public float SPRITE_FORWARD_ANGLE { get; set; }
        static public float GetLookAtAngle(Vector2 point)
        {
            return DegreesRotation(point) + SPRITE_FORWARD_ANGLE;
        }
        static public float GetLookAtAngle(Vector2 looker, Vector2 target)
        {
            return DegreesRotation(target - looker) + SPRITE_FORWARD_ANGLE;
        }
        static public Vector2 GetLookAtVector(float angle, float magnitude)
        {
            var vector = new Vector2(0, magnitude);
            return RotatePoint(vector, -SPRITE_FORWARD_ANGLE - angle);
        }
        //KAI: look at replacing more calls with the following
        static public Vector2 GetLookAtVector(Vector2 looker, Vector2 target)
        {
            var retval = target - looker;
            retval.Normalize();
            return retval;
        }
        static public Vector2 RotatePoint(Vector2 point, float degrees)
        {
            var radians = (degrees - SPRITE_FORWARD_ANGLE) * Mathf.Deg2Rad;
            var cos = Mathf.Cos(radians);
            var sin = Mathf.Sin(radians);

            var retval = new Vector2();
            retval.x = cos * point.x - sin * point.y;
            retval.y = sin * point.x - cos * point.y;

            return retval;
        }
        static public void LookAt2D(Transform looker, Transform target, float maxDegrees = -1)
        {
            LookAt2D(looker, target.position, maxDegrees);
        }
        static public void LookAt2D(Transform looker, Vector2 target, float maxDegrees = -1)
        {
            LookAt2D(looker, new Vector3(target.x, target.y), maxDegrees);
        }
        static public void LookAt2D(Transform looker, Vector3 target, float maxDegrees = -1)
        {
            var angle = DegreesRotation(target - looker.position);
            if (maxDegrees != -1)
            {
                var currentAngle = looker.transform.rotation.eulerAngles.z - SPRITE_FORWARD_ANGLE;
                var diff = Mathf.DeltaAngle(currentAngle, angle);

                diff = Mathf.Clamp(diff, -maxDegrees, maxDegrees);
                angle = currentAngle + diff;
            }
            looker.rotation = Quaternion.Euler(0, 0, angle + SPRITE_FORWARD_ANGLE);
        }
        static public bool IsLookingAt(Transform looker, Vector2 target, float toleranceDegrees)
        {
            var lookVector = new Vector3(target.x, target.y) - looker.position;
            var angle = DegreesRotation(lookVector);

            var diff = Mathf.DeltaAngle(looker.transform.rotation.eulerAngles.z - SPRITE_FORWARD_ANGLE, angle);
            return Mathf.Abs(diff) < toleranceDegrees;
        }
        static public Vector2 GetLookAtVectorToMouse(Vector2 looker)
        {
            var screen = Camera.main.WorldToScreenPoint(looker);
            return MasterInput.impl.CurrentCursor - (Vector2)screen;
        }
        static public float GetAngleToMouse(Transform transform)
        {
            return Util.GetLookAtAngle(GetLookAtVectorToMouse(transform.position));
        }
        static public float DegreesBetweenPoints(Vector2 a, Vector2 b)
        {
            return DegreesRotation(a - b);
        }
        static public float DegreesRotation(Vector2 point)
        {
            return Mathf.Atan2(point.y, point.x) * Mathf.Rad2Deg;
        }
        static public float DegreesRotation(float x, float y)
        {
            return Mathf.Atan2(y, x) * Mathf.Rad2Deg;
        }
        // returns a signed difference between two angles useful for evolving one to the other
        //static public float diffRadians(float source, float target)
        //{
        //    var raw = target - source;
        //    return Mathf.Atan2(Mathf.Sin(raw), Mathf.Cos(raw));
        //}
        static public Vector3 Add(Vector2 a, Vector3 b)
        {
            return new Vector3(a.x + b.x, a.y + b.y, b.z);
        }
        static public Vector2 Add(Vector3 a, Vector2 b)
        {
            return new Vector2(a.x + b.x, a.y + b.y);
        }
        static public Vector2 Sub(Vector3 a, Vector2 b)
        {
            return new Vector2(a.x - b.x, a.y - b.y);
        }
        static public Vector2 Sub(Vector2 a, Vector3 b)
        {
            return new Vector2(a.x - b.x, a.y - b.y);
        }
        static public Vector3 ScatterRandomly(float radius)
        {
            return new Vector3(Random.Range(-radius, radius), Random.Range(-radius, radius));
        }
        /// <summary>
        /// When you want something to launch out the 'nose' of a launcher
        /// </summary>
        /// <param name="launcher">The guy with a nose</param>
        /// <param name="launchee">The projectile</param>
        static public void PrepareLaunch(Transform launcher, Transform launchee, Vector2 offset, float angle = 0)
        {
            var scale = launcher.localScale;
            var scaledOffset = new Vector2(offset.x, offset.y);
            scaledOffset.Scale(scale);
            scaledOffset.y = offset.y;  //KAI: not sure why....

            launchee.localPosition = Add(launcher.transform.position, scaledOffset);
            launchee.RotateAround(launcher.transform.position, Vector3.forward, launcher.transform.rotation.eulerAngles.z);

            launchee.Rotate(0, 0, -angle);

        }

        static float _lastCalcedScreenRectTime = -1;
        static XRect _screenRect;
        static public XRect GetScreenRectInWorldCoords(Camera camera)
        {
            if (Time.time > _lastCalcedScreenRectTime)
            {
                var pixels = camera.pixelRect;
                var screenMin = camera.ScreenToWorldPoint(Vector3.zero);
                var screenMax = camera.ScreenToWorldPoint(new Vector3(pixels.xMax, pixels.yMax));

                _screenRect = new XRect(screenMin, screenMax);

                _lastCalcedScreenRectTime = Time.time;
            }
            DebugUtil.Assert(_screenRect != null, "_screenRect failed to calculate");
            return _screenRect;
        }

        static public void DisablePhysics(GameObject go)
        {
            go.rigidbody2D.velocity = Vector2.zero;
            go.rigidbody2D.isKinematic = true;
            go.collider2D.enabled = false;
        }
        static public void EnablePhysics(GameObject go)
        {
            go.rigidbody2D.isKinematic = false;
            go.collider2D.enabled = true;
        }
        static public IEnumerator WaitForRealSeconds(float time)
        {
            float start = Time.realtimeSinceStartup;
            while (Time.realtimeSinceStartup < start + time)
            {
                yield return null;
            }
        }
        static public void SetAlpha(SpriteRenderer sprite, float alpha)
        {
            var color = sprite.color;
            color.a = alpha;
            sprite.color = color;
        }
        static public void SetAlpha(TextMesh text, float alpha)
        {
            var color = text.color;
            color.a = alpha;
            text.color = color;
        }
        static public void SetAlpha(Text text, float alpha)
        {
            var color = text.color;
            color.a = alpha;
            text.color = color;
        }
        public static Color32 UIntToColor(uint value)
        {
            byte a = (byte)(value >> 24);
            byte r = (byte)(value >> 16);
            byte g = (byte)(value >> 8);
            byte b = (byte)(value >> 0);
            return new Color32(r, g, b, a);
        } 
        static public void Log(string fmt, params object[] args)
        {
            Debug.Log(string.Format(fmt, args));
        }

        /// <summary>
        /// Tests a condition each frame until its true.  Useful for blocking a coroutine until the completion of some task
        /// e.g.
        /// var clickTarget = _clicks + 3;
        /// yield return StartCoroutine(YieldUntil(() =>
        ///     {
        ///         return clickTarget <= _clicks;
        ///     }
        /// ));
        /// </summary>
        /// <param name="condition">The condition (i.e. lambda) to execute each frame</param>
        /// <returns>A coroutine enumerator</returns>
        static public IEnumerator YieldUntil(Func<bool> condition)
        {
            while (!condition())
            {
                yield return new WaitForEndOfFrame();
            }
        }

        /// <summary>
        /// Extension method on GameObject that adds a component of type T, or returns
        /// an existing if a T is already attached.
        /// </summary>
        /// <typeparam name="T">The type of the component</typeparam>
        /// <param name="obj">The implicit "this" reference of the GameObject. This is an extension method</param>
        /// <returns>Returns the new or pre-existing component of type T</returns>
        static public T GetOrAddComponent<T>(this GameObject obj) where T : Component
        {
            T retval = obj.GetComponent<T>();
            if (retval == null)
            {
                retval = obj.AddComponent<T>();
            }
            return retval;
        }

        public static string[] SplitLines(string lines, bool skipHeader = false)
        {
            var retval = lines.Split("\n\r".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
            if (skipHeader)
            {
                var hack = new string[retval.Length - 1];
                Array.Copy(retval, 1, hack, 0, hack.Length);

                retval = hack;
            }
       
            return retval;
        }
        public static IEnumerable<string[]> ReadCSV(string CSV, bool skipHeader = false)
        {
            var lines = SplitLines(CSV, skipHeader);
            foreach (var line in lines)
            {
                yield return SplitCSVLine(line);
            }
        }
        static public string[] SplitCSVLine(string line)
        {
            return (from System.Text.RegularExpressions.Match m in System.Text.RegularExpressions.Regex.Matches(line,
            @"(((?<x>(?=[,\r\n]+))|""(?<x>([^""]|"""")+)""|(?<x>[^,\r\n]+)),?)",
            System.Text.RegularExpressions.RegexOptions.ExplicitCapture)
                    select m.Groups[1].Value).ToArray();
        }
        public sealed class CSVParseHelper
        {
            public string GetString() { return items[iItem++]; }
            public float GetFloat() { return float.Parse(items[iItem++]); }
            public int GetInt() { return int.Parse(items[iItem++]); }
            public bool GetBool()
            {
                var item = GetString().ToLowerInvariant();
                return item == "yes" || item == "1" || item == "true" || item == "sure" || item == "why not";
            }
            public Color32 GetHexColor()
            {
                uint value = Convert.ToUInt32(items[iItem++], 16);
                return UIntToColor(value);
            }
            public int Count { get { return items.Length; } }
            public void SkipField(int nFields = 1) { iItem += nFields; }

            readonly string[] items;
            int iItem = 0;
            public CSVParseHelper(string csvLine) { items = Util.SplitCSVLine(csvLine); }
        }
    }

    public static class DebugUtil
    {
        [System.Diagnostics.Conditional("DEBUG")]
        static public void Assert(bool condition, string msg = "")
        {
            if (!condition)
            {
                throw new System.ApplicationException(msg);
            }
        }

        [System.Diagnostics.Conditional("DEBUG")]
        static public void Log(object from, string str)
        {
            Debug.Log(string.Format("=={0} frame {1}: {2}", from, Time.frameCount, str));
        }
    }
}
