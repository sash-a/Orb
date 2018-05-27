using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class Player : NetworkBehaviour
{
    [SerializeField] private float maxHealth;
   
    
    void Start()
    {
        maxHealth = 100f;
    }

    public float getMaxHealth()
    {
        return maxHealth;
    }
}
