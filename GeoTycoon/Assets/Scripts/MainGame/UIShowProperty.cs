using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using TMPro;
using UnityEngine.UI;

using Photon.Pun;
using Photon.Realtime;

public class UIShowProperty : MonoBehaviourPunCallbacks
{
    MonopolyNode nodeReference = new MonopolyNode();
    Player_Mono playerReference = new Player_Mono();

    [Header("Buy Property UI")]
    [SerializeField] GameObject propertyUIPanel;

    [SerializeField] GameObject quizPanel;
    [SerializeField] GameObject descriptionPanel; 
    [SerializeField] TMP_Text propertyNameText;
    [SerializeField] Image colorField;
    [Space]
    [SerializeField] TMP_Text rentPriceText;//Without a house
    [SerializeField] TMP_Text oneHouseRentText;
    [SerializeField] TMP_Text twoHouseRentText;
    [SerializeField] TMP_Text threeHouseRentText;
    [SerializeField] TMP_Text fourHouseRentText;
    [SerializeField] TMP_Text hotelRentText;
    [Space]
    [SerializeField] TMP_Text housePriceText;
    [SerializeField] TMP_Text mortgagePriceText;
    [Space]
    [SerializeField] Button buyPropertyButton;
    [Space]
    [SerializeField] TMP_Text propertyPriceText;
    [SerializeField] TMP_Text playerMoneyText;
    [SerializeField] Text descriptionText; 
    private bool quizAnsweredCorrectly = false;

    public delegate void UpdateMessage(string message);
    public static UpdateMessage OnUpdateMessage;

    void OnEnable()
    {
        MonopolyNode.OnShowPropertyBuyPanel += ShowBuyPropertyUI;
        QuestionGetter.OnQuestionAnswered += HandleQuestionAnswered;
    }

    void OnDisable()
    {
        MonopolyNode.OnShowPropertyBuyPanel -= ShowBuyPropertyUI;
        QuestionGetter.OnQuestionAnswered -= HandleQuestionAnswered;
    }
    

    void Start()
    {
        propertyUIPanel.SetActive(false);
        quizPanel.SetActive(false);
        descriptionPanel.SetActive(false); 
    }

    void ShowBuyPropertyUI(MonopolyNode node, Player_Mono currentPlayer)
    {
        nodeReference = node;
        playerReference = currentPlayer;
        // Display the quiz panel
        if(PhotonNetwork.IsConnected) QuestionGetter.Instance.SetButton(PhotonNetwork.IsMasterClient);
        quizPanel.SetActive(true);
        propertyUIPanel.SetActive(false);
        descriptionPanel.SetActive(false); 

        // Start the timer
        QuestionGetter.Instance.StartTimer();
    }

    void HandleQuestionAnswered(bool isCorrect, string description)
    {
        quizAnsweredCorrectly = isCorrect;
        quizPanel.SetActive(false);

        if (isCorrect && nodeReference.owner.name=="")
        {
            // Top Panel content
            propertyNameText.text = nodeReference.name;
            rentPriceText.text = "$ " + nodeReference.baseRent;
            oneHouseRentText.text = "$ " + nodeReference.rentWithHouse[0];
            twoHouseRentText.text = "$ " + nodeReference.rentWithHouse[1];
            threeHouseRentText.text = "$ " + nodeReference.rentWithHouse[2];
            fourHouseRentText.text = "$ " + nodeReference.rentWithHouse[3];
            hotelRentText.text = "$ " + nodeReference.rentWithHouse[4];
            housePriceText.text = "$ " + nodeReference.houseCost;
            mortgagePriceText.text = "$ " + nodeReference.MortgageValue;
            propertyPriceText.text = "Price: $ " + nodeReference.price;
            playerMoneyText.text = "You have " + playerReference.ReadMoney;

            buyPropertyButton.interactable = playerReference.CanAfford(nodeReference.price);

            // Show the panel
            descriptionPanel.SetActive(false);
            if(!PhotonNetwork.IsConnected ||(PhotonNetwork.IsConnected && PhotonNetwork.IsMasterClient)) propertyUIPanel.SetActive(true);
            OnUpdateMessage?.Invoke("Correct answer, you now can buy current property.");
            Debug.Log("Correct answer.");
        }
        else if(isCorrect && nodeReference.owner.name!="")
        {
            MonopolyNode.instance.PayRentAfterQuiz(true, playerReference, nodeReference.owner);
        }
        else if(!isCorrect && nodeReference.owner.name!="")
        {
             MonopolyNode.instance.PayRentAfterQuiz(false, playerReference, nodeReference.owner);
        }
        else
        {
            // Show the description panel
            descriptionText.text = description;
            descriptionPanel.SetActive(true);
            StartCoroutine(CloseDescriptionPanelAfterDelay(5f));
            OnUpdateMessage?.Invoke("Incorrect answer, you cannot buy the property. End your turn.");
            Debug.Log("Incorrect answer, you cannot buy the property. End your turn.");
        }
    }

