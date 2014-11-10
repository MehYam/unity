using UnityEngine;
using System.Collections;

public sealed class FPS : MonoBehaviour
{
    public float updateInterval = 0.5f;

    UnityEngine.UI.Text _text;
    void Start()
    {
        _text = GetComponent<UnityEngine.UI.Text>();
    }

	// Update is called once per frame
    float  _frames = 0;
    float _lastInterval = 0;
	void Update()
    {
	    float now = Time.time;
        float elapsed = now - _lastInterval;

        ++_frames;

        if (elapsed > updateInterval && _frames > 0)
        {
            string fps = string.Format("FPS: {0:f1}", _frames / elapsed);
            if (_text == null)
            {
                Debug.Log(fps);
            }
            else
            {
                _text.text = fps;
            }
            _lastInterval = now;
            _frames = 0;
        }
	}
}
