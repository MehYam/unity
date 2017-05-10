using System;
using System.Collections.Generic;
using UnityEngine;

using kaiGameUtil;
using wcmp = PvT3D.WeaponComponents;

public class WeaponSchematic : MonoBehaviour, wcmp.IAmmoProductConsumer
{
    [SerializeField] TextAsset schematicFile = null;
    [SerializeField] GameObject firepoint = null;
    [SerializeField] bool inheritShipVelocity = false;

    int ammoLayer;
    ParticleSystem _ps;
    void Start()
    {
        if (gameObject.layer == LayerMask.NameToLayer("enemy"))
        {
            ammoLayer = LayerMask.NameToLayer("enemyAmmo");
        }
        else if (gameObject.layer == LayerMask.NameToLayer("friendly"))
        {
            ammoLayer = LayerMask.NameToLayer("friendlyAmmo");
        }
        _ps = firepoint.GetComponent<ParticleSystem>();

        if (schematicFile != null)
        {
            Load(schematicFile.text);
        }
        else
        {
            Debug.LogWarningFormat("Warning, no schem file found for {0}", name);
            LoadSampleSchematic();
        }
    }
    void LoadSampleSchematic()
    {
        // a sample schematic - needs to load from somewhere else
        var schem = new wcmp.Schematic(5, 3);
        schem.grid.Set(0, 0, new wcmp.Power("P", 1));
        schem.grid.Set(1, 0, new wcmp.Charger("C", 2));
        schem.grid.Set(2, 0, new wcmp.Lifetime("E", 1));
        schem.grid.Set(3, 0, new wcmp.Speed("A", 60));
        //schem.grid.Set(3, 0, new ship.LaserAccelerator("L", 100, 0.5f));

        schem.grid.Set(1, 1, new wcmp.Autofire("Af"));
        ConnectWeaponSchematic(schem);
    }
    static readonly Dictionary<string, Func<string[], wcmp.Component>> componentFactory = new Dictionary<string, Func<string[], wcmp.Component>>() 
    {
        { "power", (args) => new wcmp.Power("P", int.Parse(args[0])) },
        { "charger", (args) => new wcmp.Charger("C", int.Parse(args[0])) },
        { "lifetime", (args) => new wcmp.Lifetime("L", int.Parse(args[0])) },
        { "speed", (args) => new wcmp.Speed("S", int.Parse(args[0])) },
        { "autofire", (args) => new wcmp.Autofire("Af") }
    };
    void Load(string textFile)
    {
        // This is very unsafe and throws exceptions if something's not right.
        var lines = textFile.Split(new string[] { "\r\n", "\n" }, System.StringSplitOptions.None);
        var height = lines.Length;

        // read all the components first so we know how big the schematic needs to be
        var components = new List<wcmp.Component>();
        var positions = new List<Point<int>>();
        var max = new Point<int>(-1, -1);
        foreach (var line in lines)
        {
            var args = new List<string>(line.Split(','));
            if (args.Count < 3) break;

            var pos = new Point<int>(int.Parse(args[0]), int.Parse(args[1]));
            var cname = args[2].Trim();
            var remaining = args.GetRange(3, args.Count - 3);

            max.x = Mathf.Max(max.x, pos.x);
            max.y = Mathf.Max(max.y, pos.y);

            var component = componentFactory[cname](remaining.ToArray());
            components.Add(component);
            positions.Add(pos);
        }

        Debug.Assert(components.Count == positions.Count, "ya done messed up");

        // write to the schema
        var schem = new wcmp.Schematic(max.x + 1, max.y + 1);
        for (var i = 0; i < components.Count; ++i)
        {
            schem.grid.Set(positions[i], components[i]);
        }
        ConnectWeaponSchematic(schem);
    }
    class SchematicState
    {
        public readonly wcmp.ICharger charger;
        public readonly wcmp.IFrameHandler frameHandler;
        public readonly wcmp.IAmmoProductProducer producer;

