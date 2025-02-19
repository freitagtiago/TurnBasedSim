using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading;
using Unity.Mathematics;
using UnityEditor.Experimental.GraphView;
using UnityEditor.VersionControl;
using UnityEngine;
using UnityEngine.TextCore.Text;
using static UnityEngine.GraphicsBuffer;
using Random = UnityEngine.Random;

public class TurnBasedSystem : MonoBehaviour
{
    public static TurnBasedSystem Instance;

    public GameObject _startButton;
    [SerializeField] private TurnBasedSystemUI _battleUI;
    [SerializeField] private CharacterUI _characterUIPrefab;

    public CharacterSO[] _charactersSideASO = new CharacterSO[4];
    public CharacterSO[] _charactersSideBSO = new CharacterSO[4];

    public Character[] _charactersSideA = new Character[4];
    public Character[] _charactersSideB = new Character[4];

    private List<Character> _actionOrder = new List<Character>();

    public bool _onBattle = false;

    [SerializeField] private int _currentTurn = -1;

    public SkillSO _selectedSkill { get; private set; }
    public ItemSO _selectedItem { get; private set; }
    public Character _selectedCharacter { get; private set; }

    [Header("Combat Modifiers")]
    [SerializeField] private float _exaustedPenalization = 0.3f;
    [SerializeField] private float _defensePenalization = 2f;
    [SerializeField] private float _criticalBonus = 1.3f;
    [SerializeField] private float _criticalFator = 1.5f;
    [SerializeField] private float _stabBonus = 1.3f;
    [SerializeField] private float _weaponDamageBonus = 0.3f;
    [SerializeField] private float _minimumDamageVariation = 0.3f;
    [SerializeField] private float _maximumDamageVariation = 1f;
    [SerializeField] private int _minimumDamage = 15;
    [SerializeField] private int _maximumDamage = 9999;
    [SerializeField] public float _baseStageMofidier { get; private set; } = 12.5f;

    [Header("Matchups")]
    public List<TypeMatchup> _typeMatchupList = new List<TypeMatchup>();

    [SerializeField] public int _maxActionPoints { get; private set; } = 200;
    [Header("Special Action")]
    [SerializeField] private float _bluffFactor = 0.5f;
    [SerializeField] private float _fullSpecialBarFactor = 0.5f;
    [SerializeField] public int _currentActionPointsSideA { get; private set; } = 0;
    [SerializeField] public int _currentActionPointsSideB { get; private set; } = 0;
    [SerializeField] private float _actionPointsAttacking = 10;
    [SerializeField] private float _actionPointsDefending = 3;
    [SerializeField] private float _actionPointsBeingAttacked = 5;

    public Character _characterCharging;
    public bool _isCharging { get; private set; } = false;
    public bool _isBluffing { get; private set; } = false;
    public int _deceived { get; private set; } = 0;


    private void Awake()
    {
        if(Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(this.gameObject);
        }
    }

    public void LoadCharacters()
    {
        for(int i = 0; i < _charactersSideASO.Length; i++)
        {
            Character character = new Character();
            character.SetupCharacter(_charactersSideASO[i], 0);
            _charactersSideA[i] = character;
            _battleUI.SetupCharacterSlot(_charactersSideA[i], i, 0);
        }

        for (int i = 0; i < _charactersSideBSO.Length; i++)
        {
            Character character = new Character();
            character.SetupCharacter(_charactersSideBSO[i], 1);
            _charactersSideB[i] = character;
            _battleUI.SetupCharacterSlot(_charactersSideB[i], i, 1);
        }
    }

    public void StartBattle()
    {
        if (_onBattle)
        {
            return;
        }
        _onBattle = true;
        _startButton.SetActive(false);
        _battleUI.UpdateActionPointsUI();

        LoadCharacters();
        DefineOrder();
        StartCoroutine(AdvanceTurn());
    }

    public bool CheckCondition()
    {
        bool someoneIsAlive = false;
        foreach(Character character in _charactersSideA)
        {
            if (character._currentHP > 0)
            {
                someoneIsAlive = true;
                break;
            }
        }

        if (!someoneIsAlive)
        {
            return true;
        }
        else
        {
            someoneIsAlive = false;
        }

        foreach (Character character in _charactersSideB)
        {
            if (character._currentHP > 0)
            {
                someoneIsAlive = true;
                break;
            }
        }

        if (!someoneIsAlive)
        {
            return true;
        }
        else
        {
            return false;
        }
    }

    public IEnumerator EndBattle()
    {
        string message = "";
        if(GetCurrentActor()._side == 0)
        {
            message = "A batalha foi encerrada, a equipe A foi vitoriosa.";
        }
        else
        {
            message = "A batalha foi encerrada, a equipe B foi vitoriosa.";
        }
        _battleUI.SetupDialoguePanel(message, null);
        yield return new WaitForSecondsRealtime(3f);
        _onBattle = false;
        _charactersSideA = new Character[4];
        _charactersSideB = new Character[4];
        _startButton.SetActive(true);
        _battleUI.EndBattle();
    }

