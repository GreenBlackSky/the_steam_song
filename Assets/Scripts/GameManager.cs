﻿using System.IO;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEditor;

    
public class GameManager : MonoBehaviour
{
    public GameObject UICamera;
    public GameObject UICanvas;

    Dictionary<State, GameObject> _gameMenus;
    Dictionary<string, GameObject> _allUIMenus;

    Dictionary<string, string> _allLevels;
    GameObject _currentLevel;
    public string currentLevelID;

    Dictionary<string, string> _allAvatars; // TODO load level configuration from xml
    GameObject _currentAvatar;
    public string currentAvatarID;

    public static GameManager Instance { get; private set; }

    // TODO cut scene
    // TODO notifications
    // TODO separate ui manager from gamemanager
    public enum State {
        GAMEPLAY,
        LOADING,  // TODO async loading
        MAIN_MENU,
        NEW_GAME_MENU,
        LOAD_GAME_MENU,
        OPTIONS_MENU,
        SCREEN_OPTIONS,
        AUDIO_OPTIONS,
        CONTROLS_OPTIONS,
        COMBAT,
        PUZZLE,
        PAUSE_MENU,
        INVENTORY,
        DIALOG,
        SKILL_MENU,
        MAP_MENU,
        JOURNAL_MENU
    }
    HashSet<State> _gameplayStates = new HashSet<State>() {
        State.GAMEPLAY,
        State.COMBAT,
        State.PUZZLE,
        State.PAUSE_MENU,
        State.INVENTORY,
        State.DIALOG,
        State.SKILL_MENU,
        State.MAP_MENU,
        State.JOURNAL_MENU
    };
    HashSet<State> _mainMenuStates = new HashSet<State>() {
        State.LOADING,
        State.MAIN_MENU,
        State.NEW_GAME_MENU,
        State.LOAD_GAME_MENU,
        State.OPTIONS_MENU,
        State.SCREEN_OPTIONS,
        State.AUDIO_OPTIONS,
        State.CONTROLS_OPTIONS,
    };
    State _state = State.GAMEPLAY;

    public void SwitchState(State state) {
        bool leavingGamplay = this._gameplayStates.Contains(this._state);
        bool leavingMainMenu = this._mainMenuStates.Contains(this._state);
        bool enteringGameplay = this._gameplayStates.Contains(state);
        bool enteringMainMenu = this._mainMenuStates.Contains(state);
        if(leavingGamplay && enteringGameplay) {
            this._leaveGameplayState();
            this._state = state;
            this._enterGameplayState(state);
        } else if (leavingMainMenu && enteringMainMenu) {
            this._leaveMainMenuState();
            this._state = state;
            this._enterMainMenuState(state);
        } else if (leavingMainMenu) {
            this._leaveMainMenu();
            this._leaveMainMenuState();
            this._state = state;
            this._enterGameplay();
            this._enterGameplayState(state);
        } else if (leavingGamplay) {
            this._leaveGameplay();
            this._leaveGameplayState();
            this._state = state;
            this._enterMainMenu();
            this._enterMainMenuState(state);
        }
    }

    void _enterMainMenu() {
        UICamera.SetActive(true);
    }

    void _leaveMainMenu() {
        UICamera.SetActive(false);
    }

    void _enterGameplay() {
        Time.timeScale = 1;
        LoadLevel();
        LoadAvatar();
    }

    void _leaveGameplay() {
        Time.timeScale = 0;
        Destroy(_currentLevel);
        Destroy(_currentAvatar);
    }

    void _leaveGameplayState() {
        switch (_state) {
            case State.GAMEPLAY:
                _allUIMenus["GameplayUI"].SetActive(false);
                break;
            default:
                _gameMenus[_state].SetActive(false);
                break;
        }
    }

    void _enterGameplayState(State state) {
        switch (_state) {
            case State.GAMEPLAY:
                _allUIMenus["GameplayUI"].SetActive(true);
                break;
            default:
                _gameMenus[_state].SetActive(true);
                break;
        }
    }

    void _leaveMainMenuState() {
        switch (_state) {
            case State.MAIN_MENU:
                _allUIMenus["MainMenuPanel"].SetActive(false);
                break;
            default:
                break;
        }
    }

    void _enterMainMenuState(State state) {
        switch (_state) {
            case State.MAIN_MENU:
                this._allUIMenus["MainMenuPanel"].SetActive(true);
                break;
            default:
                break;
        }
    }


