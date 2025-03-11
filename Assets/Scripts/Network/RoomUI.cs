using Mirror;
using UnityEngine;

public class RoomUI : MonoBehaviour
{
    public MyNetworkRoomManager networkManager;
    public RoomPlayer _localPlayer;

    public void OnHostButtonClicked()
    {
        networkManager.StartHost();
    }

    public void OnJoinButtonClicked()
    {
        networkManager.StartClient();
    }

    public void OnStartGameButtonClicked()
    {
        if (NetworkServer.active)
        {
            networkManager.ServerChangeScene(networkManager.GameplayScene);
        }
    }
    public void CreateRoom()
    {
        var manager = MyNetworkRoomManager.singleton;

        manager.StartHost();
    }
}