    public IEnumerator AdvanceTurn()
    {
        if (_onBattle)
        {
            if (CheckCondition())
            {
                StartCoroutine(EndBattle());
            }
            else
            {
                bool currentActorDefined = false;
                int iterations = 0;
                while (!currentActorDefined
                    || iterations > 15)
                {
                    iterations++;
                    _currentTurn++;
                    if (_currentTurn >= _actionOrder.Count)
                    {
                        _currentTurn = 0;
                    }

                    if (_isCharging
                        && _characterCharging == _actionOrder[_currentTurn])
                    {
                        if (!HandleSpecialAction())
                        {
                            string message = "Nenhum inimigo caiu no blefe, o golpe foi cancelado.";
                            _battleUI.SetupDialoguePanel(message, () => { _battleUI.CloseDialoguePanel(); });
                        }
                        currentActorDefined = true;
                        yield return new WaitForSeconds(3f);
                        if (CheckCondition())
                        {
                            StartCoroutine(EndBattle());
                            break;
                        }
                    }
                    else if (_isCharging
                        && _characterCharging._side == _actionOrder[_currentTurn]._side
                        && _characterCharging != _actionOrder[_currentTurn])
                    {
                        continue;
                    }
                    else
                    {
                        if (_isCharging
                            && _characterCharging._currentHP <= 0)
                        {
                            _isCharging = false;
                            _characterCharging = null;
                        }

                        if (_actionOrder[_currentTurn]._currentHP > 0)
                        {
                            _actionOrder[_currentTurn].HandleStatModifier();
                            if (!_actionOrder[_currentTurn].HandleStatusCondition())
                            {
                                currentActorDefined = true;

                                if (_actionOrder[_currentTurn]._currentStatusCondition == StatusCondition.Poisoned
                                    || _actionOrder[_currentTurn]._currentStatusCondition == StatusCondition.Burned)
                                {
                                    string message = $"{_actionOrder[_currentTurn]._name} está sofrendo os efeitos de {_actionOrder[_currentTurn]._currentStatusCondition.ToString()}, irá se recuperar em {_actionOrder[_currentTurn]._remainingTurnsStatusCondition} turno(s).";

                                    _battleUI.SetupDialoguePanel(message, () => { _battleUI.CloseDialoguePanel(); });
                                    yield return new WaitForSeconds(3f);
                                }

                                UpdateCharacterSlotUI();
                            }
                            else
                            {
                                string message = "";
                                _actionOrder[_currentTurn]._inDefensiveState = false;

                                if (_actionOrder[_currentTurn]._currentStatusCondition != StatusCondition.Freezed)
                                {
                                    message = $"{_actionOrder[_currentTurn]._name} está {_actionOrder[_currentTurn]._currentStatusCondition.ToString()}, irá se recuperar em {_actionOrder[_currentTurn]._remainingTurnsStatusCondition} turno(s).";
                                }
                                else
                                {
                                    message = $"{_actionOrder[_currentTurn]._name} está {_actionOrder[_currentTurn]._currentStatusCondition.ToString()}, pode se recuperar a qualquer momento.";
                                }
                                _battleUI.SetupDialoguePanel(message, () => { _battleUI.CloseDialoguePanel(); });
                                yield return new WaitForSeconds(3f);
                            }
                        }
                    }
                }

                if (iterations > 15)
                {
                    Debug.LogError("INFINITE LOOP");
                }
                SetTurnAction();
            }
        }
    }

    private void SetTurnAction()
    {
        _actionOrder[_currentTurn]._inDefensiveState = false;
        Character currentActor = _actionOrder[_currentTurn];
        if (currentActor._side == 0)
        {
            //PLAYER
            _battleUI.SetupPlayerAction(currentActor);
        }
        else
        {
            //ENEMY
            EnemyRoutine();
        }
    }

    public void NextAction()
    {
        if (!_onBattle)
        {
            StartBattle();
        }
        else
        {
            StartCoroutine(AdvanceTurn());
        }
    }

    private void DefineOrder()
    {
        if (_charactersSideA[0].GetStat(Stats.Speed) > _charactersSideB[0].GetStat(Stats.Speed))
        {
            _actionOrder.Add(_charactersSideA[0]);
            _actionOrder.Add(_charactersSideA[1]);
            _actionOrder.Add(_charactersSideA[2]);
            _actionOrder.Add(_charactersSideA[3]);
            _actionOrder.Add(_charactersSideB[0]);
            _actionOrder.Add(_charactersSideB[1]);
            _actionOrder.Add(_charactersSideB[2]);
            _actionOrder.Add(_charactersSideB[3]);
        }
        else
        {
            _actionOrder.Add(_charactersSideB[0]);
            _actionOrder.Add(_charactersSideB[1]);
            _actionOrder.Add(_charactersSideB[2]);
            _actionOrder.Add(_charactersSideB[3]);
            _actionOrder.Add(_charactersSideA[0]);
            _actionOrder.Add(_charactersSideA[1]);
            _actionOrder.Add(_charactersSideA[2]);
            _actionOrder.Add(_charactersSideA[3]);
        }
    }

    public Character GetCurrentActor()
    {
        return _actionOrder[_currentTurn];
    }

    public void SetBasicAction(bool isBasicAttack)
    {
        if (isBasicAttack)
        {
            _selectedSkill = GetCurrentActor()._equipment._basicSkill;
            _battleUI.SetupTargetPanel(_selectedSkill);
        }
        else //Defend
        {
            _battleUI.CloseTargetPanel();
            string message = $"{GetCurrentActor()._name} se defendeu";
            _selectedSkill = null;
            _actionOrder[_currentTurn].RecoverSP((int)(_actionOrder[_currentTurn]._maxSP * 0.2f));
            _actionOrder[_currentTurn]._inDefensiveState = true;
            UpdateTargetCharacterSlotUI(_actionOrder[_currentTurn]._side, _actionOrder[_currentTurn]);
            _battleUI.SetupDialoguePanel(message, () => { StartCoroutine(AdvanceTurn()); });
        }
    }

    public void SetSelectedSkill(SkillSO skill)
    {
        _selectedSkill = skill;

        if (!skill._affectAll)
        {
            _battleUI.SetupTargetPanel(skill);
        }
        else
        {
            ApplySkillOnAllTargets();
        }
    }

