using System.Collections;
using System.Threading;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

using Photon.Pun;
using System;

public enum MonopolyNodeType
{
    Property,
    Utility,
    Railroad,
    Tax,
    Chance,
    CommunityChest,
    Go,
    Jail,
    FreeParking,
    Gotojail
}
public class MonopolyNode : MonoBehaviourPunCallbacks
{
    public static MonopolyNode instance;
    public MonopolyNodeType monopolyNodeType;
    public Image propertyColorField;
    [Header("Property Name")]
    [SerializeField] internal new string name;
    [SerializeField] TMP_Text nameText;
    [Header("Property Price")]
    public int price;
    public int houseCost;
    [SerializeField] TMP_Text priceText;
    [Header("Property Rent")]
    [SerializeField] bool calculateRentAuto;
    [SerializeField] int currentRent;
    [SerializeField] internal int baseRent;
    [SerializeField] internal List<int> rentWithHouse = new List<int>();
    int numberOfHouses;
    public int NumberOfHouses => numberOfHouses;

    [SerializeField] GameObject[] house;
    [SerializeField] GameObject hotel; 
    [Header("Property Mortgage")]
    [SerializeField] GameObject morgtgageImage;
    [SerializeField] GameObject propertyImage;
    [SerializeField] bool isMortgaged;
    [SerializeField] int mortgageValue;
    [Header("Preperty Owner")]
    [SerializeField] GameObject ownerBar;
    [SerializeField] TMP_Text ownerText;

    //DRAG A COMMUNITY CARD
    public delegate void DrawCommunityCard(Player_Mono player);
    public static DrawCommunityCard OnDrawCommunityCard;
    //DRAG A CHANCE CARD
    public delegate void DrawChanceCard(Player_Mono player);
    public static DrawChanceCard OnDrawChanceCard;
    //human input panel
    public delegate void ShowHumanPanel(bool activatePanel, bool activateRollDice, bool activateEndTurn, bool hasChanceJailCard, bool hasCommunityJailCard);
    public static ShowHumanPanel OnShowHumanPanel;
    //Property buy panel;
    public delegate void ShowPropertyBuyPanel(MonopolyNode node, Player_Mono player);
    public static ShowPropertyBuyPanel OnShowPropertyBuyPanel;
    //railroad buy panel;
    public delegate void ShowRailroadBuyPanel(MonopolyNode node, Player_Mono player);
    public static ShowRailroadBuyPanel OnShowRailroadBuyPanel;
    //utility buy panel;
    public delegate void ShowUtilityBuyPanel(MonopolyNode node, Player_Mono player);
    public static ShowUtilityBuyPanel OnShowUtilityBuyPanel;
    public Player_Mono owner;
    //Message System
    public delegate void UpdateMessage(string message);
    public static UpdateMessage OnUpdateMessage;

    public Player_Mono Owner => owner;

    private void Awake() 
    {
        instance = this;    
    }
    public void SetOwner(Player_Mono newOwner)
    {
        owner = newOwner;
        OnOwnerUpdate();
    }


    private void OnValidate()
    {
        if(nameText != null)
        {
            nameText.text = name;
        }
        //CALCULATION
        if(calculateRentAuto)
        {
            if(monopolyNodeType == MonopolyNodeType.Property)
            {
                if(baseRent > 0) 
                {
                    price = 3 * (baseRent * 10);//baseRent - 1;
                    //MORTGAGE PRICE
                    mortgageValue = price / 2;
                    rentWithHouse.Clear();                  
                        rentWithHouse.Add(baseRent * 5);
                        rentWithHouse.Add(baseRent * 5 * 3);
                        rentWithHouse.Add(baseRent * 5 * 9);
                        rentWithHouse.Add(baseRent * 5 * 16);
                        rentWithHouse.Add(baseRent * 5 * 25);    
                }
                else if(baseRent <= 0) 
                {
                    price = 0;
                    baseRent = 0;
                    rentWithHouse.Clear();
                    mortgageValue = 0;
                }
            }
            if (monopolyNodeType == MonopolyNodeType.Railroad)
            {
                mortgageValue = price / 2;
            }
            if (monopolyNodeType == MonopolyNodeType.Utility)
            {
                mortgageValue = price / 2;
            }
        }
        if (priceText != null)
        {
            priceText.text = "$ " + price;
        }
        //UPDATE THE OWNER
        OnOwnerUpdate();
        UnMortgageProperty();
        //isMortgaged = false;
    }

