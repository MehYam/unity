using UnityEngine;
using System.Collections;

public class TutorialScript : MonoBehaviour
{
    public bool Test1;
    public AnimatedText CenterPrint;

	// Use this for initialization
	void Start()
    {
        if (Test1)
        {
            StartCoroutine(Test());
        }
        StartCoroutine(Intro());
	}

    IEnumerator Test()
    {
        var messages = new string[] { "Hello", "This is a test", "Seems to work" };

        while (true)
        foreach (var message in messages)
        {
            CenterPrint.text = message;
            yield return new WaitForSeconds(2);

            AnimatedText.FadeOut(CenterPrint, Consts.TEXT_FADE_SECONDS);
            yield return new WaitForSeconds(2);
        }
    }

    IEnumerator Intro()
    {
        yield return new WaitForEndOfFrame();  //HACK to ensure that the next line doesn't nullref

        var main = Main.Instance;
        main.PlayMusic(main.music.intro);
        main.hud.curtain.Fade(1, 0);

        yield return new WaitForSeconds(Consts.TEXT_FADE_SECONDS_FAST);

        AnimatedText.FadeIn(main.hud.centerPrintTop, "We are Eukarya.", Consts.TEXT_FADE_SECONDS);

        yield return new WaitForSeconds(Consts.TEXT_FADE_SECONDS);
        AnimatedText.FadeIn(main.hud.centerPrintMiddle, "This is our story.", Consts.TEXT_FADE_SECONDS);

        yield return new WaitForSeconds(Consts.TEXT_FADE_SECONDS);
        AnimatedText.FadeOut(main.hud.centerPrintTop, Consts.TEXT_FADE_SECONDS);
        AnimatedText.FadeOut(main.hud.centerPrintMiddle, Consts.TEXT_FADE_SECONDS);

        yield return new WaitForSeconds(Consts.TEXT_FADE_SECONDS);
        main.hud.curtain.Fade(0, Consts.TEXT_FADE_SECONDS_SLOW);
        main.hud.space.Fade(1, 0); 
        AnimatedText.FadeIn(main.hud.centerPrintTop, "We travelled the stars for lifetimes,", Consts.TEXT_FADE_SECONDS);

        yield return new WaitForSeconds(Consts.TEXT_FADE_SECONDS);
        AnimatedText.FadeIn(main.hud.centerPrintMiddle, "alone and unbothered.", Consts.TEXT_FADE_SECONDS);

        yield return new WaitForSeconds(Consts.TEXT_FADE_SECONDS);
        AnimatedText.FadeOut(main.hud.centerPrintTop, Consts.TEXT_FADE_SECONDS);
        AnimatedText.FadeOut(main.hud.centerPrintMiddle, Consts.TEXT_FADE_SECONDS);

    }
}
