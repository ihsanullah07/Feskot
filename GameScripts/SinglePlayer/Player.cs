using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;


public class Player : MonoBehaviour
{
    //other scripts
    ChalManager chalManagerScript;

    public int playerId;

    public bool isChalStarted;
    public bool isChal;
    public bool isTurn;

    //for random card
    bool isBadrangiTrhown;

    public List<GameObject> havingCards;

    public List<GameObject> spadeCards;
    public List<GameObject> clubCards;
    public List<GameObject> heartCards;
    public List<GameObject> diamondCards;

    //this is just reference list for shuffling group cards
    public GameObject[] cardToShuffle = new GameObject[15];

    GameObject crntSlctdCard;
    int haveChlCrds;

    //these int are the index no for arranging card in order
    int cardsArIndex;


    public GameObject crntGo;
    private void Start()
    {
        isBadrangiTrhown = false;

        cardsArIndex = 0;

        chalManagerScript = GameObject.Find("ChalManager").GetComponent<ChalManager>();
    }

    private void Update()
    {
        pickCard_User();
    }

    //now shuffling cards again to make then in same color group
    public void shuffleCards()
    {
        //now Sorting Cards
        sortSpadeGroup();
        sortClubGroup();
        sortHeartGroup();
        sortDiamondGroup();
    }
    //this is the second shuffle where we arrange cards of each color group and bring the highest card above.
    void sortSpadeGroup()
    {
        //firRemoving all objects from list
        for (int c = 0; c < cardToShuffle.Length; c++)
        {
            cardToShuffle[c] = null;
        }

            //shuffling spade group
            //adding all object to another list 
        for (int c = 0; c < spadeCards.Count; c++)
        {
            int crdIndex = int.Parse(spadeCards[c].name);
    
            cardToShuffle[crdIndex] = spadeCards[c];
        }


        //now deleting all objects
        int cardInSpade = spadeCards.Count;

        for (int c = 0; c < cardInSpade; c++)
        {
            spadeCards.RemoveAt(0);
        }

        //now adding arranged cards back to group
        foreach(GameObject arCrd in cardToShuffle)
        {
            if (arCrd != null)
            {
                spadeCards.Add(arCrd);
            }
        }

        //now arranging cards in hand
        for (int c = 0; c < cardInSpade; c++)
        {
            spadeCards[c].transform.SetSiblingIndex(cardsArIndex);
        }
    }
    void sortClubGroup()
    {
        //firRemoving all objects from list
        for (int c = 0; c < cardToShuffle.Length; c++)
        {
            cardToShuffle[c] = null;
        }

        //shuffling spade group
        //adding all object to another list 
        for (int c = 0; c < clubCards.Count; c++)
        {
            int crdIndex = int.Parse(clubCards[c].name);

            cardToShuffle[crdIndex] = clubCards[c];
        }


        //now deleting all objects
        int cardInClub = clubCards.Count;
        for (int c = 0; c < cardInClub; c++)
        {
            clubCards.RemoveAt(0);
        }

        //now adding arranged cards back to group
        foreach (GameObject arCrd in cardToShuffle)
        {
            if (arCrd != null)
            {
                clubCards.Add(arCrd);
            }
        }

        //now arranging cards in hand
        for (int c = 0; c < cardInClub; c++)
        {
            clubCards[c].transform.SetSiblingIndex(cardsArIndex);
        }
    }
    void sortHeartGroup()
    {
        //firRemoving all objects from list
        for (int c = 0; c < cardToShuffle.Length; c++)
        {
            cardToShuffle[c] = null;
        }

        //shuffling spade group
        //adding all object to another list 
        for (int c = 0; c < heartCards.Count; c++)
        {
            int crdIndex = int.Parse(heartCards[c].name);

            cardToShuffle[crdIndex] = heartCards[c];
        }


        //now deleting all objects
        int cardInHeart = heartCards.Count;
        for (int c = 0; c < cardInHeart; c++)
        {
            heartCards.RemoveAt(0);
        }

        //now adding arranged cards back to group
        foreach (GameObject arCrd in cardToShuffle)
        {
            if (arCrd != null)
            {
                heartCards.Add(arCrd);
            }
        }

        //now arranging cards in hand
        for (int c = 0; c < cardInHeart; c++)
        {
            heartCards[c].transform.SetSiblingIndex(cardsArIndex);
        }
    }
    void sortDiamondGroup()
    {
        //firRemoving all objects from list
        for (int c = 0; c < cardToShuffle.Length; c++)
        {
            cardToShuffle[c] = null;
        }

        //shuffling spade group
        //adding all object to another list 
        for (int c = 0; c < diamondCards.Count; c++)
        {
            int crdIndex = int.Parse(diamondCards[c].name);

            cardToShuffle[crdIndex] = diamondCards[c];
        }


        //now deleting all objects
        int cardInDiamond = diamondCards.Count;
        for (int c = 0; c < cardInDiamond; c++)
        {
            diamondCards.RemoveAt(0);
        }

        //now adding arranged cards back to group
        foreach (GameObject arCrd in cardToShuffle)
        {
            if (arCrd != null)
            {
                diamondCards.Add(arCrd);
            }
        }

        //now arranging cards in hand
        for (int c = 0; c < cardInDiamond; c++)
        {
            diamondCards[c].transform.SetSiblingIndex(cardsArIndex);
        }
    }

