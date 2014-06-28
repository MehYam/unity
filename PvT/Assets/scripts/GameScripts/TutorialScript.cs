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

            CenterPrint.Clear();
            yield return new WaitForSeconds(2);
        }
    }

    IEnumerator Intro()
    {
        yield return new WaitForEndOfFrame();  //HACK to ensure that the next line doesn't nullref

        Main.Instance.hud.curtain.alpha = 1;

        yield return new WaitForSeconds(2);

        Main.Instance.hud.centerPrint.text = "I am Eukarya.";

        yield return new WaitForSeconds(2);

        Main.Instance.hud.centerPrint.text = "I am Eukarya.  This is my story";
    }
}
