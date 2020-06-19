﻿using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System;
using System.Net;
using System.Net.Sockets;
using System.Net.NetworkInformation;
using UnityEngine;
using UnityEngine.UI;
using UDPClientModule;
using UDPServerModule;
using UnityEngine.Experimental.PlayerLoop;

using Random = UnityEngine.Random;

public class NetworkManager : MonoBehaviour {

	public GameManagerMulti gameManagerScript;


	//from UDP Socket API
	private UDPClientComponent udpClient;

	//Variable that defines comma character as separator
	static private readonly char[] Delimiter = new char[] {':'};

	//useful for any gameObject to access this class without the need of instances her or you declare her
	public static NetworkManager instance;

	//flag which is determined the player is logged in the arena
	public bool onLogged = false;

	//store localPlayer
	public GameObject myPlayer;

	//local player id
	public string myId = string.Empty;

	//local player id
	public string local_player_id;

	//store all players in game
	public Dictionary<string, PlayerManager> networkPlayers = new Dictionary<string, PlayerManager>();

	//store the local players' models
	public GameObject[] localPlayersPrefabs;

	//store the networkplayers' models
	public GameObject[] networkPlayerPrefabs;

	//stores the spawn points 
	public Transform[] spawnPoints;

	//camera prefab
	public GameObject camRigPref;

	public GameObject camRig;

	public int serverPort = 3310;
	
	public int clientPort = 3000;

	public bool tryJoinServer;

	public bool waitingAnswer;

	public bool serverFound;

	public bool waitingSearch;

	public bool gameIsRunning;

	public int maxReconnectTimes = 10;

	public int contTimes;

	public float maxTimeOut;

	public float timeOut;

	public List<string> _localAddresses { get; private set; }


	//players data
	int crntSpwnPlrNo = 0;


	// Use this for initialization
	void Start () {

		// if don't exist an instance of this class
		if (instance == null) {

			//it doesn't destroy the object, if other scene be loaded
			DontDestroyOnLoad (this.gameObject);

			instance = this;// define the class as a static variable
			
			udpClient = gameObject.GetComponent<UDPClientComponent>();
			
			int randomPort = UnityEngine.Random.Range(3001, 3310);

			
		
			//find any  server in others hosts
			ConnectToUDPServer(serverPort, randomPort);

			IPHostEntry host = Dns.GetHostEntry(Dns.GetHostName());

			string address = string.Empty;

			string subAddress = string.Empty;

			_localAddresses = new List<string>();

		}
		else
		{
			//it destroys the class if already other class exists
			Destroy(this.gameObject);
		}
		
	}

	//***************************************** Reciving Client Data *****************************************//

	/// <summary>
	/// Connect client to any UDP server.
	/// </summary>
	public void ConnectToUDPServer(int _serverPort, int _clientPort)
	{


		if (udpClient.GetServerIP () != string.Empty) {

			//connect to udp server
			udpClient.connect (udpClient.GetServerIP (), _serverPort, _clientPort);

			//The On method in simplistic terms is used to map a method name to an annonymous function.
			udpClient.On ("SELECTSUIT", OnSelectingSuit);

			udpClient.On("SUITSELECTED", OnSuitSelected);

			udpClient.On("GOTACARD", OnGotCard);

			udpClient.On("GOTCHAL", onGotChal);

			udpClient.On("CHALDONE", onChalDone);

			udpClient.On("SHOWCARD", OnShowCard);

			udpClient.On ("CHALSUIT", OnSetChalSuit);

			udpClient.On ("GOTTHROW", OnGotThrow);

			udpClient.On ("THROWDONE", OnThrowDone);


			udpClient.On("PONG", OnPrintPongMsg);
			
			udpClient.On ("JOIN_SUCCESS", OnJoinGame);

			udpClient.On ("SPAWN_PLAYER", OnSpawnPlayer);

			udpClient.On ("UPDATE_MOVE", OnUpdatePosition);

			udpClient.On ("USER_DISCONNECTED", OnUserDisconnected);	
		}


	}

