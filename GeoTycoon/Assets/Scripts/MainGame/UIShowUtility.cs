using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using TMPro;
using UnityEngine.UI;

using Photon.Pun;
public class UIShowUtility : MonoBehaviourPunCallbacks
{
    MonopolyNode nodeReference;
    Player_Mono playerReference;

    [Header("Buy Utility UI")]
    [SerializeField] GameObject utilityUIPanel;
    [SerializeField] TMP_Text utilityNameText;
    [SerializeField] Image colorField;
    [Space]
    [SerializeField] TMP_Text mortgagePriceText;
    [Space]
    [SerializeField] Button buyUtilityButton;
    [Space]
    [SerializeField] TMP_Text utilityPriceText;
    [SerializeField] TMP_Text playerMoneyText;

    void OnEnable()
    {
        MonopolyNode.OnShowUtilityBuyPanel += ShowBuyUtilityUI;
        
    }

     void OnDisable()
    {
        MonopolyNode.OnShowUtilityBuyPanel -= ShowBuyUtilityUI;
    }

    void Start()
    {
        utilityUIPanel.SetActive(false);
    }

    void ShowBuyUtilityUI(MonopolyNode node, Player_Mono currentPlayer)
    {
        nodeReference = node;
        playerReference = currentPlayer;
        //top Panel content
        utilityNameText.text = node.name;
        //coloField.color = node.propertyColorField.color;
        //center the card
        //result = baseRent * (int)Mathf.Pow(2,amount-1);
        //cost of building
        mortgagePriceText.text = "$ " + node.MortgageValue;
        //bottom bar
        utilityPriceText.text = "Price: $ " + node.price;
        playerMoneyText.text = "You have" + currentPlayer.ReadMoney;
        //buy property button
        if(currentPlayer.CanAfford(node.price))
        {
            buyUtilityButton.interactable = true;
        }
        else
        {
            buyUtilityButton.interactable = false;
        }
        //show the panel
        
        if (playerReference.playerId==PhotonNetwork.LocalPlayer.ActorNumber) utilityUIPanel.SetActive(true);
        else utilityUIPanel.SetActive(false);
        if(!PhotonNetwork.IsConnected) utilityUIPanel.SetActive(true);
    }

    public void OnClickBuy()
    {
        if(PhotonNetwork.IsConnected)
        {
            PhotonView PV = GetComponent<PhotonView>();
            PV.RPC("BuyUtilityButton", RpcTarget.AllBuffered);
        }
        else
        {
            BuyUtilityButton();
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
            PV.RPC("CloseUtilityButton", RpcTarget.AllBuffered);
        }
        else
        {
            CloseUtilityButton();
        }
    }

    [PunRPC]

    public void BuyUtilityButton() //this call from the button
    {
        AudioPlayer.instance.Buy();
        //tell the player buy
        playerReference.BuyProperty(nodeReference);
        // maybe colse thr property card

        //make the button not interact anymore
        buyUtilityButton.interactable = false;
    }
    [PunRPC]
    public void CloseUtilityButton() //this call from the button
    {
        //colse the panel
        utilityUIPanel.SetActive(false);
        //clear node reference
        nodeReference=null;
        playerReference=null;
    }
}
