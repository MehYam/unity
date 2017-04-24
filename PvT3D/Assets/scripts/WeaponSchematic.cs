using UnityEngine;

using PvT3D.Util;
using kaiGameUtil;

public class WeaponSchematic : MonoBehaviour
{
    [SerializeField] GameObject firepoint = null;
    [SerializeField] GameObject ammoPrefab = null;
    [SerializeField] bool inheritShipVelocity = false;

    ParticleSystem _ps;
    void Start()
    {
        _ps = firepoint.GetComponent<ParticleSystem>();

        var schem = new ShipComponents.Schematic(5, 3);
        schem.grid.Set(0, 0, new ShipComponents.PowerModule("P", 1));
        //schem.grid.Set(1, 0, new Components.Charger("C", 10));
        schem.grid.Set(1, 0, new ShipComponents.AutofireCharger("A", 1));

        var emitter = new ShipComponents.Emitter("E");
        schem.grid.Set(2, 0, emitter);

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
        public readonly ShipComponents.Schematic schematic;
        public readonly ShipComponents.PowerModule powerModule;
        public readonly ShipComponents.Emitter emitter;

        public SchematicState(ShipComponents.Schematic schem, ShipComponents.PowerModule pm, ShipComponents.Emitter e)
        { this.schematic = schem;  this.powerModule = pm; this.emitter = e; }
    }
    SchematicState _state;
    void ConnectWeaponSchematic(ShipComponents.Schematic schem)
    {
        ShipComponents.PowerModule powerModule = null;
        ShipComponents.ICharger charger = null;
        ShipComponents.Emitter emitter = null;
        schem.grid.ForEach((x, y, component) =>
        {
            if (component is ShipComponents.PowerModule)
            {
                powerModule = component as ShipComponents.PowerModule;
            }
            else if (component is ShipComponents.Emitter)
            {
                emitter = component as ShipComponents.Emitter;
            }
            else if (component is ShipComponents.ICharger)
            {
                charger = component as ShipComponents.ICharger;
            }
        });

        powerModule.output = charger;
        charger.output = emitter;
        if (emitter != null)
        {
            emitter.PowerEmitted += OnPowerEmitted;
        }

        _state = new SchematicState(schem, powerModule, emitter);
    }
    void OnFireStart()
    {
        if (_state != null && _state.powerModule != null)
        {
            _state.powerModule.PowerOn();
        }
    }
    void OnFireEnd()
    {
        if (_state != null && _state.powerModule != null)
        {
            _state.powerModule.PowerOff();
        }
    }
    void OnFireFrame()
    {
        if (_state != null && _state.powerModule != null)
        {
            _state.powerModule.OnFixedUpdate();
        }
    }
    void OnPowerEmitted(float power)
    {
        if (ammoPrefab != null)
        {
            // line the shot up
            var shot = GameObject.Instantiate(ammoPrefab);
            shot.transform.position = firepoint.transform.position;
            shot.transform.rotation = firepoint.transform.rotation;

            // inherit the ship's velocity
            var rb = shot.transform.GetComponent<Rigidbody>();
            if (inheritShipVelocity && gameObject.GetComponent<Rigidbody>() != null)
            {
                //KAI: a bug, turret ammo needs to pick up launcher velocity as well
                rb.velocity = gameObject.GetComponent<Rigidbody>().velocity;
            }
            const float SPEED = 25;  //KAI: this needs to come from the weapon schematic

            // impart ammo velocity in the direction of the firer
            rb.velocity += gameObject.transform.forward * SPEED;

            // spin it for kicks
            rb.angularVelocity = Vector3.up * 10;

            // this is a chargable shot, scale it by the power
            shot.transform.localScale = new Vector3(power, power, power);

            // particles
            _ps.Play();
        }
    }
}
