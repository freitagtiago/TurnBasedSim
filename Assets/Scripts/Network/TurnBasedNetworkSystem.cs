using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public enum State
{
    STARTING
    , SIDE_A
    , SIDE_B
    , WON
    , LOST
}

public class TurnBasedNetworkSystem : NetworkBehaviour
{
    [SerializeField] private TurnBasedSystemNetworkUI _uiHandler;

    [Header("Combat")]
    private List<Character> _actionOrder = new List<Character>();
    private NetworkConnectionToClient _sideAConn;
    private NetworkConnectionToClient _sideBConn;
    [SyncVar]
    private int _currentTurn = -1;

    [Header("Side A")]
    private Character[] _sideA;
    private List<BattleCharacter> _battleCharacterSideA = new List<BattleCharacter>();

    [Header("Side B")]
    private Character[] _sideB;
    private List<BattleCharacter> _battleCharacterSideB = new List<BattleCharacter>();

    public State _currentBattleState = State.STARTING;

    private void Start()
    {
        _sideA = null;
        _sideB = null;
    }

    [Command(requiresAuthority = false)]
    public void CmdRegisterParty(List<BattleCharacter> battleCharacter, uint clientNetId)
    {
        if (NetworkServer.spawned.TryGetValue(clientNetId, out NetworkIdentity identity))
        {
            NetworkConnectionToClient clientConnection = identity.connectionToClient;

            if (_sideA == null)
            {
                _sideA = new Character[4];
                _battleCharacterSideA = battleCharacter;
                _sideAConn = clientConnection;
                for (int i = 0; i < 4; i++)
                {
                    _sideA[i] = new Character();
                    _sideA[i].SetupCharacter(battleCharacter[i].GetBaseCharacter(), 0);
                    _sideA[i]._skillList = battleCharacter[i]._skillList;
                    _sideA[i]._itemsList = battleCharacter[i]._itemsList;
                }
            }
            else
            {
                _sideB = new Character[4];
                _battleCharacterSideB = battleCharacter;
                _sideBConn = clientConnection;
                for (int i = 0; i < 4; i++)
                {
                    _sideB[i] = new Character();
                    _sideB[i].SetupCharacter(battleCharacter[i].GetBaseCharacter(), 0);
                    _sideB[i]._skillList = battleCharacter[i]._skillList;
                    _sideB[i]._itemsList = battleCharacter[i]._itemsList;
                }
            }

            if (CanStartBattle())
            {
                RpcBuildUIOnBattleInstances(_battleCharacterSideA, _battleCharacterSideB);
                DefineOrder();
                RpcStartBattle();
                AdvanceTurn();
            }
        }
    }

    public bool CanStartBattle()
    {
        if (_sideA != null 
            && _sideB != null)
        {
            return true;
        }
        return false;
    }

    private void DefineOrder()
    {
        List<BattleCharacter> actionOrderBattleChar = new List<BattleCharacter>();
        if (_sideA[0].GetStat(Stats.Speed) > _sideB[0].GetStat(Stats.Speed))
        {
            actionOrderBattleChar.Add(_battleCharacterSideA[0]);
            actionOrderBattleChar.Add(_battleCharacterSideA[1]);
            actionOrderBattleChar.Add(_battleCharacterSideA[2]);
            actionOrderBattleChar.Add(_battleCharacterSideA[3]);
            actionOrderBattleChar.Add(_battleCharacterSideB[0]);
            actionOrderBattleChar.Add(_battleCharacterSideB[1]);
            actionOrderBattleChar.Add(_battleCharacterSideB[2]);
            actionOrderBattleChar.Add(_battleCharacterSideB[3]);

            _actionOrder.Add(_sideA[0]);
            _actionOrder.Add(_sideA[1]);
            _actionOrder.Add(_sideA[2]);
            _actionOrder.Add(_sideA[3]);
            _actionOrder.Add(_sideB[0]);
            _actionOrder.Add(_sideB[1]);
            _actionOrder.Add(_sideB[2]);
            _actionOrder.Add(_sideB[3]);
        }
        else
        {
            actionOrderBattleChar.Add(_battleCharacterSideB[0]);
            actionOrderBattleChar.Add(_battleCharacterSideB[1]);
            actionOrderBattleChar.Add(_battleCharacterSideB[2]);
            actionOrderBattleChar.Add(_battleCharacterSideB[3]);
            actionOrderBattleChar.Add(_battleCharacterSideA[0]);
            actionOrderBattleChar.Add(_battleCharacterSideA[1]);
            actionOrderBattleChar.Add(_battleCharacterSideA[2]);
            actionOrderBattleChar.Add(_battleCharacterSideA[3]);

            _actionOrder.Add(_sideB[0]);
            _actionOrder.Add(_sideB[1]);
            _actionOrder.Add(_sideB[2]);
            _actionOrder.Add(_sideB[3]);
            _actionOrder.Add(_sideA[0]);
            _actionOrder.Add(_sideA[1]);
            _actionOrder.Add(_sideA[2]);
            _actionOrder.Add(_sideA[3]);
        }

        RpcDefineOrderOnClient(actionOrderBattleChar);
    }

