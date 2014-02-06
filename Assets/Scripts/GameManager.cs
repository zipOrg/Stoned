
using UnityEngine;
using System.Collections;
using System.Collections.Generic;




public class GameManager : MonoBehaviour {
	public static GameManager instance;

	public Camera mainCamera;
	public GameObject playerPrefab;
	public GameObject stonePrefab;
	public Transform playerOneSpawnPoint;
	public Transform playerTwoSpawnPoint;
	public int scoreToWin;

	public TextMesh yourScore;
	public TextMesh enemyScore;
	public TextMesh infoTextMesh;

	private Vector3 cameraStartPosition;

	private bool serverPlayerReady;
	private bool clientPlayerReady;

	private bool rematchPending;


	private void Awake()
	{
		if(instance)
		{
			Destroy(gameObject);
		}
		else
		{
			instance = this;
			cameraStartPosition = mainCamera.transform.position;
			AudioListener.volume = 0.4f;
		}
	}

	private Player CreatePlayer(int playerNumber)
	{
		GameObject player = null;
		Player playerScript = null;
		
		switch(playerNumber)
		{
		case 1:
			player = Network.Instantiate(playerPrefab,playerOneSpawnPoint.position,playerOneSpawnPoint.rotation,0) as GameObject;
			playerScript = player.GetComponent<Player>();
			playerScript.networkView.RPC ("SetSpawnPositionAndRotation",RPCMode.AllBuffered,playerOneSpawnPoint.position,playerOneSpawnPoint.eulerAngles);
			mainCamera.transform.eulerAngles = new Vector3(90.0f,0.0f,0.0f);
			break;

		case 2:
			player = Network.Instantiate(playerPrefab,playerTwoSpawnPoint.position,playerTwoSpawnPoint.rotation,0) as GameObject;
			playerScript = player.GetComponent<Player>();
			playerScript.SetDIrectionMultiplier(-1.0f);
			playerScript.networkView.RPC ("SetSpawnPositionAndRotation",RPCMode.AllBuffered,playerTwoSpawnPoint.position,playerTwoSpawnPoint.eulerAngles);
			playerScript.networkView.RPC("SetColor",RPCMode.AllBuffered,1.0f,0.0f,0.0f,1.0f);
			mainCamera.transform.eulerAngles = new Vector3(90.0f,180.0f,0.0f);
			break;

		default:
			player = Network.Instantiate(playerPrefab,playerOneSpawnPoint.position,playerOneSpawnPoint.rotation,0) as GameObject;
			playerScript = player.GetComponent<Player>();
			playerScript.networkView.RPC ("SetSpawnPositionAndRotation",RPCMode.AllBuffered,playerOneSpawnPoint.position,playerOneSpawnPoint.eulerAngles);
			mainCamera.transform.eulerAngles = new Vector3(90.0f,0.0f,0.0f);
			break;
		}

		return playerScript;
	}

	private void OnConnectedToServer()
	{
		CreatePlayer(2);
	}
	
	private void OnServerInitialized()
	{
		CreatePlayer(1);
	}

	public Stone CreateStone(Vector3 position, Quaternion rotation)
	{
		position.y = 1.4f;
		GameObject stone = Network.Instantiate(stonePrefab,position,rotation,0) as GameObject;
		return stone.GetComponent<Stone>();
	}

	public Stone CreateFirstStone()
	{
		return CreateStone(new Vector3(0.0f,1.4f,0.0f),Quaternion.identity);
	}

	private IEnumerator ShakeCameraCoroutine(float intensity, float duration)
	{
		float currentTime = 0.0f;

		while(currentTime < duration)
		{
			Vector3 randomShake = Random.insideUnitSphere;
			randomShake.y = 0.0f;
			randomShake = randomShake * intensity * (1.0f - currentTime/duration);
			mainCamera.transform.position = cameraStartPosition + randomShake;
			currentTime += Time.deltaTime;
			yield return new WaitForEndOfFrame();
		}
		mainCamera.transform.position = cameraStartPosition;
	}

	public static void ShakeCamera(float intensity, float duration)
 	{
		instance.StartCoroutine(instance.ShakeCameraCoroutine(intensity,duration));
	}

	public static void SetYourScoreText(string newText)
	{
		instance.yourScore.text = newText;
	}

	public static void SetInfoTextMeshText(string newText)
	{
		instance.infoTextMesh.text = newText;
	}

	public static void SetEnemyInfoTextMeshText(string newText)
	{
		instance.networkView.RPC ("SetInfoTextRPC",RPCMode.OthersBuffered,newText);
	}

	public static void SetEnemyScoreText(string newText)
	{
		instance.networkView.RPC ("SetEnemyScoreTextRPC",RPCMode.OthersBuffered,newText);
	}

	[RPC]
	private void SetEnemyScoreTextRPC(string newText)
	{
		enemyScore.text = newText;
	}

	[RPC]
	private void SetInfoTextRPC(string newText)
	{
		infoTextMesh.text = newText;
	}


	[RPC]
	public void SetServerPlayerReady(bool ready)
	{
		serverPlayerReady = ready;
	}

	[RPC]
	public void SetClientPlayerReady(bool ready)
	{
		clientPlayerReady = ready;
	}

	public static bool AreBothPlayersReady()
	{
		return instance.serverPlayerReady && instance.clientPlayerReady;
	}

	[RPC]
	public void ResetGameRPC()
	{
		if(!rematchPending)
		{
			rematchPending = true;
			GameObject[] stones = GameObject.FindGameObjectsWithTag("Stone");
			foreach(GameObject stone in stones)
			{
				Destroy(stone);
			}
			instance.serverPlayerReady = false;
			instance.clientPlayerReady = false;
			GameObject[] players = GameObject.FindGameObjectsWithTag("Player");
			foreach(GameObject player in players)
			{
				Player playerScript = player.GetComponent<Player>();
				playerScript.Respawn(false);
				playerScript.Init();
			}
			SetEnemyScoreText("0");
			SetYourScoreText("0");
			rematchPending = false;
		}
	}


	public static void ResetGame()
	{
		instance.networkView.RPC ("ResetGameRPC",RPCMode.AllBuffered);
	}

	public static int GetScoreToWin()
	{
		return instance.scoreToWin;
	}

	private void OnPlayerDisconnected(NetworkPlayer player)
	{
		MasterServer.UnregisterHost();
		Application.LoadLevel("Main");
	}

	private void OnDisconnectedFromServer(NetworkDisconnection disconnection)
	{
		Application.LoadLevel("Main");
	}

}
