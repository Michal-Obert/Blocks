using UnityEngine;

public class PlayerInput : MonoBehaviour
{
	//PRIVATE PROPERTIES

	private InputData m_InputData;

	// PUBLIC METHODS

	public void Init(InputData inputData)
	{
		m_InputData = inputData;
	}

	public void OnUpdate(float deltaTime)
	{
		m_InputData.Reset();

#if UNITY_STANDALONE || UNITY_EDITOR
		m_InputData.MoveDir          = new Vector2(Input.GetAxis("Horizontal"), Input.GetAxis("Vertical"));
		m_InputData.LookDir          = new Vector2(Input.GetAxis("Mouse X"), Input.GetAxis("Mouse Y"));
		m_InputData.Jump             = Input.GetKeyDown(KeyCode.Space);
		m_InputData.ConfirmBuildCube = Input.GetKeyDown(KeyCode.Mouse1);
		m_InputData.DestroyCube      = Input.GetKeyDown(KeyCode.Mouse0);
		m_InputData.Save             = Input.GetKeyDown(KeyCode.F5);
		m_InputData.Load             = Input.GetKeyDown(KeyCode.F9);

		if (Input.GetKeyDown(KeyCode.Alpha1))
			m_InputData.BuildCubeNumber = 1;
		else if (Input.GetKeyDown(KeyCode.Alpha2))
			m_InputData.BuildCubeNumber = 2;
		else if (Input.GetKeyDown(KeyCode.Alpha3))
			m_InputData.BuildCubeNumber = 3;
		else if (Input.GetKeyDown(KeyCode.Alpha4))
			m_InputData.BuildCubeNumber = 4;
#endif
	}
}