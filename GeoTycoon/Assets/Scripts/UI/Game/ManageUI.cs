using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using TMPro;
using UnityEngine.UI;

using Photon.Pun;
using Photon.Realtime;
using UnityEngine.SceneManagement;

public class ManageUI : MonoBehaviourPunCallbacks
{
    public static ManageUI instance;

    [SerializeField] GameObject managePanel; //TO SHOW AND HIDE 
    [SerializeField] Transform propertyGrid; //YO PARENT PROPERTY SETS TO IT
    [SerializeField] GameObject propertySetPrefab; //
    Player_Mono playerReference;
    MonopolyNode nodeReference;
    List<GameObject> propertyPrefabs = new List<GameObject>();
    [SerializeField] TMP_Text yourMoneyText;
    [SerializeField] TMP_Text systemMessageText;

    void Awake()
    {
        instance = this;
    }
    void Start()
    {
        managePanel.SetActive(false);
    }

    
    public void OpenManager() //CALL FROM BUTTON
    {
        playerReference = GameManager.instance.GetCurrentPlayer;
        CreateProperties();
        if(PhotonNetwork.IsMasterClient || !PhotonNetwork.IsConnected) managePanel.SetActive(true);
        UpdateMoneyText();
        //COMOPARE IF OWNER IS PLAYER REF

        //FILL PROPERTY SETS AND CREATE AS MUCH AS NEEDED
        
    }
    
    public void CloseManager()
    {
        managePanel.SetActive(false);
        ClearProperties();
    }

    void ClearProperties()
    {
        for (int i = propertyPrefabs.Count-1; i >= 0; i--)
        {
            // if(PhotonNetwork.IsMasterClient && PhotonNetwork.IsConnected && SceneManager.GetActiveScene().name == "MainGame") 
            // {
            //     Debug.Log("PhotonView.Find(0).IsMine");
            //     PhotonNetwork.Destroy(propertyPrefabs[i]);
            // }
            // else Destroy(propertyPrefabs[i]);
            Destroy(propertyPrefabs[i]);
        }
        propertyPrefabs.Clear();
    }
    

    void CreateProperties()
    {
        //GET ALL NODES AS NODE SETS
        List<MonopolyNode> processedSet = null;
        foreach (var node in playerReference.GetMonopolyNodes)
        {
            var (list, allSame) = MonopolyBoard.instance.PlayerHasAllNodesOfSet(node);
            List<MonopolyNode> nodeSet = new List<MonopolyNode>();
            nodeSet.AddRange(list);

            if(nodeSet != null && nodeSet != processedSet)
            {
                //UPDATE PROCESSED FIRST
                processedSet = nodeSet;

                nodeSet.RemoveAll(n => n.Owner != playerReference);

                //CREATE PREFAB WITH ALL NODES OWNED BY THE PLAYER
                GameObject newPropertySet = Instantiate(propertySetPrefab,propertyGrid,false);
                
                newPropertySet.GetComponent<ManagePropertyUI>().SetProperty(nodeSet,playerReference);
                propertyPrefabs.Add(newPropertySet);
            }
        }
    }

    public void UpdateMoneyText()
    {
        string showMoney = (playerReference.ReadMoney >= 0)? "<color=green>$" +playerReference.ReadMoney: "<color=red>$" +playerReference.ReadMoney;
        yourMoneyText.text = "<color=black>Your Money:</color> " + showMoney;
    }

    public void UpdateSystemMessage(string message)
    {
        systemMessageText.text = message;
    }

    public void AutoHandleFundsButton()
    {
        if(PhotonNetwork.IsConnected)
        {
            PhotonView PV = GetComponent<PhotonView>();
            PV.RPC("AutoHandleFundsMulti", RpcTarget.All, playerReference.playerId);
        }
        else AutoHandleFunds();
    }
    [PunRPC]
    public void AutoHandleFundsMulti(int playerId)//CALL FROM BUTTON
    {
        List<Player_Mono> playerList = GameManager.instance.GetPlayerList();
        foreach(Player_Mono player in playerList)
        {
            if(playerId == player.playerId) playerReference = player;
        }
        if(playerReference.ReadMoney>0)
        {
            UpdateSystemMessage("You don't need to do that, you have enough money!");
            return;
        }
        playerReference.HandleInsufficientFunds(Mathf.Abs(playerReference.ReadMoney));
        //UPDATE THE UI
        if(PhotonNetwork.IsMasterClient)
        {
            ClearProperties();
            CreateProperties();
            //UPDATE SYSTEM MESSAGE
            UpdateMoneyText();
        }
    }
    public void AutoHandleFunds()//CALL FROM BUTTON
    {
        if(playerReference.ReadMoney>0)
        {
            UpdateSystemMessage("You don't need to do that, you have enough money!");
            return;
        }
        playerReference.HandleInsufficientFunds(Mathf.Abs(playerReference.ReadMoney));
        //UPDATE THE UI
        ClearProperties();
        CreateProperties();
        //UPDATE SYSTEM MESSAGE
        UpdateMoneyText();
    }
    