	void Update()
	{
		//if it was not found a server
		if (!serverFound) {
			
			//tries to obtain a "pong" of some local server
			StartCoroutine ("PingPong");
		}
		//found server
		else
		{
			//if the player is already in game
			if (gameIsRunning)
			{
				//maintain a connection with the server to detect disconnection
				StartCoroutine ("PingPong");


				/*************** verifies the disconnection of some player ***************/
				List<string> keys = new List<string> (networkPlayers.Keys);
			
				foreach (string key in keys) {

					if (networkPlayers.ContainsKey (key)) {

						if (networkPlayers [key] != null) {

							//increases the time of wait
							networkPlayers [key].timeOut += Time.deltaTime;

							//the client is verified exceeded the time limits of wait
							if (networkPlayers [key].timeOut >= maxTimeOut) {
						
							
								//destroy network player by your id
							//	Destroy (networkPlayers [key].gameObject);

								//remove from the dictionary
							//	networkPlayers.Remove (networkPlayers [key].id);
							
							}
						}
					}

				}//END_FOREACH
			/*************************************************************************/	

			}
		}
	}


	/// <summary>
	/// corroutine called  of times in times to send a ping to the server
	/// </summary>
	/// <returns>The pong.</returns>
	private IEnumerator PingPong()
	{

		if (waitingSearch)
		{
			yield break;
		}

		waitingSearch = true;

		//sends a ping to server
		EmitPing ();

		//important to verify the server it is connected
		if (gameIsRunning)
		{
			//number of pings sent to the server without answer
			contTimes++;
		}


		// wait 1 seconds and continue
		yield return new WaitForSeconds(1);

		//if contTimes arrived to the maximum value of attempts means that the server is not more answering or it disconnected
		if (contTimes > maxReconnectTimes )
		{
			contTimes = 0;

			//restarts the game so that a new server is created
			RestartGame ();
		}

		waitingSearch = false;

	}

	//function to help to detect flaw in the connection
	public IEnumerator WaitAnswer()
	{
		if (waitingAnswer)
		{
			yield break;
		}
	
		tryJoinServer = true;

		waitingAnswer = true;

		CanvasManager.instance.ShowLoadingImg ();

		yield return new WaitForSeconds(5f);

		CanvasManager.instance.CloseLoadingImg ();

		waitingAnswer = false;
	   
		//if true we lost the package the servant didn't answer
		//take a look in public void OnJoinGame(SocketUDPEvent data) function
		if (tryJoinServer) {
			
			tryJoinServer = false;

			CanvasManager.instance.ShowAlertDialog ("LOST PACKAGE! PLEASE TRY AGAIN! ");

			CanvasManager.instance.CloseLoadingImg();

		}


	}

	//it generates a random id for the local player
	public string generateID()
	{
		string id = Guid.NewGuid().ToString("N");

		//reduces the size of the id
		id = id.Remove (id.Length - 15);

		return id;
	}

	//************************ Reciving Client Data ***************//


	void OnSelectingSuit(SocketUDPEvent data)
	{
		gameManagerScript.showSuitPnl = true;
	}

	void OnSuitSelected(SocketUDPEvent data)
	{
		gameManagerScript.colorSuit = data.pack[1];

		gameManagerScript.hideSuitPnl = true;
	}

	/// <summary>
	///  receives an answer of the server.
	/// from  void OnReceivePing(string [] pack,IPEndPoint anyIP ) in server
	/// </summary>
	public void OnPrintPongMsg(SocketUDPEvent data)
	{

		/*
		 * data.pack[0]= CALLBACK_NAME: "PONG"
		 * data.pack[1]= "pong!!!!"
		*/

		serverFound = true;

		contTimes = 0;

		//arrow the located text in the inferior part of the game screen
		CanvasManager.instance.txtSearchServerStatus.text = "------- server is running -------";
	
	}

