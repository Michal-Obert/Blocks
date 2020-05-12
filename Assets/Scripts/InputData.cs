
using UnityEngine;

public class InputData
{
	// PUBLIC PROPERTIES

	public bool    Save;
	public bool    Load;
	public bool    Jump;
	public bool    DestroyCube;
	public bool    ConfirmBuildCube;
	public Vector2 LookDir;
	public Vector2 MoveDir;
	public byte    BuildCubeNumber;

	//PUBLIC METHODS

	public void Reset()
	{
		Save             = false;
		Load             = false;
		Jump             = false;
		DestroyCube      = false;
		ConfirmBuildCube = false;
		LookDir          = Vector2.zero;
		MoveDir          = Vector2.zero;
		BuildCubeNumber  = 0;
	}
}