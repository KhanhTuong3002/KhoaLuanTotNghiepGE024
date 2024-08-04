using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using TMPro;
using UnityEngine.Rendering;
using JetBrains.Annotations;
using UnityEngine.UI;
using Photon.Pun;
// using UnityEditor.Experimental.GraphView;

public class TradingSystem : MonoBehaviourPunCallbacks
{
    public static TradingSystem instance;
    [SerializeField] List<Player_Mono> playerList = new List<Player_Mono>();
    public List<MonopolyNode> nodesList = new List<MonopolyNode>();
    
    [SerializeField] GameObject cardPrefab;
    [SerializeField] GameObject tradePanel;
    [SerializeField] GameObject waitPanel;
    [SerializeField] GameObject resultPanel;
    [SerializeField] TMP_Text resultMessageText;
    [Header("LEFT SIDE")]
    [SerializeField] TMP_Text leftOffererNameText;
    [SerializeField] Transform leftCardGrid;
    [SerializeField] ToggleGroup leftToggleGroup;//TO TOGGLE THE CARD SELECTION
    [SerializeField] TMP_Text leftYourMoneyText;
    [SerializeField] TMP_Text leftOfferMoney;
    [SerializeField] Slider leftMoneySlider;
    List<GameObject> leftCardPrefabList = new List<GameObject>();
    Player_Mono leftPlayerReference;
    [Header("MIDDLE")]
    [SerializeField] Transform buttonGrid;
    [SerializeField] GameObject playerButtonPrefab;
    List<GameObject> playerButtonList = new List<GameObject>();
    [Header("RIGHT SIDE")]
    [SerializeField] TMP_Text rightOffererNameText;
    [SerializeField] Transform rightCardGrid;
    [SerializeField] ToggleGroup rightToggleGroup;//TO TOGGLE THE CARD SELECTION
    [SerializeField] TMP_Text rightYourMoneyText;
    [SerializeField] TMP_Text rightOfferMoney;
    [SerializeField] Slider rightMoneySlider;
    List<GameObject> rightCardPrefabList = new List<GameObject>();
    Player_Mono rightPlayerReference;
    [Header("TRADE OFFER PANEL")]
    [SerializeField] GameObject tradeOfferPanel;
    [SerializeField] TMP_Text leftMessageText, rightMessageText, leftMoneyText, rightMoneyText;
    [SerializeField] GameObject leftCard, rightCard;
    [SerializeField] Image leftColorField, rightColorField;
    [SerializeField] Image leftPropImage, rightPropImage;
    [SerializeField] Sprite houseSprite, railroadSprite, utilitySprite;

    //STORE THE OFFER FOR HUMAN
    Player_Mono currentPlayer, nodeOwner;
    MonopolyNode requestedNode, offeredNode;
    int requestedMoney, offeredMoney;

    //Message System
    public delegate void UpdateMessage(string message);
    public static UpdateMessage OnUpdateMessage;

    private void Awake()
    {
        instance = this;
    }

