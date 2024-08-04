using System.Collections;
using System.Collections.Generic;
using Photon.Pun;
using Photon.Realtime;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ConnectPhoton : MonoBehaviourPunCallbacks
{
    // Start is called before the first frame update
    public GameObject loadingScreen;
    public GameObject lobby;
    public TMP_Text loadingText;
    public GameObject singlePlayerMenu;
    public void ConnectToServerPhoton()
    {
        singlePlayerMenu.SetActive(false);
        lobby.SetActive(false);
        Debug.Log("Connecting to Photon....", this);
        loadingScreen.SetActive(true);
        loadingText.text = "Connecting To Server...";
        // PhotonNetwork.AutomaticallySyncScene = true;
        // PhotonNetwork.NickName = MasterManager.GameSetting.NickName;
        // PhotonNetwork.GameVersion = MasterManager.GameSetting.GameVersion;

        PhotonNetwork.ConnectUsingSettings();
    }

    // Update is called once per frame
    public override void OnConnectedToMaster()
    {
        Debug.Log("Connected to PhoTon.", this);

        PhotonNetwork.AutomaticallySyncScene=true;
        loadingText.text = "Connected !";
        // Debug.Log("My Nick Name is" + PhotonNetwork.LocalPlayer.NickName, this);
        if (!PhotonNetwork.InLobby)
        {
            PhotonNetwork.JoinLobby();
        }
    /*    print("Connected to Sever.");
        print(PhotonNetwork.LocalPlayer.NickName);*/
    }

    public override void OnDisconnected(DisconnectCause cause)
    {
       // print("Disconnected Form Sever for reason "+ cause.ToString());
        // Debug.Log("Fail to connect to sever Photon" + cause.ToString());
        loadingText.text = "Lost Connection";
    }
    public override void OnJoinedLobby()
    {
        loadingScreen.SetActive(!PhotonNetwork.InLobby);
        lobby.SetActive(true);
        //print("Joined lobby");
    }
    
}

