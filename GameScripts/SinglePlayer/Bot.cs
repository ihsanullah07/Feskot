using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class Bot : MonoBehaviour
{
    ChalManager chalManagerScript;
    GameManger gameManagerScript;
    Player playerScript;

    public int playerId;

    public List<GameObject> havingCards;

    GameObject crntSlctdCard;

    private void Start()
    {
        playerScript = GetComponent<Player>();
        chalManagerScript = GameObject.Find("ChalManager").GetComponent<ChalManager>();
        gameManagerScript = GameObject.Find("GameManager").GetComponent<GameManger>();
    }

    private void Update()
    {
        if(gameManagerScript.isGameStarted)
        {
            if (playerScript.isChal)
            {
                playerScript.pickCardForChal_Ai();
            }
            else if (playerScript.isTurn)
            {
                playerScript.pickCardForThrow_Ai();
            }
        }
    }
}
