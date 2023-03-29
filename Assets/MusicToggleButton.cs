using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MusicToggleButton : MonoBehaviour
{
    [SerializeField]
    GameObject levelMusic;
    public void ClickMusicToggle()
    {
        levelMusic.SetActive(!levelMusic.activeSelf);
    }
}
