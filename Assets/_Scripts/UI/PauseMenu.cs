using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PauseMenu : PlayerMenu
{
    public void OnExitGameButton()
    {
        Game.instance.ExitGame();
    }

    public void OnPlayButton()
    {
        Game.instance.Unpause();
        playerUIManager.SwitchMenu(typeof(OverlayMenu));
    }

    public void OnSettingsButton()
    {
        playerUIManager.SwitchMenu(typeof(SettingsMenu));
    }
}