        public SchematicState(wcmp.ICharger charger, wcmp.IFrameHandler frameHandler, wcmp.IAmmoProductProducer producer)
        {
            this.charger = charger;
            this.frameHandler = frameHandler;
            this.producer = producer;
        }
    }
    SchematicState _state;
    void ConnectWeaponSchematic(wcmp.Schematic schem)
    {
        // walk through the schematic and wire things up in a left-to-right manner.  Not the final idea, but works for now
        wcmp.Component lastComponent = null;
        wcmp.Power power = null;
        wcmp.ICharger charger = null;
        wcmp.IFrameHandler frameHandler = null;
        wcmp.IAmmoProductProducer producer = null;

        schem.grid.ForEach((x, y, component) =>
        {
            if (component != null)
            {
                //KAI: wonky, but the end game is supposed to have connections between the components, so it'll be addressed then
                if (component is wcmp.Power)
                {
                    power = component as wcmp.Power;
                }
                if (component is wcmp.Autofire)
                {
                    ((wcmp.Autofire)component).charger = charger;
                }
                if (component is wcmp.ICharger)
                {
                    charger = component as wcmp.ICharger;
                    ((wcmp.ICharger)component).powerSource = power;
                }
                if (component is wcmp.IFrameHandler)
                {
                    frameHandler = component as wcmp.IFrameHandler;
                }
                if (component is wcmp.IAmmoProductProducer)
                {
                    producer = component as wcmp.IAmmoProductProducer;
                }
                if (lastComponent is wcmp.IAmmoProductProducer && component is wcmp.IAmmoProductConsumer)
                {
                    ((wcmp.IAmmoProductProducer)lastComponent).output = component as wcmp.IAmmoProductConsumer;
                }
                lastComponent = component;
            }
        });
        _state = new SchematicState(charger, frameHandler, producer);
        if (_state.producer != null)
        {
            _state.producer.output = this;
        }
    }

    //KAI: OnFirexxx kind of belong in some interface
    public void OnFireStart()
    {
        if (_state != null && _state.charger != null)
        {
            _state.charger.Charge();
        }
    }
    public void OnFireEnd()
    {
        if (_state != null && _state.charger != null)
        {
            _state.charger.Discharge();
        }
    }
    public void OnFireFrame()
    {
        if (_state != null && _state.frameHandler != null)
        {
            _state.frameHandler.OnFixedUpdate();
        }
    }

    /// <summary>
    /// ship.IConsumer implementation
    /// </summary>
    /// <param name="product">the ammo coming from the weapon components</param>
    public void ConsumeAmmoProduct(wcmp.AmmoProduct product) 
    {
        switch(product.type)
        {
            case wcmp.AmmoProduct.Type.Plasma:
                LaunchPlasmaAmmo(product);
                break;
            case wcmp.AmmoProduct.Type.Laser:
                LaunchBeamAmmo(product);
                break;
            case wcmp.AmmoProduct.Type.Shield:
                LaunchShieldAmmo(product);
                break;
            default:
                Debug.LogError("no handler for " + product.type);
                break;
        }
    }
    void OrientAmmo(GameObject ammo)
    {
        ammo.transform.parent = Main.game.ammoParent.transform;
        ammo.transform.position = firepoint.transform.position;
        ammo.transform.rotation = firepoint.transform.rotation;
        ammo.layer = ammoLayer;
    }
    void LaunchPlasmaAmmo(wcmp.AmmoProduct product)
    {
        // line the shot up
        var ammo = GameObject.Instantiate(Main.game.ammoRegistry.plasmaPrefab);
        OrientAmmo(ammo);

        // inherit the ship's velocity
        var rb = ammo.transform.GetComponent<Rigidbody>();
        if (inheritShipVelocity && gameObject.GetComponent<Rigidbody>() != null)
        {
            //KAI: a bug, turret ammo needs to pick up launcher velocity as well
            rb.velocity = gameObject.GetComponent<Rigidbody>().velocity;
        }

        // duration
        var ttl = ammo.GetComponent<TimeToLive>();
        if (ttl != null)
        {
            ttl.seconds = product.duration;
        }

        // impart ammo velocity in the direction of the firer
        rb.velocity += gameObject.transform.forward * product.speed;

        // spin it for kicks
        rb.angularVelocity = Vector3.up * 10;

        // this is a chargable shot, scale it by the power
        ammo.transform.localScale = new Vector3(product.power, product.power, product.power);

        // particles
        if (_ps != null)
        {
            _ps.Play();
        }
    }
    void LaunchBeamAmmo(wcmp.AmmoProduct product)
    {
        // line the shot up
        var ammo = GameObject.Instantiate(Main.game.ammoRegistry.beamPrefab);
        OrientAmmo(ammo);

        // duration
        var beam = ammo.GetComponent<AmmoBeam>();
        beam.duration = product.duration;
        beam.distance = product.distance;
        beam.width = product.width;
    }
    void LaunchShieldAmmo(wcmp.AmmoProduct product)
    {
        var ammo = GameObject.Instantiate(Main.game.ammoRegistry.shieldPrefab);
        OrientAmmo(ammo);
    }
}
