using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RoomsCanvases : MonoBehaviour
{
    [SerializeField]
    private CreateOrJoinRoomCanva _createOrJoinRoomCanva;
    public CreateOrJoinRoomCanva CreateOrJoinRoomCanva { get { return _createOrJoinRoomCanva; } }

    [SerializeField]
    private CurrentRoomCanva _currentRoomCanva;
    public CurrentRoomCanva CurrentRoomCanva { get { return _currentRoomCanva; } }

    private void Awake()
    {
        FirstInitialize();
    }

    private void FirstInitialize()
    {
        CreateOrJoinRoomCanva.FirstInitialize(this);
        CurrentRoomCanva.FirstInitialize(this);
    }
}
