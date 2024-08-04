using Photon.Pun;
using Photon.Realtime;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RoomListingMenu : MonoBehaviourPunCallbacks
{
    [SerializeField]
    private Transform _content;
    [SerializeField]
    private RoomListing _roomlisting;

    private List<RoomListing> _listings = new List<RoomListing>();
    private RoomsCanvases _roomCanvases;
    
    public void FirstInitialize(RoomsCanvases canvases)
    {
        _roomCanvases = canvases;
    }

    public override void OnJoinedRoom()
    {
        _roomCanvases.CurrentRoomCanva.Show();
        _content.DestroyChildren();
        _listings.Clear();
    }

    public override void OnRoomListUpdate(List<RoomInfo> roomList)
    {
        foreach (RoomInfo Info in roomList)
        {
            if(Info.RemovedFromList)
            {
                int index = _listings.FindIndex(x => x.RoomInfo.Name == Info.Name);
                if(index != -1)
                {
                    Destroy(_listings[index].gameObject);
                    _listings.RemoveAt(index);
                }
            }
            else 
            {
                int index = _listings.FindIndex(x => x.RoomInfo.Name == Info.Name);
                if (index == -1)
                {
                    RoomListing listing = Instantiate(_roomlisting, _content);
                    if (listing != null)
                    {
                        listing.SetRoomInfo(Info);
                        _listings.Add(listing);
                    }
                }
                
                   
            }

            
        }
    }
}
