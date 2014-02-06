using UnityEngine;
using System.Collections;

public class PauseMenu : MonoBehaviour {

	public void QuitGame()
	{
		Network.Disconnect();
		Application.LoadLevel("Main");
	}

	public void Resume()
	{
		GameManager.HidePauseMenu();
	}
}
