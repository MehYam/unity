using UnityEngine;
using System.Collections;

public sealed class Mob : MonoBehaviour
{
	// Use this for initialization
	void Start()
    {
        gameObject.layer = (int)Consts.CollisionLayer.MOB;
        if (Main.Instance.MobParent != null)
        {
            gameObject.transform.parent = Main.Instance.MobParent.transform;
        }

        var actor = GetComponent<Actor>();
        if (actor != null)
        {
            MobAI.Instance.AttachAI(GetComponent<Actor>());
        }
        else
        {
            Debug.LogError(string.Format("Mob {0} has no attached actor", name));
        }

        //KAI: either rethink this, or create a HerolingHost class
        actor.isCapturable = true;
	}
	
    // heroling handling - might be worth moving this to a separate HerolingHost class
    int _attachedHerolings;
    void OnHerolingCollide(Heroling heroling)
    {
        ++_attachedHerolings;

        heroling.Attach(gameObject);
    }
    void OnHerolingDetach(Heroling heroling)
    {
        --_attachedHerolings;
    }
}
