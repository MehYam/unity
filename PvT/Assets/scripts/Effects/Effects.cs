using UnityEngine;
using System.Collections.Generic;
using System.Collections.ObjectModel;

public sealed class Effects
{
    readonly ReadOnlyCollection<WorldObjectType> _explosions;
    readonly ReadOnlyCollection<WorldObjectType> _smallExplosions;
    readonly ReadOnlyCollection<WorldObjectType> _muzzleFlashes;
    readonly ReadOnlyCollection<WorldObjectType> _smoke;
    readonly WorldObjectType _vehicleExplosion;
    public Effects(Loader loader)
    {
        _explosions = Load(loader, "largeExplosion", 1);
        _smallExplosions = Load(loader, "smallExplosion", 1);
        _muzzleFlashes = Load(loader, "muzzleflash", 0);
        _smoke = Load(loader, "smoke", 1);
        _vehicleExplosion = loader.GetMisc("vehicleExplosion");
    }
    static ReadOnlyCollection<WorldObjectType> Load(Loader loader, string name, int startIndex)
    {
        var list = new List<WorldObjectType>();
        for (int i = startIndex; ; ++i)
        {
            var effect = loader.GetMisc(name + i);
            if (effect != null)
            {
                list.Add(effect);
            }
            else
            {
                break;
            }
        }
        return new ReadOnlyCollection<WorldObjectType>(list);
    }

    static WorldObjectType Rand(IList<WorldObjectType> container)
    {
        return container[Random.Range(0, container.Count)];
    }
    public WorldObjectType GetRandomExplosion()
    {
        return Rand(_explosions);
    }
    public WorldObjectType GetRandomSmallExplosion()
    {
        return Rand(_smallExplosions);
    }
    public WorldObjectType GetRandomMuzzleFlash()
    {
        return Rand(_muzzleFlashes);
    }
    public WorldObjectType GetRandomSmoke()
    {
        return Rand(_smoke);
    }
    public WorldObjectType GetVehicleExplosion()
    {
        return _vehicleExplosion;
    }
}
