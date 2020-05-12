using UnityEngine;

public class PlayerCamera : MonoBehaviour
{
	// CONFIGURATION

	[SerializeField]
	private Transform m_PlayerTransform;

	// PRIVATE PROPERTIES

	private Transform m_Transform;
	private InputData m_InputData;
	private float     m_MouseDirX;
	private float     m_MouseDirY;

	// PUBLIC METHODS

	public void Init(InputData inputData)
	{
		m_Transform = transform;
		m_InputData = inputData;
	}

	public void OnUpdate(float deltaTime)
	{
		m_MouseDirX                    += m_InputData.LookDir.x;
		m_MouseDirY                    += m_InputData.LookDir.y;
		m_MouseDirY                     = Mathf.Clamp(m_MouseDirY, -80f, 75f);
		m_Transform.localRotation       = Quaternion.AngleAxis(-m_MouseDirY, Vector3.right);
		m_PlayerTransform.localRotation = Quaternion.AngleAxis(m_MouseDirX, m_PlayerTransform.up);
	}
}