    public void SetSelectedItem(ItemSO item)
    {
        _selectedItem = item;

        if (!item._affetEntireParty)
        {
            _battleUI.SetupTargetItemPanel(item);
        }
        else
        {
            ApplyItemOnAllTargets();
        }
    }

    public void ApplySkillOnTarget(Character target)
    {
        _battleUI.CloseTargetPanel();
        GetCurrentActor().ReduceSP(_selectedSkill._cost);
        UpdateCharacterSlotUI();
        string message = CalculateAndApplyDamage(target);
        UpdateTargetCharacterSlotUI(target._side, target);
        _selectedSkill = null;
        _battleUI.SetupDialoguePanel(message, () => { StartCoroutine(AdvanceTurn()); });
    }

    public void ApplySkillOnAllTargets(string specialMessage = "")
    {
        _battleUI.CloseTargetPanel();

        string message = specialMessage == "" ? $"{GetCurrentActor()._name} usou a habilidade {_selectedSkill._name} em todos" : specialMessage;

        CalculateAndApplyDamage(null);
        UpdateTargetCharacterSlotUI(0, null);
        if (_selectedSkill._isSpecialSkill)
        {
            _battleUI.SetupDialoguePanel(message, () => {
                                                            _currentTurn--;
                                                            StartCoroutine(AdvanceTurn()); });
        }
        else
        {
            _battleUI.SetupDialoguePanel(message, () => { StartCoroutine(AdvanceTurn()); });
        }
        _selectedSkill = null;
    }

    public void ApplyItemOnAllTargets()
    {
        _battleUI.CloseTargetPanel();
        string message = $"{GetCurrentActor()._name} usou item {_selectedItem._name} em todos";

        if(GetCurrentActor()._side == 0)
        {
            foreach(Character character in _charactersSideA)
            {
                character.ApplyItem(_selectedItem);
                UpdateTargetCharacterSlotUI(character._side, character);
            }
        }
        else
        {
            foreach (Character character in _charactersSideB)
            {
                character.ApplyItem(_selectedItem);
                UpdateTargetCharacterSlotUI(character._side, character);
            }
        }
        ReduceItem();

        _battleUI.SetupDialoguePanel(message, () => { StartCoroutine(AdvanceTurn()); });
    }

    public void ApplyItemOnTarget(Character target)
    {
        _battleUI.CloseTargetPanel();
        string message = "";
        
        message = $"{GetCurrentActor()._name} usou o item {_selectedItem._name} em {target._name}";

        target.ApplyItem(_selectedItem);
        UpdateTargetCharacterSlotUI(target._side, target);
        ReduceItem();

        _battleUI.SetupDialoguePanel(message, () => { StartCoroutine(AdvanceTurn()); });
    }

    private void EnemyRoutine()
    {
        Character character = GetCurrentActor();
        Character target = null;
        if (_isCharging
            && _characterCharging._side != character._side)
        {
            if (Random.Range(0, 101) > 50)
            {
                SetBasicAction(false);
                _battleUI.SetupEnemyAction(GetCurrentActor());
                return;
            }
        }
        else if (_currentActionPointsSideB >= 100
            && Array.IndexOf(_charactersSideB,character) == 0)
        {
            int specialActionChance = _currentActionPointsSideB == 200 ? 90 : 60;

            if (Random.Range(0,101) < specialActionChance)
            {//Use Special
                bool isBluffing = Random.Range(0, 101) <= 50;

                SetSpecialAction(isBluffing);
                return;
            }
        }

        bool useItem = false;
        foreach(Character partyMember in _charactersSideB)
        {
            if (useItem)
            {
                break;
            }
            if(partyMember._currentStatusCondition != StatusCondition.None)
            {
                foreach(ItemSO item in character._itemsList)
                {
                    if(item._cureStatusCondition
                        && item._statusConditionToCure == partyMember._currentStatusCondition)
                    {
                        if(Random.Range(0,100) < 51)
                        {
                            target = item._affetEntireParty ? null : partyMember;
                            _selectedItem = item;
                            useItem = true;
                            break;
                        }
                    }
                }
            }
        }

        if (useItem)
        {
            if(target == null)
            {
                ApplyItemOnAllTargets();
            }
            else
            {
                ApplyItemOnTarget(target);
            }
            return;
        }

        // 0 - 30  Basic Attack
        // 31 - 75 Skill
        // 76 - 100 Defend

        int actionChance = Random.Range(0, 101);
        if (actionChance < 31)
        {
            target = EnemyBasicAttack(character, target);
        }
        else if (actionChance < 76)
        {
            target = EnemySkillAttack(character, target);
        }
        else
        {
            EnemyDefendAction();
        }
        _battleUI.SetupEnemyAction(GetCurrentActor());  
    }

    private void EnemyDefendAction()
    {
        SetBasicAction(false);
    }

    private Character EnemySkillAttack(Character character, Character target)
    {
        _selectedSkill = character._skillList[UnityEngine.Random.Range(0, character._skillList.Count - 1)];

        if(_selectedSkill._cost > character._currentSP)
        {
            if(Random.Range(0,101) > 50)
            {
                EnemyBasicAttack(character, target);
                return target;
            }
            else
            {
                EnemyDefendAction();
                return null;
            }
        }

        bool foundTarget = false;

        if (_selectedSkill is HealingSkillSO)
        {
            if (_selectedSkill._affectAll)
            {
                ApplySkillOnAllTargets();
            }
            else
            {
                while (!foundTarget)
                {
                    int index = Random.Range(0, 3);
                    if (_charactersSideB[index]._currentHP > 0)
                    {
                        target = _charactersSideB[index];
                        foundTarget = true;
                    }
                }
                ApplySkillOnTarget(target);
            }
        }
        else
        {
            if (_selectedSkill._affectAll)
            {
                ApplySkillOnAllTargets();
            }
            else
            {
                while (!foundTarget)
                {
                    int index = Random.Range(0, 3);
                    if (_charactersSideA[index]._currentHP > 0)
                    {
                        target = _charactersSideA[index];
                        foundTarget = true;
                    }
                }
                ApplySkillOnTarget(target);
            }
        }

        return target;
    }

