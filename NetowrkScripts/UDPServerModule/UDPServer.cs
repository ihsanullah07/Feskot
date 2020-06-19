using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using UnityEngine;
using System;
using System.Net;
using System.Net.Sockets;
using System.Net.NetworkInformation;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using UDPClientModule;

using Random = UnityEngine.Random;

namespace UDPServerModule
{

	public class UDPServer : MonoBehaviour {

		public GameManagerMulti gameManagerScrtipt;


	    public static UDPServer instance;

		//from UDP Client Module API
		private UDPClientComponent udpClient;

		public int serverSocketPort;

		UdpClient udpServer;

		private readonly object udpServerLock = new object();

		private readonly object connectedClientsLock = new object();

		static private readonly char[] Delimiter = new char[] {':'};

		private const int bufSize = 8 * 1024;

		private State state = new State();

		private IPEndPoint endPoint;

		private EndPoint epFrom = new IPEndPoint(IPAddress.Any, 0);

		private AsyncCallback recv = null;

		public enum UDPServerState {DISCONNECTED,CONNECTED,ERROR,SENDING_MESSAGE};

		public UDPServerState udpServerState;

		public string[] pack;

		private Thread tListenner;

		public string serverHostName;

		string receivedMsg = string.Empty;

		private bool stopServer = false;

		public int serverPort = 3310;

		public bool tryCreateServer;

		public bool waitingAnswer;

		public bool serverRunning;

		//public string localNetworkIP;

		public int onlinePlayers;

		public float maxTimeOut;


		//store all players in game
		public Dictionary<string, Client> connectedClients = new Dictionary<string, Client>();

		public float cont;


		//room 
		public bool isRoomFull = false;
		
		public class Client
		{
			public string  id;

			public string name;

			public Vector3 position;

			public Quaternion rotation;

			public string animation;

			public int kills = 0;

			public int health = 100;

			public float timeOut = 0f;

			public IPEndPoint remoteEP;

		}

		public class State
		{
			public byte[] buffer = new byte[bufSize];


		}


		public void Awake()
		{
			udpServerState = UDPServerState.DISCONNECTED;

		}


		// Use this for initialization
		void Start () {

			// if don't exist an instance of this class
			if (instance == null) {

				//it doesn't destroy the object, if other scene be loaded
				DontDestroyOnLoad (this.gameObject);

				instance = this;// define the class as a static variable

				udpClient = gameObject.GetComponent<UDPClientComponent>();

			}
			else
			{
				//it destroys the class if already other class exists
				Destroy(this.gameObject);
			}

		}

		void Update()
		{
			if (Input.GetKeyDown(KeyCode.Space))
			{
				NetworkManager.instance.isGameReady();

			}

			//important to avoid synchronization mistakes
			lock (connectedClientsLock) 
			{

				List<string> keys = new List<string> (connectedClients.Keys);

				foreach (string key in keys) {

					//increases the time of wait
					connectedClients [key].timeOut += Time.deltaTime;

					//the client is verified exceeded the time limits of wait
					if (connectedClients [key].timeOut >= maxTimeOut) {

						//connectedClients.Remove (connectedClients [key].id);
					}
				}
			}
		}

		//checking clients
		void allPlayers()
		{
			List<string> keys = new List<string>(connectedClients.Keys);

			foreach (string key in keys)
			{
				//increases the time of wait
				Debug.Log(connectedClients[key].id);
			}

		}

		public string GetServerStatus()
		{
			switch (udpServerState)
			{
			    case  UDPServerState.DISCONNECTED:
				 return "DISCONNECTED";
				break;

			    case  UDPServerState.CONNECTED:
				 return "CONNECTED";
				break;

			    case  UDPServerState.SENDING_MESSAGE:
				 return "SENDING_MESSAGE";
				break;

			    case  UDPServerState.ERROR:
				 return "ERROR";
				break;
			}

			return string.Empty;
		}


		//get local server ip address
		public string GetServerIP() {


			IPHostEntry host = Dns.GetHostEntry(Dns.GetHostName());

			string address = string.Empty;

			string subAddress = string.Empty;

			//search WiFI Local Network
			foreach (IPAddress ip in host.AddressList) {

				if (ip.AddressFamily == AddressFamily.InterNetwork) {


					if (!ip.ToString ().Contains ("127.0.0.1")) {
						address = ip.ToString ();
					}


				}
			}


			if (address==string.Empty) {

				return string.Empty;
			}

			else
			{

			    subAddress = address.Remove(address.LastIndexOf('.'));

				return subAddress + "." + 255;
			}

			return string.Empty;

		}

