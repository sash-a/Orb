using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Health : MonoBehaviour
{

    public bool revivable;
    public float regeneration;//set to 0 for things that dont regen
    public bool invulnerable;

    public int fullHealth = 100;
    public float health;
    float timeSinceDamaged = 50f;
    public float damageImmunity = 0.5f;

    System.Random rand;

    public void takeDamage(float damage)
    {
        //Debug.Log("take damage called tsd: " + timeSinceDamaged + "  di = " + damageImmunity);
        if (timeSinceDamaged > damageImmunity)
        {
            //Debug.Log(gameObject.name + " taking damage");
            timeSinceDamaged = 0;
            health -= damage;
            if (health <= 0)
            {
                if (!revivable)
                {
                    DestroyObject();
                }
                else
                {
                    /**
                    GetComponent<AudioSource>().clip = Resources.Load<AudioClip>("Sounds/Dying"+rand.Next(1,3));
                    GetComponent<AudioSource>().volume = 1;
                    GetComponent<AudioSource>().Play();
                    GameObject.Find("HUD").GetComponent<HUD>().setInfo("Player Unconscious. Revive him with R");
                    */
                }
            }

        }
    }

    private void DestroyObject()
    {
        if (gameObject.name.Equals("TriVoxel"))
        {
            if (gameObject.GetComponent<Voxel>().layer < MapManager.mapLayers-1) {
                gameObject.GetComponent<Voxel>().destroyVoxel();
            }

        }
        else {
            Destroy(gameObject);
        }
        //if (gameObject.name.Equals("TriVoxel")) gameObject.GetComponent<Voxel>().pullRandom();

    }

    public void revive()
    {
        if (health <= 0)
        {
            health = fullHealth / 2.0f;
        }
    }

    // Use this for initialization
    void Start()
    {
        health = fullHealth;
        rand = new System.Random();
    }

    // Update is called once per frame
    void Update()
    {
        regenen();
        timeSinceDamaged += Time.deltaTime;
    }

    private void regenen()
    {
        if (health < fullHealth && regeneration > 0 && health > 0)
        {
            health += regeneration * Time.deltaTime;
            if (health > fullHealth)
            {
                health = fullHealth;
            }
        }
    }
}