    private Character EnemyBasicAttack(Character character, Character target)
    {
        _selectedSkill = character._equipment._basicSkill;
        bool foundTarget = false;
        while (!foundTarget)
        {
            int index = Random.Range(0, 3);
            if (_charactersSideA[index]._currentHP > 0)
            {
                target = _charactersSideA[index];
                foundTarget = true;
            }
        }
        ApplySkillOnTarget(target);
        return target;
    }

    private string CalculateAndApplyDamage(Character target)
    {
        int finalDamage = 0;
        bool isCritical = false;
        bool hasWeaponBonus = false;
        float matchupValue = 1;
        StatusCondition condition = StatusCondition.None;

        Character currentActor = GetCurrentActor();

        if (!_selectedSkill._isSpecialSkill)
        {
            if (currentActor._currentStatusCondition == StatusCondition.Blind)
            {
                if (Random.Range(0, 101) > 50)
                {
                    return SetMessage(0
                                    , false
                                    , true
                                    , condition
                                    , target
                                    , false
                                    , false
                                    , matchupValue);
                }
            }
            else if (currentActor._currentStatusCondition == StatusCondition.Confused)
            {
                if (Random.Range(0, 101) > 50)
                {
                    currentActor.ApplyDamage(currentActor._equipment._basicSkill._baseForce * -1);
                    if (currentActor._side == 0)
                    {
                        _battleUI.UpdateCharacterSlotUI(Array.IndexOf(_charactersSideA, currentActor), 0);
                    }
                    else
                    {
                        _battleUI.UpdateCharacterSlotUI(Array.IndexOf(_charactersSideB, currentActor), 1);
                    }
                    return SetMessage(currentActor._equipment._basicSkill._baseForce
                                       , false
                                       , false
                                       , condition
                                       , target
                                       , false
                                       , true
                                       , matchupValue);
                }
            }
        }
        

        if (_selectedSkill is HealingSkillSO)
        {
            if (!_selectedSkill._affectAll)
            {
                HealingSkillSO healingSkill = _selectedSkill as HealingSkillSO;

                int cureValue = (int)(target._maxHP * healingSkill._cureValue);

                target.ApplyDamage(cureValue);

                if (healingSkill._removeDebuffs
                            || target._currentHP > 0)
                {
                    target.RemoveAllDebuffs();
                }

                if (healingSkill._cureStatusCondition
                    || target._currentHP > 0)
                {
                    target.ApplyStatusCondition(StatusCondition.None);
                }

                _battleUI.UpdateCharacterSlotUI(Array.IndexOf(target._side == 0 ? _charactersSideA : _charactersSideB, target), 0);
            }
            else
            {
                HealingSkillSO healingSkill = _selectedSkill as HealingSkillSO;

                if (currentActor._side == 0)
                {
                    foreach (Character character in _charactersSideA)
                    {
                        int cureValue = (int)(character._maxHP * healingSkill._cureValue);
                        if (healingSkill._canRevive
                            || character._currentHP > 0)
                        {
                            character.ApplyDamage(cureValue);
                        }

                        if (healingSkill._removeDebuffs
                            || character._currentHP > 0)
                        {
                            character.RemoveAllDebuffs();
                        }

                        if (healingSkill._cureStatusCondition
                            || character._currentHP > 0)
                        {
                            character.ApplyStatusCondition(StatusCondition.None);
                        }

                        _battleUI.UpdateCharacterSlotUI(Array.IndexOf(_charactersSideA, character), 0);
                    }
                }
                else
                {
                    foreach (Character character in _charactersSideB)
                    {
                        int cureValue = (int)(character._maxHP * healingSkill._cureValue);
                        if (healingSkill._canRevive
                            || character._currentHP > 0)
                        {
                            character.ApplyDamage(cureValue);
                        }

                        if (healingSkill._removeDebuffs
                            || character._currentHP > 0)
                        {
                            character.RemoveAllDebuffs();
                        }

                        if (healingSkill._cureStatusCondition
                            || character._currentHP > 0)
                        {
                            character.ApplyStatusCondition(StatusCondition.None);
                        }

                        _battleUI.UpdateCharacterSlotUI(Array.IndexOf(_charactersSideB, character), 1);
                    }
                }
            }
        }
        else
        {
            if (!_selectedSkill._isSpecialSkill)
            {
                if (CheckIfAttackMissed())
                {
                    return SetMessage(0
                                       , false
                                       , true
                                       , condition
                                       , target
                                       , false
                                       , false
                                       , matchupValue);
                }
            }

            if (_selectedSkill is StatSkillSO)
            {
                if (!_selectedSkill._affectAll)
                {
                    target.ApplyStatModifier((_selectedSkill as StatSkillSO)._statModifier);
                }
                else
                {
                    if (currentActor._side == 0)
                    {
                        foreach (Character character in _charactersSideB)
                        {
                            if (CheckIfAttackMissed())
                            {
                                if (character._currentHP > 0)
                                {
                                    character.ApplyStatModifier((_selectedSkill as StatSkillSO)._statModifier);
                                    _battleUI.UpdateCharacterSlotUI(Array.IndexOf(_charactersSideB, character), 1);
                                }
                            }
                        }
                    }
                    else
                    {
                        foreach (Character character in _charactersSideA)
                        {
                            if (CheckIfAttackMissed())
                            {
                                if (character._currentHP > 0)
                                {
                                    character.ApplyStatModifier((_selectedSkill as StatSkillSO)._statModifier);
                                    _battleUI.UpdateCharacterSlotUI(Array.IndexOf(_charactersSideA, character), 0);
                                }
                            }
                        }
                    }
                }
            }
            else if (_selectedSkill is StatusConditionSkillSO)
            {
                if (!_selectedSkill._affectAll)
                {
                    if (target._currentStatusCondition == StatusCondition.None)
                    {
                        target.ApplyStatusCondition((_selectedSkill as StatusConditionSkillSO)._statusCondition);
                        condition = target._currentStatusCondition;
                        if (target._side == 0)
                        {
                            _battleUI.UpdateCharacterSlotUI(Array.IndexOf(_charactersSideA, target), 0);
                        }
                        else
                        {
                            _battleUI.UpdateCharacterSlotUI(Array.IndexOf(_charactersSideB, target), 1);
                        }
                    }
                }
                else
                {
                    if (currentActor._side == 0)
                    {
                        foreach (Character character in _charactersSideB)
                        {
                            if (character._currentHP > 0
                                && character._currentStatusCondition != StatusCondition.None)
                            {
                                character.ApplyStatusCondition((_selectedSkill as StatusConditionSkillSO)._statusCondition);
                                _battleUI.UpdateCharacterSlotUI(Array.IndexOf(_charactersSideB, character), 1);
                            }
                        }
                    }
                    else
                    {
                        foreach (Character character in _charactersSideA)
                        {
                            if (character._currentHP > 0
                                && character._currentStatusCondition != StatusCondition.None)
                            {
                                if (!CheckIfAttackMissed())
                                {
                                    character.ApplyStatusCondition((_selectedSkill as StatusConditionSkillSO)._statusCondition);
                                    _battleUI.UpdateCharacterSlotUI(Array.IndexOf(_charactersSideA, character), 0);
                                }
                            }
                        }
                    }
                }
            }
            else //Physical or Magical
            {
                int attackerStat = 0;
                int defenderStat = 0;

                if (target == null)
                {
                    if (currentActor._side == 0)
                    {
                        if (_selectedSkill is PhysicalSkillSO)
                        {
                            attackerStat = currentActor.GetStat(Stats.PhysicalAttack);
                        }
                        else
                        {
                            attackerStat = currentActor.GetStat(Stats.MagicalAttack);
                        }

                        foreach (Character character in _charactersSideB)
                        {
                            if (_selectedSkill is PhysicalSkillSO)
                            {
                                defenderStat = character.GetStat(Stats.PhysicalDefense);
                            }
                            else
                            {
                                defenderStat = character.GetStat(Stats.MagicalDefense);
                            }

                            if (CheckIfIsCritical(currentActor.GetStat(Stats.Speed)
                                , currentActor.GetStat(Stats.Luck)
                                , character.GetStat(Stats.Speed)))
                            {
                                isCritical = true;
                            }

                            float weaponBonus = GetBonusByWeaponDamageType(character);
                            hasWeaponBonus = weaponBonus > 1f;

                            if (_selectedSkill._causeStatusCondition
                                && Random.Range(0, 101) < _selectedSkill._statusConditionChance)
                            {
                                condition = _selectedSkill._statusCondition;
                                character.ApplyStatusCondition(condition);
                            }

                            if (_selectedSkill._applyStatModifier
                                && Random.Range(0, 101) < _selectedSkill._modifierChance)
                            {
                                character.ApplyStatModifier(_selectedSkill._statModifier);
                            }

                            GetTypeMatchupBonus(_selectedSkill._type, character._type, out matchupValue);
                            finalDamage = GetDamage(attackerStat
                                                    , defenderStat
                                                    , _selectedSkill._baseForce
                                                    , isCritical
                                                    , character._inDefensiveState
                                                    , weaponBonus
                                                    , CheckIfIsStab(currentActor._type, _selectedSkill._type)
                                                    , matchupValue);
                            character.ApplyDamage(finalDamage * -1);
                            _battleUI.UpdateCharacterSlotUI(Array.IndexOf(_charactersSideB, character), 1);
                        }
                    }
                    else
                    {
                        if (_selectedSkill is PhysicalSkillSO)
                        {
                            attackerStat = currentActor.GetStat(Stats.PhysicalAttack);
                        }
                        else
                        {
                            attackerStat = currentActor.GetStat(Stats.MagicalAttack);
                        }

                        foreach (Character character in _charactersSideA)
                        {
                            if (_selectedSkill is PhysicalSkillSO)
                            {
                                defenderStat = character.GetStat(Stats.PhysicalDefense);
                            }
                            else
                            {
                                defenderStat = character.GetStat(Stats.MagicalDefense);
                            }

                            if (CheckIfIsCritical(currentActor.GetStat(Stats.Speed)
                                , currentActor.GetStat(Stats.Luck)
                                , character.GetStat(Stats.Speed)))
                            {
                                isCritical = true;
                            }
                            float weaponBonus = GetBonusByWeaponDamageType(character);
                            hasWeaponBonus = weaponBonus > 1f;

                            if (_selectedSkill._causeStatusCondition
                                && Random.Range(0, 101) < _selectedSkill._statusConditionChance)
                            {
                                condition = _selectedSkill._statusCondition;
                                character.ApplyStatusCondition(condition);
                            }
                            if (_selectedSkill._applyStatModifier
                                 && Random.Range(0, 101) < _selectedSkill._modifierChance)
                            {
                                character.ApplyStatModifier(_selectedSkill._statModifier);
                            }

                            GetTypeMatchupBonus(_selectedSkill._type, character._type, out matchupValue);

                            finalDamage = GetDamage(attackerStat
                                                    , defenderStat
                                                    , _selectedSkill._baseForce
                                                    , isCritical
                                                    , character._inDefensiveState
                                                    , weaponBonus
                                                    , CheckIfIsStab(currentActor._type, _selectedSkill._type)
                                                    , matchupValue);
                            character.ApplyDamage(finalDamage * -1);
                            _battleUI.UpdateCharacterSlotUI(Array.IndexOf(_charactersSideA, character), 0);
                        }
                    }
                }
                else
                {
                    if (_selectedSkill is PhysicalSkillSO)
                    {
                        attackerStat = currentActor.GetStat(Stats.PhysicalAttack);
                        defenderStat = target.GetStat(Stats.PhysicalDefense);
                    }
                    else
                    {
                        attackerStat = currentActor.GetStat(Stats.MagicalAttack);
                        defenderStat = target.GetStat(Stats.MagicalDefense);
                    }

                    if (CheckIfIsCritical(currentActor.GetStat(Stats.Speed)
                        , currentActor.GetStat(Stats.Luck)
                        , target.GetStat(Stats.Speed)))
                    {
                        isCritical = true;
                    }
                    float weaponBonus = GetBonusByWeaponDamageType(target);
                    hasWeaponBonus = weaponBonus > 1f;

                    if (_selectedSkill._causeStatusCondition
                                && Random.Range(0, 101) < _selectedSkill._statusConditionChance)
                    {
                        condition = _selectedSkill._statusCondition;
                        target.ApplyStatusCondition(condition);
                    }

                    if (_selectedSkill._applyStatModifier
                        && Random.Range(0, 101) < _selectedSkill._modifierChance)
                    {
                        target.ApplyStatModifier(_selectedSkill._statModifier);
                    }

                    GetTypeMatchupBonus(_selectedSkill._type, target._type, out matchupValue);
                    finalDamage = GetDamage(attackerStat
                                            , defenderStat
                                            , _selectedSkill._baseForce
                                            , isCritical
                                            , target._inDefensiveState
                                            , weaponBonus
                                            , CheckIfIsStab(currentActor._type, _selectedSkill._type)
                                            , matchupValue);
                    target.ApplyDamage(finalDamage * -1);
                    if (target._side == 0)
                    {
                        _battleUI.UpdateCharacterSlotUI(Array.IndexOf(_charactersSideA, target), 0);
                    }
                    else
                    {
                        _battleUI.UpdateCharacterSlotUI(Array.IndexOf(_charactersSideB, target), 1);
                    }
                }
            }
        }
        return SetMessage(finalDamage
                        , isCritical
                        , false
                        , condition
                        , target
                        , hasWeaponBonus
                        , false
                        , matchupValue);
    }