		/// <summary>
		/// Creates a UDP Server in in the associated client
		/// called method when the button "start" on HUDCanvas is pressed
		/// </summary>
		public void CreateServer()
		{
			if (GetServerIP()!= string.Empty) {

				if (NetworkManager.instance.serverFound && !serverRunning)
				{
					CanvasManager.instance.ShowAlertDialog ("SERVER ALREADY RUNNING ON NETWORK!");
				}

				else
				{
					if (!serverRunning) {

						StartServer (serverPort);

						serverRunning = true;

						Debug.Log ("UDP Server listening on IP " + GetServerIP () + " and port " + serverPort);

						Debug.Log ("------- server is running -------");

						CanvasManager.instance.inputLogin.text = "Master";
					}

					NetworkManager.instance.EmitJoin ();
				}

			}
			else
			{
				CanvasManager.instance.ShowAlertDialog ("PLEASE CONNECT TO A WIFI NETWORK");
			}


		}



		/// <summary>
		/// Starts the server.
		/// </summary>
		/// <param name="_serverPort">Server port.</param>
		public void StartServer( int _serverPort) {


			if ( tListenner != null && tListenner.IsAlive) {

				CloseServer();

				while (tListenner != null && tListenner.IsAlive) {}

			}

			// set server port
			this.serverSocketPort = _serverPort;

			// start  listener thread
			tListenner = new Thread(
				new ThreadStart(OnListeningClients));

			tListenner.IsBackground = true;

			tListenner.Start();

		}



		//***************************   recieveing data sent to server   ***************************

		/// <summary>
		/// Raises the listening clients event.
		/// </summary>
		/// 
		//recieveing clients data
		public void  OnListeningClients()
		{



			udpServer = new UdpClient (serverSocketPort);

			udpServer.Client.ReceiveTimeout = 300; // msec


			while (stopServer == false) {
				try {

					udpServerState = UDPServerState.CONNECTED;

					IPEndPoint anyIP = new IPEndPoint(IPAddress.Any, 0);

					byte[] data = udpServer.Receive(ref anyIP);

					string text = Encoding.ASCII.GetString(data);

					receivedMsg  = text;

					pack = receivedMsg.Split (Delimiter);


					switch(pack[0] )
					{
						case "GAMEREADY":
							OnGameReady(pack, anyIP);
							break;

						case "GAMESTART":
							OnGameStart(pack, anyIP);	
							break;

						case "SELECTASUIT":
							OnSuitSelected(pack, anyIP);
							break;

						case "CHAL":
							OnChal(pack, anyIP);
							break;

						case "THROW":
							OnThrow(pack, anyIP);
							break;

						case "PING":
						OnReceivePing(pack,anyIP);//processes the received package
					  break;

					  case "JOIN":
						OnReceiveJoin(pack,anyIP);//processes the received package
					  break;

					  case "MOVE":
						OnReceiveMove(pack,anyIP);//processes the received package
					  break;

					  case "disconnect":
						OnReceiveDisconnect(pack,anyIP);//processes the received package
					  break;



					}//END_SWTCH

				}//END_TRY
				catch (Exception err)
				{
					//print(err.ToString());
				}
			}//END_WHILE
		}

		public string generateID()
		{
			return Guid.NewGuid().ToString("N");
		}

		void OnGameReady(string[] pack, IPEndPoint anyIP)
		{
			gameManagerScrtipt.readyToStartGM();
		}

		//NOW START	 GAME
		void OnGameStart(string[] pack, IPEndPoint anyIP)
		{
			gameManagerScrtipt.startGame();
		}

