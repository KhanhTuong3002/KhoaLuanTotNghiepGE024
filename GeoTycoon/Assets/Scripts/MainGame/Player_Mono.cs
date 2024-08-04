using System.Collections;
using System.Collections.Generic;
using System.Linq;
//using UnityEditor.Experimental.GraphView;
using UnityEngine;
using TMPro;
using System.Linq.Expressions;
using Photon.Pun;

[System.Serializable]
public class Player_Mono
{
    public enum PlayerType
    {
        HUMAN,
        AI
    }//HUMAN
    public PlayerType playerType;
    public string name;
    public int playerId;
    public bool isStillPlayingMulti;
    int money;
    MonopolyNode currentnode;
    bool isInjail;
    int numTurnsInJail = 0;
    [SerializeField] GameObject myTonken;
    [SerializeField] List<MonopolyNode> myMonopolyNodes = new List<MonopolyNode>();
    public List<MonopolyNode> GetMonopolyNodes => myMonopolyNodes;

    bool hasChanceJailFreeCard, hasCommunityJailFreeCard ;
    public bool HasChanceJailFreeCard => hasChanceJailFreeCard;
    public bool HasCommunityJailFreeCard => hasCommunityJailFreeCard;
    //PLAYERINFOR
    Player_MonoInfor myInfor = new Player_MonoInfor();


    //AI
    int aiMoneySavity = 200;

    //AI STATES
    public enum AiStates
    {
        IDLE,
        TRADING
    }

    public AiStates aiState;


    //RETURN SOME INFORS
    public bool IsInjail => isInjail;
    public GameObject MyTonken => myTonken;
    public MonopolyNode MyMonopolyNode => currentnode;
    public int ReadMoney => money;
    
    //Message System
    public delegate void UpdateMessage(string message);
    public static UpdateMessage OnUpdateMessage;

    public delegate void ShowHumanPanel(bool activatePanel, bool activateRollDice, bool activateEndTurn, bool hasChanceJailCard, bool hasCommunityJailCard);
    public static ShowHumanPanel OnShowHumanPanel;

    public void Inititialize(MonopolyNode startNode, int startMoney, Player_MonoInfor info, GameObject token)
    {
        currentnode = startNode;
        money = startMoney;
        myInfor = info;
        myInfor.SetPlayerNameandCash(name, money);
        myTonken = token;
        myInfor.ActivateArrow(false);
    }
    public void SetMyCurrentNode(MonopolyNode newNode)// turn is over
    {
        currentnode = newNode;
        //Player Landed on node so lets

        newNode.PlayerLandedOnNode(this);
        // if its ai player
        if(playerType == PlayerType.AI)
        {
            // check if can build houses
            CheckIfPlayerHasASet();
            //Check for unmortgage properties
            UnMortgageProperties();

            //Check if he could trde for missing properties
            //TradingSystem.instance.findMissingProperty(this);
        }
    }

    public void CollectMoney(int amount)
    {
        AudioPlayer.instance.Collect();

        money += amount;
        myInfor.SetPlayerCash(money);
        if(playerType == PlayerType.HUMAN && GameManager.instance.GetCurrentPlayer == this)
        {
            bool canEndTurn = !GameManager.instance.RolledADouble && ReadMoney>=0 && GameManager.instance.HasRolledDice;
            bool canRollDice = (GameManager.instance.RolledADouble && ReadMoney >= 0) || (!GameManager.instance.HasRolledDice && ReadMoney >= 0);
            //show UI
            if(PhotonNetwork.IsMasterClient || !PhotonNetwork.IsConnected) OnShowHumanPanel.Invoke(true,canRollDice,canEndTurn,hasChanceJailFreeCard,hasCommunityJailFreeCard);
            else if(!PhotonNetwork.IsMasterClient) OnShowHumanPanel.Invoke(false,canRollDice,canEndTurn,hasChanceJailFreeCard,hasCommunityJailFreeCard);
    
        }
    }
    internal bool CanAfford (int price)
    {
        return price <= money;
    }

    public void BuyProperty(MonopolyNode node)
    {
        money -= node.price;
        node.SetOwner(this);
        //update UI
        myInfor.SetPlayerCash(money);
        //set ownership
        myMonopolyNodes.Add(node);
        //sort all nodes by price
        SortPropertyByPrice();
    }

