using UnityEditor;
using UnityEngine;

[System.Serializable]
public abstract class SpellType
{
	public const string PLAYER_TAG = "Player";
	public const string VOXEL_TAG = "TriVoxel";
	public const string MAGICIAN_TAG = "Magician";
	public const string GUNNER_TAG = "Gunner";
	public const string SHIELD_TAG = "Shield";
    
	public Transform player;
	public MagicAttack magic;
    
	public float range;
	public float mana;
	public bool isActive;
    
	public ParticleSystem fx;
	public ParticleSystem handFx;

	public Camera cam;

	public LayerMask mask;

	public abstract void attack();

	public abstract void startAttack();

	public abstract void endAttack();
}