    private void Start()
    {
        tradePanel.SetActive(false);
        resultPanel.SetActive(false);
        tradeOfferPanel.SetActive(false);
        waitPanel.SetActive(false);
    }
    //--------------------------- FIND MISSING PROPOERTY IN SET ---------------------------AI
    public void FindMissingProperty(Player_Mono currentPlayer)
    {
        List<MonopolyNode> processedSet = null;
        MonopolyNode requestNode = null;
        foreach (var node in currentPlayer.GetMonopolyNodes)
        {
                var (list, allSame) = MonopolyBoard.instance.PlayerHasAllNodesOfSet(node);
                List<MonopolyNode> nodeSet = new List<MonopolyNode>();
                nodeSet.AddRange(list);
            //check �f all habe been purchased
            bool notAllPurchased = list.Any(n => n.Owner == null);
            //AI owns This full set Already;
            if(allSame || processedSet == list || notAllPurchased)
            {
                processedSet = list;
                continue;
            }
            //find the owner by other player
            //buy check if we have more than avegere
            if(list.Count == 2)
            {
                requestNode = list.Find(n => n.Owner != currentPlayer && n.Owner != null);
                if(requestNode != null)
                {
                    //make offer to the owner
                    MakeTradeDecision(currentPlayer, requestNode.Owner,requestNode);
                    break;
                }
            }
            if(list.Count >= 3)
            {
                int hasMostOfSet = list.Count(n => n.Owner == currentPlayer);
                if(hasMostOfSet >= 2)
                {
                    requestNode = list.Find(n => n.Owner != currentPlayer && n.Owner != null);
                    //make offer to the owner of the node
                    MakeTradeDecision(currentPlayer, requestNode.Owner, requestNode);
                    break;
                }
            }
        }

        //CONTINUE IF NOTHING IS FOUND
        if (requestNode == null)
        {
            currentPlayer.ChangeState(Player_Mono.AiStates.IDLE);
        }
    }
    //----------------------------- Make Trade Decision --------------------------------------
      void MakeTradeDecision(Player_Mono currntPlayer, Player_Mono nodeOwner,MonopolyNode requestedNode)
    {
        //Trade With money if Posible
        if(currntPlayer.ReadMoney >= CaulateValueOfNode(requestedNode))
        {
            //trade with money only

            //make trade offer
            MakeTradeOffer(currntPlayer, nodeOwner, requestedNode,null,CaulateValueOfNode(requestedNode),0);
            return;
        }

        bool foundDecision = false;
        //find all incomplete set and exclude the set with the request node
        foreach(var node in currntPlayer.GetMonopolyNodes)
        {
            var checkedSet = MonopolyBoard.instance.PlayerHasAllNodesOfSet(node).list;
            if(checkedSet.Contains(requestedNode))
            {
                //stop checking here
                continue;
            }
            // Valid Node Check
            if (checkedSet.Count(n => n.Owner == currntPlayer) == 1)
            {
                if(CaulateValueOfNode(node) + currntPlayer.ReadMoney >= requestedNode.price)
                {
                    int diffreence = CaulateValueOfNode(requestedNode) - CaulateValueOfNode(node);
                    //dif = 600 -300 > 0
                    //valid trade posible
                    if(diffreence > 0)
                    {
                        MakeTradeOffer(currntPlayer, nodeOwner, requestedNode, node, diffreence, 0);
                    }
                    else
                    {
                        MakeTradeOffer(currntPlayer, nodeOwner, requestedNode, node, 0, Mathf.Abs(diffreence));
                    }
                  
                    //make a trade offer
                    foundDecision = true;
                    break;
                }
            }
        }
        //find out if only one node of the found sey is owned

        //Caculate the value of that node and see if wirh enough money it could be affordanable

        // if so .. make trade offer
        if (!foundDecision)
        {
            currentPlayer.ChangeState(Player_Mono.AiStates.IDLE);
        }
    }
    //-----------------------------Make a trade offer--------------------------------
    void MakeTradeOffer(Player_Mono currentPlayer,Player_Mono nodeOwner, MonopolyNode requestedNode, MonopolyNode offeredNode, int offeredMoney, int requestedMoney)
    {
        int currentPlayerId = 0;
        if(currentPlayer!=null) currentPlayerId = currentPlayer.playerId;

        int nodeOwnerId = 0;
        if(nodeOwner!=null) nodeOwnerId = nodeOwner.playerId;

        string requestedNodeName = "";
        if(requestedNode!=null) requestedNodeName = requestedNode.name;

        string offeredNodeName = "";
        if(offeredNode!=null) offeredNodeName = offeredNode.name;

        if (nodeOwner.playerType == Player_Mono.PlayerType.AI)
        {
            ConsiderTradeOffer(currentPlayer, nodeOwner, requestedNode, offeredNode, offeredMoney, requestedMoney);
        }
        else if(nodeOwner.playerType == Player_Mono.PlayerType.HUMAN) 
        {
            //show Ui for Human
            if(PhotonNetwork.IsConnected && PhotonNetwork.IsMasterClient)
            {
                PhotonView PV = GetComponent<PhotonView>();
                PV.RPC("ShowTradeOfferPanelMulti", RpcTarget.All, currentPlayerId, nodeOwnerId, requestedNodeName, offeredNodeName, offeredMoney, requestedMoney);
            }
            else if (!PhotonNetwork.IsConnected)
            {
                ShowTradeOfferPanel(currentPlayer, nodeOwner, requestedNode, offeredNode, offeredMoney, requestedMoney);
            }
        }
    }

