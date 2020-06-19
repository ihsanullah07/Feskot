using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UDPServerModule;
using UnityEditor;

public class GameManagerMulti : MonoBehaviour
{
    //other managers
    public UDPServer server;

    public static GameManagerMulti instance;


    //other gameobjects
    public Canvas readyToStrtPnl;

    public GameObject suitSlctingPnl;

    //code

    public GameObject[] allCardsObj;
    public GameObject[] players;
    public Sprite blankCard;

    public bool isGameStarted;

    //players cards
    public List<GameObject> havingCards;

    public List<GameObject> player0Card;
    public List<GameObject> player1Card;
    public List<GameObject> player2Card;
    public List<GameObject> player3Card;

    //panels
    public GameObject suitSelectingPanel;

    public List<GameObject> allCards, shuffledCards;




    //Game in Start
    public bool isGameReady;

    public bool isShuffeled;

    public bool isCardDistributed;

    //dis 5 cards
    public int playerNo, hand;
    public bool is5Cards;

    //dis 4 cards
    public bool is4CardsDis;

    //selecting suit
    public bool showSuitPnl;
    public bool hideSuitPnl;

    public Sprite[] suits;
    public Image crntSuitImgUi;

    //giving first chal to player who select suit
    public int suitSlctPlrNo;



    //chal 
    public string currntChalSuit, colorSuit;

    public int crntPlayerChal;
    public int throwsDone;

    public bool doChal;
    int plrId, crdN;
    string crdT;

    //thow
    public bool doThrow;


    //passing turn to nextplayer
    public float turnTimer;
    public float maxTurnTime;
    public Image[] turnTimerImg;

    //cards on ground
    public GameObject[] showCards;
    bool isCrdEnable;
    int shCrdPlrNo;

    public List<int> currentRoundCards;
    public List<int> currentRoundCuts;

    //all round results
    public bool doResult;

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

    public int cardNo = 0;

    //**** Important ***//

    //re think variables
    public string currntSuit;

    void Start()
    {
        //easy();
        readyToStrtPnl.enabled = false;

        isShuffeled = false;
    }
    private void Update()
    {
        //is game ready just before starting game
        if (isGameReady)
        {
            readyToStrtPnl.enabled = true;
        }
        else if (!isGameReady && isGameStarted)
        {
            readyToStrtPnl.enabled = false;
        }

        //shuffeling cards at start
        //and distributing cards
        if (isCardDistributed)
        {
            startDistribution();
        }


        //selcting suit
        if (showSuitPnl)
            showSuitPanel();

        if (hideSuitPnl)
            hideSuitPanel();

        //distributing reaming 4 cards for each player
        if (is4CardsDis)
        {
            give4Cards();
            give4Cards();

            giveChal(suitSlctPlrNo);

            is4CardsDis = false;
        }

        //chal
        if (doChal)
        {
            chalCrd(plrId, crdN, crdT);
        }

        if (doThrow)
        {
            throwCard(plrId, crdN, crdT);
        }

        //result

        if(doResult)
        {
            roundResult();

            doResult = false;
        }

        //showing card
        if(isCrdEnable)
        {
            enableCard(shCrdPlrNo);

            isCrdEnable = false;
        }
    }

    public void readyToStartGM()
    {
        isGameReady = true;
    }

    public void startGame()
    {
        isGameReady = false;
        isGameStarted = true;

        isCardDistributed = true;

        Debug.Log("Game Started");


        /*
       selectingSuit();

       */
    }

    void startDistribution()
    {
        shuffleCards();

        //give five cards to players
        give5Cards();

        //now selecting a suit
        selectingSuit();

        isCardDistributed = false;
    }



    void shuffleCards()
    {
        for (int a = 0; a < allCardsObj.Length; a++)
        {
            allCards.Add(allCardsObj[a]);
        }

        for (int s = 0; s < allCardsObj.Length; s++)
        {
            cardNo = Random.Range(0, allCards.Count);

            shuffledCards.Add(allCards[cardNo]);
            allCards.RemoveAt(cardNo);
        }
        Debug.Log("shuffeled");
    }

