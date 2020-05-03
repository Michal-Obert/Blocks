using UnityEngine;

public class DebugData : MonoBehaviour
{
	// CONFIGURATION

	[SerializeField]
	private UnityEngine.UI.Text m_DebugText;

	// PRIVATE PROPERTIES

	private float m_FPSCheckTimer;
	private Game  m_Game;

	// MONO OVERRIDES

	void Start()
	{
		m_Game = GameObject.Find("Game").GetComponent<Game>();
	}

	void Update()
	{
		m_FPSCheckTimer += Time.deltaTime;

		if (m_FPSCheckTimer > 0.5f) //no point allocating every frame
		{
			m_DebugText.text = $"FPS: {(int)(1 / Time.unscaledDeltaTime)}\n" +
				               $"CurrentChunk: {m_Game.PlayerChunkPosition.ToString("0")}\n" +
				               $"LookChunk:    {m_Game.PlayerLookChunk.ToString("0")}";
			m_FPSCheckTimer  = 0;
		}
	}
}