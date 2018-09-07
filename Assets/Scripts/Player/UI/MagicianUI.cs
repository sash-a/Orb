using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class MagicianUI : PlayerUI
{
    #region Variables

    // Components
    private NetHealth health;
    private ResourceManager resourceManager;
    public NetHealth shieldHealth;
    public MagicAttack magic;
    public DamageIndicator damageIndicator;
    
    // Bars
    [SerializeField] private RectTransform healthBar;
    [SerializeField] private RectTransform shieldBar;
    [SerializeField] private RectTransform energyBar;

    // Slot borders
    [SerializeField] private Image magicSlot0;
    [SerializeField] private Image magicSlot1;
    [SerializeField] private Image magicSlot2;
    [SerializeField] private Image magicSlot3;

    [SerializeField] private Sprite borderEquipped;
    [SerializeField] private Sprite borderUnequipped;


    // Game state indicators
    [SerializeField] private TextMeshProUGUI energyCount;
    [SerializeField] private TextMeshProUGUI livingPlayerCount;
    [SerializeField] private TextMeshProUGUI killCount;

    // Other classes UIs
    public GameObject gunnerUI;

    #endregion

    private void Start()
    {
        if (player != null && player.GetComponent<Identifier>().typePrefix != Identifier.magicianType)
        {
            Debug.LogError("Displaying incorrect HUD");
        }
    }

    public void setUp(GameObject localPlayer)
    {
        player = localPlayer;
        if (player != null && player.GetComponent<Identifier>().typePrefix != Identifier.magicianType)
        {
            Debug.LogError("Displaying incorrect HUD");
            return;
        }

        health = player.GetComponent<NetHealth>();
        resourceManager = player.GetComponent<ResourceManager>();
        magic = player.GetComponent<MagicAttack>();

        setHealth(1);
        setShield();

        isPaused = false;
    }

    void Update()
    {
        setHealth(health.getHealthPercent());
        setShield();
        setEnergy(resourceManager.getEnergy(), resourceManager.getMaxEnergy());
        showEquipped();
    }

    void showEquipped()
    {
        if (magic.currentWeapon == 0)
        {
            magicSlot0.sprite = borderEquipped;
            magicSlot1.sprite = borderUnequipped;
            magicSlot2.sprite = borderUnequipped;
        }
        else if (magic.currentWeapon == 1)
        {
            magicSlot0.sprite = borderUnequipped;
            magicSlot1.sprite = borderEquipped;
            magicSlot2.sprite = borderUnequipped;
        }
        else if (magic.currentWeapon == 2)
        {
            magicSlot0.sprite = borderUnequipped;
            magicSlot1.sprite = borderUnequipped;
            magicSlot2.sprite = borderEquipped;
        }
    }

    void setHealth(float h)
    {
        healthBar.localScale = new Vector3(1f, h, 1f);
    }

    void setShield()
    {
        if (shieldHealth == null)
        {
            shieldBar.localScale = new Vector3(1f, 0, 1f);
            return;
        }

        if (shieldHealth.maxHealth != 0)
            shieldBar.localScale = new Vector3(1f, shieldHealth.getHealth() / shieldHealth.maxHealth, 1f);
    }

    void setEnergy(float amount, float maxEnergy)
    {
        energyCount.text = (int) amount + "";
        energyBar.localScale = new Vector3(1f, amount / maxEnergy, 1f);
    }

    public void onShieldUp(NetHealth health)
    {
        shieldHealth = health;
    }

    public void onShieldDown()
    {
        shieldHealth = null;
    }
}