    private string SetMessage(int damage
                            , bool isCritical
                            , bool missed
                            , StatusCondition statusCondition
                            , Character target
                            , bool hasWeaponBonus
                            , bool confused
                            , float matchupValue)
    {
        string message = "";

        if(target != null)
        {
            if (_selectedSkill._isBasicSkill)
            {
                message = $"{GetCurrentActor()._name} usou o ataque básica em {target._name}." + Environment.NewLine;
            }
            else
            {
                message = $"{GetCurrentActor()._name} usou a habilidade {_selectedSkill._name} em {target._name}." + Environment.NewLine;
            }

            if (missed)
            {
                message += " O ataquer falhou." + Environment.NewLine;
            }
            else if (confused)
            {
                message += " Como estava confuso acabou atacando a si próprio." + Environment.NewLine;
                message += $" Causou {damage} de dano a si mesmo." + Environment.NewLine;
            }
            else
            {
                message += isCritical ? " Foi um ataque crítico!" + Environment.NewLine : "";
                message += _selectedSkill._baseForce > 0 ? $" Causou {damage} de dano." + Environment.NewLine : "";
                message += _selectedSkill is HealingSkillSO ? $" {target._name} teve seus pontos de vida curados." + Environment.NewLine : "";
                message += target._inDefensiveState && damage > 0 ? $" O dano foi reduzido pois estava se defendendo." + Environment.NewLine : "";
                message += statusCondition != StatusCondition.None ? $" O alvo ficou {statusCondition.ToString()}." + Environment.NewLine : "";
                message += hasWeaponBonus ? $"O golpe recebeu bonus da arma utilizada." + Environment.NewLine : "";

                if(damage > 0)
                {
                    switch (matchupValue)
                    {
                        case 0.75f:
                            message += $" O golpe foi pouco efetivo." + Environment.NewLine;
                            break;
                        case 1.5f:
                            message += $" O golpe foi muito efetivo." + Environment.NewLine;
                            break;
                        default:
                            break;
                    }
                }
            }
        }
        else
        {
            message = $"{GetCurrentActor()._name} usou a habilidade de área {_selectedSkill._name}." + Environment.NewLine;
        }
        return message;
    }

