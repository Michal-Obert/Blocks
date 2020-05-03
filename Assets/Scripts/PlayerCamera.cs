using UnityEngine;

public class PlayerCamera : MonoBehaviour
{
	// CONFIGURATION

	[SerializeField]
	private Transform m_PlayerTransform;

	// PRIVATE PROPERTIES

	private Transform m_Transform;
	private float     m_MouseDirX;
	private float     m_MouseDirY;

	// MONO OVERRIDES

	private void Awake()
	{
		m_Transform      = transform;
		Cursor.lockState = CursorLockMode.Locked;
	}

	void Update()
	{
		m_MouseDirX                    += Input.GetAxis("Mouse X");
		m_MouseDirY                    += Input.GetAxis("Mouse Y");
		m_MouseDirY                     = Mathf.Clamp(m_MouseDirY, -80f, 75f);
		m_Transform.localRotation       = Quaternion.AngleAxis(-m_MouseDirY, Vector3.right);
		m_PlayerTransform.localRotation = Quaternion.AngleAxis(m_MouseDirX, m_PlayerTransform.up);
	}
}