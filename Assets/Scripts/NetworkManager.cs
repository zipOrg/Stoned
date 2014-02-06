using UnityEngine;
using System.Collections;

public class NetworkManager : MonoBehaviour {

	public static NetworkManager instance;

	private MainMenu menuScript;
	private HostData[] availableServers;
	private bool showGUI = true;

	private bool refreshingGameList = false;
	private bool creatingGame = false;

	private void Awake()
	{
		if(instance)
		{
			Destroy(gameObject);
		}
		else
		{
			instance = this;

		}
	}

	private void CreateServer(string serverName)
	{
		if(Network.HavePublicAddress())
		{
			Debug.Log("Przebijamy sie przez NAT");
		}
		else
		{
			Debug.Log("Nie przebijamy się przez NAT");
		}
		Network.InitializeServer(2,32167,!Network.HavePublicAddress());
		MasterServer.RegisterHost("Stoned",serverName);
	}

	private void JoinServer(HostData serverHostData)
	{
		Network.Connect(serverHostData);
	}

	private void RefreshServerList()
	{
		refreshingGameList = true;
		MasterServer.RequestHostList("Stoned");
	}

	private void OnMasterServerEvent(MasterServerEvent masterServerEvent)
	{
		switch(masterServerEvent)
		{
		case MasterServerEvent.HostListReceived:
			availableServers = MasterServer.PollHostList();
			refreshingGameList = false;
			break;

		default:
			break;
		}
	}

	private void DisplayServerList()
	{
		foreach(HostData hostData in availableServers)
		{
			if(GUILayout.Button(hostData.gameName))
			{
				JoinServer(hostData);
			}
		}
	}

	private void DisplayJoinAndCreateButtons()
	{
		GUILayout.BeginHorizontal();

		if(GUILayout.Button("Create Game"))
		{
			CreateServer("testowa giera");
		}

		if(GUILayout.Button("Refresh Server List"))
		{
			RefreshServerList();	
		}
		GUILayout.EndHorizontal();
	}

	private void OnConnectedToServer()
	{
		showGUI = false;
		menuScript.HideMenu();
	}

	private void OnServerInitialized()
	{
		showGUI = false;
		menuScript.HideMenu();
	}

	public void ShowCreateWindow()
	{
		showCreateGameModalWindow = true;
		showCantCreateModalDialog = false;
		showJoinGameModalWindow = false;
		showCantJoinModalDialog = false;
		createdGameName = "Game";
	}

	public void ShowJoinWindow()
	{
		RefreshServerList();
		showCreateGameModalWindow = false;
		showCantCreateModalDialog = false;
		showJoinGameModalWindow = true;
		showCantJoinModalDialog = false;
	}

	public void SetMainMenuScript(MainMenu menuScript)
	{
		this.menuScript = menuScript;
	}

	private void OnGUI()
	{
		if(showGUI)
		{
			if(showCreateGameModalWindow)
			{

				if(showCantCreateModalDialog)
				{
					GUI.ModalWindow(0,new Rect(Screen.width - xOffset - createGameWindowWidth,yOffset,createGameWindowWidth,createGameWidnowHeight),CantCreateGameModalDialog,"Warning!");
				}
				else
				{
					GUI.ModalWindow(0,new Rect(Screen.width - xOffset - createGameWindowWidth,yOffset,createGameWindowWidth,createGameWidnowHeight),CreateGameModalWindow,createModalWindowTitle);
				}
			}
			else if(showJoinGameModalWindow)
			{
				if(showCantJoinModalDialog)
				{
					GUI.ModalWindow(0,new Rect(Screen.width - xOffset - createGameWindowWidth,yOffset,createGameWindowWidth,createGameWidnowHeight),CantJoinGameModalDialog,"Warning!");
				}
				else
				{
					GUI.ModalWindow(0,new Rect(Screen.width - xOffset - joinGameWindowWidth,yOffset,joinGameWindowWidth,joinGameWidnowHeight),JoinGameModalWindow,joinModalWindowTitle);
				}
			}
		}

	}




