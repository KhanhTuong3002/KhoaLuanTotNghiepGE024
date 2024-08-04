using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

using Photon.Pun;
using TMPro;
using Photon.Realtime;
using UnityEngine.SceneManagement;
using System.Linq;

public class CreateAndJoinRoom : MonoBehaviourPunCallbacks
{
    public GameObject lobbyLayout;
    public GameObject multiPlayerMenu;
    public TMP_InputField createId;
    public TMP_InputField joinId;
    public TMP_InputField nickName;
    public TMP_InputField setQuestionId;

    public static CreateAndJoinRoom instance;

    private void Awake() 
    {
        instance = this;
    }
    public string GetSetId()
    {
        return setQuestionId.text;
    }
    [PunRPC]
    public void AllLeaveRoom()
    {
        PhotonNetwork.LeaveRoom();
        multiPlayerMenu.SetActive(false);
        lobbyLayout.SetActive(true);
    }

    public void CreateRoom()
    {
        if(createId.text == "") return;
        SetValidator.Instance.GetData(setQuestionId.text);
        StartCoroutine(Create());
    }

    public IEnumerator Create()
    {
        yield return new WaitForSeconds(1.5f);
        bool isSetValid = SetValidator.Instance.GetValid();
        if (isSetValid)
        {
            PhotonNetwork.NickName = nickName.text;
            PhotonNetwork.CreateRoom(createId.text);
        }
    }
    public void JoinRoom()
    {
        if(joinId.text == "") return;
        PhotonNetwork.NickName = nickName.text;
        PhotonNetwork.JoinRoom(joinId.text);
    }
    public void LeaveRoom()
    {
        if(PhotonNetwork.IsMasterClient)
        {
            PhotonNetwork.CurrentRoom.IsOpen = false;
            PhotonView PV = GetComponent<PhotonView>();
            PV.RPC("AllLeaveRoom", RpcTarget.Others);
        }
        PhotonNetwork.LeaveRoom();
        multiPlayerMenu.SetActive(false);
        lobbyLayout.SetActive(true);
    }
    public void LeaveLobby()
    {
        PhotonNetwork.LeaveLobby();
        PhotonNetwork.Disconnect();
        SceneManager.LoadScene("MainMenu");
    }

    public override void OnJoinedRoom()
    {
        multiPlayerMenu.SetActive(true);
        if(PhotonNetwork.IsMasterClient)
        {
            Debug.Log("You are room master.");
            PhotonNetwork.CurrentRoom.MaxPlayers = 4;
        }
        
        Debug.Log("Join Success");
        lobbyLayout.SetActive(false);

        // if(!PhotonNetwork.IsMasterClient)
        // {
            
        // }


    }
    public override void OnJoinRoomFailed(short returnCode, string message)
    {
        Debug.Log("Join Failed");
        Debug.Log("" + returnCode);
        Debug.Log("" + message);
        foreach (Transform child in transform)
        {
            Destroy(child.gameObject);
        }
    }

    

    public override void OnLeftRoom()
    {
        //PhotonNetwork.LeaveRoom(true);
        PhotonNetwork.NickName = null;
        Debug.Log("Leave room");
    }
}
