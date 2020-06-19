using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.EventSystems;



using UnityStandardAssets.CrossPlatformInput;
using UDPServerModule;

/// <summary>
///Manage Network player if isLocalPlayer variable is false
/// or Local player if isLocalPlayer variable is true.
/// </summary>
public class PlayerManager : MonoBehaviour {

    //other managers
    public GameManagerMulti gameManagerScript;

	public string	id;

	public string name;

	public int cont;

	public bool isOnline;

	public bool isLocalPlayer;

	public float timeOut;

	public TextMeshProUGUI nameText;

    //*************************** player manager code ******************
    //our player manager variables

    public int playerId;
    public bool canChal;
    public bool canThrow;
    public bool canCut;
    public bool canRndmCrd;


    int curntChalCards;



    //*************************** players spawn positions ******************
    public Transform[] playerPos = new Transform[4];

	public GameObject crntPlayer;


    //for random card
    bool isBadrangiTrhown;

    public List<GameObject> havingCards;

    public List<GameObject> spadeCards;
    public List<GameObject> clubCards;
    public List<GameObject> heartCards;
    public List<GameObject> diamondCards;


    //this is just reference list for shuffling group cards
    public GameObject[] cardToShuffle = new GameObject[15];

    public GameObject crntSlctdCard;


    //these int are the index no for arranging card in order
    int cardsArIndex;

    public GameObject crntGo;


    // ************************ Start Code here *********************
    private void Start()
    {
        gameManagerScript = GameObject.Find("GameManager").GetComponent<GameManagerMulti>();

           isBadrangiTrhown = false;

        cardsArIndex = 0;

        GameObject parent = GameObject.Find("Canvas");

        transform.SetParent(parent.transform);

        //checking if room is full
        if (UDPServer.instance.connectedClients.Count == 4)
        {
            //Debug.Log(UDPServer.instance.connectedClients.Count);
            NetworkManager.instance.isGameReady();
        }
    }

    private void Update()
    {
        pickCard_User();
    }


    //now shuffling cards again to make then in same color group

    //setting name
    public void SetPlayerName(string name)
	{
		nameText.text = name;
	}


	//setup player position as instanciated
	void setPlace()
	{
		GameObject networkManager = GameObject.Find("NetworkManager");

		//finding parent (canvas) and position to where this player should be.
		GameObject parent = GameObject.Find("Canvas");

		transform.SetParent(parent.transform);


		//finding all spawn points
		for (int pPos = 0; pPos < 4; pPos++)
		{
			playerPos[pPos] = GameObject.Find("PlayerPos" + pPos.ToString()).transform;
		}

		//now finding all players in game
		UDPServerModule.UDPServer server = networkManager.GetComponent<UDPServer>();
		int joinedPlayers = server.connectedClients.Count;

		int playerPosNo = 0;

		switch (joinedPlayers)
		{
			case 1:
				playerPosNo = 0;
				break;


			case 2:
				playerPosNo = 3;
				break;


			case 3:
				playerPosNo = 2;
				break;


			case 4:
				playerPosNo = 1;
				break;
		}

		List<string> keys = new List<string>(server.connectedClients.Keys);



		foreach (string key in keys)
		{
			//Debug.Log(server.connectedClients[key].id);

			crntPlayer = GameObject.Find(server.connectedClients[key].id);

			//crntPlayer.transform.position = playerPos[playerPosNo].position;

			Debug.Log(crntPlayer.name);

			playerPosNo++;
		}
	}

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
        foreach (GameObject arCrd in cardToShuffle)
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
        if (Input.GetMouseButtonDown(0))
        {
            if (crntSlctdCard = EventSystem.current.currentSelectedGameObject)
            {
                if (canChal)
                {
                    //sent chal to server
                    NetworkManager.instance.chal(id, crntSlctdCard.name, crntSlctdCard.tag);
                }
                else if (canThrow)
                {
                    //checking all cards 
                    checkingHvngCrds();

                    processChal(crntSlctdCard);
                }
            }
        }
    }

    void checkingHvngCrds()
    {
        curntChalCards = 0;

        //counting curnt sut cards
        foreach (GameObject crd in gameManagerScript.havingCards)
        {
            if (crd.tag == gameManagerScript.currntChalSuit)
            {
                curntChalCards++;
            }
        }
    }

    void processChal(GameObject slctdCard)
    {
        int hvngCard = havingCards.Count;


        if(slctdCard.tag != gameManagerScript.currntChalSuit)
        {
            if(curntChalCards > 0)
            {
                //we have the chaled card
                Debug.Log("you selected wronge suit");
            }
            else if(curntChalCards <= 0)
            {
                //challing cut
                //send a throw to server
                NetworkManager.instance.doThrow(id, crntSlctdCard.name, crntSlctdCard.tag);

                Debug.Log("chaled another card");
            }
        }
        else 
        {
            //checing if we are selecting the write group card
            //send a throw to server
            NetworkManager.instance.doThrow(id, crntSlctdCard.name, crntSlctdCard.tag);

        }
    }

    void cutDone()
    {
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

}