    //distribution of cards 
    public void give5Cards()
    {
        playerNo = 4;
        hand = 5;

        for (int p = 0; p < playerNo; p++)
        {
            for (int r = 0; r < hand; r++)
            {
                GameObject crd = shuffledCards[r];

                server.sendCardToPlayer(p, crd.name, crd.tag);
            }

            //for removing the first 5 added cards
            for (int r = 0; r < hand; r++)
            {
                shuffledCards.RemoveAt(0);
            }
        }
    }

    public void gotCard(string crdName, string crdTag)
    {
        foreach (GameObject c in allCardsObj)
        {
            if (c.name == crdName && c.tag == crdTag)
            {
                GameObject newCard = Instantiate(c);


                GameObject parent = GameObject.Find(NetworkManager.instance.myId);

                havingCards.Add(newCard);

                newCard.transform.SetParent(parent.transform.GetChild(1));
                newCard.name = c.name;
            }
        }
    }

    //sending data for selecting suit to a random client
    void selectingSuit()
    {
        //now getting a randome player and send info to him for select a suit

        //int plrNo = Random.Range(0, 4);


        server.plrToSlctSuit(0);

        suitSlctPlrNo = 0;
    }

    //selecting suit if we are lucky to be picked
    //this function we will be called as we recieved me to select suit from server
    public void showSuitPanel()
    {
        suitSlctingPnl.SetActive(true);

        Debug.Log("suit panel showed");

        showSuitPnl = false;
    }
    public void hideSuitPanel()
    {
        suitSlctingPnl.SetActive(false);

        //now set suit sprite
        int colorSuitNo = 0;

        switch (colorSuit)
        {
            case "Spade":
                colorSuitNo = 0;
                break;

            case "Club":
                colorSuitNo = 1;
                break;

            case "Heart":
                colorSuitNo = 2;
                break;

            case "Diamond":
                colorSuitNo = 3;
                break;
        }

        crntSuitImgUi.sprite = suits[colorSuitNo];

        hideSuitPnl = false;
    }


    //suit selected by input from user
    public void selectSuit(string suit)
    {
        currntSuit = suit;
        colorSuit = suit;

        suitSelectingPanel.SetActive(false);

        //set crnt suit img
        setCurrentSuitImg(suit);


        //start to chal and throws from players
        isGameStarted = true;

        //chal started
        for (int r = 0; r < 4; r++)
        {
            players[r].GetComponent<Player>().isChalStarted = true;
        }
    }
    void setCurrentSuitImg(string suit)
    {
        switch (suit)
        {
            case "Spade":
                crntSuitImgUi.sprite = suits[0];
                break;

            case "Club":
                crntSuitImgUi.sprite = suits[1];
                break;

            case "Heart":
                crntSuitImgUi.sprite = suits[2];
                break;

            case "Diamond":
                crntSuitImgUi.sprite = suits[3];
                break;
        }

        crntSuitImgUi.SetNativeSize();
    }
    public void give4Cards()
    {
        //first giving 4 cards
        playerNo = 4;
        hand = 4;

        for (int p = 0; p < playerNo; p++)
        {
            for (int r = 0; r < hand; r++)
            {
                GameObject crd = shuffledCards[r];

                server.sendCardToPlayer(p, crd.name, crd.tag);
            }

            //for removing the first 5 added cards
            for (int r = 0; r < hand; r++)
            {
                shuffledCards.RemoveAt(0);
            }
        }
    }


    //giving chal to player
    public void giveChal(int playerNo)
    {
        server.giveChalToPlr(playerNo);
    }

    public void giveThrow(int playerNo)
    {
        server.giveThrowToPlr(playerNo);
    }


    //******************** recieving these functions from server ********************

