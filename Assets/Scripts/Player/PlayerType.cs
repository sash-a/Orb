using UnityEngine;
using UnityEngine.Networking;

public class PlayerType : NetworkBehaviour
{
    [SerializeField] private GameObject[] magicianAttributes;
    [SerializeField] private GameObject[] gunnerAttributes;

    [SerializeField] public GameObject attack;

    [SerializeField] private AttackComponentsContainer attribContainer;

    [SyncVar(hook = "CmdOnTypeChange")] public string type;
    public static readonly string MAGICIAN_TYPE = "magician";
    public static readonly string GUNNER_TYPE = "gunner";

    private void pickGunner()
    {
        setMagicianAttributes(false);
        setGunnerAttributes(true);

        attack = gunnerAttributes[0];

        GetComponent<Identifier>().typePrefix = "Gunner";
        attribContainer.enabled = false;
    }

    private void pickMagician()
    {
        setMagicianAttributes(true);
        setGunnerAttributes(false);

        attack = magicianAttributes[0];

        GetComponent<Identifier>().typePrefix = "Magician";
        attribContainer.enabled = false;
    }

    private void setMagicianAttributes(bool b)
    {
        foreach (var attribute in magicianAttributes)
            attribute.SetActive(b);

        if (b)
        {
            gameObject.AddComponent<MagicAttack>().setAttributes(attribContainer.cam, attribContainer.mask,
                attribContainer.shield, attribContainer.telekenObjectPos);
        }
    }

    private void setGunnerAttributes(bool b)
    {
        foreach (var attribute in gunnerAttributes)
            attribute.SetActive(b);

        if (b)
        {
            gameObject.AddComponent<WeaponAttack>().setAttributes(attribContainer.cam, attribContainer.mask,
                attribContainer.PistolMuzzleFlash, attribContainer.AssaultMuzzleFlash,
                attribContainer.ShotgunMuzzleFlash,
                attribContainer.SniperMuzzleFlash, attribContainer.hitEffect, attribContainer.VoxelDestroyEffect,
                attribContainer.explosionEffect, attribContainer.grenadeSpawn,
                attribContainer.grenadePrefab, attribContainer.throwForce);
        }
    }

    [Command]
    public void CmdOnTypeChange(string newType)
    {
        if (newType == GUNNER_TYPE) pickGunner();
        else if (newType == MAGICIAN_TYPE) pickMagician();

        Debug.Log(newType);
    }
}