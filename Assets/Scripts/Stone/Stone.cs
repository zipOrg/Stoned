using UnityEngine;
using System.Collections;

public class Stone : MonoBehaviour {

	public ParticleSystem fatalParticle;

	private Rigidbody stoneRigidbody;
	private NetworkView stoneNetworkView;
	//interpolation common props
	private float serializationTime;
	private float lastSerializationTime;
	private float serializationDelay;
	private float interpolationRatio;

	//position interpolation cache
	private Vector3 serializationStartPosition;
	private Vector3 serializationEndPosition;
	//rotation interpolation cache
	private Quaternion serializationStartRotation;
	private Quaternion serializationEndRotation;

	private Player lastPlayerThatPickedTheStone;

	private bool moveInstantly;

	private bool firstSerialization = true;

	private Vector3 lastPosition;
	private Vector3 newPosition;

	private bool fatal;


	private void Awake()
	{
		stoneRigidbody = rigidbody;
		stoneNetworkView = networkView;
		serializationTime = 0.0f;
		lastSerializationTime = 0.0f;
		serializationDelay = 0.0f;
		serializationStartPosition = transform.position;
		serializationEndPosition = transform.position;
		serializationStartRotation = transform.rotation;
		serializationEndRotation = transform.rotation;
		lastPosition = transform.position;
		newPosition = transform.position;
		firstSerialization = true;
		stoneRigidbody.isKinematic = true;
		fatal = false;
	}


	private void Update()
	{
		if(Network.isServer)
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
		else
		{
			stoneRigidbody.position = new Vector3(stoneRigidbody.position.x,1.4f,stoneRigidbody.position.z);
			fatal = stoneRigidbody.velocity.magnitude > 20.0f; //jesli szybsze niz 10m/s to zabija
		}

		if(fatal)
		{
			if(!fatalParticle.isPlaying)
			{
				fatalParticle.Play();
			}
		}
		else
		{
			if(!fatalParticle.isStopped)
			{
				fatalParticle.Stop();
			}
		}
	}

	private void InterpolateMovement()
	{
		serializationTime += Time.deltaTime;
		interpolationRatio = serializationTime/serializationDelay;
		stoneRigidbody.position = Vector3.Lerp(serializationStartPosition,serializationEndPosition,interpolationRatio);
		stoneRigidbody.rotation = Quaternion.Lerp(serializationStartRotation,serializationEndRotation,interpolationRatio);
		stoneRigidbody.position = new Vector3(stoneRigidbody.position.x,1.4f,stoneRigidbody.position.z);
	}

	private void MoveInstantly()
	{
		stoneRigidbody.position = serializationEndPosition;
		stoneRigidbody.rotation = serializationEndRotation;
		stoneRigidbody.position = new Vector3(stoneRigidbody.position.x,1.4f,stoneRigidbody.position.z);
	}

	public void ParentToPlayerTransform(NetworkViewID newParentID)
	{
		NetworkView newParentPlayerNetworkView = NetworkView.Find(newParentID);
		Player player = newParentPlayerNetworkView.transform.GetComponent<Player>();
		lastPlayerThatPickedTheStone = player;
		transform.parent = player.stoneGrabTransform;
	}

	[RPC]
	public void OnPickUp(NetworkViewID owner, NetworkViewID parentId)
	{
		stoneRigidbody.isKinematic = true;
		gameObject.layer = 11;
		ParentToPlayerTransform(parentId);
		transform.localPosition = new Vector3(-0.35f,-0.35f,-1.0f);
	}

	[RPC]
	public void OnThrow()
	{

		if(Network.isClient)
		{
			gameObject.layer = 8;
			Vector3 desiredPosition = transform.position;
			desiredPosition.y = 1.4f;
			transform.position = desiredPosition;
			transform.parent = null;
			stoneRigidbody.isKinematic = false;	
		}
		else
		{
			Vector3 desiredPosition = transform.position;
			desiredPosition.y = 1.4f;
			transform.position = desiredPosition;
			transform.parent = null;
		}

	}

