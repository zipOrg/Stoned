using UnityEngine;
using System.Collections;

public class StoneTrigger : MonoBehaviour {
	public Stone stoneScript;

	private void OnTriggerEnter(Collider other)
	{
		stoneScript.StoneTriggerEnter(other);
	}

	private void OnTriggerExit(Collider other)
	{
		stoneScript.StoneTriggerExit(other);
	}
}
