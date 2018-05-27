using UnityEngine;
using UnityEngine.Networking;

public class CursorLock : MonoBehaviour
{
    void Start()
    {
        
    }
    
    void Update()
    {
        // Unlock on escape
        if (Input.GetKeyDown(KeyCode.Escape)) unlockCursor();

        // Lock on L
        if (Input.GetKeyDown(KeyCode.L)) lockCursor();
    }

    public static void unlockCursor()
    {
        Cursor.lockState = CursorLockMode.None;
    }

    public static void lockCursor()
    {
        Cursor.lockState = CursorLockMode.Locked;
    }
}