    void SortPropertyByPrice()
    {
        myMonopolyNodes = myMonopolyNodes.OrderBy (_node => _node.price).ToList();
    }

    internal void PayRent(int rentAmount,Player_Mono owner)
    {
        //dont have enough money
        if(money < rentAmount) 
        {
          //handle insufficent funds > AI
          if  (playerType == PlayerType.AI){
            HandleInsufficientFunds(rentAmount);
            }else{
                OnShowHumanPanel.Invoke(true,false,false,hasChanceJailFreeCard,hasCommunityJailFreeCard);
            }
        }
        money -= rentAmount;
        owner.CollectMoney(rentAmount);
        //Update Ui
        myInfor.SetPlayerCash(money);
    }

    internal void PayMoney(int amount)
    {
        AudioPlayer.instance.Pay();
        //dont have enough money
        if (money < amount)
        {
            if  (playerType == PlayerType.AI){
            HandleInsufficientFunds(amount);
            }
            // else{
            //     OnShowHumanPanel.Invoke(true,false,false);
            // }
        }
        money -= amount;
        //Update Ui
        myInfor.SetPlayerCash(money);

        if(playerType == PlayerType.HUMAN && GameManager.instance.GetCurrentPlayer == this)
        {
            bool canEndTurn = !GameManager.instance.RolledADouble && ReadMoney>=0 && GameManager.instance.HasRolledDice;
            bool canRollDice = (GameManager.instance.RolledADouble && ReadMoney>=0) || (!GameManager.instance.HasRolledDice && ReadMoney >= 0);
            //show UI
            if(PhotonNetwork.IsMasterClient || !PhotonNetwork.IsConnected) OnShowHumanPanel.Invoke(true,canRollDice,canEndTurn,hasChanceJailFreeCard,hasCommunityJailFreeCard);
            else if(!PhotonNetwork.IsMasterClient) OnShowHumanPanel.Invoke(false,canRollDice,canEndTurn,hasChanceJailFreeCard,hasCommunityJailFreeCard);
            
            
        
        }
    }

    //--------------------------JAIL-------------------------------------

    public void GoToJail(int indexOnBoard)
    {
        isInjail = true;
        //Reposition Player
        //myTonken.transform.position = MonopolyBoard.instance.route[8].transform.position;
        //currentnode = MonopolyBoard.instance.route[8];
        MonopolyBoard.instance.MovePlayertonken(CalculateDistanceFromJail(indexOnBoard), this);
        GameManager.instance.ResetRolledADouble();
    }

    public void RollToJail()
    {
        isInjail = true;
        GameManager.instance.EndTurnButton();

    }

    public void setOutOfJail()
    {
        isInjail = false;
        //reset turn in jail
        numTurnsInJail = 0;
    }

    int CalculateDistanceFromJail(int indexOnBoard)
    {
        int result = 0;
        int indexOfjail = 8;
        if(indexOnBoard > indexOfjail)
        {
            result = (indexOnBoard - indexOfjail) * -1;
        }
        else
        {
            result = (indexOfjail - indexOnBoard);
        }
        return result;
    }


    public int NumTurnInjail => numTurnsInJail;

    public void IcreaseNumTurnInJail()
    {
        numTurnsInJail++;
    }

