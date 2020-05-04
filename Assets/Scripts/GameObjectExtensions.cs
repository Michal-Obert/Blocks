using UnityEngine;

public static class GameObjectExtensions
{
	public static void SetActiveSafe(this GameObject gameObject, bool active)
	{
		if (gameObject.activeSelf != active)
			gameObject.SetActive(active);
	}
}