	[RPC]
	public void ApplyForce(Vector3 force)
	{
		if(!stoneRigidbody.isKinematic)
		{
			stoneRigidbody.AddForce(force,ForceMode.Impulse);
		}
	}

	[RPC]
	public void RemovePlayerReference()
	{
		lastPlayerThatPickedTheStone = null;
	}

	[RPC]
	public void SetColor(float r, float g, float b, float a)
	{
		Color colorToSet = new Color(r,g,b,a);
		renderer.material.color = colorToSet;
		fatalParticle.startColor = colorToSet;

	}



	private void OnSerializeNetworkView(BitStream stream, NetworkMessageInfo info)
	{
		Vector3 syncPosition = transform.position;
		Quaternion syncRotation = transform.rotation;
		bool syncFatal = fatal;
		if(stream.isWriting)
		{
			syncPosition = stoneRigidbody.position;
			stream.Serialize(ref syncPosition);

			syncRotation = stoneRigidbody.rotation;
			stream.Serialize(ref syncRotation);

			syncFatal = fatal;
			stream.Serialize(ref syncFatal);
		}
		else
		{
			serializationTime = 0.0f;
			serializationDelay = Time.time - lastSerializationTime;
			lastSerializationTime = Time.time;

			//position
			stream.Serialize(ref syncPosition);
			if(firstSerialization)
			{
				firstSerialization = false;
				stoneRigidbody.position = syncPosition;
				serializationStartPosition = stoneRigidbody.position;
				serializationEndPosition = stoneRigidbody.position;
			}
			else
			{
				serializationStartPosition = stoneRigidbody.position;
				serializationEndPosition = syncPosition;
			}
			//end position

			//rotation
			stream.Serialize(ref syncRotation);
			serializationStartRotation = stoneRigidbody.rotation;
			serializationEndRotation = syncRotation;
			//end rotation

			stream.Serialize(ref syncFatal);
			fatal = syncFatal;
		}
		
	}

	public void StoneTriggerEnter(Collider other)
	{
		if(Network.isClient)
		{
			if(other.gameObject.tag.Equals("Player"))
			{
				Player collidingPlayer = other.gameObject.GetComponent<Player>();
				if(fatal && !collidingPlayer.GetGodMode())
				{
					Vector3 playerPosition = collidingPlayer.transform.position;
					collidingPlayer.networkView.RPC("Respawn",RPCMode.AllBuffered);
					stoneNetworkView.RPC("RemovePlayerReference",RPCMode.AllBuffered);
					Stone newStone = GameManager.instance.CreateStone(playerPosition,Quaternion.identity);
					Color colorToSet = collidingPlayer.bodyRenderer.material.color;
					newStone.stoneNetworkView.RPC ("SetColor",RPCMode.AllBuffered,colorToSet.r,colorToSet.g,colorToSet.b,colorToSet.a);
					SoundManager.PlayStoneImpactSound(transform.position);
				}
				else
				{
					stoneRigidbody.isKinematic = true;
				}
			}
			else if(other.gameObject.tag.Equals("Stone"))
			{
				Stone collidingStone = other.gameObject.GetComponent<Stone>();
				collidingStone.stoneRigidbody.isKinematic = false;
				SoundManager.PlayStoneImpactSound(transform.position);
			}
			else if(other.gameObject.tag.Equals("Arena"))
			{
				SoundManager.PlayStoneImpactSound(transform.position);
			}
		}
		else
		{
			//nofyn
		}
	}

	public void StoneTriggerExit(Collider other)
	{
		if(Network.isClient)
		{
			if(other.gameObject.tag.Equals("Player"))
			{
				if(!transform.parent)
				{
					stoneRigidbody.isKinematic = false;
				}
			}
		}
		else
		{
			if(other.tag.Equals("Player"))
			{
				if(!transform.parent)
				{
					gameObject.layer = 8;
				}
			}
		}


	}

	public NetworkView GetNetworkView()
	{
		return stoneNetworkView;
	}

	public bool IsFatal()
	{
		return fatal;
	}









}
