using UnityEngine;

public class Player : MonoBehaviour
{
	// CONSTANTS

	private const int LEFT_BUTTON  = 0;
	private const int RIGHT_BUTTON = 1;
	private const int DAMAGE       = 42;

	// CONFIGURATION

	[SerializeField]
	private Transform m_PlayerCamera;

	// PUBLIC PROPERTIES

	public Vector3 Position { get { return m_Transform.position; } }
	public System.Action<Vector3, Vector3> OnCubeHit;

	// PRIVATE PROPERTIES

	private Transform  m_Transform;
	private GameObject m_GameObject;
	private LayerMask  m_LayerMask;


	void Awake()
	{
		m_Transform  = transform;
		m_GameObject = gameObject;
		m_LayerMask  = 1 << LayerMask.NameToLayer("Default");
	}

	void Update()
	{
		if (Input.GetMouseButtonDown(LEFT_BUTTON))
		{
			if (Physics.Raycast(m_PlayerCamera.transform.position, m_PlayerCamera.forward, out var hit, 2f, m_LayerMask))
			{
				var cube = hit.collider.gameObject.GetComponent<Cube>();
				if (cube.TakeDamage(DAMAGE))
				{
					var hitTransform = hit.collider.transform;
					OnCubeHit(hitTransform.parent.position, hitTransform.localPosition);
				}
			}
		}
		if (Input.GetMouseButtonDown(RIGHT_BUTTON))
		{
		}
	}

	public void Spawn(Vector3 position)
	{
		m_Transform.position = position;
		m_GameObject.SetActive(true);
	}
}