using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class Player_MonoInfor : MonoBehaviour
{
    [SerializeField] TMP_Text PlayerNameText; 
    [SerializeField] TMP_Text PlayerCashText;
    [SerializeField] GameObject activePlayerArrow;
    
    public void SetPlayerName(string newName)
    {
        PlayerNameText.text = newName;
    }
    public void SetPlayerCash(int currentCash)
    {
        Debug.Log("4." + currentCash);
        PlayerCashText.text = "$ " + currentCash.ToString();
    }

    public void SetPlayerNameandCash(string newName, int currentCash)
    {
        SetPlayerName(newName);
        SetPlayerCash(currentCash);
    }

    public void ActivateArrow(bool active)
    {
        activePlayerArrow.SetActive(active);
    }
}
