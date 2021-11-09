using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class TitleScreen : MonoBehaviour
{
    public void OnPlay()
    {
        SceneManager.LoadScene("Island");
    }

    public void OnExitGame()
    {
        Game.instance.ExitGame();
    }
}