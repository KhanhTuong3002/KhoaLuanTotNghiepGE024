using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

using Photon.Pun;
using UnityEngine.Networking;

public class SetValidator : MonoBehaviourPunCallbacks
{
    public static SetValidator Instance { get; private set;}
    public TMP_Text WarningMessageOff;
    public TMP_Text WarningMessageOnl;
    public string URL;
    bool isDataFound = false;
    
    // Start is called before the first frame update

    private void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        WarningMessageOff.gameObject.SetActive(false);
        WarningMessageOnl.gameObject.SetActive(false);      
    }

    // Update is called once per frame

    public void SetIsValid(bool isSetValid)
    {
        isDataFound = isSetValid;
    }
    public bool GetValid()
    {
        if(PhotonNetwork.IsConnected)
        {
            if(isDataFound) WarningMessageOnl.gameObject.SetActive(false);   
            else WarningMessageOnl.gameObject.SetActive(true);   
        }
        else
        {
            if(isDataFound) WarningMessageOff.gameObject.SetActive(false);   
            else WarningMessageOff.gameObject.SetActive(true);   
        }
        return isDataFound;
    }


    public void GetData(string setID)
    {
        if(setID=="") setID = "ec74b952-469c-4fc4-a175-498776ac12e3";
        StartCoroutine(FetchData(setID));
        Debug.Log("****QuizManager receive your SetID:**** " + setID);
        // return GetValid();
    }


    public IEnumerator FetchData(string setID)
    {
        
        using (UnityWebRequest request = UnityWebRequest.Get(URL + setID))
        {
            yield return request.SendWebRequest();
            Debug.Log("Searching...");
            if (request.result == UnityWebRequest.Result.ConnectionError || request.result == UnityWebRequest.Result.ProtocolError)
            {
                Debug.Log(request.error);
                Debug.Log("Set not found");
                SetIsValid(false);
            }
            else
            {
                Debug.Log("Set found");
                isDataFound = true;
                SetIsValid(true);
            }
        }
        
    }
}
