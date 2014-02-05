using UnityEngine;
using System.Collections;

public class NetworkManager : MonoBehaviour {


	private HostData[] availableServers;
	private bool showGUI = true;

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
		MasterServer.RequestHostList("Stoned");
	}

	private void OnMasterServerEvent(MasterServerEvent masterServerEvent)
	{
		switch(masterServerEvent)
		{
		case MasterServerEvent.HostListReceived:
			availableServers = MasterServer.PollHostList();
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
	}

	private void OnServerInitialized()
	{
		showGUI = false;
	}


	private void OnGUI()
	{

		if(showGUI)
		{
			DisplayJoinAndCreateButtons();
			
			if(availableServers != null)
			{
				DisplayServerList();
			}
		}

	}




}
