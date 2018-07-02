using UnityEngine;

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

    [SerializeField] private GameObject pauseMenu;

    void Start()
    {
        health = player.GetComponent<NetHealth>();
        resourceManager = player.GetComponent<ResourceManager>();

        setHealth(1);
        setEnergy(1);

        PauseMenu.isPaused = false;
    }

    void Update()
    {
        setHealth(health.getHealthPercent());
        setEnergy(resourceManager.getEnergy() / resourceManager.getMaxEnergy());

        if (Input.GetKeyDown(KeyCode.Escape))
        {
            togglePauseMenu();
        }
    }

    void togglePauseMenu()
    {
        pauseMenu.SetActive(!pauseMenu.activeSelf);
        PauseMenu.isPaused = pauseMenu.activeSelf;

        Debug.Log("Toggle pause currently: " + pauseMenu.activeSelf);
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