    //-----------------------SREET REPAIR-----------------------------------
    public int[] CountHousesAndHotels()
    {
        int houses = 0; //GOES TO INDEX 0
        int hotels = 0; //GOES TO INDEX 1

        foreach (var node in myMonopolyNodes)
        {
            if(node.NumberOfHouses!=5)
            {
                houses+= node.NumberOfHouses;
            }
            else 
            {
                hotels+=1;
            }
        }

        int[] allBuildings = new int[]{houses, hotels};
        return allBuildings;
    }
    //---------------------------HANDLE INSUFFICIENT FUND---------------------------
    public void HandleInsufficientFunds(int amountToPay)
    {
        int housesToSell = 0; // AVAILABLE HOUSE TO SELL
        int allHouses = 0;
        int propertiesToMortgage = 0;
        int allPropertiesToMortgage = 0;

        //COUNT ALL HOUSES
        foreach (var node in myMonopolyNodes)
        {
            allHouses += node.NumberOfHouses;
        }

        //LOOP THROUGH THE PROPERTIES AND TRY TO SELL AS MUCH AS NEEDED
        while (money < amountToPay && allHouses > 0 )
        {
            foreach (var node in myMonopolyNodes)
            {
                housesToSell = node.NumberOfHouses;
                if (housesToSell > 0)
                {
                    CollectMoney(node.SellHouseOrHotel());
                    allHouses--;
                    // DO WE NEED MORE MONEY?
                    if (money >= amountToPay)
                    {
                        return;
                    }
                }
            }
        }
        // MORTGAGE 
        foreach (var node in myMonopolyNodes)
        {
            allPropertiesToMortgage += (!node.IsMortgaged) ? 1 : 0;
        }

        //LOOP THROUGH THE PROPERTIES AND TRY TO MORTGAGE AS MUCH AS NEEDED
        while (money < amountToPay && allPropertiesToMortgage > 0)
        {
            foreach (var node in myMonopolyNodes)
            {
                propertiesToMortgage = (!node.IsMortgaged) ? 1 : 0;
                if (propertiesToMortgage > 0)
                {
                    CollectMoney(node.MortagageProperty());
                    allPropertiesToMortgage--;
                    // DO WE NEED MORE MONEY?
                    if (money >= amountToPay)
                    {
                        return;
                    }
                }
            }
        }
        if(playerType == PlayerType.AI )
        {
             // WE GO BANKRUPT IF WE REACH THIS POINT
            Bankrupt();
        }
       
    }
    //---------------------------BANKRUPT GAME OVER ---------------------------
    internal void Bankrupt()
    {
        //TAKE OUT THE PLAYER OF THE GAME

        //SEND A MESSAGE TO MESSAGE SYSTEM 
        OnUpdateMessage.Invoke(name + " <color=red>is Bankrupt</color>");
        //CLEAR ALL WHAT THE PLAYER HAS OWNED
        for (int i = myMonopolyNodes.Count - 1; i >= 0; i--)
        {
            myMonopolyNodes[i].resetNode();
        }

        if (hasChanceJailFreeCard)
        {
            ChanceField.instance.AddBackJailFreeCard();
        }
        if (hasCommunityJailFreeCard)
        {
            CommunityChest.instance.AddBackJailFreeCard();
        }
        // REMOVE THE PLAYER
        GameManager.instance.RemovePlayer(this);
    }

/*    public void RemoveProperty(MonopolyNode node)
    {
        myMonopolyNodes.Remove(node);
    }*/
    //---------------------------UNMORTGAGE PROPERTY ---------------------------
    void UnMortgageProperties()
    {
        //FOR AI
        foreach (var node in myMonopolyNodes)
        {
            if (node.IsMortgaged)
            {
                int cost = node.MortgageValue + (int)(node.MortgageValue * 0.1f); //10% Interest
                //CAN WE AFFORT TO UNMORTGAGE
                if (money >= aiMoneySavity + cost)
                {
                    PayMoney(cost);
                    node.MortagageProperty();
                }
            }
        }
    }
    //---------------------------CHECK IF PLAYER HAS A PROPERTY SET---------------------------
    void CheckIfPlayerHasASet()
    {
        //call it only once per set
        List<MonopolyNode> prosetssedSet = null;
        //store and compare
        foreach (var node in myMonopolyNodes)
        {
            var (list, allsame) = MonopolyBoard.instance.PlayerHasAllNodesOfSet(node);
            if (!allsame)
            {
                continue;
            }
            List<MonopolyNode> nodeSets = list;
            if (nodeSets != null && nodeSets != prosetssedSet)
            {
                bool hasMortgagedNode = nodeSets.Any(node => node.IsMortgaged) ? true : false;
                if (!hasMortgagedNode)
                {
                    if (nodeSets[0].monopolyNodeType == MonopolyNodeType.Property)
                    {
                        //we could build a House on set
                        BuildHouseOrHotelEvenly(nodeSets);
                        //Update Process set over here
                        prosetssedSet = nodeSets;
                    }
                }
            }
        }
    }
    //---------------------------BUILD HOUSE ENVENLY ON NODE SETS---------------------------
    internal void BuildHouseOrHotelEvenly(List<MonopolyNode> nodesToBuildOn)
    {
        int minHouse = int.MaxValue;
        int maxHouse = int.MinValue;
        //get min and max number of  houses currently on the property
        foreach (var node in nodesToBuildOn)
        {
            int numOfHouse = node.NumberOfHouses;
            if (numOfHouse < minHouse)
            {
                minHouse = numOfHouse;
            }
            if (numOfHouse > maxHouse && numOfHouse < 5)
            {
                maxHouse = numOfHouse;
            }
        }

        //buy houses on the properties for max allowed on the property
        foreach (var node in nodesToBuildOn)
        {
            if (node.NumberOfHouses == minHouse && node.NumberOfHouses < 5 && CanAffordHouse(node.houseCost))
            {
                node.BuildHouseOrHotel();
                PayMoney(node.houseCost);
                //stop the loof if it only should run one
                break;
            }
        }
    }
    internal void SellHouseEvenly(List<MonopolyNode> nodesToSellFrom)
    {
        int minHouse = int.MaxValue;
        bool houseSold = false;
        foreach (var node in nodesToSellFrom)
        {
            minHouse = Mathf.Min(minHouse, node.NumberOfHouses);
        }
        for (int i = nodesToSellFrom.Count - 1; i >= 0; i--)
        {
            if (nodesToSellFrom[i].NumberOfHouses > minHouse)
            {
                CollectMoney(nodesToSellFrom[i].SellHouseOrHotel());
                houseSold = true;
                break;
            }
        }
        if (!houseSold)
        {
            CollectMoney(nodesToSellFrom[nodesToSellFrom.Count - 1].SellHouseOrHotel());
        }
    }

