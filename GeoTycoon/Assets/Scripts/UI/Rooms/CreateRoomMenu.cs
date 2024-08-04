using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Photon.Realtime;

public class CreateRoomMenu : MonoBehaviourPunCallbacks
{
    [SerializeField]
    private Text _roomName;

    [SerializeField]
    private Dropdown _playerCountDropdown;


    private RoomsCanvases _roomsCanvases;
    public void FirstInitialize(RoomsCanvases canvases)
    {
        _roomsCanvases = canvases;
    }

    public void OnClick_CreateRoom()
    {
        //create Rooms
        //Join OR create Room
        if(!PhotonNetwork.IsConnected) return;

        RoomOptions options = new RoomOptions();

        // Lấy số lượng người chơi được chọn từ dropdown
        int selectedValue = int.Parse(_playerCountDropdown.options[_playerCountDropdown.value].text);
        options.MaxPlayers = (byte)selectedValue;

        PhotonNetwork.JoinOrCreateRoom(_roomName.text, options, TypedLobby.Default);

    }

    public override void OnCreatedRoom()
    {
        Debug.Log("Create Room Successfuly.", this);
        _roomsCanvases.CurrentRoomCanva.Show();
    }

    public override void OnCreateRoomFailed(short returnCode, string message)
    {
        Debug.Log("Room Creation Failed." +message, this);
    }
}
