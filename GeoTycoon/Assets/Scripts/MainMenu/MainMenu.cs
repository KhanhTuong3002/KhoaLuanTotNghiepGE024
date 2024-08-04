using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;
using Photon.Pun;
using Photon.Pun.Demo.PunBasics;
using Photon.Realtime;
using System.Linq;
using Unity.VisualScripting;
using Photon.Pun.UtilityScripts;
using JetBrains.Annotations;
public class MainMenu : MonoBehaviourPunCallbacks
{
    public GameObject lobby;
    public GameObject loadingScreen;
    public GameObject multiPlayerMenu;
    public Button BackToMenu;

    public GameObject singlePlayerMenu;
    public TMP_InputField setIdFieldOff;
    public TMP_InputField setIdFieldMulti;

    public Button startGameButton;


    public string[] playerNameList = new string[4];
    public int[] playerIdList = new int[4];
    public string setQuestionId;

    [Serializable]
    public class PlayerSelect
    {
        public TMP_InputField nameInput;
        public TMP_Dropdown typeDropdown;
        public TMP_Dropdown colorDropdown;
        public Toggle toggle;
    }
    [Serializable]
    public class PlayerSelectMulti
    {
        public TMP_InputField nameInput;
        public TMP_Dropdown typeDropdown;
        public TMP_Dropdown colorDropdown;
        public Toggle toggle;
        public int playerId;
    }
    [SerializeField] PlayerSelect[] playerSelection;

    [SerializeField] public PlayerSelectMulti[] playerSelectionMulti;

    public string supportUrl;
    public string webUrl;

    

    [PunRPC]
    void SyncSetting(string[] currentPlayerNameList, int[] currentPlayerIdList, string mySetQuestionId)
    {
        
        playerNameList = currentPlayerNameList;
        playerIdList = currentPlayerIdList;
        setQuestionId = mySetQuestionId;
        UpdateListing();
    }

    [PunRPC]
    void SyncStart()
    {
        foreach (var player in playerSelectionMulti)
        {
            if (player.toggle.isOn)
            {
                MultiSetting newSet = new MultiSetting(player.nameInput.text, player.typeDropdown.value, player.colorDropdown.value, player.playerId, setQuestionId);
                GameSettings.AddMultiSetting(newSet);
            }
        }
    }
    public void Start()
    {
        lobby.SetActive(false);
        loadingScreen.SetActive(false);
        multiPlayerMenu.SetActive(false);
        singlePlayerMenu.SetActive(true);
        BackToMenu.onClick.AddListener(OnClickBackToMenu);
        GameSettings.ClearSetting();
    }
    void OnClickBackToMenu()
    {
        SceneManager.LoadScene("StartMenu");
    }

    
    public IEnumerator RoomListing(float delayTime)
    {
        yield return new WaitForSeconds(delayTime);
        if (PhotonNetwork.IsConnected && PhotonNetwork.PlayerList.Count() > 0)
        {
            UpdateListing();
            PhotonView PV = GetComponent<PhotonView>();
            PV.RPC("SyncSetting", RpcTarget.Others, (object)playerNameList, (object)playerIdList, setQuestionId);
        }
    }

    public void UpdateListing()
    {
        for(int i = 0; i < playerSelectionMulti.Count(); i++)
        {
            if (playerNameList[i] != "") playerSelectionMulti[i].toggle.isOn = true;
            playerSelectionMulti[i].nameInput.text = playerNameList[i];
            playerSelectionMulti[i].playerId = playerIdList[i];
        }
        setIdFieldMulti.text = setQuestionId;
    }