    //---------------------------- Can Affort----------------------------------------
    public bool CanAffordHouse(int price)
    {
        if (playerType == PlayerType.AI)//AI Only
        {
            return (money - aiMoneySavity) >= price;
        }
        //Human Only
        return money >= price;
    }
    //----------------------------- Selector ----------------------------------------
    public void ActivateSelector(bool active)
    {
        myInfor.ActivateArrow(active);
    }
    //----------------------------- TRADING SYSTEM ------------------------------------------

    //---------------------------- Remove and Add node ----------------------------------
     public void AddProperty(MonopolyNode node)
    {
        myMonopolyNodes.Add(node);
        //sort property by price
        SortPropertyByPrice();
    }
    public void RemoveProperty(MonopolyNode node)
    {
        myMonopolyNodes.Remove(node);
        //sort property by price
        SortPropertyByPrice();
    }

    //----------------------------- STATE MACHINE ------------------------------------------
    public void ChangeState(AiStates state)
    {
        if(playerType == PlayerType.HUMAN)
        {
            return;
        }
        aiState = state;
        switch (aiState)
        {
            case AiStates.IDLE:
                {
                    //CONTINUE THE GAME
                    GameManager.instance.Continue();
                    
                }
                break;
            case AiStates.TRADING:
                {
                    //HOLD THE GAME UNTIL CONTINUED
                    TradingSystem.instance.FindMissingProperty(this);
                }
                break;
        }
    }

    //--------------------------- JAIL FREE CARD ------------------------------------------

    public void AddChanceJailFreeCard()
    {
        hasChanceJailFreeCard = true;
    }
    public void AddCommunityJailFreeCard()
    {   
        hasCommunityJailFreeCard = true;
    }
    public void UseCommunityJailFreeCard() //JAIL2
    {
        if (!IsInjail)
        {
            return;
        }
        hasCommunityJailFreeCard = false;
        setOutOfJail();
        CommunityChest.instance.AddBackJailFreeCard();
        OnUpdateMessage.Invoke(name+ " <color:Green>used the jail free card!</color>");
    }
    public void UseChanceJailFreeCard() //JAIL1
    {
        if (!IsInjail)
        {
            return;
        }
        hasChanceJailFreeCard = false;
        setOutOfJail();
        ChanceField.instance.AddBackJailFreeCard();
        OnUpdateMessage.Invoke(name+ " <color:Green>used the jail free card!</color>");
    }

    //--------------------------- HOUSE AND HOTLE - CAN AFFORT AND COUNT ------------------



}
