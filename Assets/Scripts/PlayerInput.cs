using UnityEngine;

public class PlayerInput : MonoBehaviour
{
	// PUBLIC PROPERTIES

#if UNITY_EDITOR || UNITY_ANDROID
	public AndroidButtons Buttons;
#endif

	//PRIVATE PROPERTIES

	private InputData m_InputData;
	private Vector2   m_TouchBeginPos;

	// PUBLIC METHODS

	public void Init(InputData inputData)
	{
		m_InputData = inputData;
#if UNITY_ANDROID
		Buttons.LeftArrow .OnHold += () => m_InputData.MoveDir.x   = -1;
		Buttons.RightArrow.OnHold += () => m_InputData.MoveDir.x   =  1; 
		Buttons.UpArrow   .OnHold += () => m_InputData.MoveDir.y   =  1; 
		Buttons.DownArrow .OnHold += () => m_InputData.MoveDir.y   = -1;
		Buttons.Break     .OnHold += () => m_InputData.DestroyCube = true;
		Buttons.Jump      .onClick.AddListener(() => { m_InputData.MoveDir.y      = 1;  m_InputData.Jump = true; } );
		Buttons.BuildCube1.onClick.AddListener(() => m_InputData.BuildCubeNumber  = 1    );
		Buttons.BuildCube2.onClick.AddListener(() => m_InputData.BuildCubeNumber  = 2    );
		Buttons.BuildCube3.onClick.AddListener(() => m_InputData.BuildCubeNumber  = 3    );
		Buttons.BuildCube4.onClick.AddListener(() => m_InputData.BuildCubeNumber  = 4    );
		Buttons.Save      .onClick.AddListener(() => m_InputData.Save             = true );
		Buttons.Load      .onClick.AddListener(() => m_InputData.Load             = true );
		Buttons.Build     .onClick.AddListener(() => m_InputData.ConfirmBuildCube = true );
#endif
	}
	public void OnUpdate(float deltaTime)
	{
#if UNITY_STANDALONE || UNITY_EDITOR
		m_InputData.MoveDir          = new Vector2(Input.GetAxis("Horizontal"), Input.GetAxis("Vertical"));
		m_InputData.LookDir          = new Vector2(Input.GetAxis("Mouse X"), Input.GetAxis("Mouse Y"));
		m_InputData.Jump             = Input.GetKeyDown(KeyCode.Space);
		m_InputData.ConfirmBuildCube = Input.GetKeyDown(KeyCode.Mouse1);
		m_InputData.DestroyCube      = Input.GetKey(KeyCode.Mouse0);
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

#elif UNITY_ANDROID
		var touches = Input.touchCount;
		for(int i = 0, count = touches; i < count; i++)
		{
			var touch = Input.GetTouch(i);
			if (IsOnRightSideOfScreen(touch.position) && touch.type == TouchType.Direct)
			{
				switch (touch.phase)
				{
					case TouchPhase.Began:
						m_TouchBeginPos = touch.position;
						break;
					case TouchPhase.Moved:
						m_InputData.LookDir = touch.deltaPosition.normalized;
						break;
					case TouchPhase.Stationary:
						var dir = touch.position - m_TouchBeginPos;
						if(dir.sqrMagnitude > 1)
							dir.Normalize();
						m_InputData.LookDir = dir;
						break;
				}
			}
		}
#endif
	}

	// MONO OVERRIDES

	void LateUpdate()
	{
		m_InputData.Reset();
	}


#if UNITY_ANDROID
	void OnDestroy()
	{
		Buttons.LeftArrow.OnHold  = null;
		Buttons.RightArrow.OnHold = null;
		Buttons.UpArrow.OnHold    = null;
		Buttons.DownArrow.OnHold  = null;
		Buttons.Break.OnHold      = null;
		Buttons.Jump.onClick      .RemoveAllListeners();
		Buttons.BuildCube1.onClick.RemoveAllListeners();
		Buttons.BuildCube2.onClick.RemoveAllListeners();
		Buttons.BuildCube3.onClick.RemoveAllListeners();
		Buttons.BuildCube4.onClick.RemoveAllListeners();
		Buttons.Save.onClick      .RemoveAllListeners();
		Buttons.Load.onClick      .RemoveAllListeners();
		Buttons.Build.onClick     .RemoveAllListeners();
	}
#endif

	// PRIVATE METHODS

	private static bool IsOnRightSideOfScreen(Vector2 pos)
	{
		return pos.x > 0.5f * Screen.width;
	}
}