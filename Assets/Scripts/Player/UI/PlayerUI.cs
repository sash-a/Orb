using UnityEngine;

public class PlayerUI : MonoBehaviour
{
    [SerializeField] protected GameObject player;


    public static bool isPaused;

    void Start()
    {
        isPaused = false;
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            togglePauseMenu();
        }
    }

    void togglePauseMenu()
    {
        isPaused = !isPaused;
    }
}