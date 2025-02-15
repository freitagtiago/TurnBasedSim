using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.TextCore.Text;
using static UnityEngine.GraphicsBuffer;
using Random = UnityEngine.Random;

public class TurnBasedSystem : MonoBehaviour
{
    public static TurnBasedSystem Instance;

    [SerializeField] private BattleState _currentState;
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
        _currentState = BattleState.START;
        LoadCharacters();
        DefineOrder();
        AdvanceTurn();
        _onBattle = true;       
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

    public void EndBattle()
    {
        _onBattle = false;
        _charactersSideA = new Character[4];
        _charactersSideB = new Character[4];
        _battleUI.EndBattle();
    }

    public void AdvanceTurn()
    {
        if (CheckCondition())
        {
            EndBattle();
        }
        else
        {
            bool currentActorDefined = false;

            while (!currentActorDefined)
            {
                _currentTurn++;
                if (_currentTurn >= _actionOrder.Count)
                {
                    _currentTurn = 0;
                }

                if (_actionOrder[_currentTurn]._currentHP > 0)
                {
                    if (!_actionOrder[_currentTurn].HandleStatusCondition())
                    {
                        currentActorDefined = true;

                        if (_actionOrder[_currentTurn]._side == 0)
                        {
                            _battleUI.UpdateCharacterSlotUI(Array.IndexOf(_charactersSideA, _actionOrder[_currentTurn]), 0);
                        }
                        else
                        {
                            _battleUI.UpdateCharacterSlotUI(Array.IndexOf(_charactersSideB, _actionOrder[_currentTurn]), 1);
                        }
                    }
                    else
                    {
                        Debug.Log("AÇÃO BLOQUEADA");
                        //MOSTRAR DE ALGUMA FORMA
                    }
                }
            }
            SetAction();
        }
    }

    private void SetAction()
    {
        Character currentActor = _actionOrder[_currentTurn];
        if(currentActor._side == 0)
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
            AdvanceTurn();
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

            _battleUI.SetupDialoguePanel(message, () => { AdvanceTurn(); });
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
        string message = CalculateAndApplyDamage(target);
        _selectedSkill = null;
        _battleUI.SetupDialoguePanel(message, () => { AdvanceTurn(); });
    }

    public void ApplySkillOnAllTargets()
    {
        _battleUI.CloseTargetPanel();
        string message = $"{GetCurrentActor()._name} usou a habilidade {_selectedSkill._name} em todos";
        _selectedSkill = null;

        _battleUI.SetupDialoguePanel(message, () => { AdvanceTurn(); });
    }

    public void ApplyItemOnAllTargets()
    {
        _battleUI.CloseTargetPanel();
        string message = $"{GetCurrentActor()._name} usou item {_selectedItem._name} em todos";
        _selectedItem = null;

        _battleUI.SetupDialoguePanel(message, () => { AdvanceTurn(); });
    }
    public void ApplyItemOnTarget(Character target)
    {
        _battleUI.CloseTargetPanel();
        string message = "";
        
        message = $"{GetCurrentActor()._name} usou o item {_selectedItem._name} em {target._name}";

        _selectedItem = null;

        _battleUI.SetupDialoguePanel(message, () => { AdvanceTurn(); });
    }

    private void EnemyRoutine()
    {
        Character character = GetCurrentActor();
        Character target = null;

        // 0 - 30  Basic Attack
        // 31 - 75 Skill
        // 76 - 100 Defend

        int actionChance = Random.Range(0, 101);
        if(actionChance < 31)
        {
            _selectedSkill = character._equipment._basicSkill;
            bool foundTarget = false;
            while (!foundTarget)
            {
                int index = UnityEngine.Random.Range(0, 3);
                if (_charactersSideA[index]._currentHP > 0)
                {
                    target = _charactersSideA[index];
                    foundTarget = true;
                }
            }
            ApplySkillOnTarget(target);
        }
        else if (actionChance < 76)
        {
            _selectedSkill = character._skillList[UnityEngine.Random.Range(0, character._skillList.Count - 1)];

            bool foundTarget = false;

            if(_selectedSkill is HealingSkillSO)
            {
                while (!foundTarget)
                {
                    int index = UnityEngine.Random.Range(0, 3);
                    if (_charactersSideB[index]._currentHP > 0)
                    {
                        target = _charactersSideB[index];
                        foundTarget = true;
                    }
                }
            }
            else
            {
                while (!foundTarget)
                {
                    int index = UnityEngine.Random.Range(0, 3);
                    if (_charactersSideA[index]._currentHP > 0)
                    {
                        target = _charactersSideA[index];
                        foundTarget = true;
                    }
                }
            }
            ApplySkillOnTarget(target);
        }
        else
        {
            SetBasicAction(false);
        }
         _battleUI.SetupEnemyAction(GetCurrentActor());
    }

