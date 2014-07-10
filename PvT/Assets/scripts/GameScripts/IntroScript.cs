using UnityEngine;
using System.Collections;
using System.Collections.Generic;

using PvT.Util;

public class IntroScript : MonoBehaviour
{
    public GameObject HeroSchool;
	void Start()
    {
        DebugUtil.Log(this, "Start");

        StartCoroutine(IntroSequence());
	}

    IEnumerator IntroSequence()
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
        var screen = Util.GetScreenRectInWorldCoords(Camera.main);
        school.transform.position = new Vector2(0, screen.bottom - 1.5f);
        yield return new WaitForSeconds(Consts.TEXT_FADE_SECONDS);

        var tween = school.AddComponent<TweenPosition>();
        tween.To(Vector3.zero - Vector3.up, 10);

        AnimatedText.FadeIn(main.hud.centerPrintTop, "For lifetimes we roamed the stars,", Consts.TEXT_FADE_SECONDS);

        yield return new WaitForSeconds(Consts.TEXT_FADE_SECONDS);
        AnimatedText.FadeIn(main.hud.centerPrintMiddle, "Sifting the dust, unbothered.", Consts.TEXT_FADE_SECONDS);

        yield return new WaitForSeconds(Consts.TEXT_FADE_SECONDS);
    
        AnimatedText.FadeOut(main.hud.centerPrintTop, Consts.TEXT_FADE_SECONDS);
        AnimatedText.FadeOut(main.hud.centerPrintMiddle, Consts.TEXT_FADE_SECONDS);

        yield return new WaitForSeconds(Consts.TEXT_FADE_SECONDS);

        AnimatedText.FadeIn(main.hud.centerPrintTop, "Until one day.", Consts.TEXT_FADE_SECONDS);

        yield return new WaitForSeconds(Consts.TEXT_FADE_SECONDS);

        AnimatedText.FadeOut(main.hud.centerPrintTop, Consts.TEXT_FADE_SECONDS_FAST);

        yield return new WaitForSeconds(Consts.TEXT_FADE_SECONDS_FAST);

        // Introduce enemies
        AnimatedText.FadeIn(main.hud.centerPrintTop, "We were found.", Consts.TEXT_FADE_SECONDS);

        var mobParent = new GameObject("mobParent");

        SetupMobTween(mobParent, "CYGNUS", school.transform.position, screen.min - Vector2.one);
        yield return new WaitForSeconds(0.25f);
        SetupMobTween(mobParent, "ROCINANTE", school.transform.position, new Vector2(screen.right + 1, 0));
        yield return new WaitForSeconds(0.25f);
        SetupMobTween(mobParent, "ESOX", school.transform.position, new Vector2(screen.right + 1, screen.bottom - 1));
        yield return new WaitForSeconds(0.5f);
        SetupMobTween(mobParent, "CYGNUS", school.transform.position, screen.max + Vector2.one);
        yield return new WaitForSeconds(0.25f);
        SetupMobTween(mobParent, "CYGNUS3", school.transform.position, new Vector2(screen.left - 1, screen.top + 1));

        StartCoroutine(CollapseSchool(school));

        yield return new WaitForSeconds(Consts.TEXT_FADE_SECONDS);
        AnimatedText.FadeOut(main.hud.centerPrintTop, Consts.TEXT_FADE_SECONDS_SLOW);

        main.hud.curtain.Fade(1, Consts.TEXT_FADE_SECONDS);

        yield return new WaitForSeconds(Consts.TEXT_FADE_SECONDS);

        main.hud.space.Fade(0, 0);

        // clean up actors
        GameObject.Destroy(school);
        GameObject.Destroy(mobParent);

        GlobalGameEvent.Instance.FireIntroOver(this);
    }

    void SetupMobTween(GameObject parent, string vehicle, Vector2 target, Vector2 start)
    {
        var main = Main.Instance;
        var mob = main.game.SpawnMob(vehicle);
        mob.transform.position = start;
        mob.transform.parent = parent.transform;

        var bf = ActorBehaviorFactory.Instance;

        mob.GetComponent<Actor>().behavior = new CompositeBehavior(
            bf.CreatePositionTween(Vector3.Lerp(start, target, 0.75f), 4),
            bf.CreateFacePoint(target)
        );
    }

    IEnumerator CollapseSchool(GameObject school)
    {
        var mainHero = school.transform.FindChild("scaler/mainHero");

        var children = new List<TweenPosition>();
        for (var i = 0; i < school.transform.childCount; ++i)
        {
            var child = school.transform.GetChild(i).GetComponent<SpriteRenderer>();
            if (child != null && child != mainHero)
            {
                var tween = child.gameObject.AddComponent<TweenPosition>();
                children.Add(tween);
            }
        }

        foreach (var tween in children)
        {
            yield return new WaitForSeconds(Random.Range(0.05f, 0.2f));
            tween.To(mainHero.transform.position, Random.Range(0.1f, 0.5f), (go) =>
                {
                    GameObject.Destroy(go);
                }
            );
        }

        yield break;
    }
}