		void OnSuitSelected(string[] pack, IPEndPoint anyIP)
		{
			//getting info back to suit selecter
			//to hide its panel

			Dictionary<string, string> send_pack = new Dictionary<string, string>();

			var response = string.Empty;

			byte[] msg = null;

			//JSON package
			send_pack["callback_name"] = "SUITSELECTED";

			send_pack["color_suit"] = pack[1];

			response = send_pack["callback_name"] + ':' + send_pack["color_suit"];

			//buffering response in byte array
			msg = Encoding.ASCII.GetBytes(response);


			//seding suit info to all clients

			//send answer in broadcast

			foreach (KeyValuePair<string, Client> entry in connectedClients)
			{
				//send answer to all clients in connectClients list
				udpServer.Send(msg, msg.Length, entry.Value.remoteEP);

			}//END_FOREACH

			//giving remaining cards
			gameManagerScrtipt.is4CardsDis = true;

		}


		void OnChal(string[] pack, IPEndPoint anyIP)
		{
			int crdNo = int.Parse(pack[2]);
			string crdTag = pack[3];

			//this int just for increment so we get the playrNo when crnt plr id match the one who chaled
			int plrNo = 0;

			List<string> keys = new List<string>(connectedClients.Keys);

			foreach (string key in keys)
			{
				if(connectedClients[key].id == pack[1])
				{
					gameManagerScrtipt.chal(plrNo, crdNo, crdTag);
				}

				plrNo++;
			}


			//sending a info back to client for removing card from game

			Dictionary<string, string> send_pack = new Dictionary<string, string>();

			var response = string.Empty;

			byte[] msg = null;


			send_pack["callback_name"] = "CHALDONE";

			send_pack["candName"] = crdNo.ToString();

			send_pack["cardTag"] = crdTag;

			response = send_pack["callback_name"] + ':' + send_pack["candName"] + ':' + send_pack["cardTag"];

			msg = Encoding.ASCII.GetBytes(response);

			udpServer.Send(msg, msg.Length, anyIP); // echo to client

		}

		void OnThrow(string[] pack, IPEndPoint anyIP)
		{
			Debug.Log("Throw recieved");

			int crdNo = int.Parse(pack[2]);
			string crdTag = pack[3];

			//this int just for increment so we get the playrNo when crnt plr id match the one who chaled
			int plrNo = 0;

			List<string> keys = new List<string>(connectedClients.Keys);

			foreach (string key in keys)
			{
				if (connectedClients[key].id == pack[1])
				{
					gameManagerScrtipt.isThrow(plrNo, crdNo, crdTag);
				}

				plrNo++;
			}


			//sending a info back to client for removing card from game

			Dictionary<string, string> send_pack = new Dictionary<string, string>();

			var response = string.Empty;

			byte[] msg = null;


			send_pack["callback_name"] = "THROWDONE";

			send_pack["candName"] = crdNo.ToString();

			send_pack["cardTag"] = crdTag;

			response = send_pack["callback_name"] + ':' + send_pack["candName"] + ':' + send_pack["cardTag"];

			msg = Encoding.ASCII.GetBytes(response);

			udpServer.Send(msg, msg.Length, anyIP); // echo to client

		}

		void OnReceivePing(string [] pack,IPEndPoint anyIP )
		{
			/*
		       * pack[0]= CALLBACK_NAME: "PONG"
		       * pack[1]= "ping"
		    */


			Dictionary<string, string> send_pack = new Dictionary<string, string>();

			Dictionary<string, string> data2 = new Dictionary<string, string>();

			var response = string.Empty;

			byte[] msg = null;

			//JSON package
			send_pack["callback_name"] = "PONG";

			//store "pong!!!" message in msg field
			send_pack["msg"] = "pong!!!!";

			//format the data with the sifter comma for they be send from turn to udp client
			response = send_pack["callback_name"]+':'+send_pack["msg"];

			//buffering response in byte array
			msg = Encoding.ASCII.GetBytes(response);

			udpServer.Send(msg, msg.Length, anyIP); // echo to client
		}


