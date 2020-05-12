using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
	//CONFIGURATION

	[SerializeField]
	private CharacterController m_CharacterController;
	[SerializeField]
	private float               m_Speed;
	[SerializeField]
	private float               m_JumpHeight;

	//PRIVATE PROPERTIES

	private Transform m_Transform;
	private InputData m_InputData;
	private float     m_Gravity             = -4f;
	private Vector3   m_RemainingVelocity;

	// PUBLIC METHODS

	public void Init(InputData inputData)
	{
		m_Transform = transform;
		m_InputData = inputData;
	}

	public void OnUpdate(float deltaTime)
	{
		var horizontalMovement = m_InputData.MoveDir.x;
		var verticalMovement   = m_InputData.MoveDir.y;
		var movement           = Vector3.zero;

		if (m_CharacterController.isGrounded)
		{
			movement  = m_Transform.forward * verticalMovement + m_Transform.right * horizontalMovement;
			movement.Normalize();
			movement *= m_Speed;
			
			if (m_InputData.Jump)
			{
				m_RemainingVelocity   = movement;
				m_RemainingVelocity.y = Mathf.Sqrt(m_JumpHeight * -2f * m_Gravity);
			}
			else
			{
				m_RemainingVelocity = Vector3.zero;
			}
		}
		else
		{
			movement.y += m_Gravity;
		}
		m_RemainingVelocity.y += m_Gravity * deltaTime;

		m_CharacterController.Move((movement + m_RemainingVelocity) * deltaTime);
	}
}