    public override void OnJoinedRoom()
    {
        foreach (PlayerSelectMulti playerSelectionMulti in playerSelectionMulti)
        {
            playerSelectionMulti.nameInput.text = "";
            playerSelectionMulti.typeDropdown.value = 0;
            playerSelectionMulti.toggle.isOn = false;

        }
        if (PhotonNetwork.IsMasterClient)
        {
            Debug.Log("Master ID " + PhotonNetwork.MasterClient.ActorNumber);
            playerSelectionMulti[0].nameInput.text = PhotonNetwork.NickName;
            playerSelectionMulti[0].toggle.isOn = true;
            playerSelectionMulti[0].playerId = PhotonNetwork.MasterClient.ActorNumber;
            playerIdList[0] = PhotonNetwork.MasterClient.ActorNumber;
            playerNameList[0] = PhotonNetwork.NickName;
            setQuestionId = CreateAndJoinRoom.instance.GetSetId();
            if(setQuestionId == null) setQuestionId = "defaultSetID";
            setIdFieldMulti.text = setQuestionId;
        }
        else
        {
            startGameButton.gameObject.SetActive(false);
            //StartCoroutine(RoomListing(1f));
        }
        
    }

    public void StartButton()
    {
        if (PhotonNetwork.IsConnected)
        {
            foreach (var player in playerSelectionMulti)
            {
                if (player.toggle.isOn)
                {
                    MultiSetting newSet = new MultiSetting(player.nameInput.text, player.typeDropdown.value, player.colorDropdown.value, player.playerId, setIdFieldMulti.text);
                    GameSettings.AddMultiSetting(newSet);
                }
            }
            PhotonView PV = GetComponent<PhotonView>();
            PV.RPC("SyncStart", RpcTarget.OthersBuffered);
            PhotonNetwork.LoadLevel("MainGame");
        }
        else
        {
            SetValidator.Instance.GetData(setIdFieldOff.text);
            StartCoroutine(StartGame());

        }
    }

    public IEnumerator StartGame()
        {
            yield return new WaitForSeconds(1.5f);
            bool isSetValid = SetValidator.Instance.GetValid();
            Debug.Log("Is set found ? " + isSetValid);
            if (!isSetValid)
            {
                Debug.Log("Id is not valid");
            }
            else
            {
                foreach (var player in playerSelection)
                {
                    if (player.toggle.isOn)
                    {
                        Setting newSet = new Setting(player.nameInput.text, player.typeDropdown.value, player.colorDropdown.value, setIdFieldOff.text);
                        GameSettings.AddSetting(newSet);
                    }
                }
                SceneManager.LoadScene("MainGame");
            }

        }

    public override void OnPlayerEnteredRoom(Player newPlayer)
    {
        for(int i = 0; i < playerSelectionMulti.Count(); i++)
        {
            
            if (playerSelectionMulti[i].nameInput.text == "" && playerSelectionMulti[i].toggle.isOn == false)
            {
                playerNameList[i] = newPlayer.NickName;
                playerIdList[i] = newPlayer.ActorNumber;
                playerSelectionMulti[i].nameInput.text = playerNameList[i];
                playerSelectionMulti[i].playerId = playerIdList[i];
                playerSelectionMulti[i].typeDropdown.value = 0;
                playerSelectionMulti[i].toggle.isOn = true;
                break;
            }
        }
        Debug.Log("player joinned: " + newPlayer.NickName);
        if(PhotonNetwork.IsMasterClient) StartCoroutine(RoomListing(0.3f));
        

    }
    public override void OnPlayerLeftRoom(Player otherPlayer)
    {
        for(int i = 0; i < playerSelectionMulti.Count(); i++)
        {
            if (playerSelectionMulti[i].nameInput.text == otherPlayer.NickName && playerSelectionMulti[i].toggle.isOn == true)
            {
                playerNameList[i] = "";
                playerIdList[i] = -1;
                playerSelectionMulti[i].nameInput.text = playerNameList[i];
                playerSelectionMulti[i].playerId = playerIdList[i];
                playerSelectionMulti[i].typeDropdown.value = 0;
                playerSelectionMulti[i].toggle.isOn = false;
                break;
            }
            
        }
        Debug.Log("player left: " + otherPlayer.NickName);
        if(PhotonNetwork.IsMasterClient) StartCoroutine(RoomListing(0.3f));
    }

    

    public void SupportUs()
    {
        Application.OpenURL(supportUrl);
    }
    public void VisitUs()
    {
        Application.OpenURL(webUrl);
    }



}