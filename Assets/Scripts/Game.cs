using UnityEngine;

public class Game : MonoBehaviour
{
	// CONFIGURATION

	[SerializeField]
	private GameObject m_PlayerPrefab;
	[SerializeField]
	private GameObject m_CubePrefab;

	// PRIVATE PROPERTIES

	private Vector2      m_PlayerChunkPosition;
	private Transform    m_Player;
	private ChunkManager m_ChunkManager;

	//MONO OVERRIDES

	void Start()
	{
		m_ChunkManager        = new ChunkManager(m_CubePrefab);
		m_ChunkManager.GenerateInitialWorld();

		m_Player              = GameObject.Instantiate(m_PlayerPrefab).transform;
		m_Player.position     = new Vector3(0, Chunk.SIZE, 0); ;
		m_PlayerChunkPosition = new Vector2(0, 0);
	}

	private void Update()
	{
		var playerPos    = m_Player.position;
		if (playerPos.x < 0)
			playerPos.x -= Chunk.SIZE;
		if (playerPos.z < 0)
			playerPos.z -= Chunk.SIZE;

		var playerChunkX = (int)playerPos.x / Chunk.SIZE;
		var playerChunkY = (int)playerPos.z / Chunk.SIZE;
		var newChunkPos  = new Vector2(playerChunkX, playerChunkY);

		if (newChunkPos != m_PlayerChunkPosition)
		{
			m_ChunkManager.WorldResize(m_PlayerChunkPosition, newChunkPos);
			m_PlayerChunkPosition = newChunkPos;
		}
	}
}