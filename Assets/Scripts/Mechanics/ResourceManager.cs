using System;
using UnityEngine;
using UnityEngine.Networking;

public class ResourceManager : NetworkBehaviour
{
    [SerializeField] private int primaryAmmo = 50;
    [SerializeField] private int secondaryAmmo = 50;
    [SerializeField] private int maxAmmo = 1000;

    [SerializeField] private float energy = 100;
    [SerializeField] private float maxEnergy = 100;

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
    public void pickupPrimary(int amount)
    {
        if (!isLocalPlayer || amount <= 0) return;

        primaryAmmo = Math.Min(maxAmmo, primaryAmmo + amount);
    }

    /// <summary>
    /// Adds secondary ammo to the players resource manager
    /// </summary>
    /// <param name="amount">Amount of bullets to add to secondary ammo</param>
    public void pickupSecondary(int amount)
    {
        if (!isLocalPlayer || amount <= 0) return;

        secondaryAmmo = Math.Min(maxAmmo, secondaryAmmo + amount);
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
    public void usePrimary(int amount)
    {
        if (!isLocalPlayer || amount <= 0) return;

        primaryAmmo = Math.Max(0, primaryAmmo - amount);
    }

    /// <summary>
    /// Uses players secondary ammo
    /// </summary>
    /// <param name="amount">Amount of secondary ammo to use</param>
    public void useSecondary(int amount)
    {
        if (!isLocalPlayer || amount <= 0) return;

        secondaryAmmo = Math.Max(0, secondaryAmmo - amount);
    }

    public bool hasEnergy()
    {
        return energy != 0;
    }

    public float getEnergy()
    {
        return energy;
    }

    public float getMaxEnergy()
    {
        return maxEnergy;
    }
}