using UnityEngine;
using System.Collections;

namespace PvT3D.Util
{
    public struct Timer
    {
        float _delay;
        float _deadline;
        float _randomness;

        /// <summary>
        /// Constructs a limiter that tells you when baseRate seconds have elapsed
        /// </summary>
        /// <param name="delay">The number of seconds after which Now returns true</param>
        /// <param name="randomnessPct">Gives each quantum some randomness.  Passing in 0.5 makes gives rate a range of baseRate/2 to baseRase*3/2</param>
        public Timer(float delay = 0, float randomnessPct = 0)
        {
            _delay = delay;
            _deadline = 0;
            _randomness = randomnessPct;
            Start();
        }
        public void Start()
        {
            if (_randomness != 0)
            {
                float fuzziness = _randomness * _delay;
                _deadline = Time.fixedTime + Random.Range(_delay - fuzziness, _delay + fuzziness);
            }
            else
            {
                _deadline = Time.fixedTime + _delay;
            }
        }
        public void Start(float delay, float randomnessPct = 0)
        {
            _delay = delay;
            _randomness = randomnessPct;
            Start();
        }
        public void Stop()
        {
            _deadline = 0;
        }
        public bool reached { get { return Time.fixedTime >= _deadline; } }
        public float remaining { get { return Mathf.Max(0, _deadline - Time.fixedTime); } }
        public override string ToString()
        {
            return string.Format("Delay {0} randomness {1}, deadline {2}, seconds from now {3}, reached {4}", _delay, _randomness, _deadline, _deadline - Time.fixedTime, reached);
        }
    }
}
