using UnityEngine;
using System.Collections.Generic;
using System.Collections.ObjectModel;

public sealed class Effects
{
    readonly ReadOnlyCollection<Asset> _explosions;
    readonly ReadOnlyCollection<Asset> _smallExplosions;
    readonly ReadOnlyCollection<Asset> _muzzleFlashes;
    readonly ReadOnlyCollection<Asset> _smoke;
    readonly Asset _vehicleExplosion;
    public Effects(Loader loader)
    {
        _explosions = Load(loader, "largeExplosion", 1);
        _smallExplosions = Load(loader, "smallExplosion", 1);
        _muzzleFlashes = Load(loader, "muzzleflash", 0);
        _smoke = Load(loader, "smoke", 1);
        _vehicleExplosion = loader.GetMisc("vehicleExplosion");
    }
    static ReadOnlyCollection<Asset> Load(Loader loader, string name, int startIndex)
    {
        var list = new List<Asset>();
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
        return new ReadOnlyCollection<Asset>(list);
    }

    static Asset Rand(IList<Asset> container)
    {
        return container[Random.Range(0, container.Count)];
    }
    public Asset GetRandomExplosion()
    {
        return Rand(_explosions);
    }
    public Asset GetRandomSmallExplosion()
    {
        return Rand(_smallExplosions);
    }
    public Asset GetRandomMuzzleFlash()
    {
        return Rand(_muzzleFlashes);
    }
    public Asset GetRandomSmoke()
    {
        return Rand(_smoke);
    }
    public Asset GetVehicleExplosion()
    {
        return _vehicleExplosion;
    }
}
