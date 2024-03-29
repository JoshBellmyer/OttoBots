using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.InputSystem;

public class CursorController : MonoBehaviour
{
    [SerializeField] PlayerUIManager playerUIManager;
    OverlayMenu overlayMenu;

    [SerializeField] PlayerInput playerInput;

    private Vector2 rawInputDelta;
    private new Camera camera;

    private PlayerController playerController;
    private Player player;

    private List<GameObject> selectedUnitSprites = new List<GameObject>();

    private Vector3 ladderFloorPos;

    private void Start()
    {
        camera = playerInput.camera;
        playerController = GetComponent<PlayerController>();
        player = GetComponent<Player>();
        overlayMenu = playerUIManager.GetMenu<OverlayMenu>();
    }

    private void Update()
    {
        overlayMenu.MoveCursor(rawInputDelta);

        Tuple<bool, RaycastHit> hitData = overlayMenu.CastFromCursor(LayerMask.GetMask("Terrain"));

        if (player.SelectedGroup != null) {
            if (player.SelectedGroup.GetLiving() < 1) {
                DeselectUnits();
            }
        }

        if (hitData.Item1) {
            if (hitData.Item2.normal.y > 0) {
                SetLadderActive(false);
                MoveSelection(hitData.Item2.point, hitData.Item2.normal);
            }
            else {
                SetSelectionActive(false);

                if (hitData.Item2.normal.y == 0 && player.SelectedGroup != null) {
                    Unit testUnit = player.SelectedGroup.GetUnits()[0];

                    if (testUnit is LadderUnit) {
                        MoveLadder(hitData.Item2.point, hitData.Item2.normal);
                    }
                    else {
                        SetLadderActive(false);
                    }
                }
            }
        }
        else {
            SetSelectionActive(false);
            SetLadderActive(false);
        }
    }

    public void OnCursorMove(InputAction.CallbackContext value)
    {
        rawInputDelta = value.ReadValue<Vector2>();
    }

    public void OnCursorSelect(InputAction.CallbackContext context)
    {
        // Accounts for Unity bug, see https://forum.unity.com/threads/player-input-manager-adds-an-extra-player-with-index-1.1039000/
        if (!Game.instance.IsPlayerRegistered(playerController)) {
            return;
        }
        if (!context.performed) {
            return;
        }        
        if (camera == null) return; // OnCursorSelect() can happen before Start() ???

        Tuple<bool, RaycastHit> hitData = overlayMenu.CastFromCursor(Game.everythingMask);

        if (hitData.Item1) {
            Vector3 location = hitData.Item2.point;

            switch (hitData.Item2.collider.tag) {
                case "Terrain":
                    if (player.SelectedGroup == null) {
                        SelectUnitsInSquare();
                    }
                    else if (hitData.Item2.normal.y > 0) {
                        MoveUnitGroup(hitData.Item2);
                    }
                    else if (hitData.Item2.normal.y == 0 && player.Ladder.activeSelf) {
                        MoveLadderUnit();
                    }
                break;

                case "Unit":
                    TrySelectUnit(hitData.Item2.collider.GetComponent<Unit>());
                break;
            }
        }
    }

    private void SelectUnitsInSquare () {
        if (!player.GridSelection.activeSelf) {
            return;
        }

        Vector3 pos = player.GridSelection.transform.position;

        HashSet<Unit> units = TeamManager.instance.GetOnTeam($"{player.playerId}");
        Dictionary<Group, int> closeCount = new Dictionary<Group, int>();
        Group maxGroup = null;
        Unit maxUnit = null;
        int maxCount = 0;

        foreach (Unit u in units) {
            float dist = Vector3.Distance(u.transform.position, pos);

            if (dist < 0.6f) {
                if (!closeCount.ContainsKey(u.Group)) {
                    closeCount.Add(u.Group, 1);
                }
                else {
                    closeCount[u.Group]++;
                }

                if (closeCount[u.Group] > maxCount) {
                    maxGroup = u.Group;
                    maxUnit = u;
                    maxCount = closeCount[u.Group];
                }
            }
        }

        if (maxGroup != null) {
            TrySelectUnit(maxUnit);
        }
    }

