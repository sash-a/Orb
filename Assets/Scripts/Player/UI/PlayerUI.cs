using System.Collections;
using System.Runtime.InteropServices;
using UnityEngine;

public class PlayerUI : MonoBehaviour
{
    // The player using the UI
    [SerializeField] protected GameObject player;

    public static bool isPaused;
    public static PlayerUI localPlayerUI;


    public void Start()
    {
        isPaused = false;
        setCursorState(CursorLockMode.Locked, false);
        if (player.GetComponent<PlayerActions>().isLocalPlayer) {
            localPlayerUI = this;
        }
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            togglePauseMenu();
        }
    }

    public void togglePauseMenu()
    {
        isPaused = !isPaused;
    }

 

    public void setCursorState(CursorLockMode mode, bool visible)
    {
        //Debug.Log("setting cursor state to: " + mode.ToString());
        //SetCursorPos((int)Input.mousePosition.x - 2, (int)Input.mousePosition.y - 2);
        if (mode == CursorLockMode.Confined)
        {
            Cursor.lockState = CursorLockMode.None;
        }
        else {
            Cursor.lockState = mode;
        }
        Cursor.visible = visible;
    }

    IEnumerator confineMouse() {
        yield return new WaitForSecondsRealtime(0.2f);
        Cursor.lockState = CursorLockMode.Confined;
    }
}