		void OnReceiveJoin(string [] pack,IPEndPoint anyIP )
		{

			/*
		        * pack[0] = CALLBACK_NAME: "JOIN"
		        * pack[1] = player_name
		        * pack[2] = player id
		        * pack[3] = position.x
		        * pack[4] = position.y
		        * pack[5] = position.z
		    */
			


				if (!connectedClients.ContainsKey (pack [2])) {

					Dictionary<string, string> send_pack = new Dictionary<string, string> ();

					Dictionary<string, string> data2 = new Dictionary<string, string> ();

					var response = string.Empty;

					byte[] msg = null;

					Client client = new Client ();

					client.id = pack [2];//set client id

					client.name = pack [1];//set client name

					
					client.position = new Vector3 (float.Parse
				        (pack [3]), float.Parse (pack [4]), float.Parse (pack [5]));//set client position

					//set  clients's port and ip address
					client.remoteEP = anyIP;

					Debug.Log ("[INFO] player " + client.name + ": logged!");

				lock (connectedClientsLock) 
				{
					//add client in search engine
					connectedClients.Add (client.id.ToString (), client);

					onlinePlayers = connectedClients.Count;

					Debug.Log ("[INFO] Total players: " + connectedClients.Count);
				}

				/*********************************************************************************************/

				//JSON package
				send_pack ["callback_name"] = "JOIN_SUCCESS";

				//store  player info in msg field
				send_pack ["msg"] = client.id + ":" + client.name + ":" + client.position.x + ":" + client.position.y + ":" + client.position.z + ":" + client.rotation.x +
				":" + client.rotation.y + ":" + client.rotation.z + ":" + client.rotation.w;
				
				


				//format the data with the sifter comma for they be send from turn to udp client
				response = send_pack ["callback_name"] + ':' + send_pack ["msg"];

				msg = Encoding.ASCII.GetBytes (response);

				//send answer to client that called me
				udpServer.Send (msg, msg.Length, anyIP); // echo

				Debug.Log ("[INFO]sended : JOIN_SUCCESS");

				/*******************************************************************************************************************/

				/*******************************************************************************************************************/


				//sends the game clients for the client sender
				foreach (KeyValuePair<string, Client> entry in connectedClients) {

					// same id found ,already exist!!!
					if (entry.Value.id != client.id) {

						Dictionary<string, string> data3 = new Dictionary<string, string> ();

						//JSON package
						data3 ["callback_name"] = "SPAWN_PLAYER";


						data3 ["msg"] = entry.Value.id + ":" + entry.Value.name + ":" + entry.Value.position.x + ":" + entry.Value.position.y +
						":" + entry.Value.position.z + ":" + entry.Value.rotation.x +
						":" + entry.Value.rotation.y + ":" + entry.Value.rotation.z + ":" + entry.Value.rotation.w;


						//format the data with the sifter comma for they be send from turn to udp client
						response = data3 ["callback_name"] + ':' + data3 ["msg"];


						msg = Encoding.ASCII.GetBytes (response);

						//send answer to client that called me
						udpServer.Send (msg, msg.Length, anyIP); // echo

					}

				}
     /********************************************************************************************************************************/

				//JSON package
				data2 ["callback_name"] = "SPAWN_PLAYER";

				//store "pong!!!" message in msg field
				data2 ["msg"] = client.id + ":" + client.name + ":" + client.position.x + ":" + client.position.y + ":" + client.position.z + ":" + client.rotation.x +
				":" + client.rotation.y + ":" + client.rotation.z + ":" + client.rotation.w;

				//sends the client sender to all clients in game
				foreach (KeyValuePair<string, Client> entry in connectedClients) {


					if (entry.Value.id != client.id) {

						//format the data with the sifter comma for they be send from turn to udp client
						response = data2 ["callback_name"] + ':' + data2 ["msg"];


						msg = Encoding.ASCII.GetBytes (response);

						//send answer to all clients in connectClients list
						udpServer.Send (msg, msg.Length, entry.Value.remoteEP);

					}//END_IF

				}//END_FOREACH


			}//END_IF
		}

