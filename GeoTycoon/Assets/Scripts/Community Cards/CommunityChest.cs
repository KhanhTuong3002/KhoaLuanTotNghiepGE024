using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

using Photon.Pun;

public class CommunityChest :  MonoBehaviourPunCallbacks
{   
    public static CommunityChest instance;
    [SerializeField] List<SCR_CommunityCard> cards = new List<SCR_CommunityCard> ();
    [SerializeField] TMP_Text cardText;
    [SerializeField] GameObject cardHolderBackground;
    [SerializeField] float showTime = 3; //HIDE CARD AUTOMATIC AFTER 3 Seconds
    [SerializeField] Button closeCardButton;

    List<SCR_CommunityCard> cardPool = new List<SCR_CommunityCard>();
    List<SCR_CommunityCard> usedcardPool = new List<SCR_CommunityCard>();
    SCR_CommunityCard jailFreeCard;
    //CURRENT CARD AND CURRENT PLAYER
    SCR_CommunityCard pickedCard;
    Player_Mono currentPlayer;

    //Human input panel
    public delegate void ShowHumanPanel(bool activatePanel, bool activateRollDice, bool activateEndTurn, bool hasChanceJailCard, bool hasCommunityJailCard);
    public static ShowHumanPanel OnShowHumanPanel;


    void OnEnable()
    {
        MonopolyNode.OnDrawCommunityCard += DrawCard;
    }
    void OnDisable()
    {
        MonopolyNode.OnDrawCommunityCard -= DrawCard;
    }
    void Awake()
    {
        instance = this;
    }

    void Start()
    {
        cardHolderBackground.SetActive(false);
        //ADD ALL CARDS TO THE POOL
        cardPool.AddRange(cards);
        //SHUFFLE THE CARDS
        StartCoroutine(ShuffleCards());
    }

    IEnumerator ShuffleCards()
    {
        yield return new WaitForSeconds(0.6f);
        for (int i = 0; i < cardPool.Count; i++)
        {
            int index = Random.Range(0, cardPool.Count);
            if(PhotonNetwork.IsConnected  && PhotonNetwork.IsMasterClient)
            {
                PhotonView PV = GetComponent<PhotonView>();
                PV.RPC("ShuffleCommunityCardsMulti", RpcTarget.AllBuffered, index, i);
            }
            else  if(!PhotonNetwork.IsConnected)
            {
                SCR_CommunityCard tempCard = cardPool[index];
                cardPool[index] = cardPool[i];
                cardPool[i] = tempCard;
            }
        }
        //Debug.Log("community card number: " + cardPool.Count);
    }

    [PunRPC]
    public void ShuffleCommunityCardsMulti(int index, int i)
    {
        SCR_CommunityCard tempCard = cardPool[index];
        cardPool[index] = cardPool[i];
        cardPool[i] = tempCard;
        //Debug.Log("community card number: " + cardPool.Count);
    }

    void DrawCard(Player_Mono cardTaker)
    {
        //DRAW AN ACTUAL CARD
        pickedCard = cardPool[0];
        cardPool.RemoveAt(0);
        if (pickedCard.jailFreeCard)
        {
            jailFreeCard = pickedCard;

        }
        else
        {
            usedcardPool.Add(pickedCard);
        }
        if(cardPool.Count == 0)
        {
            //PUT BACK ALL CARDS
            cardPool.AddRange(usedcardPool);
            usedcardPool.Clear();
            //SHUFFLE ALL
            StartCoroutine(ShuffleCards());
        }
        //WHO IS CURRENT PLAYER
        currentPlayer = cardTaker;
        //SHOWCARD
        cardHolderBackground.SetActive(true);
        //FILL IN THE TEXT
        cardText.text = pickedCard.textOnCard;
        //DEACTIVATE BUTTON IF WE ARE AN AI PLAYER
        if(currentPlayer.playerType == Player_Mono.PlayerType.AI)
        {
            closeCardButton.interactable = false;
            Invoke("ApplyCardEffect", showTime);
        }
        else
        {
            closeCardButton.interactable = true;
            if(PhotonNetwork.IsConnected && PhotonNetwork.MasterClient.ActorNumber == currentPlayer.playerId) 
            {
                closeCardButton.gameObject.SetActive(false);
                StartCoroutine(WaitToClose());
            }
        }
    }

    public IEnumerator WaitToClose()
    {
        yield return new WaitForSeconds(3);
        CloseCardButton();
    }
    
