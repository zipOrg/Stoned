using UnityEngine;
using System.Collections;

public class SplashImage : MonoBehaviour {
	public GUITexture fadeTexture;
	public float fadeInTime;
	public float fadeOutTime;

	private void Start()
	{
		StartCoroutine(FadeInAndOut());
	}

	private IEnumerator FadeInAndOut()
	{
		bool skip = false;

		float currentTime = 0.0f;
		Color currentColor = fadeTexture.color;
		Color targetColor = new Color(0.5f,0.5f,0.5f,1.0f);

		while(!skip && currentTime < fadeInTime)
		{
			if(Input.anyKeyDown)
			{
				skip = true;
			}

			fadeTexture.color = Color.Lerp(currentColor,targetColor,Mathf.SmoothStep(0.0f,1.0f,currentTime/fadeInTime));
			currentTime += Time.deltaTime;
			yield return null;
		}

		currentTime = 0.0f;
		currentColor = fadeTexture.color;
		targetColor = new Color(0.5f,0.5f,0.5f,0.0f);

		while(!skip && currentTime < fadeOutTime)
		{
			if(Input.anyKeyDown)
			{
				skip = true;
			}
			fadeTexture.color = Color.Lerp(currentColor,targetColor,Mathf.SmoothStep(0.0f,1.0f,currentTime/fadeOutTime));
			currentTime += Time.deltaTime;
			yield return null;
		}

		Application.LoadLevel("Main");
	}

}