    public void UpdateColorField(Color color)
    {
        if(propertyColorField != null)
        {
            propertyColorField.color = color;
        }     
    }
    // MONRTGAGE CONTENT

    public void MortagagePropertyButton()
    {
        if (PhotonNetwork.IsMasterClient && PhotonNetwork.IsConnected)
        {
            PhotonView PV = GetComponent<PhotonView>();
            PV.RPC("MortagageProperty", RpcTarget.All);
        } else if (!PhotonNetwork.IsConnected) MortagageProperty();
    }

    [PunRPC]
    public int MortagageProperty()
    {
        isMortgaged = true;
        if (morgtgageImage != null)
        {
            morgtgageImage.SetActive(true);
        }
        if (propertyImage != null)
        {
            propertyImage.SetActive(false);
        }
        return mortgageValue;
    }

    public void UnMortgagePropertyButton()
    {
        if (PhotonNetwork.IsMasterClient && PhotonNetwork.IsConnected)
        {
            PhotonView PV = GetComponent<PhotonView>();
            PV.RPC("UnMortgageProperty", RpcTarget.All);
        } else if (!PhotonNetwork.IsConnected) UnMortgageProperty();
    }
    [PunRPC]

    public void UnMortgageProperty()
    {
        isMortgaged = false;
        if(morgtgageImage != null)
        {
            morgtgageImage.SetActive(false);
        }
       if(propertyImage != null)
        {
            propertyImage.SetActive(true);
        }
        
    }

    public bool IsMortgaged => isMortgaged;
    public int MortgageValue => mortgageValue;

    //UPADTE OWNER
    public void OnOwnerUpdate () 
    {
        if (ownerBar != null)
        {
            if(owner.name != "")
            {
                ownerBar.SetActive(true);
                ownerText.text = owner.name;
            }
            else
            {
                ownerBar.SetActive(false);
                ownerText.text = "";
            }
        }
    }
      
