using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class AllButton : MonoBehaviour
{
    public Slider msuicSlider;
    GameObject gController;
    void Awake()
    {
        gController = GameObject.Find("GameController");
        if (gController == null) return; 
        AudioSource music = gController.GetComponent<AudioSource>();
        msuicSlider.value = music.volume;

        //main menu var cursor disave manun
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }
    public void PlayGame()
    {
        SceneManager.LoadScene("GameLevel");
    }

    public void ExitScene()
    {
        SceneManager.LoadScene("MainMenu");
    }

    public void ChangeVolume(float volume)
    {
        if (gController == null) return;
        AudioSource music = gController.GetComponent<AudioSource>();
        music.volume = volume;
    }
}