    private void TrySelectUnit (Unit unit) {
        if (unit == null) {
            return;
        }

        int teamId = int.Parse(unit.Team);

        if (teamId != player.playerId) {
            return;
        }

        if (unit.Group == player.SelectedGroup) {
            DeselectUnits();

            return;
        }

        DeselectUnits();

        SoundPlayer.PlaySoundLocal(Sound.MenuMove, 1, false);

        player.SelectedGroup = unit.Group;
        List<Unit> unitList = unit.Group.GetUnits();

        foreach (Unit u in unitList) {
            GameObject sprite = PrefabFactory.CreateUnitSelectSprite(player, u);
            selectedUnitSprites.Add(sprite);
        }
    }

    public void DeselectUnits () {
        foreach (GameObject obj in selectedUnitSprites) {
            Destroy(obj);
        }

        selectedUnitSprites.Clear();
        SetLadderActive(false);

        player.SelectedGroup = null;
    }

    private void MoveUnitGroup (RaycastHit hit) {
        if (player.SelectedGroup == null) {
            return;
        }

        SoundPlayer.PlaySoundLocal(Sound.MenuClick, 0.9f, false);

        if (Game.online && !Game.isHost) {
            Vector3 pos = GridUtils.GetGridPos(hit.point);
            player.SelectedGroup.MoveTo(pos);
        }
        else {
            Vector3 pos = GridUtils.GetGridPos(hit.point);
            player.SelectedGroup.SetAgentEnabled(true);
            player.SelectedGroup.MoveTo(pos);
        }

        DeselectUnits();
    }

    private void MoveLadderUnit () {
        if (player.SelectedGroup == null) {
            return;
        }

        SoundPlayer.PlaySoundLocal(Sound.MenuClick, 0.9f, false);

        player.SelectedGroup.SetAgentEnabled(true);
        player.SelectedGroup.MoveTo(ladderFloorPos);
        LadderUnit ladderUnit = (LadderUnit)player.SelectedGroup.GetUnits()[0];

        ladderUnit.AttachToWall(player.Ladder.transform.position, player.Ladder.transform.forward);

        DeselectUnits();
    }

    private void MoveSelection (Vector3 position, Vector3 normal) {
        if (player.GridSelection == null) {
            return;
        }

        SetSelectionActive(true);
        player.GridSelection.transform.position = GridUtils.GetGridPos(position);
        player.GridSelection.transform.up = normal;
    }

    private void SetSelectionActive (bool newActive) {
        if (player.GridSelection == null) {
            return;
        }

        player.GridSelection.SetActive(newActive);
    }

    private void MoveLadder (Vector3 pos, Vector3 normal) {
        if (player.Ladder == null) {
            return;
        }

        Vector3 topPos = GridUtils.GetTopFromSide(pos, normal);
        Vector3 topNormal = GridUtils.GetGridNormal(topPos);
        Vector3 sidePos = GridUtils.GetSideGridPos(pos, normal);
        ladderFloorPos = GridUtils.GetGridPos(pos + (normal * 0.25f));
        Vector3 floorNormal = GridUtils.GetGridNormal(ladderFloorPos);

        if (topNormal != Vector3.up || floorNormal != Vector3.up) {
            SetLadderActive(false);
            return;
        }

        float diff1 = Mathf.Abs(pos.y - topPos.y);
        float diff2 = Mathf.Abs(pos.y - ladderFloorPos.y);

        if (diff1 >= 1 || diff2 >= 1 || diff2 == 0) {
            SetLadderActive(false);
            return;
        }

        SetLadderActive(true);
        player.Ladder.transform.position = sidePos;
        player.Ladder.transform.forward = normal;
    }

    private void SetLadderActive (bool newActive) {
        if (player.Ladder == null) {
            return;
        }

        player.Ladder.SetActive(newActive);
    }
}











