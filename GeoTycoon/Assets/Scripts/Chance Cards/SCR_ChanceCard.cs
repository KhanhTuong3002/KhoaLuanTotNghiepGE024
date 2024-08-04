using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Chance Card", menuName = "MasterManger/Cards/Chance")]
public class SCR_ChanceCard : ScriptableObject
{
    public string textOnCard; //Description
    public int rewardMoney; //GET Money
    public int penalityMoney; //PAY Money
    public int moveToBoardIndex = -1;
    public bool payToPlayer;
    [Header("MoveToLocation")]
    public bool nextRailRoad;
    public bool nextUtility;
    public int moveStepsBackward;
    [Header("Jail Content")]
    public bool goToJail;
    public bool jailFreeCard;
    [Header("Street Repairs")]
    public bool streetRepair;
    public int streetRepairsHousePrice = 25;
    public int streetRepairsHotelPrice = 100;
}