    public void PlayerLandedOnNode(Player_Mono currentPlayer)
    {
        bool playerIsHuman = currentPlayer.playerType == Player_Mono.PlayerType.HUMAN;
        bool continueTurn = true;

        //Check For node type and atc

        switch (monopolyNodeType)
        {
            case MonopolyNodeType.Property:
                if (!playerIsHuman)//Ai
                {
                    //If it owned && if we not are owner && is not mortgaged
                    if (owner.name != "" && owner != currentPlayer && !isMortgaged)
                    {
                        //pay rent to somebody

                        //caculate the  rent
                        Debug.Log("PLAYER MIGHT PAY RENT && OWNER SHIP IS : " + owner.name);
                        int renToPay = CalculatePropertyRent();
                        //pay the rent to the owner
                        currentPlayer.PayRent(renToPay, owner);


                        //show a message about what happend
                        OnUpdateMessage.Invoke(currentPlayer.name + " pay rent of: " + renToPay + " to " + owner.name);
                        Debug.Log(currentPlayer.name + "pay rent of: " + renToPay + " to " + owner.name);
                    }
                    else if (owner.name == "" && currentPlayer.CanAfford(price))
                        {
                        //buy the node
                        OnUpdateMessage.Invoke(currentPlayer.name + " buys "+ this.name);
                        Debug.Log("PLAYER COULD BUY");
                        currentPlayer.BuyProperty(this);
                        //OnOwnerUpdate();

                          //show a message about what happend 
                        }
                    else
                    {
                        //Is unowned and we cant afford it
                    }

        }               
                else //Human
                {
                    //If it owned && if we not are owner && is not mortgaged
                    if (owner.name != "" && owner != currentPlayer && !isMortgaged)
                    {
                        OnShowPropertyBuyPanel.Invoke(this, currentPlayer);
                        
                        //pay rent to somebody

                        int renToPay = CalculatePropertyRent();
                        //pay the rent to the owner
                        currentPlayer.PayRent(renToPay, owner);

                        //show a message about what happend
                        OnUpdateMessage.Invoke(currentPlayer.name + " pay rent of: " + renToPay + " to " + owner.name);
                        Debug.Log(currentPlayer.name + "pay rent of: " + renToPay + " to " + owner.name);
                    }
                    else if (owner.name == "")
                    {
                        OnShowPropertyBuyPanel.Invoke(this, currentPlayer);
                        //Show buy interface for the property
                        


                    }
                    else
                    {
                        //Is unowned and we cant afford it
                    }
                }
                break;
            case MonopolyNodeType.Utility:
                if (!playerIsHuman)//Ai
                {
                    //If it owned && if we not are owner && is not mortgaged
                    if (owner.name != "" && owner != currentPlayer && !isMortgaged)
                    {
                        //pay rent to somebody

                        //caculate the  rent
                        int renToPay = CalculateUtilityRent();
                        //pay the rent to the owner
                        currentPlayer.PayRent(renToPay, owner);


                        //show a message about what happend
                        OnUpdateMessage.Invoke(currentPlayer.name + " pay Utility rent of: " + renToPay + " to " + owner.name);
                        Debug.Log(currentPlayer.name + "pay rent of: " + renToPay + " to " + owner.name);
                    }
                    else if (owner.name == "" && currentPlayer.CanAfford(price))
                    {
                        //buy the node
                        OnUpdateMessage.Invoke(currentPlayer.name + " buys "+ this.name);
                        Debug.Log("PLAYER COULD BUY");
                        currentPlayer.BuyProperty(this);
                        OnOwnerUpdate();

                        //show a message about what happend 
                    }
                    else
                    {
                        //Is unowned and we cant afford it
                    }

                }
                else //Human
                {
                    //If it owned && if we not are owner && is not mortgaged
                    if (owner.name != "" && owner != currentPlayer && !isMortgaged)
                    {
                        //pay rent to somebody

                        //caculate the  rent
                        int renToPay = CalculateUtilityRent();
                        currentPlayer.PayRent(renToPay, owner);

                        //show a message about what happend 
                    }
                    else if (owner.name == "")
                    {
                        //Show buy interface for the property
                        OnShowUtilityBuyPanel.Invoke(this, currentPlayer);
                        

                    }
                    else
                    {
                        //Is unowned and we cant afford it
                    }
                }


                break;
            case MonopolyNodeType.Railroad:
                if (!playerIsHuman)//Ai
                {
                    //If it owned && if we not are owner && is not mortgaged
                    if (owner.name != "" && owner != currentPlayer && !isMortgaged)
                    {
                        //pay rent to somebody

                        //caculate the  rent
                        Debug.Log("PLAYER MIGHT PAY RETN && OWNER SHIP IS : " + owner.name);
                        int renToPay = CalculateRailroadRent();
                        currentRent = renToPay;
                        //pay the rent to the owner
                        currentPlayer.PayRent(renToPay, owner);


                        //show a message about what happend
                        OnUpdateMessage.Invoke(currentPlayer.name + " pay Railroad rent of: " + renToPay + " to " + owner.name);
                        Debug.Log(currentPlayer.name + " pay rent of: " + renToPay + " to " + owner.name);
                    }
                    else if (owner.name == "" && currentPlayer.CanAfford(price))
                    {
                        //buy the node
                        OnUpdateMessage.Invoke(currentPlayer.name + " buys "+ this.name);
                        Debug.Log("PLAYER COULD BUY");
                        currentPlayer.BuyProperty(this);
                        OnOwnerUpdate();

                        //show a message about what happend 
                    }
                    else
                    {
                        //Is unowned and we cant afford it
                    }

                }
                else //Human
                {
                    //If it owned && if we not are owner && is not mortgaged
                    if (owner.name != "" && owner != currentPlayer && !isMortgaged)
                    {
                        //pay rent to somebody

                        int renToPay = CalculateRailroadRent();
                        currentRent = renToPay;
                        //pay the rent to the owner
                        currentPlayer.PayRent(renToPay, owner);

                        //show a message about what happend
                        OnUpdateMessage.Invoke(currentPlayer.name + " pay Railroad rent of: " + renToPay + " to " + owner.name);
                        Debug.Log(currentPlayer.name + " pay rent of: " + renToPay + " to " + owner.name);
                    }
                    else if (owner.name == "")
                    {
                        OnShowRailroadBuyPanel.Invoke(this, currentPlayer);
                        


                    }
                    else
                    {
                        //Is unowned and we cant afford it
                    }
                }


                break;
            case MonopolyNodeType.Tax:
                GameManager.instance.AddTaxToPool(price);
                currentPlayer.PayMoney(price);
                //show a message about what happend 
                OnUpdateMessage.Invoke(currentPlayer.name + " <color=red>pays</color> tax of: "+ price);
                break;

            case MonopolyNodeType.FreeParking:
                AudioPlayer.instance.CarPark();

                int tax = GameManager.instance.GetTaxPool();
                currentPlayer.CollectMoney(tax);
                //show a message about what happend 
                OnUpdateMessage.Invoke(currentPlayer.name + " <color=green>get</color> tax of: "+ tax);
                break;
            case MonopolyNodeType.Gotojail:
                System.Threading.Thread.Sleep(1000); // Delay for 2 seconds
                int indexOnBoard = MonopolyBoard.instance.route.IndexOf(currentPlayer.MyMonopolyNode);
                currentPlayer.GoToJail(indexOnBoard);
                OnUpdateMessage.Invoke(currentPlayer.name + " <color=red>has to go to the jail!</color>");
                continueTurn = false;
                break;
            case MonopolyNodeType.Jail:
                AudioPlayer.instance.Jail();

                currentPlayer.RollToJail();
                OnUpdateMessage.Invoke(currentPlayer.name + " <color=red>has to go to the jail!</color>");                
                continueTurn = false;               
                break;

            case MonopolyNodeType.Chance:
                AudioPlayer.instance.DrawCard();

                OnDrawChanceCard.Invoke(currentPlayer);
                continueTurn = false;

                break;
            case MonopolyNodeType.CommunityChest:
                AudioPlayer.instance.DrawCard();

                OnDrawCommunityCard.Invoke(currentPlayer);
                continueTurn = false;
                break;
        }
        //stop here if needed
        if (!continueTurn)
        {
            return;
        }


        //Continue
        if(!playerIsHuman)
        {
            //Invoke("ContinueGame", GameManager.instance.SecondsBetweenTurns);
            currentPlayer.ChangeState(Player_Mono.AiStates.TRADING);
        }
        else
        {
            bool canEndTurn = !GameManager.instance.RolledADouble && currentPlayer.ReadMoney>=0;
            bool canRollDice = GameManager.instance.RolledADouble && currentPlayer.ReadMoney>=0;
            bool jail1 = currentPlayer.HasChanceJailFreeCard;
            bool jail2 = currentPlayer.HasCommunityJailFreeCard;
            //show UI

            OnShowHumanPanel.Invoke(true,canRollDice,canEndTurn,jail1,jail2);
            if (PhotonNetwork.IsConnected && !PhotonNetwork.IsMasterClient) OnShowHumanPanel.Invoke(false,false,false,false,false);
        }
    }

