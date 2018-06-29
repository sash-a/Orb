using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Ammo {

    //have given default values but will likely be different:

    //total current ammo for gun
    private int primaryAmmo = 300;
    //current ammo in magazine
    private int magazineAmmo = 30;
    //total amount of ammo that can be carried for a gun (relevant to primary)
    private int maxAmmo = 1000;
    //total amount of bullets a magazine carries (relevant to magazine)
    private int magSize = 30;

    //grenades
    private int numGrenades = 3;
    private int maxNumGrenades = 5;

    //specific to guns (ammunition)
    public Ammo(int prA, int mgA, int mxA, int mgSz)
    {
        primaryAmmo = prA;
        magazineAmmo = mgA;
        maxAmmo = mxA;
        magSize = mgSz;
    }

    //specific to grenades
    public Ammo(int numGr, int maxNum)
    {
        numGrenades = numGr;
        maxNumGrenades = maxNum;
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
