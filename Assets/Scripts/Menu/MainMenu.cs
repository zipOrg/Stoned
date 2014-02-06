using UnityEngine;
using System.Collections;

public class MainMenu : MonoBehaviour {

	public GameObject poseCamera;
	private NetworkManager networkManager;


	private void Start()
	{
		networkManager = NetworkManager.instance;
		networkManager.SetMainMenuScript(this);
	}

	public void QuitGame()
	{
		Application.Quit();
	}

	public void CreateGame()
	{
		networkManager.ShowCreateWindow();
	}

	public void JoinGame()
	{
		networkManager.ShowJoinWindow();
	}

	public void HideMenu()
	{
		Destroy(poseCamera);
		Destroy(gameObject);
	}

}
