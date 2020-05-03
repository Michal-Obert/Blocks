using UnityEngine;

public class DebugData : MonoBehaviour
{
	// CONFIGURATION

	[SerializeField]
	private UnityEngine.UI.Text m_DebugText;

	// PRIVATE PROPERTIES

	private float m_FPSCheckTimer;
	private uint  m_FPSChecks;
	private float m_FPSSum;
	private Game  m_Game;

	// MONO OVERRIDES

	void Start()
	{
		m_Game = GameObject.Find("Game").GetComponent<Game>();
	}

	void Update()
	{
		var deltaTime    = Time.deltaTime;
		var fps          = (int)(1 / deltaTime);
		m_FPSSum        += fps;
		m_FPSCheckTimer += deltaTime;
		m_FPSChecks++;

		if (m_FPSCheckTimer > 0.5f) //no point allocating every frame
		{
			m_DebugText.text =  $" FPS: {fps}\n" +
								$"~FPS: {m_FPSSum / m_FPSChecks}\n"+
								$"CurrentChunk: {m_Game.PlayerChunkPosition.ToString("0")}\n" +
								$"LookChunk:    {m_Game.PlayerLookChunk.ToString("0")}";
			m_FPSCheckTimer  = 0;
		}
	}
}