	public void OnGotCard(SocketUDPEvent data)
	{
		gameManagerScript.gotCard(data.pack[1], data.pack[2]);
	}

	public void onGotChal(SocketUDPEvent data)
	{
		GameObject player = GameObject.Find(myId);
		player.GetComponent<PlayerManager>().canChal = true;
	}

	public void onChalDone(SocketUDPEvent data)
	{
		GameObject player = GameObject.Find(myId);
		player.GetComponent<PlayerManager>().canChal = false;

		int rmvCrdNo = 0;
		int rmIdex = 0;

		//now delet the card from hand
		//and getting card no in having cards to remove it from list too
		foreach (GameObject crdToRemove in gameManagerScript.havingCards)
		{
			if (crdToRemove.name == data.pack[1] && crdToRemove.tag == data.pack[2])
			{
				Destroy(crdToRemove);

				Debug.Log("card removed");

				rmIdex = rmvCrdNo;
			}

			rmvCrdNo++;
		}

		gameManagerScript.havingCards.RemoveAt(rmIdex);
	}

	public void OnShowCard(SocketUDPEvent data)
	{
		gameManagerScript.showCardOnGround(int.Parse(data.pack[1]), data.pack[2], data.pack[3]);
	}

	public void OnSetChalSuit(SocketUDPEvent data)
	{
		gameManagerScript.currntChalSuit = data.pack[1];
	}

	public void OnGotThrow(SocketUDPEvent data)
	{
		GameObject player = GameObject.Find(myId);
		player.GetComponent<PlayerManager>().canThrow = true;
	}
	public void OnThrowDone(SocketUDPEvent data)
	{
		GameObject player = GameObject.Find(myId);
		player.GetComponent<PlayerManager>().canThrow = false;


		//now sending msg to all client to show card on screen
		int rmvCrdNo = 0;
		int rmIdex = 0;

		//now delet the card from hand
		//and getting card no in having cards to remove it from list too
		foreach (GameObject crdToRemove in gameManagerScript.havingCards)
		{
			if (crdToRemove.name == data.pack[1] && crdToRemove.tag == data.pack[2])
			{
				Destroy(crdToRemove);

				Debug.Log("card removed");

				rmIdex = rmvCrdNo;
			}

			rmvCrdNo++;
		}

		gameManagerScript.havingCards.RemoveAt(rmIdex);
	}

	//************************ Sending Data to Server *************************************//

	public void EmitPing() {

		//hash table <key, value>	
		Dictionary<string, string> data = new Dictionary<string, string>();

		//JSON package
		data["callback_name"] = "PING";

		//store "ping!!!" message in msg field
		data["msg"] = "ping!!!!";

		//The Emit method sends the mapped callback name to  the server
		udpClient.Emit (data["callback_name"] ,data["msg"]);

	}
		

	public void EmitJoin()
	{
		// verifies WiFi connection
		if (!udpClient.noNetwork) {


			if (serverFound) {

				//tries to put the player in game
				TryJoinServer ();
			}
			else
			{
				if (UDPServer.instance.serverRunning)
				{
					
					TryJoinServer ();
				} 
				else 
				{
					CanvasManager.instance.ShowAlertDialog ("PLEASE START THE SERVER");
				}
			}

		}//END_IF
		else
		{
			
			if (udpClient.noNetwork) {
				
				CanvasManager.instance.ShowAlertDialog ("PLEASE CONNECT TO ANY WIFI NETWORK");
			}

			else
			{
				if (serverFound) {

					TryJoinServer ();
				}

				else
				{
					CanvasManager.instance.ShowAlertDialog ("THERE NO ARE SERVER RUNNING ON NETWORK!");
				}

			}
		}

	}

