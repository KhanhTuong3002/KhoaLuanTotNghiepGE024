using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class RadomPlayerGenerator : MonoBehaviour
{
    [SerializeField]
    private Text _text;
    private ExitGames.Client.Photon.Hashtable _myCustompProperties = new ExitGames.Client.Photon.Hashtable();  

    private void SetCutomNumber()
    {
        System.Random rand = new System.Random();
        int result = rand.Next(0, 10);
        _text.text = result.ToString();
        _myCustompProperties["result"] = result;
        PhotonNetwork.LocalPlayer.CustomProperties = _myCustompProperties;
    }

    public void OnClick_Button()
    {
        SetCutomNumber();
    }
}
