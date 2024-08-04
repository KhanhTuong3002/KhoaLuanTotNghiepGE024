using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Community Card", menuName = "MasterManger/Cards/Community")]
public class SCR_CommunityCard : ScriptableObject
{
    public string textOnCard; //Description
    public int rewardMoney; //GET Money
    public int penalityMoney; //PAY Money
    public int moveToBoardIndex = -1;
    public bool collectFromPlayer;
    [Header("Jail Content")]
    public bool goToJail;
    public bool jailFreeCard;
    [Header("Street Repairs")]
    public bool streetRepair;
    public int streetRepairsHousePrice = 40;
    public int streetRepairsHotelPrice = 115;

}
