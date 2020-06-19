using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GameManger : MonoBehaviour
{
    //other scripts
    public ChalManager chalManagerScript;


    //ui
    public GameObject menuPanel;


    public GameObject[] allCardsObj;
    public GameObject[] players;
    public Sprite blankCard;
    public bool isGameStarted;

    //panels
    public GameObject suitSelectingPanel;

    //cnrt suit img
    public Sprite[] suits;
    public Image crntSuitImgUi;

    int playerNo, hand;

    public List<GameObject> allCards, shuffledCards;


    //selecting suit
    public string currntSuit;

    public Player player1Scritp;

    List<GameObject> pikeSuit;
    List<GameObject> heartSuit;
    List<GameObject> cloverSuit;
    List<GameObject> diamondSuit;

    public GameObject[][] meteorArray;

    void Start()
    {
        //easy();
    }

    public void easy()
    {
        menuPanel.SetActive(false);

        shuffleCards();
        give5Cards();
        selectingSuit();
    }
    public void medium()
    {

        menuPanel.SetActive(false);

        shuffleCards();
        give5Cards();
        selectingSuit();
    }
    public void hard()
    {

        menuPanel.SetActive(false);

        shuffleCards();
        give5Cards();
        selectingSuit();
    }

    void shuffleCards()
    {
        for(int a = 0; a < allCardsObj.Length; a++)
        {
            allCards.Add(allCardsObj[a]);
        }

        for(int s = 0; s < allCardsObj.Length; s++)
        {
            int cardNo = Random.Range(0, allCards.Count);

            shuffledCards.Add(allCards[cardNo]);
            allCards.RemoveAt(cardNo);
        }
    }

    //distribution of cards 
    void give5Cards()
    {
        //first giving 5 cards
        playerNo = 4;
        hand = 5;

        for (int p = 0; p < playerNo; p++)
        {
            Player playerScript = players[p].GetComponent<Player>();

            //for adding first five cards from list
            for (int h = 0; h < hand; h++)
            {
                GameObject card = Instantiate(shuffledCards[h]);
                card.name = shuffledCards[h].name;

                playerScript.havingCards.Add(card);
                card.transform.SetParent(players[p].transform);

                // reseting scale of card
                card.transform.localScale = Vector2.one;


                //adding card to its suit group
                switch (card.tag)
                {
                    case "Spade":
                        playerScript.spadeCards.Add(card);
                        break;

                    case "Club":
                        playerScript.clubCards.Add(card);
                        break;

                    case "Heart":
                        playerScript.heartCards.Add(card);
                        break;

                    case "Diamond":
                        playerScript.diamondCards.Add(card);
                        break;
                }

                if (p > 0)
                {
                    //card.GetComponent<Image>().sprite = blankCard;
                }
            }



            //for removing the first 5 added cards
            for (int r = 0; r < hand; r++)
            {
                shuffledCards.RemoveAt(0);
            }
        }
    }

    //showing suit selecting panel
    void selectingSuit()
    {
        suitSelectingPanel.SetActive(true);
    }
    //suit selected by input from user
    public void selectSuit(string suit)
    {
        currntSuit = suit;
        chalManagerScript.colorSuit = suit;

        suitSelectingPanel.SetActive(false);

        //set crnt suit img
        setCurrentSuitImg(suit);

        //now destribuiting the remaining cards
        give4Cards();
        give4Cards();

        shuffleHandCards();

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
        switch(suit)
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
    void give4Cards()
    {
        //first giving 4 cards
        playerNo = 4;
        hand = 4;

        for (int p = 0; p < playerNo; p++)
        {
            Player playerScript = players[p].GetComponent<Player>();

            //for adding first 4 cards from list
            for (int h = 0; h < hand; h++)
            {
                GameObject card = Instantiate(shuffledCards[h]);
                card.name = shuffledCards[h].name;

                playerScript.havingCards.Add(card);
                card.transform.SetParent(players[p].transform);

                // reseting scale of card
                card.transform.localScale = Vector2.one;

                if (p > 0)
                {
                    //card.GetComponent<Image>().sprite = blankCard;
                }

                //adding card to its suit group
                switch (card.tag)
                {
                    case "Spade":
                        playerScript.spadeCards.Add(card);
                        break;

                    case "Club":
                        playerScript.clubCards.Add(card);
                        break;

                    case "Heart":
                        playerScript.heartCards.Add(card);
                        break;

                    case "Diamond":
                        playerScript.diamondCards.Add(card);
                        break;
                }
            }

            //for removing the first 4 added cards
            for (int r = 0; r < hand; r++)
            {
                shuffledCards.RemoveAt(0);
            }
        }
    }


    void shuffleHandCards()
    {
        for (int r = 0; r < 4; r++)
        {
            players[r].GetComponent<Player>().shuffleCards();
        }
    }
}