    //---------------------------- Consider trade Offer ----------------------------------AI
    void ConsiderTradeOffer(Player_Mono currentPlayer, Player_Mono nodeOwner, MonopolyNode requestedNode, MonopolyNode offeredNode, int offeredMoney, int requestedMoney)
    {
        int valueOfTheTrade = (CaulateValueOfNode(requestedNode) + requestedMoney) - (CaulateValueOfNode(offeredNode) + offeredMoney);
        //300 - 600 (-300) + 0 - 300 = -600
        //(300 + request 300) - (600 + 0) 
        //(600 + req0) - (300 + offer 300)
        //Watn         //        give
        //200 + 200    > 200 + 100

        // sell a node for money only
        if(requestedNode == null && offeredNode != null && requestedMoney < nodeOwner.ReadMoney / 3 && !MonopolyBoard.instance.PlayerHasAllNodesOfSet(requestedNode).allSame)
        {
            Trade(currentPlayer, nodeOwner, requestedNode, offeredNode, offeredMoney, requestedMoney);
            if (currentPlayer.playerType == Player_Mono.PlayerType.HUMAN)
            {
                if(PhotonNetwork.IsConnected)
                {
                    PhotonView PV = GetComponent<PhotonView>();
                    PV.RPC("TradeResult", RpcTarget.All, true);
                }else TradeResult(true);
            }
            return;
        }
        // just a nomal trade
        if(valueOfTheTrade <= 0 && !MonopolyBoard.instance.PlayerHasAllNodesOfSet(requestedNode).allSame) 
        {
            //Trade the node is valid
            Trade(currentPlayer, nodeOwner, requestedNode, offeredNode, offeredMoney, requestedMoney);
            if (currentPlayer.playerType == Player_Mono.PlayerType.HUMAN)
            {
                if(PhotonNetwork.IsConnected)
                {
                    PhotonView PV = GetComponent<PhotonView>();
                    PV.RPC("TradeResult", RpcTarget.All, true);
                }else TradeResult(true);
            }
        }
        else
        {
            if (currentPlayer.playerType == Player_Mono.PlayerType.HUMAN)
            {
                if(PhotonNetwork.IsConnected)
                {
                    PhotonView PV = GetComponent<PhotonView>();
                    PV.RPC("TradeResult", RpcTarget.All, false);
                }else TradeResult(false);
            }
            //debug line or tell the player thet rejected
            Debug.Log("AI rejected trade offer");
        }
    }
    //---------------------------- Caculator Value Of node -------------------------------AI
     int CaulateValueOfNode(MonopolyNode requestedNode)
    {
        int value = 0;
        if(requestedNode != null)
        {
            if (requestedNode.monopolyNodeType == MonopolyNodeType.Property)
            {
                value = requestedNode.price + requestedNode.NumberOfHouses * requestedNode.houseCost;
            }
            else
            {
                value = requestedNode.price;
            }
            return value;
        }
        return value;
    }
    //--------------------------- Trade the node ----------------------------------------
    void Trade(Player_Mono currentPlayer, Player_Mono nodeOwner, MonopolyNode requestedNode, MonopolyNode offeredNode, int offeredMoney, int requestedMoney)
    {
        //CurrentPlayer needs to
        if(offeredMoney == null) offeredMoney = 0;
        if (requestedMoney == null) requestedMoney = 0;
        if(requestedNode !=null)
        {
            currentPlayer.PayMoney(offeredMoney);
            requestedNode.ChangeOwner(currentPlayer);
            //node owner
            nodeOwner.CollectMoney(offeredMoney);
            nodeOwner.PayMoney(requestedMoney);

            if (offeredNode != null)
            {
                offeredNode.ChangeOwner(nodeOwner);
            }
            // show the message for the ui
            string offeredNodeName = (offeredNode != null) ?" & " + offeredNode.name : "";
            OnUpdateMessage.Invoke(currentPlayer.name + " traded " + requestedNode.name + " for " + offeredMoney + offeredNodeName + " to " + nodeOwner.name);

        }
        else if(offeredNode != null && requestedNode == null)
        {
            currentPlayer.CollectMoney(requestedMoney);
            nodeOwner.PayMoney(requestedMoney);
            offeredNode.ChangeOwner(nodeOwner);
           // show the message for the ui
            OnUpdateMessage.Invoke(currentPlayer.name + " sold " + offeredNode.name + " To " + nodeOwner.name + " for " +requestedMoney);
        }    
        
        //HIDE UI FOR HUMAN ONLY
        CloseTradePanel();
        if (currentPlayer.playerType == Player_Mono.PlayerType.AI)
        {
            currentPlayer.ChangeState(Player_Mono.AiStates.IDLE);
        }
    }

