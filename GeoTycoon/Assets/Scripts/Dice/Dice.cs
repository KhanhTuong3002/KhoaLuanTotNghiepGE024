using System.Collections;
using System.Collections.Generic;
using Photon.Pun;
using UnityEngine;

public class Dice : MonoBehaviour
{
    Rigidbody rb;
    bool hasLanded;
    bool thrown;

    Vector3 initPosition;
    int diceValue;
    [SerializeField] DiceSide[] diceSides;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        initPosition = transform.position;
        rb.useGravity = false;
        rb.isKinematic = true;
    }

    void Update()
    {
        if(rb.IsSleeping() && !hasLanded && thrown)
        {
            hasLanded = true;
            rb.useGravity = false;
            rb.isKinematic = true;
            if(PhotonNetwork.IsConnected)
            {
                PhotonView PV = GetComponent<PhotonView>();
                PV.RPC("SideValueCheck", RpcTarget.AllBuffered);
            }
            else
            {
                SideValueCheck();
            }
        }
        else if(rb.IsSleeping() && hasLanded && diceValue == 0)
        {
            ReRollDice();
        }
    }

    public void RollDice()
    {
        Reset();
        if (!thrown && !hasLanded)
        {
            thrown = true;
            rb.useGravity = true;
            rb.isKinematic = false;
            rb.AddTorque(Random.Range(0,500),Random.Range(0,500),Random.Range(0,500));
        }
        //Random.Range(0,500)
        // else if (thrown && hasLanded)
        // {
        //     //RESET DICE
        //     Reset();

        // }
    }

    void Reset()
    {
        transform.position = initPosition;
        thrown = false;
        hasLanded = false;
        rb.useGravity = false;
        rb.isKinematic = true;
    }

    void ReRollDice()
    {
        Reset();
        thrown = true;
        rb.useGravity = true;
        rb.isKinematic = false;
        rb.AddTorque(Random.Range(0,500),Random.Range(0,500),Random.Range(0,500));

    }

    [PunRPC]
    void SideValueCheck()
    {
        diceValue = 0;
        foreach (var side in diceSides)
        {
            if (side.OnGround)
            {
                 diceValue = side.SideValue();
                // diceValue = 1;
                Debug.Log("ROLLED NUMBER = " + diceValue);
                break;
            }
 
        }
        GameManager.instance.ReportDiceRolled(diceValue);
    }
}