    private void AdvanceTurn()
    {
        _currentTurn++;

        bool turnDefined = false;

        while (!turnDefined)
        {
            if (_currentTurn >= _actionOrder.Count)
            {
                _currentTurn = 0;
            }

            if(_actionOrder[_currentTurn]._currentHP > 0)
            {
                turnDefined = true;
            }
            else
            {
                _currentTurn++;
            }
        }

        if (_actionOrder[_currentTurn]._side == 0)
        {
            RpcPlayTurn(_sideAConn);
            RpcWaitTurn(_sideBConn);
        }
        else
        {
            RpcPlayTurn(_sideBConn);
            RpcWaitTurn(_sideAConn);
        }
    }

    [ClientRpc]
    private void RpcDefineOrderOnClient(List<BattleCharacter> battleCharacterOrder)
    {
        if (_battleCharacterSideA[0]._baseCharacterSOName == battleCharacterOrder[0]._baseCharacterSOName)
        {
            _actionOrder.Add(_sideA[0]);
            _actionOrder.Add(_sideA[1]);
            _actionOrder.Add(_sideA[2]);
            _actionOrder.Add(_sideA[3]);
            _actionOrder.Add(_sideB[0]);
            _actionOrder.Add(_sideB[1]);
            _actionOrder.Add(_sideB[2]);
            _actionOrder.Add(_sideB[3]);
        }
        else
        {
            _actionOrder.Add(_sideB[0]);
            _actionOrder.Add(_sideB[1]);
            _actionOrder.Add(_sideB[2]);
            _actionOrder.Add(_sideB[3]);
            _actionOrder.Add(_sideA[0]);
            _actionOrder.Add(_sideA[1]);
            _actionOrder.Add(_sideA[2]);
            _actionOrder.Add(_sideA[3]);
        }
    }

    [ClientRpc]
    public void RpcBuildUIOnBattleInstances(List<BattleCharacter> sideA, List<BattleCharacter> sideB)
    {
        _sideA = new Character[4];
        _battleCharacterSideA = sideA;

        for (int i = 0; i < 4; i++)
        {
            _sideA[i] = new Character();
            _sideA[i].SetupCharacter(sideA[i].GetBaseCharacter(), 0);
            _sideA[i]._skillList = sideA[i]._skillList;
            _sideA[i]._itemsList = sideA[i]._itemsList;

            _uiHandler.SetupCharacterSlot(_sideA[i], i, 0);
        }

        _sideB = new Character[4];
        _battleCharacterSideB = sideB;
        for (int i = 0; i < 4; i++)
        {
            _sideB[i] = new Character();
            _sideB[i].SetupCharacter(sideB[i].GetBaseCharacter(), 0);
            _sideB[i]._skillList = sideB[i]._skillList;
            _sideB[i]._itemsList = sideB[i]._itemsList;

            _uiHandler.SetupCharacterSlot(_sideB[i], i, 1);
        }
    }

    [TargetRpc]
    private void RpcPlayTurn(NetworkConnection target)
    {
        Debug.Log("SUA VEZ");
        _uiHandler.SetupPlayerAction(_actionOrder[_currentTurn]);
    }

    [TargetRpc]
    private void RpcWaitTurn(NetworkConnection target)
    {
        _uiHandler.SetupDialoguePanel($"Turno do inimigo", null);
        Debug.Log("VEZ DO INIMIGO");
    }

    [ClientRpc]
    public void RpcStartBattle()
    {
        string initialSide = _actionOrder[0]._side == 0 ? "esquerdo" : "direito";
        _uiHandler.SetupDialoguePanel($"A batalha irá iniciar. O lado {initialSide} é o primeiro a atacar.", null);
    }

    [Command(requiresAuthority = false)]
    public void ProcessAction()
    {

    }

    [ClientRpc]
    public void RpcAdvanceTurn()
    {

    }

    public void CheckEndBattleCondition()
    {

    }
}
