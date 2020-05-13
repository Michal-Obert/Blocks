using UnityEngine;
using UnityEditor.Callbacks;

public class ScenePostprocess
{
#if UNITY_STANDALONE
	[PostProcessSceneAttribute()]
	public static void OnPostprocessScene()
	{
		var gameObjects = GameObject.FindGameObjectsWithTag("AndroidOnly");
		for(int i = gameObjects.Length - 1; i >= 0; --i)
		{
			GameObject.DestroyImmediate(gameObjects[i]);
		}
	}
#endif
}