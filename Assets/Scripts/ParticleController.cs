using UnityEngine;
using System.Collections;

public class ParticleController : MonoBehaviour {

	private ParticleSystem particleSystem;

	private void Awake()
	{
		this.particleSystem = gameObject.GetComponent<ParticleSystem>();
		StartCoroutine(WaitForParticleFinish());
	}

	private IEnumerator WaitForParticleFinish()
	{
		while(particleSystem.isPlaying)
		{
			yield return new WaitForSeconds(0.1f);
		}
		Destroy(gameObject);
	}
}
