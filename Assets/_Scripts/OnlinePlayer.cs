using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class OnlinePlayer : NetworkBehaviour {

    public int playerId;

    private NetworkObject netObj;
    public Group waitingGroup;
    public Player waitingPlayer;


    private void Start () {
        netObj = GetComponent<NetworkObject>();

        DontDestroyOnLoad(gameObject);
        InitMessageHandlers();

        if (Game.networkManager.IsHost) {
            playerId = (Game.networkManager.LocalClient.PlayerObject == netObj) ? 1 : 2;
        }
        else {
            playerId = (Game.networkManager.LocalClient.PlayerObject == netObj) ? 2 : 1;
        }

        // Handle the event that the other player disconnected
        Game.networkManager.OnClientDisconnectCallback += (id) => {
            if (playerId == 1) {
                Game.instance.SwitchToMainMenu();
            }
        };
    }


    [ServerRpc]
    public void SpawnUnitServerRpc (ulong clientId, UnitType unitType) {
        GameObject prefab = UnitManager.instance.GetPrefabOfType(unitType);

        if (prefab == null) {
            return;
        }

        NetworkObjectReference[] unitRef = new NetworkObjectReference[prefab.GetComponent<Unit>().groupAmount];

        NetworkGroup networkGroup = PrefabFactory.CreateNetworkGroup();
        networkGroup.GetComponent<NetworkObject>().Spawn();

        for (int i = 0; i < prefab.GetComponent<Unit>().groupAmount; i++) {
            Unit unit = UnityEngine.Object.Instantiate(prefab).GetComponent<Unit>();
            NetworkObject unitNetObj = unit.GetComponent<NetworkObject>();

            unitNetObj.Spawn();
            unitRef[i] = unitNetObj;
        }

        Boat boat = PrefabFactory.CreateBoat();
        boat.GetComponent<NetworkObject>().Spawn();

        SendSpawnedUnitsClientRpc(clientId, playerId, networkGroup.GetComponent<NetworkObject>(), boat.GetComponent<NetworkObject>(), unitRef);
    }

    [ClientRpc]
    public void SendSpawnedUnitsClientRpc (ulong clientId, int playerId, NetworkObjectReference networkGroup, NetworkObjectReference networkBoat, NetworkObjectReference[] networkUnits) {
        if (clientId != Game.networkManager.LocalClientId) {
            return;
        }
        if (this.playerId != playerId) {
            return;
        }

        if (waitingGroup != null) {
            waitingGroup.FinishOnlineSpawn(networkGroup, networkUnits);
        }
        if (waitingPlayer != null) {
            waitingPlayer.FinishOnlineBoat(networkBoat, waitingGroup);
        }

        waitingGroup = null;
        waitingPlayer = null;
    }

    [ServerRpc]
    public void SpawnBoatServerRpc (ulong clientId) {

    }

    [ServerRpc]
    public void SetParentServerRpc (NetworkObjectReference obj, NetworkObjectReference newParent) {
        NetworkObject netObj = (NetworkObject)obj;
        NetworkObject netParent = (NetworkObject)newParent;

        netObj.transform.SetParent(netParent.transform);
    }

    [ServerRpc]
    public void UnsetParentServerRpc (NetworkObjectReference obj) {
        NetworkObject netObj = (NetworkObject)obj;

        netObj.transform.SetParent(null);
    }

    [ServerRpc]
    public void SetPositionServerRpc (NetworkObjectReference obj, Vector3 pos) {
        NetworkObject netObj = (NetworkObject)obj;

        netObj.transform.position = pos;
    }

    [ServerRpc]
    public void SetRotationServerRpc (NetworkObjectReference obj, Vector3 rot) {
        NetworkObject netObj = (NetworkObject)obj;

        netObj.transform.eulerAngles = rot;
    }


    // Put handlers for incoming network messages in here
    private void InitMessageHandlers () {
        
    }
}


















