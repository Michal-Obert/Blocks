using UnityEngine;

public class Cube
{
	// PUBLIC PROPERTIES

	public bool IsActive { get; private set; }
	
	// PRIVATE PROPERTIES

	private GameObject m_GameObject;

	// CONSTRUCTOR

	public Cube(GameObject gameObject)
	{
		m_GameObject = gameObject;
		IsActive     = true;
	}

	// PUBLIC METHODS

	public void SetActive(bool active)
	{
		m_GameObject.SetActive(active);
		IsActive = active;
	}
}
