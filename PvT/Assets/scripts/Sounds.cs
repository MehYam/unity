using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

using PvT.Util;

public sealed class Sounds
{
    public enum ActorEvent { SPAWN, DEATH, COLLISION } 
    public enum GlobalEvent 
    { 
        OVERWHELM,
        POSSESSION,
        PLAYER_DEATH, 
        LEVELUP, 
        EXPLOSION, 
        ROAR,
        MOBCOLLISION
    }

    /// <summary>
    /// For variety's sake, we can have multiple audio clips per event, hence the jagged array
    /// of sounds per actor
    /// </summary>
    readonly Dictionary<ActorType, List<AudioClip>[]> _lookup = new Dictionary<ActorType, List<AudioClip>[]>();
    public void Add(ActorType actor, ActorEvent evt, AudioClip clip)
    {
        List<AudioClip>[] actorClips = null;
        if (!_lookup.TryGetValue(actor, out actorClips))
        {
            actorClips = _lookup[actor] = new List<AudioClip>[ACTOR_EVENT_TYPES];
        }
        int eventIndex = (int)evt;
        if (actorClips[eventIndex] == null)
        {
            actorClips[eventIndex] = new List<AudioClip>();
        }
        actorClips[eventIndex].Add(clip);
    }
    /// <summary>
    /// Returns the audio clip for the actor/event pair.  If multiple clips exist for this pair,
    /// one is chosen at random.
    /// </summary>
    /// <param name="actor">The actor for this event</param>
    /// <param name="evt">The event</param>
    /// <returns></returns>
    public AudioClip Get(ActorType actor, ActorEvent evt)
    {
        List<AudioClip>[] actorClips = null;
        if (_lookup.TryGetValue(actor, out actorClips))
        {
            int eventIndex = (int)evt;
            if (actorClips[eventIndex] != null)
            {
                var eventClips = actorClips[eventIndex];
                // Pick one at random, if there are multiple
                return Util.RandomArrayPick<AudioClip>(eventClips);
            }
        }
        return null;
    }
    
    readonly Dictionary<GlobalEvent, List<AudioClip>> _globalEventLookup = new Dictionary<GlobalEvent, List<AudioClip>>();
    public void Add(GlobalEvent evt, AudioClip clip)
    {
        List<AudioClip> clips = null;
        if (!_globalEventLookup.TryGetValue(evt, out clips))
        {
            _globalEventLookup[evt] = clips = new List<AudioClip>();
        }
        clips.Add(clip);
    }
    public AudioClip Get(GlobalEvent evt)
    {
        List<AudioClip> clips = null;
        if (_globalEventLookup.TryGetValue(evt, out clips))
        {
            return Util.RandomArrayPick<AudioClip>(clips);
        }
        return null;
    }

    readonly int ACTOR_EVENT_TYPES;
    public Sounds()
    {
        ACTOR_EVENT_TYPES = Enum.GetValues(typeof(ActorEvent)).Length - 1;
    }
}