    //now picking card by user
    public void pickCard_User()
    {
        if (Input.GetMouseButtonDown(0) && isChalStarted)
        {
            if (crntSlctdCard = EventSystem.current.currentSelectedGameObject)
            {
                if (isChal)
                {
                    Chal();
                }
                else if (isTurn)
                {
                    //checing if we are selecting the write group card

                    if (crntSlctdCard.tag == chalManagerScript.currntChalSuit)
                    {
                        throwCard();


                    }
                    else if (crntSlctdCard.tag != chalManagerScript.currntChalSuit)
                    {
                        foreach(GameObject crd in havingCards)
                        {
                            if(crd.tag == chalManagerScript.currntChalSuit)
                            {
                                haveChlCrds++;
                            }
                        }

                        if(haveChlCrds > 0)
                        {
                            Debug.Log("you have the card");
                        }
                        else
                        {
                            if (crntSlctdCard.tag == chalManagerScript.colorSuit)
                            {
                                cut();
                            }
                            else
                            {
                                //selected card as random
                                pickCardForRandom_Ai();
                            }
                        }
                    }
                    else
                    {
                        Debug.Log("you pick the wrong card");
                    }
                }
            }
        }
    }

    public void pickCardForChal_Ai()
    {
        //Selecting card for chal

        int slctCardNo = Random.Range(0, havingCards.Count);

        crntSlctdCard = havingCards[slctCardNo];

        Chal();
    }

    public void pickCardForThrow_Ai()
    {
        switch (chalManagerScript.currntChalSuit)
        {
            case "Spade":
                if(spadeCards.Count > 0)
                {
                    int spadeCrdNo = Random.Range(0, spadeCards.Count);
                    crntSlctdCard = spadeCards[spadeCrdNo];
                    spadeCards.RemoveAt(spadeCrdNo);
                }
                else
                {
                    pickCardForCut_Ai();
                }


                break;

            case "Club":
                if (clubCards.Count > 0)
                {
                    int clubCrdNo = Random.Range(0, clubCards.Count);
                    crntSlctdCard = clubCards[clubCrdNo];
                    clubCards.RemoveAt(clubCrdNo);
                }
                else
                {
                    pickCardForCut_Ai();
                }

                break;

            case "Heart":
                if (heartCards.Count > 0)
                {
                    int heartCrdNo = Random.Range(0, heartCards.Count);
                    crntSlctdCard = heartCards[heartCrdNo];
                    heartCards.RemoveAt(heartCrdNo);
                }
                else
                {
                    pickCardForCut_Ai();
                }

                break;

            case "Diamond":
                if (diamondCards.Count > 0)
                {
                    int diamongCrdNo = Random.Range(0, diamondCards.Count);
                    crntSlctdCard = diamondCards[diamongCrdNo];
                    diamondCards.RemoveAt(diamongCrdNo);
                }
                else
                {
                    pickCardForCut_Ai();
                }
                break;
        }

        throwCard();
    }
    void pickCardForCut_Ai()
    {
        switch (chalManagerScript.colorSuit)
        {
            case "Spade":
                if (spadeCards.Count > 0)
                {
                    int crdIndex = Random.Range(0, spadeCards.Count);
                    crntSlctdCard = spadeCards[crdIndex];
                    spadeCards.RemoveAt(crdIndex);
                }
                else
                {
                    pickCardForRandom_Ai();
                }
                break;

            case "Club":
                if (clubCards.Count > 0)
                {
                    int crdIndex = Random.Range(0, clubCards.Count);
                    crntSlctdCard = clubCards[crdIndex];
                    clubCards.RemoveAt(crdIndex);
                }
                else
                {
                    pickCardForRandom_Ai();
                }
                break;

            case "Heart":
                if (heartCards.Count > 0)
                {
                    int crdIndex = Random.Range(0, heartCards.Count);
                    crntSlctdCard = heartCards[crdIndex];
                    heartCards.RemoveAt(crdIndex);
                }
                else
                {
                    pickCardForRandom_Ai();
                }
                break;

            case "Diamond":
                if (diamondCards.Count > 0)
                {
                    int crdIndex = Random.Range(0, diamondCards.Count);
                    crntSlctdCard = diamondCards[crdIndex];
                    diamondCards.RemoveAt(crdIndex);
                }
                else
                {
                    pickCardForRandom_Ai();
                }
                break;

        }

        cut();
    }
    
