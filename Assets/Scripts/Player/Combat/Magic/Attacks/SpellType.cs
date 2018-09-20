using UnityEngine;

[System.Serializable]
public abstract class SpellType
{	
	public const string PLAYER_TAG = "Player";
	public const string VOXEL_TAG = "TriVoxel";
	
	
	public const string VOXEL = "voxel";
	public const string MAGICIAN = "magician";
	public const string GUNNER = "gunner";
	public const string SHIELD = "shield";

	public const string ATTACK_TYPE = "attack";
	public const string DIGGER_TYPE = "dig";
	public const string TELEKINESIS_TYPE = "telekinesis";
	
	public string name;
	public int equippedIndex;
	
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