using UnityEngine;

public class Cube : MonoBehaviour
{
	public enum E_Status : byte
	{
		Active,
		Disabled,
		Hidden
	}

	public enum E_Type : byte
	{
		None,
		Bedrock,
		Stone,
		Dirt,
		Sand
	}

	// PUBLIC PROPERTIES

	public E_Status Status { get; private set; }

	// PRIVATE PROPERTIES

	private GameObject m_GameObject;
	private Renderer   m_Renderer;
	private byte       m_MaxHitPoints;
	private byte       m_CurrentHitPoints;

	// MONO OVERRIDES

	void Awake()
	{
		m_GameObject = gameObject;
		m_Renderer   = GetComponent<Renderer>();
	}

	// PUBLIC METHODS

	public void Init(byte hitPoints, Material material, bool activate)
	{
		m_CurrentHitPoints  = hitPoints;
		m_MaxHitPoints      = hitPoints;
		m_Renderer.material = material;
		Status              = activate ? E_Status.Active : E_Status.Disabled;
		m_GameObject.SetActiveSafe(activate);
	}

	public void SetStatus(E_Status status)
	{
		switch (status)
		{
			case E_Status.Active:
				m_GameObject.SetActiveSafe(true);
				if(Status != E_Status.Hidden)
					m_CurrentHitPoints = m_MaxHitPoints;

				break;
			case E_Status.Disabled:
			case E_Status.Hidden:
				m_GameObject.SetActiveSafe(false);
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