using Photon.Pun;
using Photon.Realtime;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TextConect : MonoBehaviourPunCallbacks
{
    // Start is called before the first frame update
    private void Start()
    {
        Debug.Log("Cpnnecting to Photon....", this);
        PhotonNetwork.AutomaticallySyncScene = true;
        PhotonNetwork.NickName = MasterManager.GameSetting.NickName;
        PhotonNetwork.GameVersion = MasterManager.GameSetting.GameVersion;
        PhotonNetwork.ConnectUsingSettings();
    }

    // Update is called once per frame
    public override void OnConnectedToMaster()
    {
        Debug.Log("Connected to PhoTon.", this);
        Debug.Log("My Nick Name is" + PhotonNetwork.LocalPlayer.NickName, this);
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
        Debug.Log("Fail to connect to sever Photon" + cause.ToString());
    }
    public override void OnJoinedLobby()
    {
        print("Joined lobby");
    }
}