    private bool CheckIfIsCritical(int attackerSpeed, int attackerLucky, int defenderSpeed)
    {
        int criticalChance = Mathf.Clamp((int)Math.Round((attackerSpeed / defenderSpeed) * _criticalFator + attackerLucky * 0.1f), 0, 100);
        return Random.Range(0,101) <= criticalChance;
    }

    private bool CheckIfAttackMissed()
    {
        return Random.Range(0, 101) > _selectedSkill._accuracy;
    }

    private void UpdateCharacterSlotUI()
    {
        if (_actionOrder[_currentTurn]._side == 0)
        {
            _battleUI.UpdateCharacterSlotUI(Array.IndexOf(_charactersSideA, _actionOrder[_currentTurn]), 0);
        }
        else
        {
            _battleUI.UpdateCharacterSlotUI(Array.IndexOf(_charactersSideB, _actionOrder[_currentTurn]), 1);
        }
    }

    private void UpdateTargetCharacterSlotUI(int side, Character target)
    {
        if(target != null)
        {
            if (target._side == 0)
            {
                _battleUI.UpdateCharacterSlotUI(Array.IndexOf(_charactersSideA, target), 0);
            }
            else
            {
                _battleUI.UpdateCharacterSlotUI(Array.IndexOf(_charactersSideB, target), 1);
            }
        }
        else
        {
            int sideToUpdade = 0;
            if(_selectedSkill is HealingSkillSO)
            {
                sideToUpdade = _actionOrder[_currentTurn]._side;
            }
            else
            {
                if(_actionOrder[_currentTurn]._side == 0)
                {
                    sideToUpdade = 1;
                }
            }

            if(sideToUpdade == 0)
            {
                foreach(Character character in _charactersSideA)
                {
                    _battleUI.UpdateCharacterSlotUI(Array.IndexOf(_charactersSideA, character), 0);
                }
            }
            else
            {
                foreach (Character character in _charactersSideB)
                {
                    _battleUI.UpdateCharacterSlotUI(Array.IndexOf(_charactersSideB, character), 1);
                }
            }
        }
    }