    private string CalculateAndApplyDamage(Character target)
    {
        int finalDamage = 0;
        bool isCritical = false;
        StatusCondition condition = StatusCondition.None;

        if(_selectedSkill is HealingSkillSO)
        {
            if (!_selectedSkill._affectAll)
            {
                bool canRevive = (_selectedSkill as HealingSkillSO)._canRevive;
                if (GetCurrentActor()._side == 0){
                    foreach (Character character in _charactersSideA)
                    {
                        if (!canRevive)
                        {
                            if(character._currentHP > 0)
                            {
                                character.ApplyDamage((_selectedSkill as HealingSkillSO)._cureValue);
                                _battleUI.UpdateCharacterSlotUI(Array.IndexOf(_charactersSideA, character), 0);
                            }
                        }
                        else
                        {
                            character.ApplyDamage((_selectedSkill as HealingSkillSO)._cureValue);
                            _battleUI.UpdateCharacterSlotUI(Array.IndexOf(_charactersSideA, character), 0);
                        }
                    }
                }
                else
                {
                    foreach (Character character in _charactersSideB)
                    {
                        if (!canRevive)
                        {
                            if (character._currentHP > 0)
                            {
                                character.ApplyDamage((_selectedSkill as HealingSkillSO)._cureValue);
                                _battleUI.UpdateCharacterSlotUI(Array.IndexOf(_charactersSideB, character), 1);
                            }
                        }
                        else
                        {
                            character.ApplyDamage((_selectedSkill as HealingSkillSO)._cureValue);
                            _battleUI.UpdateCharacterSlotUI(Array.IndexOf(_charactersSideB, character), 1);
                        }
                    }
                }
            }
            else
            {
                target.ApplyDamage((_selectedSkill as HealingSkillSO)._cureValue);
                if(target._side == 0)
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
            if (CheckIfAttackMissed())
            {
                return SetMessage(0, false, true, condition, target);
            }

            if (_selectedSkill is StatSkillSO)
            {

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
                    if (GetCurrentActor()._side == 0)
                    {
                        foreach (Character character in _charactersSideB)
                        {
                            if (character._currentHP > 0
                                && character._currentStatusCondition != StatusCondition.None)
                            {
                                bool canCauseCondition = (Random.Range(0, 101) < (_selectedSkill as StatusConditionSkillSO)._accuracy);
                                if (canCauseCondition)
                                {
                                    character.ApplyStatusCondition((_selectedSkill as StatusConditionSkillSO)._statusCondition);
                                    _battleUI.UpdateCharacterSlotUI(Array.IndexOf(_charactersSideB, character), 1);
                                }
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
                                bool canCauseCondition = (Random.Range(0, 101) < (_selectedSkill as StatusConditionSkillSO)._accuracy);
                                if (canCauseCondition)
                                {
                                    character.ApplyStatusCondition((_selectedSkill as StatusConditionSkillSO)._statusCondition);
                                    _battleUI.UpdateCharacterSlotUI(Array.IndexOf(_charactersSideA, character), 0);
                                }
                            }
                        }
                    }
                }
            }
            else
            {
                int attackerStat = 0;
                int defenderStat = 0;
                if (_selectedSkill is PhysicalSkillSO)
                {
                    attackerStat = GetCurrentActor().GetStat(Stats.PhysicalAttack);
                    defenderStat = target.GetStat(Stats.PhysicalDefense);
                }
                else
                {
                    attackerStat = GetCurrentActor().GetStat(Stats.MagicalAttack);
                    defenderStat = target.GetStat(Stats.MagicalDefense);
                }

                finalDamage = (attackerStat - defenderStat) + _selectedSkill._baseForce;
                if (CheckIfIsCritical(GetCurrentActor().GetStat(Stats.Speed)
                    , GetCurrentActor().GetStat(Stats.Luck)
                    , target.GetStat(Stats.Speed)))
                {
                    finalDamage = (int)Math.Round(finalDamage * 1.3f);
                    isCritical = true;
                }

                finalDamage += (int)Math.Round(finalDamage * (Random.Range(0.3f, 1f)));

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

        return SetMessage(finalDamage, isCritical, false, condition, target);
    }

    private string SetMessage(int damage, bool isCritical, bool missed, StatusCondition statusCondition, Character target)
    {
        string message = "";
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
            message = message + " O ataquer falhou." + Environment.NewLine;
        }
        else
        {
            if (isCritical)
            {
                message += " Foi um ataque crítico!" + Environment.NewLine;
            }
            if(_selectedSkill is not HealingSkillSO)
            {
                message += $" Causou {damage} de dano." + Environment.NewLine;
            }   

            if(statusCondition != StatusCondition.None)
            {
                message += $" O alvo ficou {statusCondition.ToString()}." + Environment.NewLine;
            }
        }
        return message;
    }

    private bool CheckIfIsCritical(int attackerSpeed, int attackerLucky, int defenderSpeed)
    {
        //int criticalChance = Mathf.Clamp((int)Math.Round((attackerLucky + (attackerSpeed - defenderSpeed))* 0.01), 0, 100);
        int criticalChance = Mathf.Clamp((int)Math.Round((attackerSpeed / defenderSpeed) * 1.5f + attackerLucky * 0.1f), 0, 100);

        Debug.Log("CHANCE DE CRITICO DE " + criticalChance);
        return Random.Range(0,101) <= criticalChance;
    }

    private bool CheckIfAttackMissed()
    {
        return Random.Range(0, 101) > _selectedSkill._accuracy;
    }
}
