using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem;

public class SceneChange : MonoBehaviour
{

    //Tyvin: I don't know why CTRL + 1,2, or 3 doesn't work for me.
    void Update()
    {
        //Level Tutorial
        if (Keyboard.current.digit1Key.wasPressedThisFrame)
        {
            SceneManager.LoadScene(0);
        }
        //Level 1
        if (Keyboard.current.digit2Key.wasPressedThisFrame)
        {
            SceneManager.LoadScene(1);
        }
        //Level 2
        if (Keyboard.current.digit3Key.wasPressedThisFrame)
        {
            SceneManager.LoadScene(2);
        }
    }
}