	public void TryJoinServer()
	{
		Debug.Log("Player");

		//hash table <key, value>	
		Dictionary<string, string> data = new Dictionary<string, string> ();

		data ["callback_name"] = "JOIN";//set up callback name


		data["player_name"] = CanvasManager.instance.inputLogin.text;

		//it is already verified an id was generated
		if (myId.Contains (string.Empty)) {

			myId = generateID ();

			data ["player_id"] = myId;
		}
		else
		{
			data ["player_id"] = myId;
		}

		//makes the draw of a point for the player to be spawn
		//int index = Random.Range (0, spawnPoints.Length);

		int index = crntSpwnPlrNo;

		Vector3 position = new Vector3( spawnPoints [index].position.x,spawnPoints [index].position.y,spawnPoints [index].position.z );

		data["position"] = position.x+":"+position.y+":"+position.z;
		
		
	
		//send the position point to server
		string msg = data["player_name"] + ":"+data["player_id"]+":" + data["position"];

		//sends to the server through socket UDP the jo package 
		udpClient.Emit (data ["callback_name"], msg);

		//we waited for a time to verify the connection
		StartCoroutine (WaitAnswer ());
	}

	/// <summary>
	/// Joins the local player in game.
	/// </summary>
	/// <param name="_data">Data.</param>
	public void OnJoinGame(SocketUDPEvent data)
	{

		/*
		 * data.data.pack[0] = CALLBACK_NAME: "JOIN_SUCCESS" from server
		 * data.data.pack[1] = id (local player id)
		 * data.data.pack[2]= name (local player name)
		 * data.data.pack[3] = position.x (local player position x)
		 * data.data.pack[4] = position.y (local player position ...)
		 * data.data.pack[5] = position.z
		 * data. data.pack[6] = rotation.x
		 * data.data.pack[7] = rotation.y
		 * data.data.pack[8] = rotation.z
		 * data.data.pack[9] = rotation.w
		*/

		Debug.Log("Login successful, joining game");


		if (!myPlayer) {

			// take a look in PlayerManager.cs script
			PlayerManager newPlayer;
			
			if(UDPServer.instance.serverRunning)
			{
			  // newPlayer = GameObject.Instantiate( local player avatar or model, spawn position, spawn rotation)
			  newPlayer = GameObject.Instantiate (localPlayersPrefabs [0],
				new Vector3(float.Parse(data.pack[3],CultureInfo.CurrentCulture), float.Parse(data.pack[4],CultureInfo.CurrentCulture), 
					float.Parse(data.pack[5],CultureInfo.CurrentCulture)),Quaternion.identity).GetComponent<PlayerManager> ();

			}
			else
			{
			  // newPlayer = GameObject.Instantiate( local player avatar or model, spawn position, spawn rotation)
			  newPlayer = GameObject.Instantiate (localPlayersPrefabs [0],
				new Vector3(float.Parse(data.pack[3],CultureInfo.InvariantCulture), float.Parse(data.pack[4],CultureInfo.InvariantCulture), 
					float.Parse(data.pack[5],CultureInfo.InvariantCulture)),Quaternion.identity).GetComponent<PlayerManager> ();

			}
			

			Debug.Log("player instantiated");

			newPlayer.id = data.pack [1];

			//this is local player
			newPlayer.isLocalPlayer = true;

			//now local player online in the arena
			newPlayer.isOnline = true;



			//set local player's 3D text with his name
			newPlayer.SetPlayerName(data.pack[2]);


			//puts the local player on the list
			networkPlayers [data.pack [1]] = newPlayer;

			myPlayer = networkPlayers [data.pack[1]].gameObject;

			local_player_id =  data.pack [1];

			//spawn cam
			camRig = GameObject.Instantiate (camRigPref, new Vector3 (0f, 0f, -10f), Quaternion.identity);

			
			//hide the lobby menu (the input field and join buton)
			CanvasManager.instance.OpenScreen(3);

			CanvasManager.instance.CloseLoadingImg ();

			CanvasManager.instance.lobbyCamera.GetComponent<Camera> ().enabled = false;

			gameIsRunning = true;

			CanvasManager.instance.CloseLoadingImg();

			//take a look in public IEnumerator WaitAnswer()
			tryJoinServer = false;

			// the local player now is logged
			onLogged = true;

			Debug.Log("player in game");

			newPlayer.gameObject.name = data.pack[1];

			//setting player pos
			newPlayer.gameObject.transform.position = spawnPoints[crntSpwnPlrNo].transform.position;

			crntSpwnPlrNo++;
		}
	}