    //---------------------------- USER INTERFACE CONTENT ---------------------------- HUMAN
    //---------------------------- CURRENT PLAYER ------------------------------------ HUMAN
    void CreateLeftPanel()
    {
        leftOffererNameText.text = leftPlayerReference.name;

        List<MonopolyNode> referenceNodes = leftPlayerReference.GetMonopolyNodes;
        for (int i = 0; i < referenceNodes.Count; i++)
        {
            GameObject tradeCard = Instantiate(cardPrefab, leftCardGrid,false);
            //SET UP THE ACTUAL CARD CONTENT
            tradeCard.GetComponent<TradePropertyCard>().SetTradeCard(referenceNodes[i],leftToggleGroup);

            leftCardPrefabList.Add(tradeCard);
        }
        leftYourMoneyText.text = "Your money: " + leftPlayerReference.ReadMoney;
        //SET UP THE MONEY SLIDER AND TEXT
        leftMoneySlider.maxValue = leftPlayerReference.ReadMoney;
        leftMoneySlider.value = 0;
        UpdateLeftSlider(leftMoneySlider.value);
        //leftMoneySlider.onValueChanged.AddListener(UpdateLeftSlider);
        //RESET OLD CONTENT

        tradePanel.SetActive(true);
    }

    public void UpdateLeftSlider(float value)
    {
        leftOfferMoney.text = "Offer Money: $ " + leftMoneySlider.value;
    }

    public void CloseTradePanel()
    {
        tradePanel.SetActive(false);
        waitPanel.SetActive(false);
        ClearAll();
    }

    public void OpenTradePanel()
    {
        leftPlayerReference = GameManager.instance.GetCurrentPlayer;
        rightOffererNameText.text = "Select a Player";

        CreateLeftPanel();

        CreateMiddleButton();
    }
    //---------------------------- SELECTED PLAYER ----------------------------------- HUMAN
    public void ShowRightPlayer(Player_Mono player)
    {
        rightPlayerReference = player;
        //RESET THE CURRENT CONTENT
        ClearRightPanel();

        //SHOW RIGHT PLAYER OR ABOVE PLAYER
        rightOffererNameText.text = rightPlayerReference.name;
        List<MonopolyNode> referenceNodes = rightPlayerReference.GetMonopolyNodes;

        for (int i = 0; i < referenceNodes.Count; i++)
        {
            GameObject tradeCard = Instantiate(cardPrefab, rightCardGrid, false);
            //SET UP THE ACTUAL CARD CONTENT
            tradeCard.GetComponent<TradePropertyCard>().SetTradeCard(referenceNodes[i], rightToggleGroup);

            rightCardPrefabList.Add(tradeCard);
        }
        rightYourMoneyText.text = "Your money: " + rightPlayerReference.ReadMoney;
        //SET UP THE MONEY SLIDER AND TEXT
        rightMoneySlider.maxValue = rightPlayerReference.ReadMoney;
        rightMoneySlider.value = 0;
        UpdateRightSlider(rightMoneySlider.value);

        //UPDATE THE MOBNEY AND THE SLIDER
    }

