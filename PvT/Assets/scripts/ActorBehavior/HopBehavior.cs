using UnityEngine;
using System.Collections;

using PvT.Util;
/// <summary>
/// This implements a pseudo-3D hop.  Would it be better to use real 3D?
/// </summary>

public sealed class HopBehavior : MonoBehaviour
{
    static readonly float GRAVITY = 1.5f;
    static readonly float JUMP_VELOCITY = 0.75f;

    public static float AIRBORNE_TIME = 2 * JUMP_VELOCITY / GRAVITY;

    sealed class HopState
    {
        public readonly Actor actor;
        public readonly Consts.CollisionLayer layerForImpact;
        public readonly Vector3 originalScale;

        public float verticalVelocity = 0;
        public float height = 0;

        public HopState(Actor actor, Consts.CollisionLayer layerForImpact, Vector3 originalScale)
        {
            this.actor = actor;
            this.layerForImpact = layerForImpact;
            this.originalScale = originalScale;
            this.verticalVelocity = HopBehavior.JUMP_VELOCITY;
        }
    }

    HopState _state;
    public bool complete { get { return _state == null; } }
    
    /// <summary>
    /// Begins the hop.
    /// </summary>
    /// <param name="layerForImpact">The layer on which to spawn the impact shockwave.  If there is no shockwave, pass in </param>
    public void Hop(Consts.CollisionLayer layerForImpact = Consts.CollisionLayer.DEFAULT)
    {
        // perform the jump
        if (_state != null)
        {
            Debug.LogError("Hop already in progress for " + name);
        }
        else
        {
            enabled = true;
            _state = new HopState(GetComponent<Actor>(), layerForImpact, transform.localScale);
        }
    }

    void FixedUpdate()
    {
        if (_state != null)
        {
            HopFixedUpdate();
        }
    }

    void HopFixedUpdate()
    {
        if (_state.height >= 0)
        {
            // tween the jump, by simulating height
            _state.height += Time.deltaTime * _state.verticalVelocity;
            _state.verticalVelocity -= Time.deltaTime * GRAVITY;

            _state.actor.transform.localScale = (1 + _state.height) * _state.originalScale;

            var dropShadow = _state.actor.GetComponentInChildren<DropShadow>();
            if (dropShadow != null)
            {
                dropShadow.distanceModifier = _state.height;
            }
        }
        if (_state.height <= 0)
        {
            // we've landed
            _state.actor.transform.localScale = _state.originalScale;

            if (_state.layerForImpact != Consts.CollisionLayer.DEFAULT)
            {
                StartCoroutine(AnimateLanding(_state.actor, _state.layerForImpact));
            }
            _state = null;
            enabled = false;

            DebugUtil.Assert(complete);
        }
    }
    static IEnumerator AnimateLanding(Actor actor, Consts.CollisionLayer impactLayer)
    {
        var game = Main.Instance.game;

        // 4. land with some fanfare, a shockwave, and wait
        actor.gameObject.rigidbody2D.velocity = Vector2.zero;

        //var impact = game.loader.GetMisc("landingImpact").ToRawGameObject(Consts.SortingLayer.TANKBODY);
        //impact.transform.position = actor.gameObject.transform.position;

        var vibe = actor.gameObject.GetComponent<Vibrate>();
        if (vibe == null)
        {
            vibe = actor.gameObject.AddComponent<Vibrate>();
        }
        vibe.enabled = true;

        if (actor.actorType.HasWeapons)
        {
            game.SpawnAmmo(actor, actor.actorType.weapons[0], impactLayer);
        }
        yield return new WaitForSeconds(0.2f);
        vibe.enabled = false;
        yield return new WaitForSeconds(0.1f);
    }
}
