using UnityEngine;
using System.Collections;

public class MenuPoseController : MonoBehaviour {

	public float rotationSpeed;

	private void Start()
	{
		StartCoroutine(MovePoseCoroutine());
	}



	private IEnumerator MovePoseCoroutine()
	{

		yield return new WaitForSeconds(1.0f);
		float currentRotationSpeed = 0.0f;
		while(true)
		{
			if(currentRotationSpeed < rotationSpeed)
			{
				currentRotationSpeed += Time.smoothDeltaTime/2.0f;
			}
			transform.eulerAngles += Vector3.up * Time.smoothDeltaTime * currentRotationSpeed;
			yield return new WaitForEndOfFrame();
		}
	}
}
