using UnityEngine;
using System.Collections;

public class TutorialScript : MonoBehaviour
{
    public AnimatedText CenterPrint;

	// Use this for initialization
	void Start()
    {
        StartCoroutine(Test());
        StartCoroutine(Go());
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

    IEnumerator Go()
    {
        yield return null;
    }
}