    public float GetBonusByWeaponDamageType(Character target)
    {
        float bonus = 1f;

        WeaponDamageType attacker = GetCurrentActor()._equipment._weaponDamageType;
        WeaponDamageType deffender = target._equipment._weaponDamageType;

        WeaponDamageType nextDamageType;
        if (attacker == WeaponDamageType.Piercing)
        {
            nextDamageType = 0;
        }
        else
        {
            nextDamageType = attacker + 1;
        }

        if (nextDamageType == deffender)
        {
            bonus += _weaponDamageBonus;
        }

        return bonus;
    }
    private int GetDamage(int attackerStat
                        , int defenderStat
                        , int skillBaseForce
                        , bool isCritical
                        , bool inDefensiveState
                        , float weaponBonus
                        , bool isStab
                        , float typeBonus)
    {
        int finalDamage = Mathf.Clamp((attackerStat - defenderStat) + _selectedSkill._baseForce, _minimumDamage, _maximumDamage);

        if (isCritical)
        {
            finalDamage = (int)Math.Round(finalDamage * _criticalBonus);
        }

        finalDamage = (int)Math.Round(finalDamage * weaponBonus);

        if (isStab)
        {
            finalDamage += (int)Math.Round(finalDamage * _stabBonus);
        }

        finalDamage = (int)Math.Round(finalDamage * typeBonus);
        finalDamage += (int)Math.Round(finalDamage * Random.Range(_minimumDamageVariation, _maximumDamageVariation));

        if (GetCurrentActor()._currentStatusCondition == StatusCondition.Exausted)
        {
            finalDamage -= (int)Math.Round(finalDamage * _exaustedPenalization);
        }

        if (_selectedSkill._isSpecialSkill)
        {
            if (_isBluffing)
            {
                finalDamage += (int)((_deceived * _bluffFactor) * finalDamage);
            }
            if (GetCurrentActor()._side == 0) 
            {
                if(_currentActionPointsSideA == 200)
                {
                    finalDamage += (int)(finalDamage * _fullSpecialBarFactor);
                }
            }
            else
            {
                if (_currentActionPointsSideA == 200)
                {
                    finalDamage += (int)(finalDamage * _fullSpecialBarFactor);
                }
            }
        }

        if (inDefensiveState)
        {
            if (_selectedSkill._isSpecialSkill)
            {
                if (!TurnBasedSystem.Instance._isBluffing)
                {
                    finalDamage = (int)(finalDamage / (_defensePenalization * 2));
                }
                else
                {
                    finalDamage = (int)(finalDamage * 1.5f);
                }
            }
            else
            {
                finalDamage = (int)(finalDamage / _defensePenalization);
            }

        }
        else
        {
            if (_selectedSkill._isSpecialSkill)
            {
                if (TurnBasedSystem.Instance._isBluffing)
                {
                    finalDamage = (int)(finalDamage / (_defensePenalization * 2));
                }
            }
        }

        return finalDamage;
    }

    private bool CheckIfIsStab(ElementType attackerType, ElementType skillType)
    {
        if(attackerType != ElementType.Neutral
            && attackerType == skillType)
        {
            return true;
        }
        return false;
    }

    private void GetTypeMatchupBonus(ElementType skillType, ElementType defenderType, out float matchupValue)
    {
        matchupValue = 1f;

        if(skillType == ElementType.Neutral)
        {
            return;
        }

        TypeMatchup typeMatchupAttacker = _typeMatchupList.Find(tm => tm._elementalType == skillType);

        if(typeMatchupAttacker._strongAgainst != null)
        {
            ElementType isWeak = typeMatchupAttacker._strongAgainst.Find(tm => tm == defenderType);
            if (isWeak != ElementType.Neutral)
            {
                matchupValue = 1.5f;
                return;
            }
        }
        else
        {
            Debug.LogError($"Forças do tipo {typeMatchupAttacker._elementalType} estão nulas");
        }

        TypeMatchup typeMatchupDefender = _typeMatchupList.Find(tm => tm._elementalType == skillType);

        if(typeMatchupDefender._resistences != null)
        {
            ElementType isResistent = typeMatchupDefender._resistences.Find(tm => tm == skillType);
            if (isResistent != ElementType.Neutral)
            {
                matchupValue = 0.75f;
            }
        }
        else
        {
            Debug.LogError($"Resistências do tipo {typeMatchupAttacker._elementalType} estão nulas");
        }
    }

