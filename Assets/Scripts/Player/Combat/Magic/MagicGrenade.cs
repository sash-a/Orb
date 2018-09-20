using System.Collections;
using UnityEngine;
using UnityEngine.Networking;

public class MagicGrenade : NetworkBehaviour
{
    public float force;
    public float explosionTime;
    public GameObject AOEDamager;
    private Identifier caster;

    void Start()
    {
        GetComponent<Rigidbody>().AddForce(force * transform.forward);
        StartCoroutine(explode());
    }

    private void OnCollisionEnter(Collision other)
    {
        var id = other.gameObject.GetComponent<Identifier>();
        if (id != null && id.id == caster.id)
            return;

        StopAllCoroutines();
        spawnExplosions();
    }

    private IEnumerator explode()
    {
        yield return new WaitForSeconds(explosionTime);
        spawnExplosions();
    }

    [Client]
    void spawnExplosions()
    {
        // Spawn explosion effect

        if (!isServer) return;

        // Spawn AOE stuff
        GameObject AOE = Instantiate(AOEDamager);
        AOE.transform.position = transform.position;
        AreaOfEffectDamage a = AOE.GetComponent<AreaOfEffectDamage>();
        a.duration = 0.5f;
        a.damage = 420;
        a.radius = 10;
        a.mapRadius = 10;
        a.damageGunners = true;

        Debug.Log("Playing AOE");

        NetworkServer.Destroy(gameObject);
    }

    public void setCaster(GameObject caster)
    {
        this.caster = caster.GetComponent<Identifier>();
    }
}