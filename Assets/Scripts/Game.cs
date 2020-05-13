using System.Runtime.Serialization.Formatters.Binary;
using System.Collections.Generic;
using System.IO;
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
#if UNITY_EDITOR || UNITY_ANDROID
	[SerializeField]
	private AndroidButtons m_AndroidButtons;
#endif
	// PUBLIC PROPERTIES

	public  Vector2        PlayerChunkPosition { get; private set; }

	// PRIVATE PROPERTIES

	private ChunkManager   m_ChunkManager;
	private Player         m_Player;
	private InputData      m_InputData = new InputData();
	private int            m_Seed;

	//MONO OVERRIDES

	void Start()
	{
		Cursor.lockState = CursorLockMode.Locked;
		Cursor.visible   = false;
		InitGame(Random.Range(0, 100000), new Vector3(0, Chunk.SIZE, 0));
	}

	void Update()
	{
		UpdateButtons();
		m_Player.OnUpdate(Time.deltaTime);

		var playerPos    = m_Player.Position;
		var newChunkPos  = GetPlayerChunkPosition(playerPos);

		if (newChunkPos != PlayerChunkPosition)
		{
			m_ChunkManager.WorldResize(PlayerChunkPosition, newChunkPos);
			PlayerChunkPosition = newChunkPos;
		}

		if (m_InputData.Save)
			Save();
		if (m_InputData.Load)
			Load();
	}

	//PRIVATE METHODS

	private void InitGame(int seed, Vector3 playerPosition, Dictionary<(int, int), List<CubeFootprint>> footprints = null)
	{
		Random.InitState(seed);
		m_Seed              = seed;
		PlayerChunkPosition = GetPlayerChunkPosition(playerPosition);

		m_ChunkManager = new ChunkManager(m_CubePrefab, m_CubeTypesData, footprints);
		m_ChunkManager.GenerateWorld(PlayerChunkPosition);

		m_Player = Instantiate<Player>(m_PlayerPrefab);
#if UNITY_ANDROID
		m_Player.InitAndroid(m_InputData, playerPosition, m_AndroidButtons);
#else
		m_Player.InitStandalone(m_InputData, playerPosition);
#endif

		m_Player.OnCubeDestroyed += m_ChunkManager.OnPlayerDestroyedCube;
		m_Player.CanPlaceCube    += m_ChunkManager.CanPlaceCube;
		m_Player.PlaceCube       += m_ChunkManager.PlaceCubeByPlayer;
	}

	private void UpdateButtons()
	{
#if UNITY_ANDROID
		m_AndroidButtons.LeftArrow.OnUpdate();
		m_AndroidButtons.RightArrow.OnUpdate();
		m_AndroidButtons.UpArrow.OnUpdate();
		m_AndroidButtons.DownArrow.OnUpdate();
		m_AndroidButtons.Break.OnUpdate();
#endif
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
		var formatter  = new BinaryFormatter();
		var path       = Application.persistentDataPath + "/SavedGame";
		var fileStream = new FileStream(path, FileMode.Create);

		formatter.Serialize(fileStream, m_ChunkManager.Footprints);
		formatter.Serialize(fileStream, m_Player.Position.x);
		formatter.Serialize(fileStream, m_Player.Position.y);
		formatter.Serialize(fileStream, m_Player.Position.z);
		formatter.Serialize(fileStream, m_Seed);

		fileStream.Close();
	}

	private void Load()
	{
		var path = Application.persistentDataPath + "/SavedGame";
		if(File.Exists(path) == false)
		{
			Debug.LogWarning("Nothing saved. Nothing to Load");
			return;
		}

		var formatter  = new BinaryFormatter();
		var fileStream = new FileStream(path, FileMode.Open);

		var footprints = formatter.Deserialize(fileStream) as Dictionary<(int, int), List<CubeFootprint>>;
		var playerPosX = (float)formatter.Deserialize(fileStream);
		var playerPosY = (float)formatter.Deserialize(fileStream);
		var playerPosZ = (float)formatter.Deserialize(fileStream);
		var seed       = (int)  formatter.Deserialize(fileStream);

		fileStream.Close();

		var playerPos  = new Vector3(playerPosX, playerPosY, playerPosZ);

		m_ChunkManager.Dispose();
		m_ChunkManager = null;
		Object.Destroy(m_Player.gameObject);

		InitGame(seed, playerPos, footprints);
	}
}