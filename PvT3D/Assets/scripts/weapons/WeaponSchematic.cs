using UnityEngine;

using PvT3D.Util;
using ship = PvT3D.WeaponComponents;
using kaiGameUtil;

public class WeaponSchematic : MonoBehaviour, ship.IConsumer
{
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

        // a sample schematic - needs to come from somewhere else
        var schem = new ship.Schematic(5, 3);
        schem.grid.Set(0, 0, new ship.Power("P", 1));
        schem.grid.Set(1, 0, new ship.AutofireCharger("C", 2));
        //schem.grid.Set(1, 0, new ship.Charger("C", 10));
        schem.grid.Set(2, 0, new ship.Envelope("E", 1));
        schem.grid.Set(3, 0, new ship.Accelerator("A", 60));
        //schem.grid.Set(3, 0, new ship.LaserAccelerator("L", 100, 0.5f));

        ConnectWeaponSchematic(schem);
    }
    class SchematicState
    {
        public readonly ship.ICharger charger;
        public readonly ship.IFrameHandler frameHandler;
        public readonly ship.IProducer producer;

        public SchematicState(ship.ICharger charger, ship.Component lastComponent)
        {
            this.charger = charger;
            this.frameHandler = charger as ship.IFrameHandler;
            this.producer = lastComponent as ship.IProducer;
        }
    }
    SchematicState _state;
    void ConnectWeaponSchematic(ship.Schematic schem)
    {
        ship.ICharger charger = null;
        ship.Component leftComponent = null;
        for (var x = 0; x < schem.grid.size.x; ++x)
        {
            ship.Component component = schem.grid.Get(x, 0);
            if (component == null) break;

            if (component is ship.ICharger)
            {
                charger = component as ship.ICharger;
                if (leftComponent is ship.Power)
                {
                    charger.powerSource = leftComponent as ship.Power;
                }
            }
            else if (leftComponent is ship.IProducer && component is ship.IConsumer)
            {
                ((ship.IProducer)leftComponent).output = component as ship.IConsumer;
            }
            leftComponent = component;
        }
        _state = new SchematicState(charger, leftComponent);
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
    public void ConsumeAmmoProduct(ship.AmmoProduct product) 
    {
        switch(product.type)
        {
            case ship.AmmoProduct.Type.Plasma:
                LaunchPlasmaAmmo(product);
                break;
            case ship.AmmoProduct.Type.Laser:
                LaunchBeamAmmo(product);
                break;
            case ship.AmmoProduct.Type.Shield:
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
    void LaunchPlasmaAmmo(ship.AmmoProduct product)
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
    void LaunchBeamAmmo(ship.AmmoProduct product)
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
    void LaunchShieldAmmo(ship.AmmoProduct product)
    {
        var ammo = GameObject.Instantiate(Main.game.ammoRegistry.shieldPrefab);
        OrientAmmo(ammo);
    }
}
