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

	public E_Status Status           { get; private set; }
	public E_Type   Type             { get; private set; }
	public byte     CurrentHitPoints { get; private set; }

	// PRIVATE PROPERTIES

	private GameObject m_GameObject;
	private Renderer   m_Renderer;

	// PUBLIC STATIC METHODS

	public static Cube CreateCube(GameObject prefab, Transform parent, Vector3 localCoords)
	{
		var cubeObject    = Instantiate(prefab, localCoords + parent.position, Quaternion.identity, parent);
		var cube          = cubeObject.GetComponent<Cube>(); //todo: test GetComponent(typeof)
		cube.m_GameObject = cube.gameObject;
		cube.m_Renderer   = cube.GetComponent<Renderer>();
		return cube;
	}

	// PUBLIC METHODS

	public void SpawnCube(E_Type type, byte hitPoints, Material material, bool activate)
	{
		Type                = type;
		CurrentHitPoints    = hitPoints;
		m_Renderer.material = material;
		Status              = activate ? E_Status.Active : E_Status.Disabled;
		m_GameObject.SetActiveSafe(activate);
	}

	public void Hide(bool hide)
	{
		m_GameObject.SetActiveSafe(hide == false);
		Status = hide ? E_Status.Hidden : E_Status.Active;
	}

	public bool TakeDamage(byte damage)
	{
		if (CurrentHitPoints < damage)
			CurrentHitPoints = 0;
		else
			CurrentHitPoints -= damage;

		if (CurrentHitPoints == 0)
		{
			m_GameObject.SetActiveSafe(false);
			Status = E_Status.Disabled;
			return true;
		}

		return false;
	}
}