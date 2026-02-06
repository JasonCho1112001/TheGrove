using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem;

public class gameManager : MonoBehaviour
{
  
    void Start()
    {
        
    }

    void Update()
    {
        //Restart game if player pressed CTRL + R
        if (Keyboard.current.leftCtrlKey.isPressed && Keyboard.current.rKey.wasPressedThisFrame)
        {
            RestartGame();
        }
    }

    void RestartGame()
    {
        //Placeholder for restart logic, such as reloading the scene or resetting game state
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
}