    void pickCardForRandom_Ai()
    {
        int slctCardNo = Random.Range(0, havingCards.Count);

        crntSlctdCard = havingCards[slctCardNo];

        switch (crntSlctdCard.tag)
        {
            case "Spade":
                int spadeCrdNo = spadeCards.IndexOf(crntSlctdCard);

                spadeCards.RemoveAt(spadeCrdNo);

                break;
            case "Club":
                int clubCrdNo = clubCards.IndexOf(crntSlctdCard);

                clubCards.RemoveAt(clubCrdNo);

                break;

            case "Heart":
                int heartCrdNo = heartCards.IndexOf(crntSlctdCard);

                heartCards.RemoveAt(heartCrdNo);

                break;

            case "Diamond":
                int diamondCrdNo = diamondCards.IndexOf(crntSlctdCard);

                diamondCards.RemoveAt(diamondCrdNo);

                break;
        }


        isBadrangiTrhown = true;
    }

    public void Chal()
    {
        //showing card
        chalManagerScript.chalCrd(playerId, crntSlctdCard);

        //removing card from having cards
        int hvngCard = havingCards.Count;
        int rmovCrdNo = 0;
        for (int h = 0; h < hvngCard; h++)
        {
            if (crntSlctdCard == havingCards[h])
            {
                rmovCrdNo = h;
            }
        }
        havingCards.RemoveAt(rmovCrdNo);
        //removing card from its group too
        switch (crntSlctdCard.tag)
        {
            case "Spade":
                int spadeCrdNo = spadeCards.IndexOf(crntSlctdCard);
                spadeCards.RemoveAt(spadeCrdNo);
                break;

            case "Club":
                int clubCrdNo = clubCards.IndexOf(crntSlctdCard);

                clubCards.RemoveAt(clubCrdNo);

                break;

            case "Heart":
                int heartCrdNo = heartCards.IndexOf(crntSlctdCard);

                heartCards.RemoveAt(heartCrdNo);

                break;

            case "Diamond":
                int diamondCrdNo = diamondCards.IndexOf(crntSlctdCard);

                diamondCards.RemoveAt(diamondCrdNo);

                break;
        }


        Destroy(crntSlctdCard);

        isChal = false;
    }
    public void throwCard()
    {
        if (isBadrangiTrhown)
        {
            chalManagerScript.throwRndmCrd(playerId, crntSlctdCard);

            //removing card from having cards
            int hvngCard = havingCards.Count;
            int rmovCrdNo = 0;
            for (int h = 0; h < hvngCard; h++)
            {
                if (crntSlctdCard == havingCards[h])
                {
                    rmovCrdNo = h;
                }
            }
            havingCards.RemoveAt(rmovCrdNo);

            Destroy(crntSlctdCard);

            isBadrangiTrhown = false;
        }
        else
        {
            chalManagerScript.throwCrd(playerId, crntSlctdCard);

            //removing card from having cards
            int hvngCard = havingCards.Count;
            int rmovCrdNo = 0;
            for (int h = 0; h < hvngCard; h++)
            {
                if (crntSlctdCard == havingCards[h])
                {
                    rmovCrdNo = h;
                }
            }
            havingCards.RemoveAt(rmovCrdNo);

            Destroy(crntSlctdCard);
        }
        isTurn = false;
    }

    void cut()
    {
        chalManagerScript.cut(playerId, crntSlctdCard);

        //removing card from having cards
        int hvngCard = havingCards.Count;
        int rmovCrdNo = 0;
        for (int h = 0; h < hvngCard; h++)
        {
            if (crntSlctdCard == havingCards[h])
            {
                rmovCrdNo = h;
            }
        }
        havingCards.RemoveAt(rmovCrdNo);

        Destroy(crntSlctdCard);

        isTurn = false;
    }
}
