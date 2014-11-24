using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

public sealed class Sounds
{
    public enum ActorEvent { SPAWN, DEATH, COLLISION } 
    public enum GlobalEvent { OVERWHELM, PLAYER_DEATH, LEVELUP, EXPLOSION, ROAR }

    readonly Dictionary<ActorType, AudioClip[][]> _lookup;
    public void Add(ActorType actor, ActorEvent evt, AudioClip clip)
    {
        AudioClip[][] clips = null;
        if (!_lookup.TryGetValue(actor, out clips))
        {
            clips = _lookup[actor] = new AudioClip[NUM_EVENTS][];
        }
    }
    public AudioClip Get(ActorType actor, ActorEvent evt)
    {

        return null;
    }
    
    readonly Dictionary<GlobalEvent, AudioClip[]> _globalEventLookup;
    public void Add(GlobalEvent evt, AudioClip clip)
    {
    }
    public AudioClip Get(GlobalEvent evt)
    {
        return null;
    }

    readonly int NUM_EVENTS;
    public Sounds()
    {
        NUM_EVENTS = Enum.GetValues(typeof(ActorEvent)).Length - 1;
    }
}
