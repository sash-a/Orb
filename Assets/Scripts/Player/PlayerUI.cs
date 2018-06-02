using UnityEngine;
using UnityEngine.Networking;

// TODO this will need to be specialized for each type of player
// Currently only for testing the magician
//[RequireComponent(typeof(ResourceManager), typeof(NetHealth))]
public class PlayerUI : MonoBehaviour
{
    [SerializeField] private GameObject player;
    [SerializeField] private RectTransform healthBar;
    [SerializeField] private RectTransform energyBar;

    [SerializeField] private NetHealth health;
    [SerializeField] private ResourceManager resourceManager;

    void Start()
    {
        Debug.Log("Local");

        health = player.GetComponent<NetHealth>();
        resourceManager = player.GetComponent<ResourceManager>();

        setHealth(1);
        setEnergy(1);
    }

    void Update()
    {
        setHealth(health.getHealthPercent());
        setEnergy(resourceManager.getEnergy() / resourceManager.getMaxEnergy());
    }

    public void setPlayer(GameObject player)
    {
        this.player = player;
    }

    void setHealth(float h)
    {
        healthBar.localScale = new Vector3(1f, h, 1f);
    }

    void setEnergy(float energy)
    {
        energyBar.localScale = new Vector3(1f, energy, 1f);
    }
}