    public override void OnLeftRoom()
    {
        PhotonView PV = GetComponent<PhotonView>();
        PV.RPC("ContinueGameCommunity", RpcTarget.All, false);
    }

    public void CloseCardButton()
    {
        if(PhotonNetwork.IsConnected && PhotonNetwork.IsMasterClient)
        {
            PhotonView PV = GetComponent<PhotonView>();
            PV.RPC("ApplyCardEffectCommunity", RpcTarget.All);
        }
        else if(!PhotonNetwork.IsConnected)
        {
            ApplyCardEffect();
        }
    }
    
    [PunRPC]
    public void ApplyCardEffectCommunity()
    {
        ApplyCardEffect();
    }
    public void ApplyCardEffect() //CLOSE BUTTON OF THE CARD
    {
        bool isMoving = false;
        if(pickedCard.rewardMoney != 0 && !pickedCard.collectFromPlayer)
        {
            currentPlayer.CollectMoney(pickedCard.rewardMoney);
        }
        else if(pickedCard.penalityMoney != 0)
        {
            currentPlayer.PayMoney(pickedCard.penalityMoney); //HANDLE INSUFF FUNDS
        }
        else if(pickedCard.moveToBoardIndex != -1)
        {
            isMoving = true;
            //STEP TO GOAL
            int currentIndex = MonopolyBoard.instance.route.IndexOf(currentPlayer.MyMonopolyNode);
            int lengthOfBoard = MonopolyBoard.instance.route.Count;
            int stepsToMove = 0;
            if(currentIndex < pickedCard.moveToBoardIndex)
            {
                stepsToMove = pickedCard.moveToBoardIndex-currentIndex;
            }
            else if(currentIndex > pickedCard.moveToBoardIndex)
            {
                stepsToMove = lengthOfBoard-currentIndex+pickedCard.moveToBoardIndex;
            }
            //START THE MOVE
            MonopolyBoard.instance.MovePlayertonken(stepsToMove, currentPlayer);
        }
        else if(pickedCard.collectFromPlayer)
        {
            int totalCollected = 0;
            List<Player_Mono> allPlayers = GameManager.instance.GetPlayers;

            foreach (var player in allPlayers)
            {
                if(player != currentPlayer)
                {
                    //PREVENT BANKRUPCY
                    int amount = Mathf.Min(player.ReadMoney, pickedCard.rewardMoney);
                    player.PayMoney(amount);
                    totalCollected+=amount;
                }
            }
            currentPlayer.CollectMoney(totalCollected);
        }
        else if(pickedCard.streetRepair)
        {
            int[] allBuildings = currentPlayer.CountHousesAndHotels();
            int totalCosts = pickedCard.streetRepairsHousePrice * allBuildings[0] + pickedCard.streetRepairsHotelPrice * allBuildings[1];
            currentPlayer.PayMoney(totalCosts);
        }
        else if(pickedCard.goToJail)
        {
            isMoving = true;
            currentPlayer.GoToJail(MonopolyBoard.instance.route.IndexOf(currentPlayer.MyMonopolyNode));
        }
        else if(pickedCard.jailFreeCard) //JAIL FREE CARD
        {
            currentPlayer.AddCommunityJailFreeCard();
        }
        cardHolderBackground.SetActive(false);
        ContinueGameCommunity(isMoving);
    }
    [PunRPC]
    void ContinueGameCommunity(bool isMoving)
    {
        Debug.Log(isMoving);
        if(currentPlayer.playerType == Player_Mono.PlayerType.AI)
        {
            if(!isMoving)
            {
                GameManager.instance.Continue();
            }
        }
        else //HUMAN INPUTS
        {
            if (!isMoving)
            {
                Debug.Log("start");
                bool jail1 = currentPlayer.HasChanceJailFreeCard;
                bool jail2 = currentPlayer.HasCommunityJailFreeCard;
                OnShowHumanPanel.Invoke(true, GameManager.instance.RolledADouble, !GameManager.instance.RolledADouble, jail1, jail2);
                if (PhotonNetwork.IsConnected && !PhotonNetwork.IsMasterClient) OnShowHumanPanel.Invoke(false, GameManager.instance.RolledADouble, !GameManager.instance.RolledADouble, jail1, jail2);
                Debug.Log("end");
            }
        }
    }
    public void AddBackJailFreeCard()
    {
        usedcardPool.Add(jailFreeCard);
        jailFreeCard = null;
        
    }
}
