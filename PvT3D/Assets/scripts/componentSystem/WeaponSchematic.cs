using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using kaiGameUtil;
using PvT3D.Util;
using sc = PvT3D.ShipComponent;

public class WeaponSchematic : MonoBehaviour, sc.IProductConsumer, IWeaponControl
{
    enum TestSchematic { NONE, Charge, Autofire, Laser, Shield };
    [SerializeField] TestSchematic testSchematic = TestSchematic.Autofire;
    [SerializeField] TextAsset schematicFile = null;
    [SerializeField] GameObject firepoint = null;

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

        switch(testSchematic)
        {
            case TestSchematic.Autofire: LoadSampleAutofireSchematic(); break;
            case TestSchematic.Laser: LoadSampleLaserSchematic(); break;
            case TestSchematic.Shield: LoadSampleShieldSchematic(); break;
            case TestSchematic.Charge: LoadSampleChargeSchematic(); break;
            default:
                if (schematicFile == null)
                {
                    Debug.LogWarningFormat("Warning, no schem file found for {0}", name);
                    LoadSampleAutofireSchematic();
                }
                else
                {
                    Load(schematicFile.text);
                }
                break;
        }
    }
    void LoadSampleChargeSchematic()
    {
        var schem = new sc.Schematic(5, 3);
        schem.grid.Set(0, 0, new sc.Power("P", 1));
        schem.grid.Set(1, 0, new sc.Charger("C", 2));
        schem.grid.Set(2, 0, new sc.Lifetime("E", 1));
        schem.grid.Set(3, 0, new sc.Speed("A", 60));

        ConnectWeaponSchematic(schem);
    }
    void LoadSampleAutofireSchematic()
    {
        var schem = new sc.Schematic(5, 3);
        schem.grid.Set(0, 0, new sc.Power("P", 1));
        schem.grid.Set(1, 0, new sc.Charger("C", 2));
        schem.grid.Set(2, 0, new sc.Lifetime("E", 1));
        schem.grid.Set(3, 0, new sc.Speed("A", 60));
        schem.grid.Set(1, 1, new sc.Repeater("R"));

        ConnectWeaponSchematic(schem);
    }
    void LoadSampleLaserSchematic()
    {
        //KAI: probably doesn't work anymore
        var schem = new sc.Schematic(5, 3);
        schem.grid.Set(0, 0, new sc.Power("P", 1));
        schem.grid.Set(1, 0, new sc.Charger("C", 2));
        schem.grid.Set(2, 0, new sc.Lifetime("E", 1));
        schem.grid.Set(3, 0, new sc.Laser("L", 100, 0.5f));

        ConnectWeaponSchematic(schem);
    }
    void LoadSampleShieldSchematic()
    {
        var schem = new sc.Schematic(5, 3);
        schem.grid.Set(0, 0, new sc.Power("P", 100));
        schem.grid.Set(1, 0, new sc.Charger("C", 20));
        schem.grid.Set(2, 0, new sc.Shield("S", 0.1f));
        schem.grid.Set(3, 0, new sc.Lifetime("E", 3));
        schem.grid.Set(4, 0, new sc.Speed("A", 10));

        ConnectWeaponSchematic(schem);
    }
    static readonly Dictionary<string, Func<string[], sc.Component>> componentFactory = new Dictionary<string, Func<string[], sc.Component>>() 
    {
        { "power", (args) => new sc.Power("P", int.Parse(args[0])) },
        { "charger", (args) => new sc.Charger("C", int.Parse(args[0])) },
        { "lifetime", (args) => new sc.Lifetime("L", int.Parse(args[0])) },
        { "speed", (args) => new sc.Speed("S", int.Parse(args[0])) },
        { "autofire", (args) => new sc.Repeater("Af") }
    };
    void Load(string textFile)
    {
        // This is very unsafe and throws exceptions if something's not right.
        var lines = textFile.Split(new string[] { "\r\n", "\n" }, System.StringSplitOptions.None);
        var height = lines.Length;

        // read all the components first so we know how big the schematic needs to be
        var components = new List<sc.Component>();
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
        var schem = new sc.Schematic(max.x + 1, max.y + 1);
        for (var i = 0; i < components.Count; ++i)
        {
            schem.grid.Set(positions[i], components[i]);
        }
        ConnectWeaponSchematic(schem);
    }
    class SchematicState  // KAI: the degree to which we need this thing is the degree to which the component design isn't fully fleshed out, and doesn't take care of itself
    {
        public readonly sc.Power power;
        public readonly sc.ICharger charger;
        public readonly sc.IFrameHandler frameHandler;
        public readonly sc.IProductProducer productProducer;

        public SchematicState(sc.Power power, sc.ICharger charger, sc.IFrameHandler frameHandler, sc.IProductProducer productProducer)
        {
            this.power = power;
            this.charger = charger;
            this.frameHandler = frameHandler;
            this.productProducer = productProducer;
        }
    }
    SchematicState _state;
    void ConnectWeaponSchematic(sc.Schematic schem)
    {
        // walk through the schematic and wire things up in a left-to-right manner.  Not the final idea, but works for now
        sc.Component lastComponent = null;
        sc.Power power = null;
        sc.ICharger charger = null;
        sc.IFrameHandler frameHandler = null;
        sc.IProductProducer productProducer = null;

        schem.grid.ForEach((x, y, component) =>
        {
            if (component != null)
            {
                //KAI: wonky, but the end game is supposed to have connections between the components, so it'll be addressed then
                if (component is sc.Power)
                {
                    power = component as sc.Power;
                }
                if (component is sc.IChargerWrapper)
                {
                    ((sc.IChargerWrapper)component).charger = charger;
                }
                if (component is sc.ICharger)
                {
                    charger = component as sc.ICharger;
                    ((sc.ICharger)component).powerSource = power;
                }
                if (component is sc.IFrameHandler)
                {
                    frameHandler = component as sc.IFrameHandler;
                }
                if (component is sc.IProductProducer)
                {
                    productProducer = component as sc.IProductProducer;
                }
                if (component is sc.ILiveProductProducer)
                {
                    ((sc.ILiveProductProducer)component).liveProductOutput = this;
                }
                if (lastComponent is sc.IProductProducer && component is sc.IProductConsumer)
                {
                    ((sc.IProductProducer)lastComponent).productOutput = component as sc.IProductConsumer;
                }
                lastComponent = component;
            }
        });
        _state = new SchematicState(power, charger, frameHandler, productProducer);
        if (_state.productProducer != null)
        {
            _state.productProducer.productOutput = this;
        }
    }

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
    public void ConsumeProduct(sc.ComponentProduct product) 
    {
        switch(product.type)
        {
            case sc.ComponentProduct.Type.Plasma:
                LaunchPlasmaAmmo(product);
                break;
            case sc.ComponentProduct.Type.Laser:
                LaunchBeamAmmo(product);
                break;
            case sc.ComponentProduct.Type.Shield:
                HandleShieldAmmo((sc.ShieldProduct)product);
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
    void LaunchPlasmaAmmo(sc.ComponentProduct product)
    {
        // line the shot up
        var ammo = GameObject.Instantiate(Main.game.ammoRegistry.plasmaPrefab);
        OrientAmmo(ammo);

        // duration  KAI: replace this with Destroy(, t)!!!!!!
        var ttl = ammo.GetComponent<TimeToLive>();
        if (ttl != null)
        {
            ttl.seconds = product.duration;
        }

        // inherit the ship's velocity
        var rb = ammo.transform.GetComponent<Rigidbody>();
        if (product.inheritShipVelocity && gameObject.GetComponent<Rigidbody>() != null)
        {
            //KAI: a bug, turret ammo needs to pick up launcher velocity as well
            rb.velocity = gameObject.GetComponent<Rigidbody>().velocity;
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
    void LaunchBeamAmmo(sc.ComponentProduct product)
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
    Actor _currentShield;
    void HandleShieldAmmo(sc.ShieldProduct product)
    {
        // if we don't currently have a shield, create one
        if (_currentShield == null)
        {
            _currentShield = GameObject.Instantiate(Main.game.ammoRegistry.shieldPrefab).GetComponent<Actor>();
            _currentShield.lockedY = false;

            _currentShield.baseHealth = _state.power.power * (1 - product.damagePct);
            _currentShield.collisionDamage = _state.power.power * product.damagePct;

            OrientAmmo(_currentShield.gameObject);

            // attach it to the ship
            _currentShield.transform.parent = transform;
            _currentShield.gameObject.layer = LayerMask.NameToLayer("friendlyShield");

            // use a joint to attach the shield.  You don't strictly need one, but physics and collisions with walls work more realistically
            // if you have one
            var joint = gameObject.GetOrAddComponent<FixedJoint>();
            joint.connectedBody = _currentShield.gameObject.GetComponent<Rigidbody>();
        }

        _currentShield.SetHealth(product.power);

        // if we have a shield and product has speed, launch it.
        if (product.speed > 0)
        {
            // asynchronous operations in the physics system require us to use a coroutine
            StartCoroutine(DetachShield(_currentShield, product));

            _currentShield.transform.parent = Main.game.ammoParent.transform;
            _currentShield = null;
        }
    }
    IEnumerator DetachShield(Actor shield, sc.ShieldProduct product)
    {
        // disconnect the joint
        var joint = GetComponent<FixedJoint>();
        Destroy(GetComponent<FixedJoint>());

        // Destroy is asynchronous, so wait until end of frame for this to actually take effect.  
        // Otherwise, the ammo velocity we impart below will affect the ship
        yield return new WaitForEndOfFrame();

        // duration
        var ttl = shield.GetComponent<TimeToLive>();
        if (ttl != null)
        {
            ttl.seconds = product.duration;
            ttl.enabled = true;
        }

        // inherit the ship's velocity
        var rb = shield.gameObject.GetComponent<Rigidbody>();
        if (product.inheritShipVelocity && gameObject.GetComponent<Rigidbody>() != null)
        {
            //KAI: a bug, turret ammo needs to pick up launcher velocity as well
            rb.velocity = gameObject.GetComponent<Rigidbody>().velocity;
        }

        // impart ammo velocity in the direction of the firer
        rb.velocity += gameObject.transform.forward * product.speed;
    }
}
