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
        /// <summary>
        /// Flips a coin.  You can set the odds
        /// </summary>
        /// <param name="odds">The % of chance of heads</param>
        /// <returns></returns>
        static public bool CoinFlip(float odds = 0.5f)
        {
            return Random.value >= odds;
        }

        static public float SPRITE_FORWARD_ANGLE { get; set; }
        static public Quaternion GetLookAtAngle(Vector2 point)
        {
            return Quaternion.Euler(0, 0, Mathf.Atan2(point.y, point.x) * Mathf.Rad2Deg + SPRITE_FORWARD_ANGLE);
        }
        static public Vector2 GetLookAtVector(float angle, float magnitude)
        {
            var vector = new Vector2(0, magnitude);
            return RotatePoint(vector, -SPRITE_FORWARD_ANGLE - angle);
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
            var lookVector = target - looker.position;
            var angle = Mathf.Atan2(lookVector.y, lookVector.x) * Mathf.Rad2Deg;
            if (maxDegrees != -1)
            {
                var currentAngle = looker.transform.rotation.eulerAngles.z - SPRITE_FORWARD_ANGLE;
                var diff = Mathf.DeltaAngle(currentAngle, angle);

                diff = Util.Clamp(diff, -maxDegrees, maxDegrees);
                angle = currentAngle + diff;
            }
            looker.rotation = Quaternion.Euler(0, 0, angle + SPRITE_FORWARD_ANGLE);
        }
        static public bool IsLookingAt(Transform looker, Vector2 target, float toleranceDegrees)
        {
            var lookVector = new Vector3(target.x, target.y) - looker.position;
            var angle = Mathf.Atan2(lookVector.y, lookVector.x) * Mathf.Rad2Deg;

            var diff = Mathf.DeltaAngle(looker.transform.rotation.eulerAngles.z - SPRITE_FORWARD_ANGLE, angle);
            return Mathf.Abs(diff) < toleranceDegrees;
        }
        static public Quaternion GetAngleToMouse(Transform transform)
        {
            var mouse = Input.mousePosition;
            var screen = Camera.main.WorldToScreenPoint(transform.position);
            var lookDirection = new Vector2(mouse.x - screen.x, mouse.y - screen.y);
            return Util.GetLookAtAngle(lookDirection);
        }
        
        static public float DegreesBetweenPoints(Vector2 a, Vector2 b)
        {
            return DegreesRotation(a - b);
        }
        static public float DegreesRotation(Vector2 point)
        {
            return Mathf.Atan2(point.x, point.y) * Mathf.Rad2Deg;
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
        static public void RemoveAllChildren(Transform transform)
        {
            Debug.Log("Destroying children: " + transform.childCount);

            var children = new GameObject[transform.childCount];
            for (var i = 0; i < transform.childCount; ++i)
            {
                children[i] = transform.GetChild(i).gameObject;
            }
            foreach (var child in children)
            {
                GameObject.Destroy(child);
            }
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
            DebugUtil.Assert(_screenRect != null);
            return _screenRect;
        }

        static public float Clamp(float value, float min, float max)
        {
            return Mathf.Max(min, Mathf.Min(value, max));
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

            public int Count { get { return items.Length; } }
            public void SetIndex(int i) { iItem = i; }

            readonly string[] items;
            int iItem = 0;
            public CSVParseHelper(string csvLine) { items = Util.SplitCSVLine(csvLine); }
        }
    }

    public static class DebugUtil
    {
        [System.Diagnostics.Conditional("DEBUG")]
        static public void Assert(bool condition)
        {
            if (!condition) throw new System.Exception();
        }

        [System.Diagnostics.Conditional("DEBUG")]
        static public void Log(object from, string str)
        {
            Debug.Log(string.Format("=={0} frame {1}: {2}", from, Time.frameCount, str));
        }
    }
}
