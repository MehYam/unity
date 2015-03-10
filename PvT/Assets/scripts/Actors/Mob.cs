using UnityEngine;
using System.Collections;

public sealed class Mob : MonoBehaviour
{
	// Use this for initialization
	void Start()
    {
        gameObject.layer = (int)Consts.CollisionLayer.MOB;
        Main.Instance.ParentMob(transform);

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
	void OnDestroy()
    {
        if (_overwhelm.bar != null)
        {
            GameObject.Destroy(_overwhelm.bar.gameObject);
        }
    }
    void FixedUpdate()
    {
        HandleOverwhelm();
    }
    static readonly Vector3 OVERWHELM_BAR_POSITION = new Vector3(0, -0.5f);
    void Update()
    {
        if (_attachedHerolings > 0)
        {
            if (_overwhelm.bar == null)
            {
                var bar = (GameObject)GameObject.Instantiate(Main.Instance.assets.OverwhelmProgressBar);
                _overwhelm.bar = bar.GetComponent<ProgressBar>();

                Main.Instance.ParentEffect(bar.transform);
            }
            _overwhelm.bar.gameObject.SetActive(true);

            var actor = GetComponent<Actor>();

            float overwhelmPct = actor.health == 0 ? 0 : Mathf.Min(_attachedHerolings * Consts.HEROLING_HEALTH_OVERWHELM / actor.health, 1);
            _overwhelm.bar.percent = overwhelmPct;
            if (_overwhelm.Update(_attachedHerolings, Mathf.CeilToInt(actor.health / Consts.HEROLING_HEALTH_OVERWHELM)))
            {
                _overwhelm.bar.text = string.Format("{0}/{1}", _overwhelm.lastNumerator, _overwhelm.lastDenominator);
            }
            _overwhelm.bar.transform.position = transform.position + OVERWHELM_BAR_POSITION;
        }
        else if (_overwhelm.bar != null)
        {
            _overwhelm.bar.gameObject.SetActive(false);
        }
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        collision.gameObject.SendMessage("OnDamagingCollision", GetComponent<Actor>());
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

    // overwhelm handling
    struct OverwhelmBarState
    {
        // this allows us to not generate strings every frame
        public ProgressBar bar;
        public int lastNumerator;
        public int lastDenominator;
        public bool Update(int num, int denom)
        {
            if (lastNumerator != num || lastDenominator != denom)
            {
                lastNumerator = num;
                lastDenominator = denom;
                return true;
            }
            return false;
        }
    }
    OverwhelmBarState _overwhelm;

    bool _overwhelmed = false;
    void HandleOverwhelm()
    {
        var actor = GetComponent<Actor>();

        float overwhelmPct = actor.health == 0 ? 0 : Mathf.Min(_attachedHerolings * Consts.HEROLING_HEALTH_OVERWHELM / actor.health, 1); 
        if (_overwhelmed && overwhelmPct < 1)
        {
            // un-overwhelm
            actor.behaviorOverride = null;
            _overwhelmed = false;

            RemoveBlinker(transform);
            var blinker = transform.FindChild(Consts.BLINKER_TAG);
            if (blinker != null)
            {
                GameObject.Destroy(blinker.gameObject);
            }
        }
        else if (!_overwhelmed && overwhelmPct == 1f)
        {
            // act overwhelmed
            actor.behaviorOverride = ActorBehaviorFactory.Instance.CreateHerolingOverwhelmBehavior();
            _overwhelmed = true;

            var blinker = (GameObject)GameObject.Instantiate(Main.Instance.assets.OverwhelmedIndicator);
            blinker.transform.parent = transform;
            blinker.transform.localPosition = Vector3.zero;
            blinker.name = Consts.BLINKER_TAG;

            Main.Instance.game.PlaySound(Sounds.GlobalEvent.OVERWHELM, transform.position);
        }
    }

    static void RemoveBlinker(Transform host)
    {
        var blinker = host.FindChild(Consts.BLINKER_TAG);
        if (blinker != null)
        {
            GameObject.Destroy(blinker.gameObject);
        }
    }
}
