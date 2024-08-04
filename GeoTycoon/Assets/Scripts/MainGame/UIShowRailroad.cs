using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using TMPro;
using UnityEngine.UI;

using Photon.Pun;
public class UIShowRailroad : MonoBehaviourPunCallbacks
{
    MonopolyNode nodeReference;
    Player_Mono playerReference;

    [Header("Buy Railroad UI")]
    [SerializeField] GameObject railroadUIPanel;
    [SerializeField] TMP_Text railroadNameText;
    [SerializeField] Image colorField;
    [Space]
    [SerializeField] TMP_Text oneRailroadRentText;
    [SerializeField] TMP_Text twoRailroadRentText;
    [SerializeField] TMP_Text threeRailroadRentText;
    [SerializeField] TMP_Text fourRailroadRentText;
    [Space]
    [SerializeField] TMP_Text mortgagePriceText;
    [Space]
    [SerializeField] Button buyRailroadButton;
    [Space]
    [SerializeField] TMP_Text railroadPriceText;
    [SerializeField] TMP_Text playerMoneyText;

    void OnEnable()
    {
        MonopolyNode.OnShowRailroadBuyPanel += ShowBuyRailroadUI;
        
    }

     void OnDisable()
    {
        MonopolyNode.OnShowRailroadBuyPanel -= ShowBuyRailroadUI;
    }

    void Start()
    {
        railroadUIPanel.SetActive(false);
    }

    void ShowBuyRailroadUI(MonopolyNode node, Player_Mono currentPlayer)
    {
        nodeReference = node;
        playerReference = currentPlayer;
        //top Panel content
        railroadNameText.text = node.name;
        //coloField.color = node.propertyColorField.color;
        //center the card
        //result = baseRent * (int)Mathf.Pow(2,amount-1);
        oneRailroadRentText.text = "$ " + node.baseRent * (int)Mathf.Pow(2,1-1);
        twoRailroadRentText.text = "$ " + node.baseRent * (int)Mathf.Pow(2,2-1);
        threeRailroadRentText.text = "$ " + node.baseRent * (int)Mathf.Pow(2,3-1);
        fourRailroadRentText.text = "$ " + node.baseRent * (int)Mathf.Pow(2,4-1);
        //cost of building
        mortgagePriceText.text = "$ " + node.MortgageValue;
        //bottom bar
        railroadPriceText.text = "Price: $ " + node.price;
        playerMoneyText.text = "You have" + currentPlayer.ReadMoney;
        //buy property button
        if(currentPlayer.CanAfford(node.price))
        {
            buyRailroadButton.interactable = true;
        }
        else
        {
            buyRailroadButton.interactable = false;
        }
        //show the panel
        if (playerReference.playerId==PhotonNetwork.LocalPlayer.ActorNumber) railroadUIPanel.SetActive(true);
        else railroadUIPanel.SetActive(false);
        if(!PhotonNetwork.IsConnected) railroadUIPanel.SetActive(true);
        
    }

    public void OnClickBuy()
    {
        if(PhotonNetwork.IsConnected)
        {
            PhotonView PV = GetComponent<PhotonView>();
            PV.RPC("BuyRailroadButton", RpcTarget.AllBuffered);
        }
        else
        {
            BuyRailroadButton();
        }
    }

    public override void OnLeftRoom()
    {
        OnClickClose();
    }

    public void OnClickClose()
    {
        if(PhotonNetwork.IsConnected)
        {
            PhotonView PV = GetComponent<PhotonView>();
            PV.RPC("CloseRailroadButton", RpcTarget.AllBuffered);
        }
        else
        {
            CloseRailroadButton();
        }
    }

    [PunRPC]

    public void BuyRailroadButton() //this call from the button
    {
        AudioPlayer.instance.Buy();
        //tell the player buy
        playerReference.BuyProperty(nodeReference);
        // maybe colse thr property card

        //make the button not interact anymore
        buyRailroadButton.interactable = false;
    }
    [PunRPC]
    public void CloseRailroadButton() //this call from the button
    {
        //colse the panel
        railroadUIPanel.SetActive(false);
        //clear node reference
        nodeReference=null;
        playerReference=null;
    }
}