	private float xOffset = 50.0f;
	private float yOffset = 50.0f;


	#region Create Game Modal
	private string createModalWindowTitle = "Create Game";
	private string createdGameName = "Game";

	private bool showCreateGameModalWindow = false;
	private bool showCantCreateModalDialog = false;

	private float createGameWindowWidth = 480.0f;
	private float createGameWidnowHeight = 135.0f;



	private void CreateGameModalWindow(int id)
	{
		GUILayout.FlexibleSpace();
		GUILayout.Label("Enter name of your game here:");
		createdGameName = GUILayout.TextField(createdGameName);
		GUILayout.FlexibleSpace();
		GUILayout.BeginHorizontal();
		if(GUILayout.Button("Cancel"))
		{
			showCreateGameModalWindow = false;
		}
		if(GUILayout.Button("Create!"))
		{
			if(!creatingGame)
			{
				StartCoroutine(CreateGameCoroutine());
			}
		}
		GUILayout.EndHorizontal();
	}

	private void CantCreateGameModalDialog(int id)
	{
		GUILayout.Label("The game with this name already exists!\nPlease select a different name.");
		GUILayout.FlexibleSpace();
		if(GUILayout.Button("Ok"))
		{
			showCantCreateModalDialog = false;
		}
	}


	private IEnumerator CreateGameCoroutine()
	{
		creatingGame = true;
		RefreshServerList();
		while(refreshingGameList)
		{
			yield return new WaitForSeconds(0.1f);
		}
		if(IsGameNameAvailable(createdGameName))
		{
			//tworzymy gre
			CreateServer(createdGameName);
		}
		else
		{
			showCantCreateModalDialog = true;
		}
		creatingGame = false;
	}

	private bool IsGameNameAvailable(string gameName)
	{
		bool retVal = true;
		foreach(HostData host in availableServers)
		{
			if(host.gameName.Equals(host))
			{
				retVal = false;
			}
		}
		return retVal;
	}

	#endregion

	#region Join Game Modal
	private string joinModalWindowTitle = "Join Game";
	private HostData selectedHost = null;

	private bool showJoinGameModalWindow = false;
	private bool showCantJoinModalDialog = false;

	private float joinGameWindowWidth = 480.0f;
	private float joinGameWidnowHeight = 400.0f;

	private Vector2 scrollViewPositionCache;

	private void CantJoinGameModalDialog(int id)
	{
		GUILayout.Label("Can't join game");
		GUILayout.FlexibleSpace();
		if(GUILayout.Button("Ok"))
		{
			showCantJoinModalDialog = false;
		}
	}

	private void JoinGameModalWindow(int id)
	{
		GUILayout.Label("Select a game to join!");
		scrollViewPositionCache = GUILayout.BeginScrollView(scrollViewPositionCache,false,true,GUILayout.Height(250));
		if(availableServers != null)
		{
			foreach(HostData host in availableServers)
			{
				if(GUILayout.Button(host.gameName +"\tPlayers: "+ host.connectedPlayers+"/2"))
				{
					selectedHost = host;
				}
			}
		}
		GUILayout.EndScrollView();
		if(selectedHost != null)
		{
			GUILayout.Label("Selected game: "+selectedHost.gameName);
		}
		else
		{
			GUILayout.Label("No selected game");
		}

		GUILayout.FlexibleSpace();
		GUILayout.BeginHorizontal();
		if(GUILayout.Button("Cancel"))
		{
			showCreateGameModalWindow = false;
		}
		if(GUILayout.Button("Refresh!"))
		{
			if(!refreshingGameList)
			{
				RefreshServerList();
			}
		}
		if(GUILayout.Button("Join!"))
		{
			if(selectedHost != null)
			{
				JoinServer(selectedHost);
			}
		}
		GUILayout.EndHorizontal();
	}


	#endregion


}
