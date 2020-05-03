using UnityEngine;

public class Cube : MonoBehaviour
{
	public enum E_Status : byte
	{
		Active,
		Disabled,
		Hidden
	}

	// PUBLIC PROPERTIES

	public E_Status Status { get; private set; }

	// PRIVATE PROPERTIES

	private GameObject m_GameObject;
	private byte       m_MaxHitPoints;
	private byte       m_CurrentHitPoints;

	// CONSTRUCTOR

	public void Init(byte hitPoints)
	{
		m_GameObject       = gameObject;
		m_CurrentHitPoints = hitPoints;
		m_MaxHitPoints     = hitPoints;
		Status             = E_Status.Active;
	}

	// PUBLIC METHODS

	public void SetStatus(E_Status status)
	{
		switch (status)
		{
			case E_Status.Active:
				m_GameObject.SetActive(true);
				if(Status != E_Status.Hidden)
					m_CurrentHitPoints = m_MaxHitPoints;

				break;
			case E_Status.Disabled:
			case E_Status.Hidden:
				m_GameObject.SetActive(false);
				break;
		}
		Status = status;
	}

	public bool TakeDamage(byte damage)
	{
		if (m_CurrentHitPoints < damage)
			m_CurrentHitPoints = 0;
		else
			m_CurrentHitPoints -= damage;

		if (m_CurrentHitPoints == 0)
		{
			SetStatus(E_Status.Disabled);
			return true;
		}

		return false;
	}
}
