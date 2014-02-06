using UnityEngine;
using System.Collections;

public class Button : MonoBehaviour {
	public string functionToInvoke;
	public float functionDelay;
	public MonoBehaviour scriptWithFunctionToInvoke;

	//scale options
	public float mouseOverScale;
	public float normalScale;
	public float mouseDownScale;
	public float scaleTime;

	private void InvokeFunction()
	{
		if(!scriptWithFunctionToInvoke.IsInvoking())
		{
			scriptWithFunctionToInvoke.Invoke(functionToInvoke,functionDelay);
		}
	}

	private void OnMouseUpAsButton()
	{
		StopAllCoroutines();
		StartCoroutine(ChangeScale(normalScale,scaleTime));
		InvokeFunction();
	}

	private void OnMouseEnter()
	{
		StopAllCoroutines();
		StartCoroutine(ChangeScale(mouseOverScale,scaleTime));
	}

	private void OnMouseExit()
	{
		StopAllCoroutines();
		StartCoroutine(ChangeScale(normalScale,scaleTime));
	}

	private void OnMouseDown()
	{
		StopAllCoroutines();
		StartCoroutine(ChangeScale(mouseDownScale,scaleTime));
	}

	private void OnMouseUp()
	{
		StopAllCoroutines();
		StartCoroutine(ChangeScale(normalScale,scaleTime));
	}

	private IEnumerator ChangeScale(float scale, float time)
	{
		float currentTime = 0.0f;
		Vector3 currentScale = transform.localScale;
		Vector3 targetScale = Vector3.one * scale;
		targetScale.z = 1.0f;

		while(currentTime < time)
		{
			transform.localScale = Vector3.Lerp(currentScale,targetScale,currentTime/time);
			currentTime += Time.deltaTime;
			yield return new WaitForEndOfFrame();
		}
		transform.localScale = Vector3.one * scale;
	}

	

}