    public void MortagagePropertyMulti(int playerReferenceId, string nodeName)
    {
        PhotonView PV = GetComponent<PhotonView>();
        PV.RPC("MortgageMultiPlayer", RpcTarget.All, playerReferenceId, nodeName);
    }

    [PunRPC]
    void MortgageMultiPlayer(int playerReferenceId, string nodeName)
    {
        List<Player_Mono> playerList = GameManager.instance.GetPlayerList();
        List<MonopolyNode> nodeList = MonopolyBoard.instance.GetNodeList();
        foreach(Player_Mono player in playerList)
        {
            if(playerReferenceId == player.playerId) playerReference = player;
        }
        foreach(MonopolyNode node in nodeList)
        {
            if (nodeName == node.name) nodeReference = node;
        }

        playerReference.CollectMoney(nodeReference.MortagageProperty());
        UpdateMoneyText();
    }

    
    public void UnMortagagePropertyMulti(int playerReferenceId, string nodeName)
    {
        PhotonView PV = GetComponent<PhotonView>();
        PV.RPC("UnMortgageMultiPlayer", RpcTarget.All, playerReferenceId, nodeName);
    }
    [PunRPC]
    void UnMortgageMultiPlayer(int playerReferenceId, string nodeName)
    {
        List<Player_Mono> playerList = GameManager.instance.GetPlayerList();
        List<MonopolyNode> nodeList = MonopolyBoard.instance.GetNodeList();
        foreach(Player_Mono player in playerList)
        {
            if(playerReferenceId == player.playerId) playerReference = player;
        }
        foreach(MonopolyNode node in nodeList)
        {
            if (nodeName == node.name) nodeReference = node;
        }
        playerReference.PayMoney(nodeReference.MortagageProperty());
        nodeReference.UnMortgageProperty();
        UpdateMoneyText();
    }

    public void BuyHouseMulti(int playerId, string[] nodename)
    {
        PhotonView PV = GetComponent<PhotonView>();
        PV.RPC("BuyHouseMultiCall", RpcTarget.All, playerId, (object)nodename);
    }

    [PunRPC]
    void BuyHouseMultiCall(int playerId, string[] nodename)
    {
        Player_Mono buyer = new Player_Mono();
        List<Player_Mono> playerList = GameManager.instance.GetPlayerList();
        List<MonopolyNode> nodeToBuy = new List<MonopolyNode>();
        List<MonopolyNode> nodeList = MonopolyBoard.instance.GetNodeList();
        foreach(Player_Mono player in playerList)
        {
            if (player.playerId == playerId)
            {
                buyer = player;
                break;
            }
        }
        Debug.Log("" + buyer.name);
        foreach(string nodeName in nodename)
        {
            foreach(MonopolyNode node in nodeList)
            {
                if(node.name == nodeName)
                {
                    nodeToBuy.Add(node);
                    break;
                }
            }
        }
        Debug.Log("" + nodeToBuy[0].name);
        buyer.BuildHouseOrHotelEvenly(nodeToBuy);

        
    }

    public void SellHouseMulti(int playerId, string[] nodename)
    {
        PhotonView PV = GetComponent<PhotonView>();
        PV.RPC("SellHouseMultiCall", RpcTarget.All, playerId, (object)nodename);
    }

    [PunRPC]
    void SellHouseMultiCall(int playerId, string[] nodename)
    {
        Player_Mono seller = new Player_Mono();
        List<Player_Mono> playerList = GameManager.instance.GetPlayerList();
        List<MonopolyNode> nodeToSell = new List<MonopolyNode>();
        List<MonopolyNode> nodeList = MonopolyBoard.instance.GetNodeList();
        foreach(Player_Mono player in playerList)
        {
            if (player.playerId == playerId)
            {
                seller = player;
                break;
            }
        }
        foreach(string nodeName in nodename)
        {
            foreach(MonopolyNode node in nodeList)
            {
                if(node.name == nodeName)
                {
                    nodeToSell.Add(node);
                    break;
                }
            }
        }
        seller.SellHouseEvenly(nodeToSell);
    }
}
