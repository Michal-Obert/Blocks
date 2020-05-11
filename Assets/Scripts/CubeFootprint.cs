using UnityEngine;

using static Cube;

[System.Serializable]
public class CubeFootprint
{
	// PUBLIC PROPERTIES

	[SerializeField] public float  LocalCoordsX    { get; private set; }
	[SerializeField] public float  LocalCoordsY    { get; private set; }
	[SerializeField] public float  LocalCoordsZ    { get; private set; }
	[SerializeField] public E_Type Type            { get; private set; }
	[SerializeField] public byte   RemainingHealth { get; private set; }

	public bool IsActive { get { return Type != E_Type.None; } }

	// CONSTRUCTOR

	public CubeFootprint(Vector3 localCoords, E_Type type, byte remainingHealth)
	{
		SetData(localCoords, type, remainingHealth);
	}

	public void SetData(Vector3 localCoords, E_Type type, byte remainingHealth)
	{
		LocalCoordsX    = localCoords.x;
		LocalCoordsY    = localCoords.y;
		LocalCoordsZ    = localCoords.z;
		Type            = type;
		RemainingHealth = remainingHealth;
	}
}