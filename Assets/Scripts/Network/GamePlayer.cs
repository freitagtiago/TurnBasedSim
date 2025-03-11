using Mirror;
using Newtonsoft.Json;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class GamePlayer : NetworkBehaviour
{
    public Character[] _characters;

    private void Start()
    {
        if (isLocalPlayer)
        {
            _characters = new Character[4];
            BattleCharacter[] battleChar = new BattleCharacter[4];
            for (int i = 0; i < 4; i++)
            {
                battleChar[i] = new BattleCharacter();
                battleChar[i] = JsonConvert.DeserializeObject<BattleCharacter>(PlayerPrefs.GetString("partymember" + i));

                _characters[i] = new Character();
                _characters[i].SetupCharacter(battleChar[i].GetBaseCharacter(), 0);
                _characters[i]._skillList = battleChar[i]._skillList;
                _characters[i]._itemsList = battleChar[i]._itemsList;
            }

            FindObjectOfType<TurnBasedNetworkSystem>().CmdRegisterParty(battleChar.ToList(), netId);
        }
    }
}
