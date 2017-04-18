using UnityEngine;

using PvT3D.Util;
using kaiGameUtil;

public class WeaponSchematic : MonoBehaviour
{
    [SerializeField] GameObject firepoint;
    [SerializeField] GameObject ammoPrefab;
    [SerializeField] bool inheritShipVelocity;

    ParticleSystem _ps;
    void Start()
    {
        _ps = firepoint.GetComponent<ParticleSystem>();

        var schem = new Components.Schematic(5, 3);
        schem.grid.Set(0, 0, new Components.PowerModule("P", 1));
        //schem.grid.Set(1, 0, new Components.Charger("C", 10));
        schem.grid.Set(1, 0, new Components.AutofireCharger("A", 1));

        var emitter = new Components.Emitter("E");
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
        public readonly Components.Schematic schematic;
        public readonly Components.PowerModule powerModule;
        public readonly Components.Emitter emitter;

        public SchematicState(Components.Schematic schem, Components.PowerModule pm, Components.Emitter e)
        { this.schematic = schem;  this.powerModule = pm; this.emitter = e; }
    }
    SchematicState _state;
    void ConnectWeaponSchematic(Components.Schematic schem)
    {
        Components.PowerModule powerModule = null;
        Components.ICharger charger = null;
        Components.Emitter emitter = null;
        schem.grid.ForEach((x, y, component) =>
        {
            if (component is Components.PowerModule)
            {
                powerModule = component as Components.PowerModule;
            }
            else if (component is Components.Emitter)
            {
                emitter = component as Components.Emitter;
            }
            else if (component is Components.ICharger)
            {
                charger = component as Components.ICharger;
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
