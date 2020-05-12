using UnityEngine;

public class GuiButton : MonoBehaviour
{
	// PUBLIC DELEGATES

	public System.Action OnHold;

	// PRIVATE PROPERTIES

	private bool m_IsDown;

	// PUBLIC METHODS

	public void OnUpdate()
	{
		if(m_IsDown && OnHold != null)
			OnHold();
	}

	// LISTENERS

	void OnPointerDown()
	{
		if (OnHold != null)
			OnHold();

		m_IsDown = true;
	}

	void OnPointerUp()
	{
		m_IsDown = false;
	}
}