	/// <summary>
	/// Raises the spawn player event.
	/// </summary>
	/// <param name="_msg">Message.</param>
	void OnSpawnPlayer(SocketUDPEvent data)
	{

		/*
		 * data.pack[0] = SPAWN_PLAYER
		 * data.pack[1] = id (network player id)
		 * data.pack[2]= name
		 * data.pack[3] = position.x
		 * data.pack[4] = position.y
		 * data.pack[5] = position.z
		 * data.pack[6] = rotation.x
		 * data.pack[7] = rotation.y
		 * data.pack[8] = rotation.z
		 * data.pack[9] = rotation.w
		*/

		if (onLogged ) {

		
			bool alreadyExist = false;

			//verify all players to  prevents copies
			foreach(KeyValuePair<string, PlayerManager> entry in networkPlayers)
			{
				// same id found ,already exist!!! 
				if (entry.Value.id== data.pack [1])
				{
					alreadyExist = true;
				}
			}
			if (!alreadyExist) {

				Debug.Log("creating a new player");

				PlayerManager newPlayer;

				// newPlayer = GameObject.Instantiate( network player avatar or model, spawn position, spawn rotation)
				newPlayer = GameObject.Instantiate (networkPlayerPrefabs [0],
					new Vector3(float.Parse(data.pack[3]), float.Parse(data.pack[4]), 
						float.Parse(data.pack[5])),Quaternion.identity).GetComponent<PlayerManager> ();


				//it is not the local player
				newPlayer.isLocalPlayer = false;

				//network player online in the arena
				newPlayer.isOnline = true;

				//set the network player 3D text with his name

				newPlayer.gameObject.name = data.pack [1];

				//puts the local player on the list
				networkPlayers [data.pack [1]] = newPlayer;

				//setting player pos
				newPlayer.gameObject.transform.position = spawnPoints[crntSpwnPlrNo].transform.position;

				crntSpwnPlrNo++;
			}

		}

	}

	/// <summary>
	///  Update the network player position to local player.
	/// </summary>
	/// <param name="_msg">Message.</param>
	void OnUpdatePosition(SocketUDPEvent data)
	{

		/*
		 * data.pack[0] = UPDATE_MOVE
		 * data.pack[1] = id (network player id)
		 * data.pack[2] = position.x
		 * data.pack[3] = position.y
		 * data.pack[4] = position.z
		*/

		//it reduces to zero the accountant meaning that answer of the server exists to this moment
		contTimes = 0;

		if (networkPlayers [data.pack [1]] != null) {
		
			//find network player
			PlayerManager netPlayer = networkPlayers [data.pack [1]];
			netPlayer.timeOut = 0f;
	
		 
	    }


	}



	//start game network here
	public void isGameReady()
	{
		//hash table <key, value>
		Dictionary<string, string> data = new Dictionary<string, string>();

		//JSON package
		data["callback_name"] = "GAMEREADY";

		data["local_player_id"] = myPlayer.GetComponent<PlayerManager>().id;

		string msg = data["local_player_id"];

		//sends to the server through socket UDP the jo package 
		udpClient.Emit(data["callback_name"], msg);

	}

	public void startGame()
	{
		//hash table <key, value>
		Dictionary<string, string> data = new Dictionary<string, string>();

		//JSON package
		data["callback_name"] = "GAMESTART";

		data["local_player_id"] = myPlayer.GetComponent<PlayerManager>().id;

		//send the position point to server
		string msg = data["local_player_id"];

		//sends to the server through socket UDP the jo package 
		udpClient.Emit(data["callback_name"], msg);
	}

