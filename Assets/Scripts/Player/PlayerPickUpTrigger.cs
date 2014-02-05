using UnityEngine;
using System.Collections;

public class PlayerPickUpTrigger : MonoBehaviour {
	private Player playerScript;

	private void Awake()
	{
		playerScript = transform.root.GetComponent<Player>();
	}

	private void OnTriggerEnter(Collider other)
	{
		Debug.Log("Enter " + other.name);
		Stone triggeredStone = other.gameObject.GetComponent<Stone>();
		if(triggeredStone)
		{
			playerScript.AddStoneToPickUpList(triggeredStone);
		}
	}

	private void OnTriggerExit(Collider other)
	{
		Debug.Log("Exit "+other.name);
		Stone triggeredStone = other.gameObject.GetComponent<Stone>();
		if(triggeredStone)
		{
			playerScript.RemoveStoneFromPickUpList(triggeredStone);
		}
	}



}
