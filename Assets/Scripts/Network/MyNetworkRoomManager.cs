using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class MyNetworkRoomManager : NetworkRoomManager
{
    public bool _canStartGame = true;


    public override void OnRoomServerConnect(NetworkConnectionToClient conn)
    {
        base.OnRoomServerConnect(conn);
        Debug.Log($"Jogador {conn.connectionId} conectou à sala.");
    }

    public override void OnRoomServerDisconnect(NetworkConnectionToClient conn)
    {
        base.OnRoomServerDisconnect(conn);
        Debug.Log($"Jogador {conn.connectionId} desconectou da sala.");
    }

    public override void OnRoomServerPlayersReady()
    {
        Debug.Log("Todos os jogadores estão prontos. Iniciando o jogo...");
        base.OnRoomServerPlayersReady();        
    }
}
