using System;
using UnityEngine;
using UnityEngine.Networking;

public class ResourceManager : NetworkBehaviour
{
    [SerializeField] [SyncVar] private float energy = 100;
    [SerializeField] private float maxEnergy = 100;

    [SerializeField] private float gunnerShield = 0;
    [SerializeField] private float maxGunnerShield = 100;

    /// <summary>
    /// Adds energy to the players resource manager
    /// </summary>
    /// <param name="amount">Amount of energy to gain</param>
    public void gainEnery(float amount)
    {
        if (!isLocalPlayer || amount <= 0) return;

        energy = Math.Min(maxEnergy, energy + amount);
    }

    /// <summary>
    /// Adds primary ammo to the players resource manager
    /// </summary>
    /// <param name="amount">Amount of bullets to add to primary ammo</param>
    public void pickupPrimary(int amount, Ammo A) //, ammo
    {
        if (!isLocalPlayer || amount <= 0) return;

        //primaryAmmo = Math.Min(maxAmmo, primaryAmmo + amount);
        A.setPrimaryAmmo(Math.Min(A.getMaxAmmo(), A.getPrimaryAmmo() + amount));
    }

    /// <summary>
    /// Adds magazine ammo to the players resource manager
    /// </summary>
    /// <param name="amount">Amount of bullets to add to magazine ammo</param>
    public void reloadMagazine(int amount, Ammo A)
    {
        if (!isLocalPlayer || amount <= 0) return;

        if (A.getPrimaryAmmo() > A.getMagSize())
        {
            //magazineAmmo = Math.Min(magSize, magazineAmmo + amount);
            A.setMagAmmo(Math.Min(A.getMagSize(), A.getMagAmmo() + amount));

            //replenish mag bullets from primary
            usePrimaryAmmo(amount, A);
        }
        else
        {
            A.setMagAmmo(A.getPrimaryAmmo());
            A.setPrimaryAmmo(0);
        }
    }

    public void pickupGrenade(int amount, Ammo A)
    {
        if (!isLocalPlayer || amount <= 0) return;

        A.setNumGrenades(Math.Min(A.getMaxNumGrenades(), A.getNumGrenades() + amount));
    }

    /// <summary>
    /// Uses players energy
    /// </summary>
    /// <param name="amount">Amount of energy to use</param>
    public void useEnergy(float amount)
    {
        if (!isLocalPlayer || amount <= 0) return;

        energy = Math.Max(0, energy - amount);
    }

    /// <summary>
    /// Uses players primary ammo
    /// </summary>
    /// <param name="amount">Amount of primary ammo to use</param>
    public void usePrimaryAmmo(int amount, Ammo A)
    {
        if (!isLocalPlayer || amount <= 0) return;

        //primaryAmmo = Math.Max(0, primaryAmmo - amount);
        A.setPrimaryAmmo(Math.Max(0, A.getPrimaryAmmo() - amount));
    }

    /// <summary>
    /// Uses players magazine ammo
    /// </summary>
    /// <param name="amount">Amount of magazine ammo to use</param>
    public void useMagazineAmmo(int amount, Ammo A)
    {
        if (!isLocalPlayer || amount <= 0) return;

        //magazineAmmo = Math.Max(0, magazineAmmo - amount);
        A.setMagAmmo(Math.Max(0, A.getMagAmmo() - amount));
    }

    public void useGrenade(int amount, Ammo A)
    {
        if (!isLocalPlayer || amount <= 0) return;

        A.setNumGrenades(Math.Max(0, A.getNumGrenades() - amount));
    }

    // TODO this is returning false server side?
    public bool hasEnergy()
    {
        return energy > 0;
    }

    public float getEnergy()
    {
        return energy;
    }

    public float getMaxEnergy()
    {
        return maxEnergy;
    }

    public float getShield()
    {
        return gunnerShield;
    }

    public void setGunnerShield(float amount)
    {
        gunnerShield = Mathf.Max(0.0f, Mathf.Min(maxGunnerShield, gunnerShield));
    }

    public float getGunnerShieldPercent()
    {
        return gunnerShield / maxGunnerShield;
    }
}