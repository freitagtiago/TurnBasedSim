using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class SelectionUIHandler : MonoBehaviour
{
    [SerializeField] private Transform _charactersScrollView;
    [SerializeField] private CharacterButton _partyMemberPrefab;
    [SerializeField] private readonly string _scriptableObjectsCharactersFolder = "Characters/";
    [SerializeField] private readonly string _scriptableObjectsItemFolder = "Item/";
    [SerializeField] private CharacterSO[] _availableCharacters;

    public static Action<CharacterSO> OnSelectCharacter;
    public static int _currentSlot = 0;

    public static Character[] _selectedCharacters = new Character[4];
    public static Character[] _enemyCharacters = new Character[4];
    [SerializeField] private PartyMemberSlot[] _partyMemberSlot;
    [SerializeField] private ItemSO[] _itemList;
    [SerializeField] private TextMeshProUGUI _message;

    [Header("Hero Info")]
    [SerializeField] private TextMeshProUGUI _characterName;
    [SerializeField] private Image _characterImage;
    [SerializeField] private TextMeshProUGUI _characterDescription;
    [Header("Stat Group")]
    [SerializeField] private TextMeshProUGUI _characterType;
    [SerializeField] private TextMeshProUGUI _statHP;
    [SerializeField] private TextMeshProUGUI _statSP;
    [SerializeField] private TextMeshProUGUI _statPhysicalAttack;
    [SerializeField] private TextMeshProUGUI _statPhysicalDefense;
    [SerializeField] private TextMeshProUGUI _statMagicalAttack;
    [SerializeField] private TextMeshProUGUI _statMagicalDefense;
    [SerializeField] private TextMeshProUGUI _statSpeed;
    [SerializeField] private TextMeshProUGUI _statCharisma;
    [SerializeField] private TextMeshProUGUI _statLuck;
    [Header("Equipment Group")]
    [SerializeField] private TextMeshProUGUI _equipmentName;
    [SerializeField] private TextMeshProUGUI _equipmentDamageType;
    [SerializeField] private TextMeshProUGUI _equipmentDescription;
    [Header("Skills Group")]
    [SerializeField] private ScrollRect _skillScrollView;
    [SerializeField] private Transform _skillScrollViewContent;
    [SerializeField] private SkillToggle _skillTogglePrefab;
    [Header("Item Group")]
    [SerializeField] private ScrollRect _itemScrollView;
    [SerializeField] private Transform _itemScrollViewContent;
    [SerializeField] private ItemToggle _itemTogglePrefab;

    private void OnEnable()
    {
        OnSelectCharacter += SelectCharacter;
        _selectedCharacters = new Character[4];
        _enemyCharacters = new Character[4];
}

    private void OnDisable()
    {
        OnSelectCharacter -= SelectCharacter;
    }

    private void Start()
    {
        _itemList = Resources.LoadAll<ItemSO>(_scriptableObjectsItemFolder);
        LoadAvailableCharacters();
    }

    private void LoadAvailableCharacters()
    {
        _availableCharacters = Resources.LoadAll<CharacterSO>(_scriptableObjectsCharactersFolder);
        foreach (CharacterSO character in _availableCharacters)
        {
            CharacterButton partyMemberButton = Instantiate(_partyMemberPrefab, _charactersScrollView);
            partyMemberButton.Setup(character);
        }
    }

    private void SelectCharacter(CharacterSO character)
    {
        if(character == null)
        {
            ResetInfo();
            return;
        }

        if (_selectedCharacters[_currentSlot] != null
            && _selectedCharacters[_currentSlot]._baseCharacter._name == character._name)
        {
            ResetScrollViews();
            LoadCharacterInfo();
        }
        else
        {
            if (CharacterAlreadySelected(character))
            {
                return;
            }

            ResetScrollViews();
            _selectedCharacters[_currentSlot] = new Character();
            _selectedCharacters[_currentSlot].SetupCharacter(character, 0);
            _partyMemberSlot[_currentSlot].Setup(_selectedCharacters[_currentSlot]);

            LoadCharacterInfo();
        }
    }

    private bool CharacterAlreadySelected(CharacterSO character)
    {
        foreach(Character partyMember in _selectedCharacters)
        {
            if(partyMember != null 
                && partyMember._baseCharacter == character)
            {
                return true;
            }
        }
        return false;
    }

    private void LoadCharacterInfo()
    {
        _characterImage.sprite = _selectedCharacters[_currentSlot]._baseCharacter._portrait;
        _characterName.text = _selectedCharacters[_currentSlot]._name;
        _characterDescription.text = _selectedCharacters[_currentSlot]._description;

        _characterType.text = _selectedCharacters[_currentSlot]._type.ToString();
        _statHP.text = _selectedCharacters[_currentSlot].GetStat(Stats.HealthPoints).ToString();
        _statSP.text = _selectedCharacters[_currentSlot].GetStat(Stats.SpecialPoints).ToString();
        _statPhysicalAttack.text = _selectedCharacters[_currentSlot].GetStat(Stats.PhysicalAttack).ToString();
        _statPhysicalDefense.text = _selectedCharacters[_currentSlot].GetStat(Stats.PhysicalDefense).ToString();
        _statMagicalAttack.text = _selectedCharacters[_currentSlot].GetStat(Stats.MagicalAttack).ToString();
        _statMagicalDefense.text = _selectedCharacters[_currentSlot].GetStat(Stats.MagicalDefense).ToString();
        _statSpeed.text = _selectedCharacters[_currentSlot].GetStat(Stats.Speed).ToString();
        _statCharisma.text = _selectedCharacters[_currentSlot].GetStat(Stats.Charisma).ToString();
        _statLuck.text = _selectedCharacters[_currentSlot].GetStat(Stats.Luck).ToString();

        _equipmentName.text = _selectedCharacters[_currentSlot]._equipment._name;
        _equipmentDamageType.text = _selectedCharacters[_currentSlot]._equipment._weaponDamageType.ToString();
        _equipmentDescription.text = _selectedCharacters[_currentSlot]._equipment._description;

        foreach(SkillSO skill in _selectedCharacters[_currentSlot]._baseCharacter._availableSkills)
        {
            SkillToggle skillToggle = Instantiate(_skillTogglePrefab, _skillScrollViewContent);
            skillToggle.Setup(skill, this);
            skillToggle._toggle.isOn = _selectedCharacters[_currentSlot]._skillList.Contains(skill);
        }

        foreach (ItemSO item in _itemList)
        {
            ItemToggle itemToggle = Instantiate(_itemTogglePrefab, _itemScrollViewContent);
            itemToggle.Setup(item, this);
            itemToggle._toggle.isOn = _selectedCharacters[_currentSlot]._itemsList.Contains(item);
        }

        ValidateSkill(null, false);
        ValidateItem(null, false);
    }

    private void ResetInfo()
    {
        ResetScrollViews();
        _characterImage.sprite = null;
        _characterName.text = "Não definido";
        _characterDescription.text = "";

        _characterType.text = "Não definido";
        _statHP.text = "0";
        _statSP.text = "0";
        _statPhysicalAttack.text = "0";
        _statPhysicalDefense.text = "0";
        _statMagicalAttack.text = "0";
        _statMagicalDefense.text = "0";
        _statSpeed.text = "0";
        _statCharisma.text = "0";
        _statLuck.text = "0";

        _equipmentName.text = "Não definido";
        _equipmentDamageType.text = "Não definido";
        _equipmentDescription.text = "";
    }

    private void ResetScrollViews()
    {
        SkillToggle[] skillsToggles = _skillScrollViewContent.GetComponentsInChildren<SkillToggle>();

        foreach (SkillToggle skill in skillsToggles)
        {
            Destroy(skill.gameObject);
        }
        
        ItemToggle[] itemToggles = _itemScrollViewContent.GetComponentsInChildren<ItemToggle>();

        foreach (ItemToggle item in itemToggles)
        {
            Destroy(item.gameObject);
        }
        _skillScrollView.normalizedPosition = new Vector2(0, 1);
        _itemScrollView.normalizedPosition = new Vector2(0, 1);
    }

    public void ValidateSkill(SkillSO skill, bool activate)
    {
        if(skill == null)
        {
            if (_selectedCharacters[_currentSlot]._skillList.Count > 5)
            {
                HandleSkillToggles(false);
            }else
            {
                HandleSkillToggles(true);
            }
        }
        else
        {
            if (_selectedCharacters[_currentSlot]._skillList.Count > 5
            && !_selectedCharacters[_currentSlot]._skillList.Contains(skill))
            {
                HandleSkillToggles(false);
            }
            else
            {
                if (_selectedCharacters[_currentSlot]._skillList.Contains(skill))
                {
                    if (!activate)
                    {
                        _selectedCharacters[_currentSlot]._skillList.Remove(skill);
                    }
                }
                else
                {
                    _selectedCharacters[_currentSlot]._skillList.Add(skill);
                }

                if (_selectedCharacters[_currentSlot]._skillList.Count < 6)
                {
                    HandleSkillToggles(true);
                }
                else
                {
                    HandleSkillToggles(false);
                }
            }
        }
    }

    public void ValidateItem(ItemSO item, bool activate)
    {
        if(item == null)
        {
            if(_selectedCharacters[_currentSlot]._itemsList.Count > 3)
            {
                HandleItemToggles(false);
            }
            else
            {
                HandleItemToggles(true);
            }
        }
        else
        {
            if (_selectedCharacters[_currentSlot]._itemsList.Count > 3
            && !_selectedCharacters[_currentSlot]._itemsList.Contains(item))
            {
                HandleItemToggles(false);
            }
            else
            {
                if (_selectedCharacters[_currentSlot]._itemsList.Contains(item))
                {
                    if (!activate)
                    {
                        _selectedCharacters[_currentSlot]._itemsList.Remove(item);
                    }
                }
                else
                {
                    _selectedCharacters[_currentSlot]._itemsList.Add(item);
                }

                if (_selectedCharacters[_currentSlot]._itemsList.Count < 3)
                {
                    HandleItemToggles(true);
                }
                else
                {
                    HandleItemToggles(false);
                }
            }
        }
    }

    private void HandleSkillToggles(bool activate)
    {
        Debug.Log(activate ? "Ainda pode selecionar" : "Não pode selecionar mais");

        foreach(SkillToggle skillToggle in _skillScrollViewContent.GetComponentsInChildren<SkillToggle>())
        {
            if (!skillToggle._toggle.isOn)
            {
                skillToggle._toggle.interactable = activate;
            }
        }
    }

    private void HandleItemToggles(bool activate)
    {
        Debug.Log(activate ? "Ainda pode selecionar" : "Não pode selecionar mais");

        foreach (ItemToggle itemToggle in _itemScrollViewContent.GetComponentsInChildren<ItemToggle>())
        {
            if (!itemToggle._toggle.isOn)
            {
                itemToggle._toggle.interactable = activate;
            }
        }
    }

    public void ValidateSelection()
    {
        string message = "";
        foreach(Character character in _selectedCharacters)
        {
            if(character == null)
            {
                message += $"É necessário selecionar 4 personagens." + Environment.NewLine;
                break;
            }

            if(character._skillList.Count == 0 
                || character._skillList.Count > 6)
            {
                message += $"Verifique as habilidades selecionadas de {character._name}. O número de habilidades selecionadas deve ficar entre 1 e 6." + Environment.NewLine;
            }
        }

        if(message == "")
        {
            if (SelectEnemyTeam())
            {
                SceneManager.LoadScene(1);
            }
        }
        else
        {
            StartCoroutine(HandleMessage(message));
        }
    }

    private IEnumerator HandleMessage(string message)
    {
        _message.text = message;
        yield return new WaitForSecondsRealtime(10f);
        _message.text = "";
    }

    private bool SelectEnemyTeam()
    {
        bool enemyIsReady = false;
        List<CharacterSO> enemySide = new List<CharacterSO>();
        int iterarions = 0;

        while (!enemyIsReady)
        {
            int index = UnityEngine.Random.Range(0, _availableCharacters.Length);
             if (!enemySide.Contains(_availableCharacters[index]))
            {
                enemySide.Add(_availableCharacters[index]);

                if(enemySide.Count == 4)
                {
                    enemyIsReady = true;
                }
            }
        }

        for (int i = 0; i < 4; i++ )
        {
            _enemyCharacters[i] = new Character();
            _enemyCharacters[i].SetupCharacter(enemySide[i], 1);

            if(_enemyCharacters[i]._baseCharacter._availableSkills.Count < 7)
            {
                _enemyCharacters[i]._skillList.Clear();
                foreach (SkillSO skill in _enemyCharacters[i]._baseCharacter._availableSkills)
                {
                    _enemyCharacters[i]._skillList.Add(skill);
                }
            }
            else
            {
                _enemyCharacters[i]._skillList.Clear();
                bool skillsReady = false;
                int skillCount = _enemyCharacters[i]._baseCharacter._availableSkills.Count;
                int iterations = 0;

                while (!skillsReady)
                {
                    SkillSO skill = _enemyCharacters[i]._baseCharacter._availableSkills[UnityEngine.Random.Range(0, skillCount)];

                    if (!_enemyCharacters[i]._skillList.Contains(skill))
                    {
                        _enemyCharacters[i]._skillList.Add(skill);
                    }

                    if(_enemyCharacters[i]._skillList.Count == 6
                        || iterations == 18)
                    {
                        skillsReady = true;
                    }
                    iterations++;
                }
            }
            
            for(int index = 0; index < 3; index++)
            {
                int itemIndex = UnityEngine.Random.Range(0, _itemList.Length);
                ItemSO item = _itemList[itemIndex];
                if (!_enemyCharacters[i]._itemsList.Contains(item))
                {
                    _enemyCharacters[i]._itemsList.Add(item);
                }
            }
        }
        return true;
    }
}
