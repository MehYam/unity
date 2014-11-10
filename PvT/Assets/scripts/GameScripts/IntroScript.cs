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
        var hud = main.hud;
        main.PlayMusic(main.music.intro);
        hud.curtain.Fade(1, 0);

        // Text
        yield return new WaitForSeconds(Consts.TEXT_FADE_SECONDS_FAST);

        AnimatedText.FadeIn(hud.centerPrints.top, "We are Eukarya.", Consts.TEXT_FADE_SECONDS);

        yield return new WaitForSeconds(Consts.TEXT_FADE_SECONDS);
        AnimatedText.FadeIn(hud.centerPrints.middle, "This is our story.", Consts.TEXT_FADE_SECONDS);

        yield return new WaitForSeconds(Consts.TEXT_FADE_SECONDS);
        AnimatedText.FadeOut(hud.centerPrints.top, Consts.TEXT_FADE_SECONDS);
        AnimatedText.FadeOut(hud.centerPrints.middle, Consts.TEXT_FADE_SECONDS);

        // Lift curtain on space and heros
        hud.curtain.Fade(0, Consts.TEXT_FADE_SECONDS_SLOW);
        hud.space.Fade(1, 0);
        var school = (GameObject)GameObject.Instantiate(HeroSchool);
        var screen = Util.GetScreenRectInWorldCoords(Camera.main);
        school.transform.position = new Vector2(0, screen.bottom - 1.5f);
        yield return new WaitForSeconds(Consts.TEXT_FADE_SECONDS);

        var tween = school.AddComponent<TweenPosition>();
        tween.To(Vector3.zero - Vector3.up, 10);

        AnimatedText.FadeIn(hud.centerPrints.top, "For lifetimes we roamed the stars,", Consts.TEXT_FADE_SECONDS);

        yield return new WaitForSeconds(Consts.TEXT_FADE_SECONDS);
        AnimatedText.FadeIn(hud.centerPrints.middle, "Sifting the dust, unbothered.", Consts.TEXT_FADE_SECONDS);

        yield return new WaitForSeconds(Consts.TEXT_FADE_SECONDS);
    
        AnimatedText.FadeOut(hud.centerPrints.top, Consts.TEXT_FADE_SECONDS);
        AnimatedText.FadeOut(hud.centerPrints.middle, Consts.TEXT_FADE_SECONDS);

        yield return new WaitForSeconds(Consts.TEXT_FADE_SECONDS);

        AnimatedText.FadeIn(hud.centerPrints.top, "We lived in peace and simplicity,", Consts.TEXT_FADE_SECONDS);

        yield return new WaitForSeconds(Consts.TEXT_FADE_SECONDS);
        AnimatedText.FadeIn(hud.centerPrints.middle, "Not knowing of others.", Consts.TEXT_FADE_SECONDS);

        yield return new WaitForSeconds(Consts.TEXT_FADE_SECONDS);

        AnimatedText.FadeOut(hud.centerPrints.top, Consts.TEXT_FADE_SECONDS);
        AnimatedText.FadeOut(hud.centerPrints.middle, Consts.TEXT_FADE_SECONDS);

        yield return new WaitForSeconds(Consts.TEXT_FADE_SECONDS);

        AnimatedText.FadeIn(hud.centerPrints.top, "Until one day.", Consts.TEXT_FADE_SECONDS);

        yield return new WaitForSeconds(Consts.TEXT_FADE_SECONDS);

        AnimatedText.FadeOut(hud.centerPrints.top, Consts.TEXT_FADE_SECONDS_FAST);

        yield return new WaitForSeconds(Consts.TEXT_FADE_SECONDS_FAST);

        var mobParent = new GameObject("mobParent");

        SetupMobTween(mobParent, "ESOX", school.transform.position, screen.min - Vector2.one);
        yield return new WaitForSeconds(0.25f);
        SetupMobTween(mobParent, "PIKE", school.transform.position, new Vector2(screen.right + 1, 0));
        yield return new WaitForSeconds(0.25f);
        SetupMobTween(mobParent, "PIKE0", school.transform.position, new Vector2(screen.right + 1, screen.bottom - 1));
        yield return new WaitForSeconds(0.5f);
        SetupMobTween(mobParent, "ESOX", school.transform.position, screen.max + Vector2.one);
        yield return new WaitForSeconds(0.25f);
        SetupMobTween(mobParent, "ESOX", school.transform.position, new Vector2(screen.left - 1, screen.top + 1));

        StartCoroutine(Fear(school));

        // Introduce enemies
        AnimatedText.FadeIn(hud.centerPrints.top, "We were found.", Consts.TEXT_FADE_SECONDS);

        yield return new WaitForSeconds(Consts.TEXT_FADE_SECONDS);
        AnimatedText.FadeOut(hud.centerPrints.top, Consts.TEXT_FADE_SECONDS_SLOW);

        hud.curtain.Fade(1, Consts.TEXT_FADE_SECONDS);

        yield return new WaitForSeconds(Consts.TEXT_FADE_SECONDS);

        hud.space.Fade(0, 0);

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
            bf.CreatePositionTween(Vector3.Lerp(start, target, Random.Range(0.5f, 0.7f)), Random.Range(3, 6)),
            bf.CreateFacePoint(target)
        );
    }

    IEnumerator Fear(GameObject school)
    {
        var mainHero = school.transform.FindChild("scaler/mainHero");

        var children = new List<TweenPosition>();
        for (var i = 0; i < school.transform.childCount; ++i)
        {
            var child = school.transform.GetChild(i).GetComponent<SpriteRenderer>();
            if (child != null && child != mainHero)
            {
                var tween = child.gameObject.GetOrAddComponent<TweenPosition>();
                children.Add(tween);

                child.gameObject.GetOrAddComponent<Vibrate>();
            }
        }

        foreach (var tween in children)
        {
            yield return new WaitForSeconds(Random.Range(0.05f, 0.2f));
            var scatterOnMainHero = mainHero.transform.position + Util.ScatterRandomly(0.3f);
            tween.To(scatterOnMainHero, Random.Range(0.3f, 1f), 
                (go) =>
                {
                    go.transform.localScale.Scale(new Vector3(0.5f, 0.5f));
                }
            );
        }

        yield break;
    }
}
