using UnityEngine;
using System.Collections;

public class SpawnPointTrigger : MonoBehaviour {


	private void OnTriggerExit(Collider other)
	{
		if(other.gameObject.tag.Equals("Player"))
		{
			Player collidingPlayer = other.gameObject.GetComponent<Player>();
			collidingPlayer.SetGodMode(false);
		}
	}
}