    //void ContinueGame()
    //{
    //    //if the last roll was a double
    //    if (GameManager.instance.RolledADouble)
    //    {
    //        //roll again
    //        GameManager.instance.RollDice();
    //    }
    //    else
    //    {

    //        //not a double
    //        //switch player
    //        GameManager.instance.SwitchPlayer();
    //    }
    //}

    public void PayRentAfterQuiz(bool isCorrect, Player_Mono currentPlayer)
    {

        //pay rent to somebody
        int renToPay = CalculatePropertyRent();
        //pay the rent to the owner
        if(isCorrect) renToPay = Mathf.RoundToInt(renToPay * 0.25f);
        currentPlayer.PayRent(renToPay, owner);

        //show a message about what happend
        if(isCorrect) OnUpdateMessage.Invoke(currentPlayer.name + " has answeared correctly " + " pay discounted rent of: " + renToPay + " to " + owner.name);
        else OnUpdateMessage.Invoke(currentPlayer.name + " pay rent of: " + renToPay + " to " + owner.name);
        Debug.Log(currentPlayer.name + "pay rent of: " + renToPay + " to " + owner.name);
    }

    int CalculatePropertyRent()
    {
        switch (numberOfHouses)
        {
            case 0:
                // Check if owner has the full set of this nodes
                var (list, allSame) = MonopolyBoard.instance.PlayerHasAllNodesOfSet(this);

                if (allSame)
                {
                    currentRent = baseRent * 2;
                }
                else
                {
                    currentRent = baseRent;
                }

                break;

            case 1:
                currentRent = rentWithHouse[0];
                break;

            case 2:
                currentRent = rentWithHouse[1];
                break;

            case 3: //hotel
                currentRent = rentWithHouse[2];
                break;

            case 4:
                currentRent = rentWithHouse[3];
                break;

            case 5: //hotel
                currentRent = rentWithHouse[4];
                break;

        }

        return currentRent; 
    }

