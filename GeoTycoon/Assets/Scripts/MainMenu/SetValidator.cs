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
            if(isDataFound) WarningMessageOnl.text = "<color=green>Question pack found!</color>"; 
            else WarningMessageOnl.text = "<color=red>Question pack not found</color>";

            WarningMessageOnl.gameObject.SetActive(true);   
        }
        else
        {
            if(isDataFound) WarningMessageOff.text = "<color=green>Question pack found!</color>";
            else WarningMessageOff.text = "<color=red>Question pack not found</color>";

            WarningMessageOff.gameObject.SetActive(true);   
        }
        StartCoroutine(DisableWarning());
        return isDataFound;
    }

    public IEnumerator DisableWarning()
    {
        yield return new WaitForSeconds(1.5f);
        WarningMessageOnl.gameObject.SetActive(false);   
        WarningMessageOff.gameObject.SetActive(false); 
    }

//<b><color=red>has to stay in Jail</color></b>
    public void GetData(string setID)
    {
        if(PhotonNetwork.IsConnected)
        {
            WarningMessageOnl.text = "Searching...";
            WarningMessageOnl.gameObject.SetActive(true);   
        }
        else
        {
            WarningMessageOff.text = "Searching...";
            WarningMessageOff.gameObject.SetActive(true);   
        }
        
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
            if (request.result == UnityWebRequest.Result.ConnectionError || request.result == UnityWebRequest.Result.ProtocolError || request.downloadHandler.text.Length < 3)
            {
                Debug.Log(request.downloadHandler.text);
                Debug.Log(request.error);
                Debug.Log("Set not found");
                SetIsValid(false);
            }
            else
            {
                Debug.Log(request.downloadHandler.text);
                Debug.Log("Set found");
                isDataFound = true;
                SetIsValid(true);
            }
        }
        
    }
}