	public void selectSuit(string suit)
	{
		//hash table <key, value>
		Dictionary<string, string> data = new Dictionary<string, string>();

		//JSON package
		data["callback_name"] = "SELECTASUIT";

		data["suitName"] = suit;

		string msg = data["suitName"];

		//sends to the server through socket UDP the jo package 
		udpClient.Emit(data["callback_name"], msg);
	}

	//send chal to server
	public void chal(string playerID, string cardName, string cardTag)
	{
		//hash table <key, value>
		Dictionary<string, string> data = new Dictionary<string, string>();

		//JSON package
		data["callback_name"] = "CHAL";

		data["playerID"] = playerID;

		data["cardName"] = cardName;

		data["cardTag"] = cardTag;


		string msg = data["playerID"] + ":" +  data["cardName"] + ":" + data["cardTag"];

		udpClient.Emit(data["callback_name"], msg);
	}

	//send throw to server
	public void doThrow(string playerID, string cardName, string cardTag)
	{
		//hash table <key, value>
		Dictionary<string, string> data = new Dictionary<string, string>();

		//JSON package
		data["callback_name"] = "THROW";

		data["playerID"] = playerID;

		data["cardName"] = cardName;

		data["cardTag"] = cardTag;


		string msg = data["playerID"] + ":" + data["cardName"] + ":" + data["cardTag"];

		udpClient.Emit(data["callback_name"], msg);
	}




	void GameOver()
	{
		if(myPlayer)
		{
			//hash table <key, value>
			Dictionary<string, string> data = new Dictionary<string, string>();

			//JSON package
			data["callback_name"] = "disconnect";

			data ["local_player_id"] = local_player_id;

			if (UDPServer.instance.serverRunning) {

				data ["isMasterServer"] = "true";
			}
			else 
			{
				data ["isMasterServer"] = "false";
			}
				
			//send the position point to server
			string msg = data["local_player_id"]+":"+data ["isMasterServer"];

			//Debug.Log ("emit disconnect");

			//we make four attempts of similar sending of preventing the loss of packages
			udpClient.Emit (data["callback_name"] ,msg);

			udpClient.Emit (data["callback_name"] ,msg);

			udpClient.Emit (data["callback_name"] ,msg);

			udpClient.Emit (data["callback_name"] ,msg);
		}

		if (udpClient != null) {

			udpClient.disconnect ();


		}
	}


	/// <summary>
	/// inform the local player to destroy offline network player
	/// </summary>
	/// <param name="_msg">Message.</param>
	//disconnect network player
	void OnUserDisconnected(SocketUDPEvent data )
	{

		/*
		 * data.pack[0]  = USER_DISCONNECTED
		 * data.pack[1] = id (network player id)
		 * data.pack[2] = isMasterServer
		*/
		Debug.Log ("disconnect!");

		if (bool.Parse (data.pack [2])) {
			
			RestartGame ();
		}
		else
		{
			
				if (networkPlayers [data.pack [1]] != null) {
				
					//destroy network player by your id
					Destroy (networkPlayers [data.pack [1]].gameObject);

					//remove from the dictionary
					networkPlayers.Remove (data.pack [1]);
				}
		}


	}

	public void RestartGame()
	{
		CanvasManager.instance.txtSearchServerStatus.text = "PLEASE START SERVER";

		Destroy (camRig.gameObject);
		foreach(KeyValuePair<string, PlayerManager> entry in networkPlayers)
		{
			if (networkPlayers [entry.Key] != null) {
				Destroy (networkPlayers [entry.Key].gameObject);
			}
		}

		networkPlayers.Clear ();

		gameIsRunning = false;

		serverFound = false;

		myId = string.Empty;

		CanvasManager.instance.OpenScreen (0);

	}



	void OnApplicationQuit() {

		Debug.Log("Application ending after " + Time.time + " seconds");

		GameOver ();
			
	}
		
}
