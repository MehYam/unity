using UnityEngine;

using PvT3D.Util;
using ship = PvT3D.ShipComponents;
using kaiGameUtil;

public class WeaponSchematic : MonoBehaviour, ship.IConsumer
{
    [SerializeField] GameObject firepoint = null;
    [SerializeField] GameObject prefabPlasmaAmmo = null;
    [SerializeField] GameObject prefabBeamAmmo = null;
    [SerializeField] bool inheritShipVelocity = false;

    ParticleSystem _ps;
    void Start()
    {
        _ps = firepoint.GetComponent<ParticleSystem>();

        var schem = new ship.Schematic(5, 3);
        schem.grid.Set(0, 0, new ship.Power("P", 1));
        schem.grid.Set(1, 0, new ship.AutofireCharger("C", 1));
        //schem.grid.Set(1, 0, new ship.Charger("C", 10));
        schem.grid.Set(2, 0, new ship.Envelope("E", 1));
        schem.grid.Set(3, 0, new ship.Accelerator("A", 80));

        ConnectWeaponSchematic(schem);
    }
    bool fireState = false;
    void FixedUpdate()
    {
        var fv = InputUtil.GetFiringVector(transform.position);
        if (fv != Vector3.zero)
        {
            // determine the change in state
            if (!fireState)
            {
                fireState = true;
                OnFireStart();
            }
            OnFireFrame();
        }
        else if (fireState)
        {
            OnFireEnd();
            fireState = false;
        }
    }
    class SchematicState
    {
        public readonly ship.ICharger charger;
        public readonly ship.IFrameHandler frameHandler;
        public readonly ship.IProducer producer;

        public SchematicState(ship.ICharger charger, ship.SimpleComponent lastComponent)
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
        ship.SimpleComponent leftComponent = null;
        for (var x = 0; x < schem.grid.size.x; ++x)
        {
            ship.SimpleComponent component = schem.grid.Get(x, 0);
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
    void OnFireStart()
    {
        if (_state != null && _state.charger != null)
        {
            _state.charger.Charge();
        }
    }
    void OnFireEnd()
    {
        if (_state != null && _state.charger != null)
        {
            _state.charger.Discharge();
        }
    }
    void OnFireFrame()
    {
        if (_state != null && _state.frameHandler != null)
        {
            _state.frameHandler.OnFixedUpdate();
        }
    }
    public void ConsumeProduct(ship.AmmoProduct product) 
    {
        if (prefabPlasmaAmmo != null)
        {
            LaunchPlasmaAmmo(product);
        }
    }
    void LaunchPlasmaAmmo(ship.AmmoProduct product)
    {
        // line the shot up
        var shot = GameObject.Instantiate(prefabPlasmaAmmo);
        shot.transform.position = firepoint.transform.position;
        shot.transform.rotation = firepoint.transform.rotation;

        // inherit the ship's velocity
        var rb = shot.transform.GetComponent<Rigidbody>();
        if (inheritShipVelocity && gameObject.GetComponent<Rigidbody>() != null)
        {
            //KAI: a bug, turret ammo needs to pick up launcher velocity as well
            rb.velocity = gameObject.GetComponent<Rigidbody>().velocity;
        }

        // duration
        var ttl = shot.GetComponent<TimeToLive>();
        if (ttl != null)
        {
            ttl.seconds = product.timeToLive;
        }

        // impart ammo velocity in the direction of the firer
        rb.velocity += gameObject.transform.forward * product.speed;

        // spin it for kicks
        rb.angularVelocity = Vector3.up * 10;

        // this is a chargable shot, scale it by the power
        shot.transform.localScale = new Vector3(product.damage, product.damage, product.damage);

        // particles
        _ps.Play();
    }
    void LaunchBeamAmmo(ship.AmmoProduct product)
    {
        // line the shot up
        var shot = GameObject.Instantiate(prefabBeamAmmo);
        shot.transform.position = firepoint.transform.position;
        shot.transform.rotation = firepoint.transform.rotation;

        // duration
        var beam = shot.GetComponent<AmmoBeam>();
        beam.duration = product.timeToLive;
    }
}