		/// <summary>
		/// proccess players position.
		/// </summary>
		/// <param name="pack">Pack.</param>
		/// <param name="anyIP">Any I.</param>
		void OnReceiveMove(string [] pack,IPEndPoint anyIP )
		{
			/*
		      * data.pack[0] = CALLBACK_NAME: "MOVE"
		      * data.pack[1] = player_id
		      * data.pack[2] = position.x
		      * data.pack[3] = position.y
		      * data.pack[4] = possition.z

		    */


			Dictionary<string, string> send_pack = new Dictionary<string, string>();

			Dictionary<string, string> data2 = new Dictionary<string, string>();

			var response = string.Empty;

			byte[] msg = null;
			lock (connectedClientsLock) {
				if (connectedClients.ContainsKey (pack [1])) {

					connectedClients [pack [1]].timeOut = 0f;
					connectedClients [pack [1]].position = new Vector3 (float.Parse (pack [2]), float.Parse (pack [3])
				, float.Parse (pack [4]));

					//JSON package
					send_pack ["callback_name"] = "UPDATE_MOVE";

					send_pack ["player_id"] = connectedClients [pack [1]].id;

					Vector3 position = new Vector3 (connectedClients [pack [1]].position.x,
						                   connectedClients [pack [1]].position.y, connectedClients [pack [1]].position.z);
										   
					

					send_pack ["position"] = position.x + ":" + position.y + ":" + position.z;

					send_pack ["msg"] = send_pack ["player_id"] + ":" + send_pack ["position"];

					response = send_pack ["callback_name"] + ':' + send_pack ["msg"];

					msg = Encoding.ASCII.GetBytes (response);
				}


				//send answer in broadcast
				foreach (KeyValuePair<string, Client> entry in connectedClients) {

				  //send answer to all clients in connectClients list
				  udpServer.Send (msg, msg.Length, entry.Value.remoteEP);

				}//END_FOREACH

		    }
		}


		//**********************  sending data  to Client  ***********************

		//send data to a client for select suit
		public void plrToSlctSuit(int playerId)
		{
			// getting ip for desired player using playerid

			Dictionary<string, string> send_pack = new Dictionary<string, string>();

			var response = string.Empty;

			byte[] msg = null;


			//JSON package
			send_pack["callback_name"] = "SELECTSUIT";

			//store "pong!!!" message in msg field
			send_pack["msg"] = "pong!!!!";

			//format the data with the sifter comma for they be send from turn to udp client
			response = send_pack["callback_name"] + ':' + send_pack["msg"];

			//buffering response in byte array
			msg = Encoding.ASCII.GetBytes(response);

			//Debug.Log(connectedClients.ContainsKey(pack[2]));

			int playerNo = 0;

			foreach (KeyValuePair<string, Client> entry in connectedClients)
			{
				//send answer to all clients in connectClients list
				if(playerId == playerNo)
				{
					udpServer.Send(msg, msg.Length, entry.Value.remoteEP);

					Debug.Log("deta sent");
				}

				playerNo++;

			}
		}

		public void sendCardToPlayer(int playerId, string cardName, string cardTag)
		{

			Dictionary<string, string> send_pack = new Dictionary<string, string>();

			var response = string.Empty;

			byte[] msg = null;
				

			//JSON package
			send_pack["callback_name"] = "GOTACARD";

			send_pack["card_name"] = cardName;

			send_pack["card_tag"] = cardTag;

			//format the data with the sifter comma for they be send from turn to udp client
			response = send_pack["callback_name"] + ':' + send_pack["card_name"] + ':' + send_pack["card_tag"];

			//buffering response in byte array
			msg = Encoding.ASCII.GetBytes(response);

			int playerNo = 0;

			foreach (KeyValuePair<string, Client> entry in connectedClients)
			{
				if (playerId == playerNo)
				{
					udpServer.Send(msg, msg.Length, entry.Value.remoteEP);
				}

				playerNo++;
			}
		}
		public void giveChalToPlr(int playerId)
		{

			Dictionary<string, string> send_pack = new Dictionary<string, string>();

			var response = string.Empty;

			byte[] msg = null;

			//JSON package
			send_pack["callback_name"] = "GOTCHAL";
			send_pack["player_id"] = playerId.ToString();

			//format the data with the sifter comma for they be send from turn to udp client
			response = send_pack["callback_name"] + ':' + send_pack["player_id"];

			//buffering response in byte array
			msg = Encoding.ASCII.GetBytes(response);

			int playerNo = 0;

			foreach (KeyValuePair<string, Client> entry in connectedClients)
			{
				if (playerId == playerNo)
				{
					udpServer.Send(msg, msg.Length, entry.Value.remoteEP);
				}

				playerNo++;
			}
		}