    //SET UP MIDDLE
    void CreateMiddleButton()
    {
        //CLEAR CONTENT
        for (int i = playerButtonList.Count - 1; i >= 0; i--)
        {
            Destroy(playerButtonList[i]);
        }
        playerButtonList.Clear();

        //LOOP THROUGHT ALL PLAYER 
        List<Player_Mono> allPlayers = new List<Player_Mono>();
        allPlayers.AddRange(GameManager.instance.GetPlayers);
        allPlayers.Remove(leftPlayerReference);

        //AND THE BUTTONS FOR THEM
        foreach (var player in allPlayers)
        {
            GameObject newPlayerButton = Instantiate(playerButtonPrefab,buttonGrid,false);
            newPlayerButton.GetComponent<TradePlayerButton>().SetPlayer(player);

            playerButtonList.Add(newPlayerButton);
        }
    }

    void ClearAll()//IF WE OPEN OR CLOSE TRADE SYSTEM
    {
        rightOffererNameText.text = "Select a Player";
        rightYourMoneyText.text = "Your Money: $ 0";
        rightMoneySlider.maxValue = 0;
        rightMoneySlider.value = 0;
        UpdateRightSlider(rightMoneySlider.value);

        //CLEAR MIDDLE BUTTONS
        for (int i = playerButtonList.Count - 1; i >= 0; i--)
        {
            Destroy(playerButtonList[i]);
        }
        playerButtonList.Clear();

        //CLEAR LEFT CARD CONTENT
        for (int i = leftCardPrefabList.Count - 1; i >= 0; i--)
        {
            Destroy(leftCardPrefabList[i]);
        }
        leftCardPrefabList.Clear();

        //CLEAR RIGHT CARD CONTENT
        for (int i = rightCardPrefabList.Count - 1; i >= 0; i--)
        {
            Destroy(rightCardPrefabList[i]);
        }
        rightCardPrefabList.Clear();
    }

    void ClearRightPanel()
    {
        //CLEAR RIGHT CARD CONTENT
        for (int i = rightCardPrefabList.Count - 1; i >= 0; i--)
        {
            Destroy(rightCardPrefabList[i]);
        }
        rightCardPrefabList.Clear();
        //RESET THE SLIDER
        //SET UP THE MONEY SLIDER AND TEXT
        rightMoneySlider.maxValue = 0;
        rightMoneySlider.value = 0;
        UpdateRightSlider(rightMoneySlider.value);
    }

    public void UpdateRightSlider(float value)
    {
        rightOfferMoney.text = "Requested Money: $ " + rightMoneySlider.value;
    }

    //-----------------------------MAKE OFFER--------------------------HUMAN
    public void MakeOfferButton()//HUMAN INPUT BUTTON
    {
        MonopolyNode requestedNode = null;
        MonopolyNode offeredNode = null;
        if (rightPlayerReference == null)
        {
            //ERROR MESSAGE HERE? - NO PLAYER TO TRADE WITH

            return;
        }

        //LEFT SELECTED NODE
        Toggle offeredToggle = leftToggleGroup.ActiveToggles().FirstOrDefault();
        if (offeredToggle != null)
        {
            offeredNode = offeredToggle.GetComponentInParent<TradePropertyCard>().Node();
        }

        //RIGHT SELECTED NODE
        Toggle requestedToggle = rightToggleGroup.ActiveToggles().FirstOrDefault();
        if (requestedToggle != null)
        {
            requestedNode = requestedToggle.GetComponentInParent<TradePropertyCard>().Node();
        }
        // if(requestedNode == null || offeredNode == null)
        // {
        //     Debug.Log("Phải chọn đủ 2 node bên trái và phải mới được trade");
        //     return;
        // }
        MakeTradeOffer(leftPlayerReference,rightPlayerReference,requestedNode, offeredNode,(int)leftMoneySlider.value,(int)rightMoneySlider.value);
    }


