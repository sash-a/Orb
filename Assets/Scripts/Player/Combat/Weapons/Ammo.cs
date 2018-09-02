using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class Ammo
{
    //have given default values but will likely be different:

    //total current ammo for gun
    [SerializeField] private int primaryAmmo = 300;

    //current ammo in magazine
    [SerializeField] private int magazineAmmo = 30;

    //total amount of ammo that can be carried for a gun (relevant to primary)
    [SerializeField] private int maxAmmo = 1000;

    //total amount of bullets a magazine carries (relevant to magazine)
    [SerializeField] private int magSize = 30;

    //grenades
    [SerializeField] private int numGrenades = 3;
    [SerializeField] private int maxNumGrenades = 5;

    // Pricing
    public int cost;
    public int ammoPerPurchase;

    //specific to guns (ammunition)
    public Ammo(int prA, int mgA, int mxA, int mgSz, int cost, int ammoPerPurchase)
    {
        primaryAmmo = prA;
        magazineAmmo = mgA;
        maxAmmo = mxA;
        magSize = mgSz;
        this.cost = cost;
        this.ammoPerPurchase = ammoPerPurchase;
    }

    //specific to grenades
    public Ammo(int numGr, int maxNum, int ammoCost, int ammoPerPurchase)
    {
        numGrenades = numGr;
        maxNumGrenades = maxNum;
        cost = ammoCost;
        this.ammoPerPurchase = ammoPerPurchase;
    }

    public int getPrimaryAmmo()
    {
        return primaryAmmo;
    }

    public int getMagAmmo()
    {
        return magazineAmmo;
    }

    public int getMaxAmmo()
    {
        return maxAmmo;
    }

    public int getMagSize()
    {
        return magSize;
    }

    public int getNumGrenades()
    {
        return numGrenades;
    }

    public int getMaxNumGrenades()
    {
        return maxNumGrenades;
    }

    public void setPrimaryAmmo(int amount)
    {
        primaryAmmo = amount;
    }

    public void setMagAmmo(int amount)
    {
        magazineAmmo = amount;
    }

    public void setNumGrenades(int amount)
    {
        numGrenades = amount;
    }
}