    int CalculateUtilityRent()
    {
        List<int> lastRolledDice = GameManager.instance.LastRolledDice;

        int result = 0;
        var (list, allSame) = MonopolyBoard.instance.PlayerHasAllNodesOfSet(this);
        if(allSame)
        {
            result = (lastRolledDice[0] + lastRolledDice[1]) * 10;
        }
        else
        {
            result = (lastRolledDice[0] + lastRolledDice[1]) * 4;
        }
        return result;
    }
    int CalculateRailroadRent()
    {
        List<int> lastRolledDice = GameManager.instance.LastRolledDice;

        int result = 0;
        var (list, allSame) = MonopolyBoard.instance.PlayerHasAllNodesOfSet(this);
        int amount = 0;
        foreach (var item in list)
        {
            amount += (item.owner == this.owner) ? 1 : 0;
        }
        Debug.Log(list.Count);
        result = baseRent * (int)Mathf.Pow(2,amount-1);
        
        return result;
    }

    void VisualizeHouses()
    {
        switch (numberOfHouses)
        {
            case 0:
                house[0].SetActive(false);
                house[1].SetActive(false);
                house[2].SetActive(false);
                house[3].SetActive(false);
                hotel.SetActive(false);
                break;
            case 1:
                house[0].SetActive(true);
                house[1].SetActive(false);
                house[2].SetActive(false);
                house[3].SetActive(false);
                hotel.SetActive(false);
                break;
            case 2:
                house[0].SetActive(true);
                house[1].SetActive(true);
                house[2].SetActive(false);
                house[3].SetActive(false);
                hotel.SetActive(false);
                break;
            case 3:
                house[0].SetActive(true);
                house[1].SetActive(true);
                house[2].SetActive(true);
                house[3].SetActive(false);
                hotel.SetActive(false);
                break;
            case 4:
                house[0].SetActive(true);
                house[1].SetActive(true);
                house[2].SetActive(true);
                house[3].SetActive(true);
                hotel.SetActive(false);
                break;
            case 5:
                house[0].SetActive(false);
                house[1].SetActive(false);
                house[2].SetActive(false);
                house[3].SetActive(false);
                hotel.SetActive(true);
                break;
        }
    }

    public void BuildHouseOrHotel()
    {
        if (monopolyNodeType ==MonopolyNodeType.Property)
        {
            numberOfHouses++;
            VisualizeHouses();
        }
    }
    public int SellHouseOrHotel()
    {
        if (monopolyNodeType == MonopolyNodeType.Property && numberOfHouses>0)
        {
            numberOfHouses--;
            VisualizeHouses();
            return houseCost / 2; // USE ANY NUMBER HERE
        }
        return 0;
    }

    public void resetNode()
    {
        //if is morgtaged
        if(isMortgaged)
        {
            propertyImage.SetActive(true ); 
            morgtgageImage.SetActive(false );
            isMortgaged = false;    
        }
        //reset house and hotel

    if(monopolyNodeType == MonopolyNodeType.Property)
        {
            numberOfHouses = 0;
            VisualizeHouses();
        }
        //reset the ownner
        //remove property from owner
        owner.RemoveProperty(this);
        owner.name = "";
        owner.ActivateSelector(false);
        owner = null;
        //update UI
        OnOwnerUpdate();
    }

    //---------------------------Trading System --------------------------

    //-------------------------- Chage Node Owner ----------------------------
    public void ChangeOwner(Player_Mono newOnwer)
    {
        owner.RemoveProperty(this);
        newOnwer.AddProperty(this);
        SetOwner(newOnwer);

    }
}
