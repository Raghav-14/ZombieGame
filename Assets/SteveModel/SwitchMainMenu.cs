using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
public class SwitchMainMenu : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        Invoke("SwitchToMainMenu", 10);
    }

    void SwitchToMainMenu()
    {
        SceneManager.LoadScene("MainMenu");
    }
}