    //-----------------------------TRADE RESULT--------------------------HUMAN
    [PunRPC]
    void TradeResult(bool accepted)
    {
        if (accepted)
        {
            resultMessageText.text = rightPlayerReference.name + "<b><color=green> accepted </color></b>" + "the trade.";
        }
        else
        {
            resultMessageText.text = rightPlayerReference.name + "<b><color=red> rejected </color></b>" + "the trade.";
        }
        resultPanel.SetActive(true);
    }

    //-----------------------------TRADE OFFER PANEL--------------------------HUMAN
    void ShowTradeOfferPanel(Player_Mono _currentPlayer, Player_Mono _nodeOwner, MonopolyNode _requestedNode, MonopolyNode _offeredNode, int _offeredMoney, int _requestedMoney)
    {
        //FILL THE ACTUAL OFFER CONTENT
        currentPlayer = _currentPlayer;
        nodeOwner = _nodeOwner;
        requestedNode = _requestedNode;
        offeredNode = _offeredNode;
        requestedMoney = _requestedMoney;
        offeredMoney = _offeredMoney;
        //SHOW PANEL CONTENT
        tradeOfferPanel.SetActive(true);
        leftMessageText.text = currentPlayer.name + " offers:";
        rightMessageText.text = "For " + nodeOwner.name + "'s:";
        leftMoneyText.text = "+$" + offeredMoney;
        rightMoneyText.text = "+$" + requestedMoney;

        leftCard.SetActive(offeredNode!= null?true:false);
        rightCard.SetActive(requestedNode!=null?true:false);

        if (leftCard.activeInHierarchy)
        {
            leftColorField.color = (offeredNode.propertyColorField != null)?offeredNode.propertyColorField.color : Color.white;
            switch (offeredNode.monopolyNodeType)
            {
                case MonopolyNodeType.Property:
                    leftPropImage.sprite = houseSprite;
                    leftPropImage.color = Color.white;
                    break;
                case MonopolyNodeType.Railroad:
                    leftPropImage.sprite = railroadSprite;
                    leftPropImage.color = Color.white;
                    break;
                case MonopolyNodeType.Utility:
                    leftPropImage.sprite = utilitySprite;
                    leftPropImage.color = Color.black;
                    break;
            }
        }

        if (rightCard.activeInHierarchy)
        {
            rightColorField.color = (requestedNode.propertyColorField != null) ? requestedNode.propertyColorField.color : Color.white;
            switch (requestedNode.monopolyNodeType)
            {
                case MonopolyNodeType.Property:
                    rightPropImage.sprite = houseSprite;
                    rightPropImage.color = Color.white;
                    break;
                case MonopolyNodeType.Railroad:
                    rightPropImage.sprite = railroadSprite;
                    rightPropImage.color = Color.white;
                    break;
                case MonopolyNodeType.Utility:
                    rightPropImage.sprite = utilitySprite;
                    rightPropImage.color = Color.black;
                    break;
            }
        }
    }
    
