using UnityEngine;
using System.Collections;

public sealed class PossessionUndoSequence : MonoBehaviour
{
	// Use this for initialization
	void Start()
    {
	    StartCoroutine(Sequence());
	}
	
    IEnumerator Sequence()
    {
        yield return new WaitForSeconds(0.3f);

        // 1. Stop all activity and pause
        var timeScale = Time.timeScale;
        Time.timeScale = 0;

        // 2. Spin hero fast to a stop
        var start = Time.realtimeSinceStartup;
        var lastSpin = start;
        float elapsed = 0;
        do
        {
            var now = Time.realtimeSinceStartup;
            elapsed = now - start;

            var pctDone = elapsed / Consts.HEROLING_OVERWHELM_DURATION;
            var rotationsPerSec = Consts.HEROLING_OVERWHELM_ROTATIONS_PER_SEC * (1 - pctDone);

            transform.Rotate(0, 0, (now - lastSpin) * 360 * rotationsPerSec);
            lastSpin = now;

            yield return new WaitForEndOfFrame();
        }
        while (elapsed < Consts.HEROLING_OVERWHELM_DURATION);

        // 3. Resume all activity
        Time.timeScale = timeScale;

        GetComponent<Actor>().GrantInvuln(Consts.POST_DEPOSSESSION_INVULN);

        GlobalGameEvent.Instance.FireDepossessionComplete();

        Destroy(this);
    }
}
