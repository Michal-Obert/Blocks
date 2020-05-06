﻿using UnityEngine;
using CubeType = Cube.E_Type ;

//TODO: Separate Player & input & building
public class Player : MonoBehaviour
{
	// CONSTANTS

	private const int   LEFT_BUTTON  = 0;
	private const int   RIGHT_BUTTON = 1;
	private const int   DAMAGE       = 1;
	private const float RAYCAST_TIME = 0.25f;

	// CONFIGURATION

	[SerializeField]
	private Transform  m_PlayerCamera;
	[SerializeField]
	private Transform  m_Hand;
	[SerializeField]
	private GameObject m_PlacingCube;
	[SerializeField]
	private GameObject m_HighlightingCube;

	// PUBLIC PROPERTIES

	public Vector3 Position { get { return m_Transform.position; } }

	// PUBLIC DELEGATES

	public System.Func<Vector3, Vector3, Vector3, bool> CanPlaceCube;
	public System.Action<CubeType, Vector3, Vector3>    PlaceCube;
	public System.Action<Vector3, Vector3>              OnCubeDestroyed;

	// PRIVATE PROPERTIES

	private Transform  m_Transform;
	private GameObject m_GameObject;
	private LayerMask  m_LayerMask; 
	private float      m_RaycastTargetCheckTimer;
	private bool       m_CubePlacementApproved; 
	private Cube       m_TargetCube;
	private Vector3    m_PlacingCubePosition;
	private Vector3    m_PlacingCubesChunkPosition;
	private CubeType   m_SelectedPlacingType;

	// MONO OVERRIDES

	void Awake()
	{
		m_Transform  = transform;
		m_GameObject = gameObject;
		m_LayerMask  = 1 << LayerMask.NameToLayer("Default");
		m_HighlightingCube.transform.parent = null;
	}

	void OnDestroy()
	{
		OnCubeDestroyed = null;
		PlaceCube       = null;
		CanPlaceCube    = null;
	}

	void Update()
	{
		m_RaycastTargetCheckTimer += Time.deltaTime;
		if (m_SelectedPlacingType == CubeType.None)
		{
			if (m_RaycastTargetCheckTimer >= RAYCAST_TIME)
			{
				if (Physics.Raycast(m_PlayerCamera.transform.position, m_PlayerCamera.forward, out var hit, GetPlayerRange(m_PlayerCamera.forward), m_LayerMask))
				{
					m_TargetCube                          = hit.collider.gameObject.GetComponent<Cube>();
					m_HighlightingCube.transform.position = m_TargetCube.transform.position;
					m_HighlightingCube.transform.rotation = Quaternion.identity;
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

		HandleDestroying();

		if (Input.GetKeyDown(KeyCode.Alpha1))
			SelectType(CubeType.Bedrock);
		
		if (Input.GetKeyDown(KeyCode.Alpha2))
			SelectType(CubeType.Stone);
		
		if (Input.GetKeyDown(KeyCode.Alpha3))
			SelectType(CubeType.Dirt);
	
		if (Input.GetKeyDown(KeyCode.Alpha4))
			SelectType(CubeType.Sand);
	}

	// PUBLIC METHODS

	public void Spawn(Vector3 position)
	{
		m_Transform.position = position;
		m_GameObject.SetActive(true);
	}

	// PRIVATE METHODS

	private void SetPlacingCubeToHand()
	{
		m_PlacingCube.transform.parent        = m_Hand;
		m_PlacingCube.transform.localPosition = Vector3.zero;
		m_PlacingCube.transform.localScale    = new Vector3(0.1f, 0.1f, 0.1f);
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

	private void HandleDestroying()
	{
		if (Input.GetMouseButtonDown(LEFT_BUTTON))
		{
			if (m_SelectedPlacingType != CubeType.None) //this is switch from building
			{
				SelectType(CubeType.None);
				return;
			}

			if (m_TargetCube != null && m_TargetCube.TakeDamage(DAMAGE))
			{
				var hitTransform = m_TargetCube.transform;
				OnCubeDestroyed(hitTransform.parent.position, hitTransform.localPosition);
				m_HighlightingCube.SetActive(false);
			}
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
		else if (Input.GetMouseButtonDown(RIGHT_BUTTON))
		{
			PlaceCube(m_SelectedPlacingType, m_PlacingCubesChunkPosition, m_PlacingCubePosition);
		}
	}

	private void ShowPlacingCube(Vector3 placingPosition)
	{
		m_PlacingCube.transform.rotation   = Quaternion.identity;
		m_PlacingCube.transform.parent     = null;
		m_PlacingCube.transform.localScale = Vector3.one;
		m_PlacingCube.transform.position   = placingPosition;
	}
}