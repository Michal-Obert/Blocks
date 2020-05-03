using UnityEngine;

public class Game : MonoBehaviour
{
	// CONFIGURATION

	[SerializeField]
	private GameObject m_PlayerPrefab;
	[SerializeField]
	private GameObject m_CubePrefab;

	// PUBLIC PROPERTIES

	public  Vector2      PlayerChunkPosition { get; private set; }
	public  Vector2      PlayerLookChunk     { get { return -Vector2.one; } }

	// PRIVATE PROPERTIES

	private Transform    m_Player;
	private ChunkManager m_ChunkManager;
	private int          m_PerlinOffset; //TODO: Persist between sessions

	//MONO OVERRIDES

	void Start()
	{
		m_PerlinOffset      = Random.Range(0, 10000);
		m_ChunkManager      = new ChunkManager(m_CubePrefab, m_PerlinOffset);
		m_ChunkManager.GenerateInitialWorld();

		m_Player            = GameObject.Instantiate(m_PlayerPrefab).transform;
		m_Player.position   = new Vector3(0, Chunk.SIZE, 0); ;
		PlayerChunkPosition = new Vector2(0, 0);
	}

	void Update()
	{
		var playerPos    = m_Player.position;
		if (playerPos.x < 0)
			playerPos.x -= Chunk.SIZE;
		if (playerPos.z < 0)
			playerPos.z -= Chunk.SIZE;

		var playerChunkX = (int)playerPos.x / Chunk.SIZE;
		var playerChunkY = (int)playerPos.z / Chunk.SIZE;
		var newChunkPos  = new Vector2(playerChunkX, playerChunkY);

		if (newChunkPos != PlayerChunkPosition)
		{
			m_ChunkManager.WorldResize(PlayerChunkPosition, newChunkPos);
			PlayerChunkPosition = newChunkPos;
		}
	}
}