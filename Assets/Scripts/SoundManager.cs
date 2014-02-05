using UnityEngine;
using System.Collections;

public class SoundManager : MonoBehaviour {
	public static SoundManager instance;
	public GameObject stoneImpactSoundPrefab;
	public GameObject playerImpactSoundPrefab;
	public GameObject pickUpStoneSoundPrefab;
	public GameObject throwStoneSoundPrefab;

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


	public static void PlayStoneImpactSound(Vector3 position)
	{
		//Network.Instantiate(instance.stoneImpactSoundPrefab,position,Quaternion.identity,0);
	}

	public static void PlayePlayerImpactSound(Vector3 position)
	{
		GameObject.Instantiate(instance.playerImpactSoundPrefab,position,Quaternion.identity);
	}

	public static void PlayPickUpStoneSound(Vector3 position)
	{
		Network.Instantiate(instance.pickUpStoneSoundPrefab,position,Quaternion.identity,0);
	}

	public static void PlayThrowStoneSound(Vector3 position)
	{
		Network.Instantiate(instance.throwStoneSoundPrefab,position,Quaternion.identity,0);
	}
}
