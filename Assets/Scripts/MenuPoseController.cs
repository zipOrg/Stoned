using UnityEngine;
using System.Collections;

public class MenuPoseController : MonoBehaviour {

	public float animationSpeed;
	public float timeToReachAnimationSpeed;

	private void Awake()
	{
		animation["rotate"].speed = 0.0f;
		StartCoroutine(StartAnimation());
	}



	private IEnumerator StartAnimation()
	{
		float currentTime = 0.0f;
		while(currentTime < timeToReachAnimationSpeed)
		{
			animation["rotate"].speed = Mathf.Lerp(0.0f,animationSpeed,Mathf.SmoothStep(0.0f,1.0f,currentTime/timeToReachAnimationSpeed));
			currentTime += Time.deltaTime;
			yield return new WaitForEndOfFrame();
		}
	}
}