    void Start() {
        PrepareUI();
        this._allLevels = GetResourcesPaths("Assets/Levels/");
        this._allAvatars = GetResourcesPaths("Assets/Avatars/");
        Instance = this;
        SwitchState(State.MAIN_MENU);
    }

    void Update() {
        if(this._mainMenuStates.Contains(this._state)) {
            this._processManMenuInput();
        } else if (this._gameplayStates.Contains(this._state)) {
            this._processGameplayInput();
        } else {

        }
    }

    void _processManMenuInput() {

    }

    void _processGameplayInput() {
        if(Input.GetKeyDown(KeyCode.I)) {
            SwitchState(State.INVENTORY);
        } else if (Input.GetKeyDown(KeyCode.J)) {
            SwitchState(State.JOURNAL_MENU);
        } else if (Input.GetKeyDown(KeyCode.Tab)) {
            SwitchState(State.GAMEPLAY);
        } else if (Input.GetKeyDown(KeyCode.Escape)) {
            SwitchState(State.PAUSE_MENU);
        } 
    } 

    Dictionary<string, string> GetResourcesPaths(string resourcePath) {
        Dictionary<string, string> ret = new Dictionary<string, string>();
        // TODO set in editor
        List<string> paths = new List<string>(Directory.GetFiles(@"" + resourcePath));
        foreach(string path in paths) {
            if(path.EndsWith("meta")) {
                continue;
            }
            string[] pathParts = path.Split('/');
            string name = pathParts[pathParts.Length - 1].Split('.')[0];
            ret[name] = path;
        }
        return ret;
    }

    void PrepareUI() {
        _allUIMenus = new Dictionary<string, GameObject>();
        // TODO set in editor
        List<string> paths = new List<string>(Directory.GetFiles(@"" + "Assets/UI/"));
        paths.AddRange(Directory.GetFiles(@"" + "Assets/UI/Menus/"));
        foreach(string path in paths) {
            if(path.EndsWith("meta")) {
                continue;
            }
            UnityEngine.Object prefab = AssetDatabase.LoadAssetAtPath(path, typeof(GameObject));
            string[] pathParts = path.Split('/');
            string name = pathParts[pathParts.Length - 1].Split('.')[0];
             _allUIMenus[name] = Instantiate(prefab, UICanvas.transform, false) as GameObject;  
        }
        _gameMenus = new Dictionary<State, GameObject>() {
            {State.PAUSE_MENU, _allUIMenus["PauseMenuPanel"]},
            {State.INVENTORY, _allUIMenus["InventoryPanel"]},
            {State.SKILL_MENU, _allUIMenus["SkillMenuPanel"]},
            {State.MAP_MENU, _allUIMenus["MapMenuPanel"]},
            {State.DIALOG, _allUIMenus["DialogPanel"]},
            {State.COMBAT, _allUIMenus["CombatUI"]},
            {State.JOURNAL_MENU, _allUIMenus["QuestsPanel"]},
        };
    }

    void LoadLevel() {
        UnityEngine.Object prefab = AssetDatabase.LoadAssetAtPath(_allLevels[currentLevelID], typeof(GameObject));
        _currentLevel =  Instantiate(prefab) as GameObject;
        DialogueManager.LoadAllSpeakers(currentLevelID);
        QuestManager.Init(currentLevelID);
    }

    void LoadAvatar() {
        
        UnityEngine.Object prefab = AssetDatabase.LoadAssetAtPath(_allAvatars[currentAvatarID], typeof(GameObject));
        _currentAvatar = Instantiate(prefab) as GameObject;
        // TODO set in editor
        GameObject interactButton = _allUIMenus["GameplayUI"].transform.Find("InteractButton").gameObject;
        CollectingHero itemsHero = _currentAvatar.GetComponent<CollectingHero>();
        QuestDoingHero questHero = _currentAvatar.GetComponent<QuestDoingHero>();

        _currentAvatar.GetComponent<InteractingHero>().interactionButton = interactButton;
        interactButton.GetComponent<Button>().onClick.AddListener(() => _currentAvatar.GetComponent<InteractingHero>().interact());
        itemsHero.InventoryPanel = _allUIMenus["InventoryPanel"];
        questHero.QuestsUI = _allUIMenus["QuestsPanel"];
    }
}