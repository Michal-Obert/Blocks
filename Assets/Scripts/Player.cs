using UnityEngine;

using CubeType = Cube.E_Type;

public class Player : MonoBehaviour
{
	// CONSTANTS

	private const int   DAMAGE       = 1;
	private const float RAYCAST_TIME = 0.15f;
	private const float DAMAGE_TIME  = 0.4f;

	// CONFIGURATION

	[SerializeField]
	private Transform  m_PlayerCamera;
	[SerializeField]
	private Transform  m_Hand;
	[SerializeField]
	private GameObject m_PlacingCube;
	[SerializeField]
	private GameObject m_HighlightingCube;


	// PLAYER COMPONENTS

	[SerializeField]
	private PlayerCamera   m_CameraComponent;
	[SerializeField]
	private PlayerMovement m_MovementComponent;
	[SerializeField]
	private PlayerInput    m_InputComponent;

	// PUBLIC PROPERTIES

	public Vector3 Position { get { return m_Transform.position; } }

	// PUBLIC DELEGATES

	public System.Func<Vector3, Vector3, Vector3, bool> CanPlaceCube;
	public System.Action<CubeType, Vector3, Vector3>    PlaceCube;
	public System.Action<Vector3, Vector3>              OnCubeDestroyed;

	// PRIVATE PROPERTIES

	private Transform  m_Transform;
	private Transform  m_HighlightingCubeTransform;
	private Transform  m_PlacingCubeTransform;
	private GameObject m_GameObject;
	private InputData  m_InputData;
	private LayerMask  m_LayerMask; 
	private float      m_RaycastTargetCheckTimer;
	private float      m_DestroyCubeCheckTimer;
	private bool       m_CubePlacementApproved; 
	private Cube       m_TargetCube;
	private Vector3    m_PlacingCubePosition;
	private Vector3    m_PlacingCubesChunkPosition;
	private CubeType   m_SelectedPlacingType;

	// MONO OVERRIDES

	void OnDestroy()
	{
		Destroy(m_HighlightingCube);
		Destroy(m_PlacingCube);

		m_HighlightingCube          = null;
		m_HighlightingCubeTransform = null;
		m_PlacingCube               = null;
		m_PlacingCubeTransform      = null;
		CanPlaceCube                = null;
		PlaceCube                   = null;
		OnCubeDestroyed             = null;
	}

	// PUBLIC METHODS

#if UNITY_ANDROID
	public void InitAndroid(InputData inputData, Vector3 playerPosition, AndroidButtons buttons)
	{
		m_InputComponent.Buttons = buttons;
		Init(inputData, playerPosition);
	}
#else
	public void InitStandalone(InputData inputData, Vector3 playerPosition)
	{
		Init(inputData, playerPosition);
	}
#endif

	public void OnUpdate(float deltaTime)
	{
		m_InputComponent.OnUpdate(deltaTime);
		m_MovementComponent.OnUpdate(deltaTime);
		m_CameraComponent.OnUpdate(deltaTime);

		m_RaycastTargetCheckTimer += deltaTime;
		if (m_SelectedPlacingType == CubeType.None)
		{
			if (m_RaycastTargetCheckTimer >= RAYCAST_TIME)
			{
				if (Physics.Raycast(m_PlayerCamera.transform.position, m_PlayerCamera.forward, out var hit, GetPlayerRange(m_PlayerCamera.forward), m_LayerMask))
				{
					m_TargetCube                          = hit.collider.gameObject.GetComponent<Cube>();
					m_HighlightingCubeTransform.position = m_TargetCube.transform.position;
					m_HighlightingCubeTransform.rotation = Quaternion.identity;
					m_HighlightingCube.SetActiveSafe(true);
				}
				else
				{
					m_TargetCube = null;
					m_HighlightingCube.SetActiveSafe(false);
				}
				m_RaycastTargetCheckTimer = 0f;
			}
		}
		else
		{
			HandleBuilding();
		}

		HandleDestroying(deltaTime);

		switch (m_InputData.BuildCubeNumber)
		{
			case 1:
				SelectType(CubeType.Sand);
				break;
			case 2:
				SelectType(CubeType.Dirt);
				break;
			case 3:
				SelectType(CubeType.Stone);
				break;
			case 4:
				SelectType(CubeType.Bedrock);
				break;
		}
	}

	// PRIVATE METHODS

	private void Init(InputData inputData, Vector3 position)
	{
		m_InputData                        = inputData;
		m_Transform                        = transform;
		m_Transform.position               = position;
		m_GameObject                       = gameObject;
		m_PlacingCubeTransform             = m_PlacingCube.transform;
		m_HighlightingCubeTransform        = m_HighlightingCube.transform;
		m_HighlightingCubeTransform.parent = null;
		m_LayerMask                        = 1 << LayerMask.NameToLayer("Default");

		m_InputComponent.Init(inputData);
		m_CameraComponent.Init(inputData);
		m_MovementComponent.Init(inputData);

		m_GameObject.SetActive(true);
	}