    private void ReduceItem()
    {
        GetCurrentActor()._itemsList.Remove(_selectedItem);
        _selectedItem = null;
    }

    public void AddActionPoints(int sideToAddPoints, bool isAttacking, bool isDefending, int damage)
    {
        if(damage > 0)
        {
            return;
        }

        damage = Mathf.Abs(damage);
        int pointsToEarn = 0;
        
        if(isAttacking)
        {
            pointsToEarn = (int)(Mathf.Round(damage * (_actionPointsAttacking / 100)));
        }else if (isDefending)
        {
            pointsToEarn = (int)(Mathf.Round(damage * (_actionPointsDefending / 100)));
        }
        else
        {
            pointsToEarn = (int)(Mathf.Round(damage * (_actionPointsBeingAttacked / 100)));
        }

        if (sideToAddPoints == 0)
        {
            _currentActionPointsSideA = Mathf.Clamp(_currentActionPointsSideA + pointsToEarn, 0, _maxActionPoints);
        }
        else
        {
            _currentActionPointsSideB = Mathf.Clamp(_currentActionPointsSideB + pointsToEarn, 0, _maxActionPoints);
        }
        _battleUI.UpdateActionPointsUI();
    }

    public void SetSpecialAction(bool isBluffing)
    {
        _characterCharging = GetCurrentActor();
        _isCharging = true;
        _isBluffing = isBluffing;

        string message = $"A equipe de {GetCurrentActor()._name} está preparando um ataque. Todos os turnos serão usados para a preparação.";
        _battleUI.SetupDialoguePanel(message, () => { StartCoroutine(AdvanceTurn()); });
    }

    private bool HandleSpecialAction()
    {
        _isCharging = false;
        _characterCharging = null;
        string message = "";

        SkillSO specialSkill = GetCurrentActor()._baseCharacter._specialSkill;
        _selectedSkill = specialSkill;
        if (_isBluffing)
        {
            _deceived = 0;
            List<Character> enemyList = GetSideList(GetCurrentActor()._side, true);

            foreach(Character character in enemyList)
            {
                if (character._currentHP > 0
                    && character._inDefensiveState)
                {
                    _deceived++;
                }
            }

            if(_deceived == 0)
            {
                if(GetCurrentActor()._side == 0)
                {
                    _currentActionPointsSideA = 0;
                }
                else
                {
                    _currentActionPointsSideB = 0;
                }
                _battleUI.UpdateActionPointsUI();
                return false;
            }

            List<StatModifier> modifierList = specialSkill._modifiersToAdd;

            foreach (Character character in  GetSideList(GetCurrentActor()._side, false))
            {
                if(character._currentHP > 0)
                {
                    if(specialSkill._restoreHPFactor > 0)
                    {
                        character.ApplyDamage((int)(character._maxHP * specialSkill._restoreHPFactor));
                    }
                    if (specialSkill._removeDebuff)
                    {
                        character.RemoveAllDebuffs();
                    }
                    if (specialSkill._cureAllStatusConditions)
                    {
                        character.ApplyStatusCondition(StatusCondition.None);
                    }
                    foreach(StatModifier modifier in modifierList)
                    {
                        modifier._stage = _deceived;
                        modifier._minumumTurns = _deceived;
                        modifier._maximumTurns = _deceived;

                        character.ApplyStatModifier(modifier);
                    }
                    UpdateTargetCharacterSlotUI(character._side, character);
                }
            }

            if (GetCurrentActor()._side == 0)
            {
                _currentActionPointsSideA = _deceived * 10;
            }
            else
            {
                _currentActionPointsSideB = _deceived * 10;
            }

            _selectedSkill = specialSkill;
            message = $"A equipe de  {GetCurrentActor()._name} blefou e {_deceived} adversários baixaram a guarda. " + Environment.NewLine;
            message += _deceived > 0 ? $"{_deceived} foram enganados e baixaram sua guarda, assim foram alvo de um golpe devastador. " + Environment.NewLine : "";
            message += _deceived > 0 ? $"A equipe de {GetCurrentActor()._name} recebeu os benefícios do blefe." + Environment.NewLine : "";
            ApplySkillOnAllTargets(message);
        }
        else
        {
            _selectedSkill = specialSkill;
            message = $"A equipe de  {GetCurrentActor()._name} não blefou e usou o ataque especial. " + Environment.NewLine;
            message += "Personagens que não se defenderam sofreram um dano maior. " + Environment.NewLine;
            ApplySkillOnAllTargets(message);
            if (GetCurrentActor()._side == 0)
            {
                _currentActionPointsSideA = 0;
            }
            else
            {
                _currentActionPointsSideB = 0;
            }
        }
        _battleUI.UpdateActionPointsUI();
        _deceived = 0;
        return true;
    }

    private List<Character> GetSideList(int side, bool returnEnemyList)
    {
        if (side == 0)
        {
            if (returnEnemyList)
            {
                return _charactersSideB.ToList();
            }
            else
            {
                return _charactersSideA.ToList();
            }
        }
        else
        {
            if (returnEnemyList)
            {
                return _charactersSideA.ToList();
            }
            else
            {
                return _charactersSideB.ToList();
            } 
        }
    }
}
