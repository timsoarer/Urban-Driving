using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

public class MenuToggles : MonoBehaviour
{
    public GameObject mainMenu;
    public GameObject settingsMenu;

    public void SetVolume(float value)
    {
        AudioListener.volume = value;
    }

    public void ToggleSettings()
    {
        mainMenu.SetActive(!mainMenu.activeSelf);
        settingsMenu.SetActive(!settingsMenu.activeSelf);
    }
}
