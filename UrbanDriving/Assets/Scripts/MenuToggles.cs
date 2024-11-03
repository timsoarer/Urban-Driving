using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.Rendering;

public class MenuToggles : MonoBehaviour
{
    [SerializeField]
    private GameObject mainMenu;
    [SerializeField]
    private GameObject settingsMenu;
    [SerializeField]
    private AudioMixer mainMixer;

    public void MuteGroup(string audioGroupName)
    {
        mainMixer.SetFloat(audioGroupName, -80.0f);
    }

    public void UnmuteGroup(string audioGroupName)
    {
        mainMixer.SetFloat(audioGroupName, 0f);
    }
    public void ToggleSettings()
    {
        mainMenu.SetActive(!mainMenu.activeSelf);
        settingsMenu.SetActive(!settingsMenu.activeSelf);
    }
}
