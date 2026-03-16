using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem;

public class SceneChange : MonoBehaviour
{

    //Tyvin: I don't know why CTRL + 1,2, or 3 doesn't work for me.
    void Update()
    {
        if (Keyboard.current.shiftKey.isPressed)
        {
            //Level Tutorial
            if (Keyboard.current.qKey.wasPressedThisFrame)
            {
                SceneManager.LoadScene("Tutorial Level");
            }
            //Level 1
            if (Keyboard.current.wKey.wasPressedThisFrame)
            {
                SceneManager.LoadScene("Level 1");
            }
            //Level 2
            if (Keyboard.current.eKey.wasPressedThisFrame)
            {
                SceneManager.LoadScene("Level 2");
            }
        }
        
        if (Keyboard.current.escapeKey.wasPressedThisFrame)
        {
            SceneManager.LoadScene("Main Menu");
        }
    }
}