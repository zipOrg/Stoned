using UnityEngine;
using System.Collections;

public class SoundController : MonoBehaviour {

	private void Awake()
	{
		StartCoroutine(WaitForSoundFinish());
	}

	private IEnumerator WaitForSoundFinish()
	{
		while(audio.isPlaying)
		{
			yield return new WaitForSeconds(0.1f);
		}
		Destroy(gameObject);
	}

}
