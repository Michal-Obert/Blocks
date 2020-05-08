using UnityEngine;

public class Game : MonoBehaviour
{
	// CONFIGURATION

	[SerializeField]
	private Player         m_PlayerPrefab;
	[SerializeField]
	private GameObject     m_CubePrefab;
	[SerializeField]
	private CubeTypeData[] m_CubeTypesData;

	// PUBLIC PROPERTIES

	public  Vector2        PlayerChunkPosition { get; private set; }

	// PRIVATE PROPERTIES

	private ChunkManager   m_ChunkManager;
	private Player         m_Player;
	private int            m_Seed;

	//MONO OVERRIDES

	void Start()
	{
		InitGame(Random.Range(0, 100000), new Vector3(0, Chunk.SIZE, 0));
	}

	void Update()
	{
		var playerPos    = m_Player.Position;
		var newChunkPos  = GetPlayerChunkPosition(playerPos);

		if (newChunkPos != PlayerChunkPosition)
		{
			m_ChunkManager.WorldResize(PlayerChunkPosition, newChunkPos);
			PlayerChunkPosition = newChunkPos;
		}

		if (Input.GetKeyDown(KeyCode.F5))
			Save();
		if (Input.GetKeyDown(KeyCode.F9))
			Load();
	}

	//PRIVATE METHODS

	private void InitGame(int seed, Vector3 playerPosition)
	{
		Random.InitState(seed);
		m_Seed              = seed;
		PlayerChunkPosition = GetPlayerChunkPosition(playerPosition);

		m_ChunkManager = new ChunkManager(m_CubePrefab, m_CubeTypesData);
		m_ChunkManager.GenerateWorld(PlayerChunkPosition);

		m_Player = Instantiate<Player>(m_PlayerPrefab);
		m_Player.Spawn(playerPosition);
		m_Player.OnCubeDestroyed += m_ChunkManager.OnPlayerDestroyedCube;
		m_Player.CanPlaceCube    += m_ChunkManager.CanPlaceCube;
		m_Player.PlaceCube       += m_ChunkManager.PlaceCube;
	}

	private Vector2 GetPlayerChunkPosition(Vector3 playerPos)
	{
		if (playerPos.x < 0)
			playerPos.x -= Chunk.SIZE;
		if (playerPos.z < 0)
			playerPos.z -= Chunk.SIZE;

		var playerChunkX = (int)playerPos.x / Chunk.SIZE;
		var playerChunkY = (int)playerPos.z / Chunk.SIZE;
		return new Vector2(playerChunkX, playerChunkY);
	}

	private void Save()
	{
		PlayerPrefs.SetFloat("PlayerPosX", m_Player.Position.x );
		PlayerPrefs.SetFloat("PlayerPosY", m_Player.Position.y );
		PlayerPrefs.SetFloat("PlayerPosZ", m_Player.Position.z );
		PlayerPrefs.SetInt("Seed", m_Seed);
	}

	private void Load()
	{
		m_ChunkManager.Dispose();
		var playerPos  = new Vector3(PlayerPrefs.GetFloat("PlayerPosX"), PlayerPrefs.GetFloat("PlayerPosY"), PlayerPrefs.GetFloat("PlayerPosZ"));
		var seed       = PlayerPrefs.GetInt("Seed");
		Object.Destroy(m_Player.gameObject);

		InitGame(seed, playerPos);
	}
}