using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.UI;
using UnityEngine.UI;

public class UIController : MonoBehaviour
{
    [SerializeField] PlayerUIManager playerUIManager;
    Player player;

    PlayerController playerController;
    private MultiplayerEventSystem eventSystem;    

    private void Start()
    {
        player = GetComponent<Player>();
        eventSystem = GetComponent<MultiplayerEventSystem>();
        playerController = GetComponent<PlayerController>();
        player.SelectedUnitIndex = 0;
    }

    public void OnDeployUnit(InputAction.CallbackContext context)
    {
        if (Game.instance.IsPlayerRegistered(playerController))
        {
            if (context.performed)
            {
                Debug.Log("Deploying: " + player.SelectedUnitType);
                playerController.Boat.SetSail();
            }
        }
    }

    public void OnUnitSelectChange(InputAction.CallbackContext context)
    {
        if (Game.instance.IsPlayerRegistered(playerController))
        {
            if (context.performed)
            {
                player.SelectedUnitIndex += (int)context.ReadValue<float>();
                playerUIManager.GetMenu<OverlayMenu>().SetSelectedUnitIndex(player.SelectedUnitIndex);
            }
        }
    }

    public void OnPause(InputAction.CallbackContext context)
    {
        if (Game.instance.IsPlayerRegistered(playerController)) // Accounts for Unity bug, see https://forum.unity.com/threads/player-input-manager-adds-an-extra-player-with-index-1.1039000/
        {
            if (context.performed)
            {
                Game.instance.Pause();

                playerController.SetControlsActivated(true);
                playerController.SetActionMap("UI");
                playerUIManager.SwitchMenu(typeof(PauseMenu));
            }
        }
    }

    public void OnUnpause(InputAction.CallbackContext context)
    {
        if (Game.instance.IsPlayerRegistered(playerController))
        {
            if (context.performed)
            {
                Game.instance.Unpause();
                playerUIManager.SwitchMenu(typeof(OverlayMenu));
            }
        }
    }

    public void OnNavigate(InputAction.CallbackContext context)
    {
        if (Game.instance.IsPlayerRegistered(playerController))
        {
            if (context.performed)
            {
                if(eventSystem.currentSelectedGameObject == null || !eventSystem.currentSelectedGameObject.activeInHierarchy)
                {
                    SelectSomething();
                }
                else
                {
                    Debug.Log(eventSystem.currentSelectedGameObject.name);
                }
            }
        }
    }

    public void SelectSomething()
    {
        Selectable something = FindObjectOfType<Selectable>();
        if (something && eventSystem)
        {
            eventSystem.SetSelectedGameObject(something.gameObject, new BaseEventData(eventSystem));
        }
    }
}