    public void chal(int playerId, int cardNo, string cardTag)
    {
        plrId = playerId;
        crdN = cardNo;
        crdT = cardTag;

        doChal = true;
    }
    void chalCrd(int playerId, int cardNo, string cardTag)
    {

        //sending current chal suit to all players
        server.sendChalSuit(cardTag);

        currentRoundCards[playerId] = cardNo;

        throwsDone++;

        //setting current player  chal no
        crntPlayerChal = playerId;

        crntPlayerChal++;

        //now secding msg to all client to show card on screen
        server.showGroundCard(playerId, cardNo.ToString(), cardTag);

        nextPlayerTurn();


        Debug.Log("Chal Done");
        doChal = false;
    }

    //showing the show card on ground upon reciveing msg from serever
    public void showCardOnGround(int plrNo, string crdName, string crdTag)
    {
        foreach (GameObject crdToShow in allCardsObj)
        {
            if (crdToShow.name == crdName && crdToShow.tag == crdTag)
            {
                showCards[plrNo].GetComponent<Image>().sprite = crdToShow.GetComponent<Image>().sprite;

                shCrdPlrNo = plrNo;

                isCrdEnable = true;
            }
        }
    }
    //showing a by trigger it
    void enableCard(int plrNo)
    {
        showCards[plrNo].SetActive(true);
    }


    //for throw
    public void isThrow(int playerId, int cardNo, string cardTag)
    {

        plrId = playerId;
        crdN = cardNo;
        crdT = cardTag;

        doThrow = true;
    }


    void throwCard(int playerId, int cardNo, string cardTag)
    {
        //check the chaled card
        if(cardTag == currntChalSuit)
        {
            currentRoundCards[playerId] = cardNo;
        }
        else if(cardTag == colorSuit)
        {
            currentRoundCuts[playerId] = cardNo;
        }



        throwsDone++;

        Debug.Log(throwsDone);

        //setting current player  chal no
        crntPlayerChal = playerId;

        crntPlayerChal++;

        //now secding msg to all client to show card on screen
        server.showGroundCard(playerId, cardNo.ToString(), cardTag);

        //if the result is ready
        if (throwsDone > 1)
        {
            throwsDone = 0;

            doResult = true;
        }
        else
        {
            //now set next player turn
            nextPlayerTurn();
        }


        doThrow = false;

        Debug.Log("Throw Done");
    }

    public void nextPlayerTurn()
    {
        if (crntPlayerChal > 1)
        {
            crntPlayerChal = 0;
        }

        giveThrow(crntPlayerChal);
    }

    void roundResult()
    {

        if (isCutDone)
        {
            //getting result when a cut is done
            //now getting the hidhest card no 

            Debug.Log("Cut Result Start");

            int maxCard = 0;

            for (int i = 0; i < currentRoundCuts.Count; i++)
            {
                int crntV = currentRoundCuts[i];

                if (crntV > maxCard)
                {
                    maxCard = crntV;
                }
            }

            winnerPlayer = currentRoundCuts.IndexOf(maxCard);

            isCutDone = false;
        }
        else
        {
            //getting result when a cut is not done
            //now getting the hidhest card no 

            int maxCard = currentRoundCards[0];

            for (int i = 0; i < currentRoundCards.Count; i++)
            {
                int crntV = currentRoundCards[i];

                if (crntV > maxCard)
                {
                    maxCard = crntV;
                }
            }

            winnerPlayer = currentRoundCards.IndexOf(maxCard);
        }

        Debug.Log(winnerPlayer);

        if (winnerPlayer == 0 || winnerPlayer == 3)
        {
            team1Wins();
        }
        else
        {
            team2Wins();
        }

        //setting winner player chal
        //setWinnerChal(winnerPlayer);

        crntPlayerChal = winnerPlayer;

        Debug.Log(winnerPlayer);

        giveChal(crntPlayerChal);
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
}
