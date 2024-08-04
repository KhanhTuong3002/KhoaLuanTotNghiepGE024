using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

using Photon.Pun;

public class MonopolyBoard : MonoBehaviourPunCallbacks
{
    public static MonopolyBoard instance;

    public List<MonopolyNode> route = new List<MonopolyNode>();

    [System.Serializable]
    public class NodeSet
    {
        public Color setColor = Color.white;
        public List<MonopolyNode> nodesInSetList = new List<MonopolyNode>();
    }

    public List<NodeSet> nodeSetList = new List<NodeSet>();

    private void Awake()
    {
        instance = this;
    }
    private void OnValidate()
    {
        route.Clear();
        foreach(Transform node in transform.GetComponentInChildren<Transform>())
        {
            route.Add(node.GetComponent<MonopolyNode>());
        }
    }
    public List<MonopolyNode> GetNodeList()
    {
        return route;
    }
    private void OnDrawGizmos()
    {
        if (route.Count > 1) 
        {
         for(int i = 0; i < route.Count; i++)
            {
                Vector3 current = route[i].transform.position;
                Vector3 next = (i + 1 < route.Count) ? route[i+1].transform.position:current;

                Gizmos.color = Color.green;
                Gizmos.DrawLine(current, next);

            }
        }
    }

    public void MovePlayertonken(int steps, Player_Mono player)
    {
        StartCoroutine(MovePlayerInSteps(steps, player));
    }

    public void MovePlayertonken(MonopolyNodeType type, Player_Mono player)
    {
        int indexOfNextNodeType = -1;
        int indexOnBoard = route.IndexOf(player.MyMonopolyNode);
        int startSearchIndex = (indexOnBoard+1 % route.Count);
        int nodeSearches = 0;
        while(indexOfNextNodeType == -1 && nodeSearches < route.Count){
            if(route[startSearchIndex].monopolyNodeType == type){
                indexOfNextNodeType = startSearchIndex;
            }
            startSearchIndex=(startSearchIndex+1)%route.Count;
            nodeSearches++ ;
        }
        if(indexOfNextNodeType == -1){ return; }
        StartCoroutine(MovePlayerInSteps(nodeSearches, player));
    }

    IEnumerator MovePlayerInSteps(int steps, Player_Mono player)
    {
        //Just wait a second
        yield return new WaitForSeconds(0.5f);
        int stepsLeft = steps;
        GameObject tonkenTomove = player.MyTonken;
        int indexOnBoard = route.IndexOf(player.MyMonopolyNode);
        bool moveOverGo = false;
        bool isMovingForward = steps > 0;
        if(isMovingForward)
        {
            while (stepsLeft > 0)
            {
                indexOnBoard++;
                // is this over go?
                if (indexOnBoard > route.Count - 1)
                {
                    indexOnBoard = 0;
                    moveOverGo = true;
                }
                //Get start and end positions
                //Vector3 startPos = tonkenTomove.transform.position;
                Vector3 endPos = route[indexOnBoard].transform.position;
                //perform the move
                while (MoveToNextNode(tonkenTomove, endPos, 40))
                {
                    yield return null;
                }
                stepsLeft--;
            }
        }
        else
        {
            while (stepsLeft < 0)
            {
                indexOnBoard--;
                // is this over go?
                if (indexOnBoard < 0)
                {
                    indexOnBoard = route.Count-1;
                }
                //Get start and end positions
                //Vector3 startPos = tonkenTomove.transform.position;
                Vector3 endPos = route[indexOnBoard].transform.position;
                //perform the move
                while (MoveToNextNode(tonkenTomove, endPos, 30))
                {
                    yield return null;
                }
                stepsLeft++;
            }
        }

        //Get go Money
        if(moveOverGo)
        {
            //Collect money on the player
            player.CollectMoney(GameManager.instance.GetGoMoney);
        }
        //set new node on the current  player

        player.SetMyCurrentNode(route[indexOnBoard]);
    }
    
    bool MoveToNextNode(GameObject tonkenTomove, Vector3 endPos, float speed)
    {
        return endPos != (tonkenTomove.transform.position = Vector3.MoveTowards(tonkenTomove.transform.position,endPos,speed * Time.deltaTime));

    }

    public (List<MonopolyNode> list , bool allSame) PlayerHasAllNodesOfSet(MonopolyNode node)
    {
        bool allSame = false;   
        foreach (var nodeSet in nodeSetList)
        {
            if (nodeSet.nodesInSetList.Contains(node))
            {
                //linq
                allSame = nodeSet.nodesInSetList.All(_node => _node.Owner == node.Owner);
                return (nodeSet.nodesInSetList, allSame);

            }
        }
        return (null, allSame);
    }

}