    IEnumerator CloseDescriptionPanelAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        descriptionPanel.SetActive(false);
    }


    // void ShowBuyPropertyUI(MonopolyNode node, Player_Mono currentPlayer)
    // {
    //     nodeReference = node;
        
    //     playerReference = currentPlayer;
        
    //     //top Panel content
    //     propertyNameText.text = node.name;
    //     //coloField.color = node.propertyColorField.color;
    //     //center the card
    //     rentPriceText.text = "$ " + node.baseRent;
    //     oneHouseRentText.text = "$ " + node.rentWithHouse[0];
    //     twoHouseRentText.text = "$ " + node.rentWithHouse[1];
    //     threeHouseRentText.text = "$ " + node.rentWithHouse[2];
    //     fourHouseRentText.text = "$ " + node.rentWithHouse[3];
    //     hotelRentText.text = "$ " + node.rentWithHouse[4];
    //     //cost of building
    //     housePriceText.text = "$ " + node.houseCost;
    //     mortgagePriceText.text = "$ " + node.MortgageValue;
    //     //bottom bar
    //     propertyPriceText.text = "Price: $ " + node.price;
    //     playerMoneyText.text = "You have" + currentPlayer.ReadMoney;
    //     //buy property button
    //     if(currentPlayer.CanAfford(node.price))
    //     {
    //         buyPropertyButton.interactable = true;
    //     }
    //     else
    //     {
    //         buyPropertyButton.interactable = false;
    //     }
    //     //show the panel
    //     if (playerReference.playerId==PhotonNetwork.LocalPlayer.ActorNumber) propertyUIPanel.SetActive(true);
    //     else propertyUIPanel.SetActive(false);
    //     if(!PhotonNetwork.IsConnected) propertyUIPanel.SetActive(true);
    // }

    public void OnClickBuy()
    {
        if(PhotonNetwork.IsConnected)
        {
            PhotonView PV = GetComponent<PhotonView>();
            PV.RPC("BuyPropertyButton", RpcTarget.AllBuffered);
        }
        else
        {
            BuyPropertyButton();
        }
    }

    public void OnClickClose()
    {
        if(PhotonNetwork.IsConnected)
        {
            PhotonView PV = GetComponent<PhotonView>();
            PV.RPC("ClosePropertyButton", RpcTarget.AllBuffered);
        }
        else
        {
            ClosePropertyButton();
        }
    }

    public override void OnLeftRoom()
    {
        OnClickClose();
    }
    
    [PunRPC]

    public void BuyPropertyButton() //this call from the button
    {
        AudioPlayer.instance.Buy();
        //tell the player buy
        playerReference.BuyProperty(nodeReference);
        // maybe colse thr property card

        //make the button not interact anymore
        buyPropertyButton.interactable = false;
    }

    [PunRPC]
    public void ClosePropertyButton() //this call from the button
    {
        //colse the panel
        propertyUIPanel.SetActive(false);
        //clear node reference
        nodeReference=null;
        playerReference=null;
    }
}
