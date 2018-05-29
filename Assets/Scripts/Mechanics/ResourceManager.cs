using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class ResourceManager : NetworkBehaviour
{
    [SerializeField] private int primaryAmmo = 50;
    [SerializeField] private int secondaryAmmo = 50;
    [SerializeField] private int maxAmmo = 1000;

    [SerializeField] private int energy = 100;
    [SerializeField] private int maxEnergy = 100;

    // TODO I think energy gain and drain should have its own class
    // List of origionalCoroutine
    private List<Coroutine> origionalEnergyDrains;

    // Maps the initial coroutine to the most recently called coroutine
    private Dictionary<Coroutine, Coroutine> currentEnergyDrains;

    void Start()
    {
        origionalEnergyDrains = new List<Coroutine>();
        currentEnergyDrains = new Dictionary<Coroutine, Coroutine>();
    }

    /// <summary>
    /// Adds energy to the players resource manager
    /// </summary>
    /// <param name="amount">Amount of energy to gain</param>
    public void gainEnery(int amount)
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
    public void useEnergy(int amount)
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

    public Coroutine beginEnergyDrain(int rate)
    {
        Debug.Log("Starting Energy drain");
        var d = StartCoroutine(drainEnergy(rate, origionalEnergyDrains.Count));
        origionalEnergyDrains.Add(d);
        currentEnergyDrains.Add(d, d);
        return d;
    }

    public void endEnergyDrain(Coroutine coroutine)
    {
        StopCoroutine(currentEnergyDrains[coroutine]);

        Debug.Log("Ended energy drain");
    }

    private IEnumerator drainEnergy(int rate, int listPos)
    {
        useEnergy(1);
        yield return new WaitForSeconds(1 / (float) rate);
        // Calls itself
        Debug.Log("Current energy: " + energy);
        var d = StartCoroutine(drainEnergy(rate, listPos));
        currentEnergyDrains[origionalEnergyDrains[listPos]] = d;
    }
}