    [PunRPC]
    void ShowTradeOfferPanelMulti(int _currentPlayer, int _nodeOwner, string _requestedNode, string _offeredNode, int _offeredMoney, int _requestedMoney)
    {
        //FILL THE ACTUAL OFFER CONTENT
        playerList = GameManager.instance.GetPlayerList();
        nodesList = MonopolyBoard.instance.GetNodeList();

        foreach(Player_Mono player in playerList)
        {
            if(player.playerId == _currentPlayer) currentPlayer = player;
            if(player.playerId == _nodeOwner) nodeOwner = player;
        }
        foreach (var node in nodesList)
        {
            if(node.name == _requestedNode) requestedNode = node;
            if(node.name == _offeredNode) offeredNode = node;
        }

        requestedMoney = _requestedMoney;
        offeredMoney = _offeredMoney;
        //SHOW PANEL CONTENT
        if(PhotonNetwork.LocalPlayer.ActorNumber == _nodeOwner) tradeOfferPanel.SetActive(true);
        if(PhotonNetwork.IsMasterClient) waitPanel.SetActive(true);
        leftMessageText.text = currentPlayer.name + " offers:";
        rightMessageText.text = "For " + nodeOwner.name + "'s:";
        leftMoneyText.text = "+$" + offeredMoney;
        rightMoneyText.text = "+$" + requestedMoney;

        leftCard.SetActive(offeredNode!= null?true:false);
        rightCard.SetActive(requestedNode!=null?true:false);

        if (leftCard.activeInHierarchy)
        {
            leftColorField.color = (offeredNode.propertyColorField != null)?offeredNode.propertyColorField.color : Color.white;
            switch (offeredNode.monopolyNodeType)
            {
                case MonopolyNodeType.Property:
                    leftPropImage.sprite = houseSprite;
                    leftPropImage.color = Color.white;
                    break;
                case MonopolyNodeType.Railroad:
                    leftPropImage.sprite = railroadSprite;
                    leftPropImage.color = Color.white;
                    break;
                case MonopolyNodeType.Utility:
                    leftPropImage.sprite = utilitySprite;
                    leftPropImage.color = Color.black;
                    break;
            }
        }

        if (rightCard.activeInHierarchy)
        {
            rightColorField.color = (requestedNode.propertyColorField != null) ? requestedNode.propertyColorField.color : Color.white;
            switch (requestedNode.monopolyNodeType)
            {
                case MonopolyNodeType.Property:
                    rightPropImage.sprite = houseSprite;
                    rightPropImage.color = Color.white;
                    break;
                case MonopolyNodeType.Railroad:
                    rightPropImage.sprite = railroadSprite;
                    rightPropImage.color = Color.white;
                    break;
                case MonopolyNodeType.Utility:
                    rightPropImage.sprite = utilitySprite;
                    rightPropImage.color = Color.black;
                    break;
            }
        }
    }

    public void AcceptButton()
    {
        if(PhotonNetwork.IsConnected)
        {
            PhotonView PV = GetComponent<PhotonView>();
            PV.RPC("AcceptOffer", RpcTarget.All);
        }
        else AcceptOffer();
    }
    public override void OnLeftRoom()
    {
        if(currentPlayer==null || nodeOwner == null) return;
        if(PhotonNetwork.LocalPlayer.ActorNumber == currentPlayer.playerId || PhotonNetwork.LocalPlayer.ActorNumber == nodeOwner.playerId)
        {
            RejectButton();
        }
    }
    public void RejectButton()
    {
        if(PhotonNetwork.IsConnected)
        {
            PhotonView PV = GetComponent<PhotonView>();
            PV.RPC("RejectOffer", RpcTarget.All);
        }
        else RejectOffer();
    }
    [PunRPC]
    public void AcceptOffer()
    {
        Trade(currentPlayer, nodeOwner, requestedNode, offeredNode, offeredMoney, requestedMoney);
        ResetOffer();
        
    }
    [PunRPC]
    public void RejectOffer()
    {
        currentPlayer.ChangeState(Player_Mono.AiStates.IDLE);
        ResetOffer();
    }
    void ResetOffer()
    {
        currentPlayer = null;
        nodeOwner = null;
        requestedNode = null;
        offeredNode = null;
        requestedMoney = 0;
        offeredMoney = 0;
        waitPanel.SetActive(false);
    }
}
