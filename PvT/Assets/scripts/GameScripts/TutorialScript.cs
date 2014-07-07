using UnityEngine;
using System.Collections;

using PvT.Util;

public class TutorialScript : MonoBehaviour
{
    public AnimatedText CenterPrint;
    public GameObject HeroSchool;

	// Use this for initialization
	void Start()
    {
        StartCoroutine(Intro());
	}

    IEnumerator Intro()
    {
        yield return new WaitForEndOfFrame();  //HACK to ensure that the next line doesn't nullref

        var main = Main.Instance;
        main.PlayMusic(main.music.intro);
        main.hud.curtain.Fade(1, 0);

        // Text
        yield return new WaitForSeconds(Consts.TEXT_FADE_SECONDS_FAST);

        AnimatedText.FadeIn(main.hud.centerPrintTop, "We are Eukarya.", Consts.TEXT_FADE_SECONDS);

        yield return new WaitForSeconds(Consts.TEXT_FADE_SECONDS);
        AnimatedText.FadeIn(main.hud.centerPrintMiddle, "This is our story.", Consts.TEXT_FADE_SECONDS);

        yield return new WaitForSeconds(Consts.TEXT_FADE_SECONDS);
        AnimatedText.FadeOut(main.hud.centerPrintTop, Consts.TEXT_FADE_SECONDS);
        AnimatedText.FadeOut(main.hud.centerPrintMiddle, Consts.TEXT_FADE_SECONDS);

        // Lift curtain on space and heros
        main.hud.curtain.Fade(0, Consts.TEXT_FADE_SECONDS_SLOW);
        main.hud.space.Fade(1, 0);
        var school = (GameObject)GameObject.Instantiate(HeroSchool);
        var rect = Util.GetScreenRectInWorldCoords(Camera.main);
        school.transform.position = new Vector2(0, rect.bottom - 1);
        yield return new WaitForSeconds(Consts.TEXT_FADE_SECONDS);

        var tween = school.AddComponent<TweenPosition>();
        tween.To(Vector3.zero - Vector3.up, 5);

        AnimatedText.FadeIn(main.hud.centerPrintTop, "For lifetimes we've travelled the stars,", Consts.TEXT_FADE_SECONDS);

        yield return new WaitForSeconds(Consts.TEXT_FADE_SECONDS);
        AnimatedText.FadeIn(main.hud.centerPrintMiddle, "consuming the dust, alone and unbothered.", Consts.TEXT_FADE_SECONDS);

        yield return new WaitForSeconds(Consts.TEXT_FADE_SECONDS);
    }

    void FadeToMap()
    {
        var main = Main.Instance;
        AnimatedText.FadeOut(main.hud.centerPrintTop, Consts.TEXT_FADE_SECONDS);
        AnimatedText.FadeOut(main.hud.centerPrintMiddle, Consts.TEXT_FADE_SECONDS);
    }
}
