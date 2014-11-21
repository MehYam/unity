using UnityEngine;
using UnityEngine.UI;

using System.Collections;

using PvT.Util;

public class testbutton : MonoBehaviour
{
    public GameObject ToShowPrefab;

    Fader _fader;
    public void OnClick()
    {
        Debug.Log("OnClick " + Time.frameCount);

        if (!_fader)
        {
            var badgeGO = (GameObject)Instantiate(ToShowPrefab);
            var badge = badgeGO.GetComponent<Graphic>();

            badge.rectTransform.SetParent(GameObject.Find("/Canvas").transform, false);
            badge.rectTransform.anchoredPosition = Vector2.zero;

            _fader = badgeGO.GetOrAddComponent<Fader>();
            _fader.Fade(0, 0, false);
        }
        if (_fader.alpha == 1)
        {
            _fader.Fade(0, 3);
        }
        else
        {
            _fader.Fade(1, 0.5f);
        }
    }
}
