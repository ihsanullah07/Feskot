using System.Collections;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Threading;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class ChalManager : MonoBehaviour
{
    public GameManger gameManagerScritpt;

    public GameObject[] players; //player gameObjects

    public Player[] playersScritps; //player scripts

    

    //chal 
    public string currntChalSuit, colorSuit;
    public GameObject crntSlctdCard;
    public int crntPlayerChal;
    public int throwsDone;
    public bool isRsltDone;

    //passing turn to nextplayer
    public float turnTimer;
    public float  maxTurnTime;
    public Image[] turnTimerImg;

    //cards on ground
    public GameObject[] showCards;

    public List<string> currentRound;
    public List<string> currentRoundCuts;

    //all round results
    public Transform[] team1CheckMarks;
    public int team1WonRounds, team1WonDeals;
    public TextMeshProUGUI wonRoundsTextT1, wonDealsTextT1;

    public Transform[] team2CheckMarks;
    public int team2WonRounds, team2WonDeals;
    public TextMeshProUGUI wonRoundsTextT2, wonDealsTextT2;

    int winnerPlayer;

    public GameObject checkMark;
    public Transform checkMarkParent;

    //cuts
    public bool isCutDone;


    void Start()
    {
        isCutDone = false;

        gettingPlayersData();

        turnTimer = maxTurnTime;
    }


    void Update()
    {
        if (gameManagerScritpt.isGameStarted)
        {
            //round();
        }
    }

    //getting all data at startup
    void gettingPlayersData()
    {
        for (int s = 0; s < 4; s++)
        {
            playersScritps[s] = players[s].GetComponent<Player>();
        }
    }

    public void chalCrd(int playerId, GameObject chalCard)
    {
        Debug.Log("Chal");

        showCards[playerId].SetActive(true);
        showCards[playerId].GetComponent<Image>().sprite = chalCard.GetComponent<Image>().sprite;

        currntChalSuit = chalCard.tag;

        currentRound[playerId] = chalCard.name;

        throwsDone++;

        //setting current player  chal no
        crntPlayerChal = playerId;


        //now set next player turn
        //only if there is not 4 cards on the ground

        if (throwsDone > 3 && !isRsltDone)
        {
            gameManagerScritpt.isGameStarted = false;
            throwsDone = 0;
            isRsltDone = true;

            Invoke("roundResult", 1);
        }
        else
        {
            StartCoroutine(nextPlayerTurn());
        }
    }
    //for throw
    public void throwCrd(int playerId, GameObject chalCard)
    {
        Debug.Log("Throw");

        showCards[playerId].SetActive(true);
        showCards[playerId].GetComponent<Image>().sprite = chalCard.GetComponent<Image>().sprite;

        currentRound[playerId] = chalCard.name;

        throwsDone++;

        //setting current player  chal no
        crntPlayerChal = playerId;


        //now set next player turn
        //only if there is not 4 cards on the ground

        if (throwsDone > 3 && !isRsltDone)
        {
            gameManagerScritpt.isGameStarted = false;
            throwsDone = 0;
            isRsltDone = true;

            Invoke("roundResult", 1);
        }
        else
        {
            StartCoroutine(nextPlayerTurn());
        }
    }

    //for cut 
    public void cut(int playerId, GameObject chalCard)
    {
        Debug.Log("Cut");

        showCards[playerId].SetActive(true);
        showCards[playerId].GetComponent<Image>().sprite = chalCard.GetComponent<Image>().sprite;


        currentRoundCuts[playerId] = chalCard.name;

        isCutDone = true;

        throwsDone++;

        //setting current player  chal no
        crntPlayerChal = playerId;



        //now set next player turn
        //only if there is not 4 cards on the ground
    }

    //for throwing a random card as badrangi
    public void throwRndmCrd(int playerId, GameObject chalCard)
    {
        //Debug.Log(crntPlayerChal);

        showCards[playerId].SetActive(true);
        showCards[playerId].GetComponent<Image>().sprite = chalCard.GetComponent<Image>().sprite;

        throwsDone++;

        //setting current player  chal no
        crntPlayerChal = playerId;

        //now set next player turn
        //only if there is not 4 cards on the ground

        if (throwsDone > 3 && !isRsltDone)
        {
            gameManagerScritpt.isGameStarted = false;
            throwsDone = 0;
            isRsltDone = true;

            Invoke("roundResult", 1);
        }
        else
        {
            StartCoroutine(nextPlayerTurn());
        }
    }

    void round()
    {
        //passing chall to each player
        turnTimer -= Time.deltaTime;

        turnTimerImg[crntPlayerChal].fillAmount = turnTimer / maxTurnTime;

        if (turnTimer < 0)
        {
            nextPlayerTurn();
        }
    }


    public IEnumerator nextPlayerTurn()
    {
        yield return new WaitForSeconds(.5f);

        turnTimer = maxTurnTime;

        crntPlayerChal++;

        if(crntPlayerChal > 3)
        {
            crntPlayerChal = 0;
        }

        playersScritps[crntPlayerChal].isTurn = true;
    }

    void roundResult()
    {
        Debug.Log(" Result Done ");

        //hiding all card on ground
        for (int c = 0; c < 4; c++)
        {
            showCards[c].SetActive(false);
        }


        if (isCutDone)
        {
            //getting result when a cut is done
            //now getting the hidhest card no 
            int maxCard = 0;

            for (int i = 0; i < currentRoundCuts.Count; i++)
            {
                if (currentRoundCuts[i] != null)
                {
                    int crntV = int.Parse(currentRoundCuts[i]);

                    if (crntV > maxCard)
                    {
                        maxCard = crntV;
                    }
                }
            }

            winnerPlayer = currentRoundCuts.IndexOf(maxCard.ToString());

            isCutDone = false;
        }
        else
        {
            //getting result when a cut is not done
            //now getting the hidhest card no 

            int maxCard = int.Parse(currentRound[0]);

            for (int i = 0; i < currentRound.Count; i++)
            {
                int crntV = int.Parse(currentRound[i]);

                if (crntV > maxCard)
                {
                    maxCard = crntV;
                }
            }

            winnerPlayer = currentRound.IndexOf(maxCard.ToString());
        }



        if (winnerPlayer == 0 || winnerPlayer == 3)
        {
            team1Wins();
        }
        else
        {
            team2Wins();
        }

        //setting winner player chal
        setWinnerChal(winnerPlayer);

        //resume chal again
        gameManagerScritpt.isGameStarted = true;

        isRsltDone = false;
    }

    void team1Wins()
    {
        team1WonRounds++;
        wonRoundsTextT1.text = team1WonRounds.ToString();

        GameObject mr = Instantiate(checkMark);
        mr.transform.SetParent(checkMarkParent);
        mr.transform.position = team1CheckMarks[team1WonRounds - 1].position;
    }
    void team2Wins()
    {
        team2WonRounds++;
        wonRoundsTextT2.text = team2WonRounds.ToString();

        GameObject mr = Instantiate(checkMark);
        mr.transform.SetParent(checkMarkParent);
        mr.transform.position = team2CheckMarks[team2WonRounds - 1].position;
    }

    void setWinnerChal(int winPlrNo)
    {
        //now clearing the whole round array
        for (int c = 0; c < 4; c++)
        {
            currentRound[c] = "0";
        }

        for (int c = 0; c < 4; c++)
        {
            currentRoundCuts[c] = "0";
        }

        //setting chal for winner player
        playersScritps[winPlrNo].isChal = true;
    }
}