	private void SetPlacingCubeToHand()
	{
		m_PlacingCubeTransform.parent        = m_Hand;
		m_PlacingCubeTransform.localPosition = Vector3.zero;
		m_PlacingCubeTransform.localScale    = new Vector3(0.1f, 0.1f, 0.1f);
	}

	private Vector3 PlacingPositionOffset(RaycastHit hit)
	{
		Vector3 incomingVec = hit.normal - Vector3.up;

		if (incomingVec == new Vector3(0, 0, 0))
			return Vector3.up;

		if (incomingVec == new Vector3(0, -1, -1))
			return Vector3.back;

		if (incomingVec == new Vector3(0, -1, 1))
			return Vector3.forward;

		if (incomingVec == new Vector3(-1, -1, 0))
			return Vector3.left;

		if (incomingVec == new Vector3(1, -1, 0))
			return Vector3.right;

		if (incomingVec == new Vector3(0, -2, 0))
			return Vector3.down;

		return Vector3.zero;
	}

	private void SelectType(CubeType type)
	{
		m_SelectedPlacingType = m_SelectedPlacingType == type ? CubeType.None : type;
		if(m_SelectedPlacingType != CubeType.None)
		{
			m_PlacingCube.SetActive(true);
			SetPlacingCubeToHand();
		}
		else
		{
			m_PlacingCube.SetActive(false);
		}
	}

	private float GetPlayerRange(Vector3 lookDirection)
	{
		return 2 + (Mathf.Max(Vector3.Angle(lookDirection, Vector3.up) - 90, 0)) / 120f;
	}

	private void HandleDestroying(float deltaTime)
	{
		if (m_InputData.DestroyCube)
		{
			if (m_SelectedPlacingType != CubeType.None) //this is switch from building
			{
				SelectType(CubeType.None);
				return;
			}

			if(m_TargetCube != null)
			{
				m_DestroyCubeCheckTimer     += deltaTime;
				if (m_DestroyCubeCheckTimer >= DAMAGE_TIME) 
				{
					m_DestroyCubeCheckTimer  = 0;
					if(m_TargetCube.TakeDamage(DAMAGE))
					{
						var hitTransform     = m_TargetCube.transform;
						OnCubeDestroyed(hitTransform.parent.position, hitTransform.localPosition);
						m_HighlightingCube.SetActive(false);
					}
				}
			}
			else
			{
				m_DestroyCubeCheckTimer = 0f;
			}
		}
		else
		{
			m_DestroyCubeCheckTimer = 0f;
		}
	}

	private void HandleBuilding()
	{
		if (m_TargetCube != null) //this is switch from destroying
		{
			m_HighlightingCube.SetActive(false);
			m_TargetCube = null;
		}

		var previouslyApproved = m_CubePlacementApproved;
		if (m_RaycastTargetCheckTimer >= RAYCAST_TIME)
		{
			if (Physics.Raycast(m_PlayerCamera.transform.position, m_PlayerCamera.forward, out var hit, GetPlayerRange(m_PlayerCamera.forward), m_LayerMask))
			{
				var cube                        = hit.collider.gameObject;
				var cubeTransform               = cube.transform;
				m_PlacingCubePosition           = cubeTransform.localPosition + PlacingPositionOffset(hit);
				var placingCubeWorldPosition    = m_PlacingCubesChunkPosition + m_PlacingCubePosition;
				if (Vector3.Distance(m_Transform.position, placingCubeWorldPosition) > 0.97f && 
					CanPlaceCube(cubeTransform.parent.position, cubeTransform.localPosition, m_PlacingCubePosition))
				{
					m_CubePlacementApproved     = true;
					m_PlacingCubesChunkPosition = cubeTransform.parent.position;
					ShowPlacingCube(m_PlacingCubesChunkPosition + m_PlacingCubePosition);
				}
				else
				{
					m_CubePlacementApproved = false;
				}
			}
			else
			{
				m_CubePlacementApproved = false;
			}
			m_RaycastTargetCheckTimer = 0f;
		}

		if (m_CubePlacementApproved == false)
		{
			if (previouslyApproved)
				SetPlacingCubeToHand();
		}
		else if (m_InputData.ConfirmBuildCube)
		{
			PlaceCube(m_SelectedPlacingType, m_PlacingCubesChunkPosition, m_PlacingCubePosition);
		}
	}

	private void ShowPlacingCube(Vector3 placingPosition)
	{
		m_PlacingCubeTransform.rotation   = Quaternion.identity;
		m_PlacingCubeTransform.parent     = null;
		m_PlacingCubeTransform.localScale = Vector3.one;
		m_PlacingCubeTransform.position   = placingPosition;
	}
}