		//sending current chal suit to every player
		public void sendChalSuit(String chalSuit)
		{
			Dictionary<string, string> send_pack = new Dictionary<string, string>();

			var response = string.Empty;

			byte[] msg = null;


			//JSON package
			send_pack["callback_name"] = "CHALSUIT";

			send_pack["chalSuit"] = chalSuit;


			response = send_pack["callback_name"] + ':' + send_pack["chalSuit"];

			//buffering response in byte array
			msg = Encoding.ASCII.GetBytes(response);

			//sending  suit to all clients
			foreach (KeyValuePair<string, Client> entry in connectedClients)
			{
				udpServer.Send(msg, msg.Length, entry.Value.remoteEP);
			}

			Debug.Log("suit Sent");
		}

		public void showGroundCard(int plrId, string cardName, string cardTag)
		{
			Dictionary<string, string> send_pack = new Dictionary<string, string>();

			var response = string.Empty;

			byte[] msg = null;


			//JSON package
			send_pack["callback_name"] = "SHOWCARD";

			send_pack["plr_no"] = plrId.ToString();

			send_pack["card_name"] = cardName;

			send_pack["card_tag"] = cardTag;


			response = send_pack["callback_name"] + ':' + send_pack["plr_no"] + ':' + send_pack["card_name"] + ':' + send_pack["card_tag"];

			//buffering response in byte array
			msg = Encoding.ASCII.GetBytes(response);


			//sending show card to all clients
			foreach (KeyValuePair<string, Client> entry in connectedClients)
			{
				udpServer.Send(msg, msg.Length, entry.Value.remoteEP);

			}
		}

		public void giveThrowToPlr(int playerId)
		{
			Dictionary<string, string> send_pack = new Dictionary<string, string>();

			var response = string.Empty;

			byte[] msg = null;

			//JSON package
			send_pack["callback_name"] = "GOTTHROW";
			send_pack["player_id"] = playerId.ToString();

			//format the data with the sifter comma for they be send from turn to udp client
			response = send_pack["callback_name"] + ':' + send_pack["player_id"];

			//buffering response in byte array
			msg = Encoding.ASCII.GetBytes(response);

			int playerNo = 0;

			foreach (KeyValuePair<string, Client> entry in connectedClients)
			{
				if (playerId == playerNo)
				{
					udpServer.Send(msg, msg.Length, entry.Value.remoteEP);
				}

				playerNo++;
			}
		}

		void OnReceiveDisconnect(string [] pack,IPEndPoint anyIP )
		{
			/*
		     * data.pack[0]= CALLBACK_NAME: "disconnect"
		     * data.pack[1]= player_id
		     * data.pack[2]= isMasterServer (true or false)
		    */


			Dictionary<string, string> send_pack = new Dictionary<string, string>();

			Dictionary<string, string> data2 = new Dictionary<string, string>();

			var response = string.Empty;

			byte[] msg = null;
			lock (connectedClientsLock) {
				if (connectedClients.ContainsKey (pack [1])) {



					//JSON package
					send_pack ["callback_name"] = "USER_DISCONNECTED";

					//store "pong!!!" message in msg field
					send_pack ["msg"] = connectedClients [pack [1]].id.ToString ();

					send_pack ["isMasterServer"] = pack [2];

					response = send_pack ["callback_name"] + ':' + send_pack ["msg"]+':'+send_pack ["isMasterServer"] ;

					msg = Encoding.ASCII.GetBytes (response);

					connectedClients.Remove (pack [1]);

					try
					{
						//send answer in broadcast
				    foreach (KeyValuePair<string, Client> entry in connectedClients) {

				    //send answer to all clients in connectClients list
				     udpServer.Send (msg, msg.Length, entry.Value.remoteEP);

				     }//END_FOREACH
					}
					catch (SocketException e)
					{
						if(e.SocketErrorCode == SocketError.WouldBlock)
						{

							udpServer.Client.Blocking = true;
							//send answer in broadcast
							udpServer.Send (msg, msg.Length, new IPEndPoint (IPAddress.Parse (GetServerIP ()),
								NetworkManager.instance.clientPort));
							udpServer.Client.Blocking = false;
						}
					}




				}
			}

		}

		void OnApplicationQuit() {

			CloseServer ();

		}


		/**
     *  DISCONNECTS SERVER
     */
		public void CloseServer() {

			udpServerState = UDPServerState.DISCONNECTED;

			stopServer = true;


			if (udpServer != null)
			{
				udpServer.Close ();
				udpServer = null;
			}

			if (tListenner!=null) {

				tListenner.Abort ();
			}

		}
	}

}
