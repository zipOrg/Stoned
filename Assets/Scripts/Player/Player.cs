using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Player : MonoBehaviour {
	public NetworkView networkView;
	public float speed;
	public float throwForceMultiplier;
	public SkinnedMeshRenderer bodyRenderer;
	public SkinnedMeshRenderer legsRenderer;
	public Transform bodyTransform;
	public Transform legsTransform;
	public float bodyMaxRotationDelta;
	public float legsMaxRotationDelta;
    public Transform stoneGrabTransform;
	public GameObject deathParticlePrefab;
	public AudioSource playerFootstepsAudio;

	private Rigidbody playerRigidbody;
	private Vector3 currentVelocityChange;
	private Vector3 targetVelocity;
    private Animator legAnimationController;
    private Animator bodyAnimationController;
	private float directionMultiplier;

	private Vector3 mouseWorldPosition;
	private Camera mainCamera;
	private Quaternion bodyTargetRotation;
	private Quaternion legTargetRotation;

	private List<Stone> stonesInPickUpRange;
	private Stone pickedUpStone;

	private Vector3 spawnPosition;
	private Vector3 spawnEulerAngles;

	private bool godMode;
	private int score;
	private bool winner;
	private bool inputEnabled;
	private Stone firstStone;
	//sync
	private float serializationTime;
	private float lastSerializationTime;
	private float serializationDelay;
	private float interpolationRatio;
	private bool moveInstantly;
	private Vector3 serializationStartPosition;
	private Vector3 serializationEndPosition;
	private Quaternion serializationStartBodyRotation;
	private Quaternion serializationEndBodyRotation;
	private Quaternion serializationStartLegsRotation;
	private Quaternion serializationEndLegsRotation;
	private bool firstSerialization;



	private void Awake()
	{
		//assigning private variables only !!!
		playerRigidbody = gameObject.rigidbody;
		currentVelocityChange = Vector3.zero;
		mouseWorldPosition = Vector3.zero;
		mainCamera = Camera.main;
		bodyTargetRotation = Quaternion.identity;
		legTargetRotation = Quaternion.identity;
		stonesInPickUpRange = new List<Stone>();
		pickedUpStone = null;
        bodyAnimationController = bodyTransform.GetComponent<Animator>();
        legAnimationController = legsTransform.GetComponent<Animator>();
		directionMultiplier = 1.0f;
		//sync
		serializationStartPosition = transform.position;
		serializationEndPosition = transform.position;
		serializationStartBodyRotation = bodyTransform.rotation;
		serializationEndBodyRotation = bodyTransform.rotation;
		serializationStartLegsRotation = legsTransform.rotation;
		serializationEndLegsRotation = legsTransform.rotation;
		playerRigidbody.isKinematic = true;
		firstSerialization = true;
		godMode = true;
		inputEnabled = false;
		SetVisible(false);
	}

	private void Start()
	{
		Init ();
	}

	public void Init()
	{
		StopAllCoroutines();
		score = 0;
		if(networkView.isMine)
		{
			playerRigidbody.isKinematic = false;
		}
		GameManager.SetInfoTextMeshText("AWAITING CONNECTION!");
		StartCoroutine(WaitForConnection());
	}

	private IEnumerator WaitForConnection()
	{
		while(Network.connections.Length == 0)
		{
			Debug.LogError(Network.connections.Length + "length"); 
			yield return new WaitForSeconds(0.2f);
		}
		StartCoroutine(WaitForForPlayerReady());
	} 

	private IEnumerator WaitForForPlayerReady()
	{
		GameManager.SetInfoTextMeshText("PRESS SPACE WHEN\nYOU ARE READY!");
		while(!inputEnabled)
		{
			if(Input.GetKeyDown(KeyCode.Space))
			{
				GameManager.SetInfoTextMeshText("YOU ARE READY!\nWAITING FOR OPPONENT");
				if(Network.isServer)
				{
					GameManager.instance.networkView.RPC("SetServerPlayerReady",RPCMode.AllBuffered,true);
				}
				else
				{
					GameManager.instance.networkView.RPC("SetClientPlayerReady",RPCMode.AllBuffered,true);
				}
			}

			if(GameManager.AreBothPlayersReady())
			{
				networkView.RPC ("SetVisible",RPCMode.AllBuffered,true);
				inputEnabled = true;
				moveInstantly = false;
				if(Network.isClient && networkView.isMine && !firstStone)
				{
					firstStone = GameManager.instance.CreateFirstStone();
					Debug.LogError("Create first stone");
				}
			}
			yield return null;
		}
		GameManager.SetInfoTextMeshText("");

	}
	
	private void FixedUpdate()
	{
		if(networkView.isMine)
		{
			if(inputEnabled)
			{
				CalculateVelocity();
				ApplyVelocity();
			}
		}
	}

	private void Update()
	{
        if(networkView.isMine)
		{
			if(inputEnabled)
			{
				CalculateMouseWorldPosition();
				RotateBodyTowardsCursor();
				RotateLegsTowardsMovementDirection();
				ControlWalkAnimation();
				if(Input.GetMouseButtonDown(0))
				{
					if(pickedUpStone)
					{
						networkView.RPC ("SetAnimationControllerTrigger",RPCMode.AllBuffered,"throw");
					}
					else
					{
						SelectStoneToPickUp();
					}
				}
			}
		}
		else
		{
			if(moveInstantly)
			{
				MoveInstantly();
			}
			else
			{
				InterpolateMovement();
			}
		}
	}

	private void InterpolateMovement()
	{
		serializationTime += Time.deltaTime;
		interpolationRatio = serializationTime/serializationDelay;
		playerRigidbody.transform.position = Vector3.Lerp(serializationStartPosition,serializationEndPosition,interpolationRatio);
		legsTransform.rotation = Quaternion.Lerp(serializationStartLegsRotation,serializationEndLegsRotation,interpolationRatio);
		bodyTransform.rotation = Quaternion.Lerp(serializationStartBodyRotation,serializationEndBodyRotation,interpolationRatio);
	}

	private void MoveInstantly()
	{
		playerRigidbody.transform.position = serializationEndPosition;
		legsTransform.rotation = serializationEndLegsRotation;
		bodyTransform.rotation = serializationEndBodyRotation;
	}

	private void ApplyVelocity()
	{
		playerRigidbody.AddForce(currentVelocityChange,ForceMode.VelocityChange);
	}

	private void CalculateVelocity()
	{
		targetVelocity = new Vector3(Input.GetAxis("Horizontal"),0.0f,Input.GetAxis("Vertical")) * directionMultiplier;
		targetVelocity = transform.TransformDirection(targetVelocity);
		targetVelocity *= speed;
		currentVelocityChange = targetVelocity - playerRigidbody.velocity;
		currentVelocityChange.y = 0.0f;
	}

	private void CalculateMouseWorldPosition()
	{
		mouseWorldPosition = mainCamera.ScreenToWorldPoint(Input.mousePosition + new Vector3(0.0f,0.0f,mainCamera.transform.position.y));
	}

	private void RotateBodyTowardsCursor()
	{
		Vector3 direction = mouseWorldPosition - transform.position;
		direction.y = 0.0f;
		bodyTargetRotation = Quaternion.LookRotation(direction);
		bodyTransform.rotation = Quaternion.RotateTowards(bodyTransform.rotation,bodyTargetRotation,bodyMaxRotationDelta);
	}

	private void RotateLegsTowardsMovementDirection()
	{
		Vector3 direction = transform.position + targetVelocity;
		direction.y = 0.0f;
		legTargetRotation = Quaternion.LookRotation(direction);
		legsTransform.rotation = Quaternion.RotateTowards(legsTransform.rotation,legTargetRotation,legsMaxRotationDelta);
	}

    private void ControlWalkAnimation()
    {
        float speedRatio = GetSpeedRatio();
        legAnimationController.SetFloat("speedRatio", speedRatio);
        bodyAnimationController.SetFloat("speedRatio", speedRatio);
		playerFootstepsAudio.pitch = 1.5f * speedRatio;
    }

	private void InterruptStoneAction()
	{
		DropPickedUpStone();
		bodyAnimationController.SetTrigger("walk");
	}

    private void ThrowAnimationEvent()
    {
        ThrowPickedUpStone();
    }

	private void PickUpStone(Stone stone)
	{
		pickedUpStone = stone;
		SoundManager.PlayPickUpStoneSound(transform.position);
		networkView.RPC ("SetAnimationControllerTrigger",RPCMode.AllBuffered,"grab");
		stone.networkView.RPC ("OnPickUp",RPCMode.AllBuffered,Network.AllocateViewID(),networkView.viewID);
		SetGodMode(false);
	}

	private void SelectStoneToPickUp()
	{
		//jezeli jest jeden w zasiegu to ez szit
		//jezeli wiecej to ten najblizej kursora
		if(stonesInPickUpRange.Count == 1)
		{
			PickUpStone(stonesInPickUpRange[0]);	
		}
		else if(stonesInPickUpRange.Count > 1)
		{
			int indexOfClosestStone = 0;
			float maximumStoneDistanceToMouseWorldPosition = 0.0f;
			for(int i = 0; i < stonesInPickUpRange.Count; i++)
			{
				Stone currentStone = stonesInPickUpRange[i];
				float currentStoneDistanceToMouseWorldPosition = Vector3.Distance(currentStone.transform.position,mouseWorldPosition);
				if(currentStoneDistanceToMouseWorldPosition > maximumStoneDistanceToMouseWorldPosition)
				{
					maximumStoneDistanceToMouseWorldPosition = currentStoneDistanceToMouseWorldPosition;
					indexOfClosestStone = i;
				}
			}
			PickUpStone(stonesInPickUpRange[indexOfClosestStone]);
		}
	}

	private void ThrowPickedUpStone()
	{
		if (pickedUpStone) 
		{
			SoundManager.PlayThrowStoneSound(transform.position);
			GameManager.ShakeCamera(0.2f,0.3f);
			Vector3 throwForce = (mouseWorldPosition - transform.position).normalized;
			throwForce *= throwForceMultiplier * pickedUpStone.rigidbody.mass;
			pickedUpStone.networkView.RPC ("OnThrow",RPCMode.AllBuffered);
			pickedUpStone.GetNetworkView().RPC("ApplyForce",RPCMode.AllBuffered,throwForce);
			pickedUpStone = null;
		}
	}

	private void DropPickedUpStone()
	{
		if (pickedUpStone) 
		{
			pickedUpStone.networkView.RPC ("OnThrow",RPCMode.AllBuffered);
			pickedUpStone = null;
		}
	}

	private void OnSerializeNetworkView(BitStream stream, NetworkMessageInfo info)
	{
		Vector3 syncPosition = transform.position;
		Quaternion syncBodyRotation = bodyTransform.rotation;
		Quaternion syncLegRotation = legsTransform.rotation;
		float syncFootstepsPitchValue = playerFootstepsAudio.pitch;
		if(stream.isWriting)
		{
			syncPosition = playerRigidbody.position;
			stream.Serialize(ref syncPosition);

			syncBodyRotation = bodyTransform.rotation;
			stream.Serialize(ref syncBodyRotation);

			syncLegRotation = legsTransform.rotation;
			stream.Serialize(ref syncLegRotation);

			syncFootstepsPitchValue = playerFootstepsAudio.pitch;
			stream.Serialize(ref syncFootstepsPitchValue);
		}
		else
		{
			serializationTime = 0.0f;
			serializationDelay = Time.time - lastSerializationTime;
			lastSerializationTime = Time.time;

			stream.Serialize(ref syncPosition);
			if(firstSerialization)
			{
				firstSerialization = false;
				playerRigidbody.position = syncPosition;
				serializationStartPosition = playerRigidbody.position;
				serializationEndPosition = playerRigidbody.position;
			}
			else
			{
				serializationStartPosition = playerRigidbody.position;
				serializationEndPosition = syncPosition;
			}

			stream.Serialize(ref syncBodyRotation);
			serializationStartBodyRotation = bodyTransform.rotation;
			serializationEndBodyRotation = syncBodyRotation;

			stream.Serialize(ref syncLegRotation);
			serializationStartLegsRotation = legsTransform.rotation;
			serializationEndLegsRotation = syncLegRotation;

			stream.Serialize(ref syncFootstepsPitchValue);
			playerFootstepsAudio.pitch = syncFootstepsPitchValue;
		}
	}

	public void AddStoneToPickUpList(Stone stone)
	{
		if(!stonesInPickUpRange.Contains(stone))
		{
			stonesInPickUpRange.Add(stone);
		}
	}

	public void RemoveStoneFromPickUpList(Stone stone)
	{
		if(stonesInPickUpRange.Contains(stone))
		{
			stonesInPickUpRange.Remove(stone);
		}
	}

	[RPC]
	public void CleanUpPickableStonesList()
	{
		for(int i = stonesInPickUpRange.Count - 1; i >= 0; i--)
		{
			if(!stonesInPickUpRange[i])
			{
				stonesInPickUpRange.RemoveAt(i);
			}
		}
	}





	[RPC]
	public void Respawn(bool score)
	{
		StartCoroutine(RespawnCoroutine(score));
	}

	private IEnumerator RespawnCoroutine(bool score)
	{
		SetVisible(false);
		SetGodMode(true);
		GameObject.Instantiate(deathParticlePrefab,transform.position+Vector3.up,Quaternion.Euler(90.0f,0.0f,0.0f));
		SoundManager.PlayePlayerImpactSound(transform.position);
		GameManager.ShakeCamera(0.6f,0.6f);
		InterruptStoneAction();
		moveInstantly = true;
		rigidbody.isKinematic = true;
		transform.position = spawnPosition;
		transform.eulerAngles = spawnEulerAngles;
		yield return new WaitForSeconds(0.5f);
		moveInstantly = false;
		if(networkView.isMine)
		{
			playerRigidbody.isKinematic = false;
		}
		SetVisible(true);
		if(score)
		{
			GetOtherPlayer().Score();
		}
	}


	public void Score()
	{
		score++;
		if(networkView.isMine)
		{
			GameManager.SetYourScoreText(score.ToString());
			GameManager.SetEnemyScoreText(score.ToString());
		}

		if(score >= GameManager.GetScoreToWin())
		{
			Win();
		}


	}

	public void Win()
	{
		inputEnabled = false;
		if(networkView.isMine)
		{
			GameManager.SetInfoTextMeshText("YOU WIN!\nPRESS R TO REMATCH");
			GameManager.SetEnemyInfoTextMeshText("YOU LOSE!\nPRESS R TO REMATCH");
		}
		StartCoroutine(WaitForRematchCoroutine());
		GetOtherPlayer().Lose();
	}

	public void Lose()
	{
		inputEnabled = false;
		StartCoroutine(WaitForRematchCoroutine());
	}

	private IEnumerator WaitForRematchCoroutine()
	{

		while(true)
		{
			if(Input.GetKeyDown(KeyCode.R))
			{

				GameManager.ResetGame();

			}
			yield return null;
		}
	}
	
	public Player GetOtherPlayer()
	{
		Player retVal = null;
		GameObject[] playerObjects = GameObject.FindGameObjectsWithTag("Player") as GameObject[];
		foreach(GameObject playgerGameObject in playerObjects)
		{
			Player playerScript = playgerGameObject.GetComponent<Player>();
			if(playerScript != this)
			{
				retVal = playerScript;
			}
		}
		return retVal;
	}

	[RPC]
	public void SetSpawnPositionAndRotation(Vector3 spawnPosition, Vector3 spawnEulerAngles)
	{
		this.spawnPosition = spawnPosition;
		this.spawnEulerAngles = spawnEulerAngles;
	}

	[RPC]
	public void SetAnimationControllerTrigger(string trigger)
	{
		bodyAnimationController.SetTrigger(trigger);
	}
		

	[RPC]
	public void SetColor(float r, float g, float b, float a)
	{
		Color newColor = new Color(r,g,b,a);
		bodyRenderer.material.color = newColor;
		legsRenderer.material.color = newColor;
	}

	[RPC]
	public void SetVisible(bool visible)
	{
		bodyRenderer.enabled = visible;
		legsRenderer.enabled = visible;
	}

	public void SetDIrectionMultiplier(float directionMultiplier)
	{
		this.directionMultiplier = directionMultiplier;
	}

	public void SetGodMode(bool godMode)
	{
		this.godMode = godMode;
	}

	public bool GetGodMode()
	{
		return godMode;
	}


	private float GetSpeedRatio()
    {
        return Mathf.Clamp01(playerRigidbody.velocity.magnitude / speed);
    }

	private void DrawDebug()
	{
		foreach(Stone s in stonesInPickUpRange)
		{
			Debug.DrawLine(transform.position,s.transform.position,Color.green);
		}
	}
}
