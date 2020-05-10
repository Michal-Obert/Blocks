using System.Collections.Generic;
using UnityEngine;

public class ChunkManager : System.IDisposable
{
	// CONSTANTS

#if UNITY_EDITOR                    //performance in editor sucks right now, this makes testing manageable 
	private const int CHUNK_VISIBILITY_RANGE = 1;
#else
	private const int CHUNK_VISIBILITY_RANGE = 3;
#endif

	// PUBLIC PROPERTIES

	public Dictionary<(int, int), List<CubeFootprint>> Footprints { get; private set; } = new Dictionary<(int, int), List<CubeFootprint>>(20);

	// PRIVATE PROPERTIES

	private List<Chunk>    m_InactiveChunks = new List<Chunk>((2 * CHUNK_VISIBILITY_RANGE + 1) * 2 - 1);
	private List<Chunk>    m_ActiveChunks   = new List<Chunk>((2 * CHUNK_VISIBILITY_RANGE + 1) * (2 * CHUNK_VISIBILITY_RANGE + 1));
	private GameObject     m_CubePrefab;
	private int            m_PerlinOffset;
	private CubeTypeData[] m_CubeTypesData;

	// CONSTRUCTOR
	
	public ChunkManager(GameObject cubePrefab, CubeTypeData[] cubeTypesData, Dictionary<(int, int), List<CubeFootprint>> footprints)
	{
		m_CubePrefab       = cubePrefab;
		m_CubeTypesData    = cubeTypesData;
		m_PerlinOffset     = Random.Range(0, 10000);
		
		if (footprints != null)
			Footprints = footprints;
	}

	// PUBLIC METHODS

	public void GenerateWorld(Vector2 playerChunkPosition)
	{
		var playerX = (int)playerChunkPosition.x;
		var playerY = (int)playerChunkPosition.y;
		for (int i = playerX - CHUNK_VISIBILITY_RANGE; i < (playerX + CHUNK_VISIBILITY_RANGE + 1); i++)
		{
			for (int j = playerY - CHUNK_VISIBILITY_RANGE; j < (playerY + CHUNK_VISIBILITY_RANGE + 1); j++)
				TrySpawnChunk(new Vector2(i, j));
		}
	}

	public void WorldResize(Vector2 oldChunkPos, Vector2 newChunkPos)
	{
		for (int i = m_ActiveChunks.Count - 1; i >= 0; --i)
		{
			var chunk = m_ActiveChunks[i];
			var xDif  = chunk.CoordX - newChunkPos.x;
			var yDif  = chunk.CoordY - newChunkPos.y;

			if (xDif * xDif + yDif * yDif > CHUNK_VISIBILITY_RANGE * CHUNK_VISIBILITY_RANGE * 2)
				ReturnChunk(chunk);
		}

		var playerChunkX = newChunkPos.x;
		var playerChunkY = newChunkPos.y;
		var xMoveDelta   = playerChunkX - oldChunkPos.x;
		var yMoveDelta   = playerChunkY - oldChunkPos.y;

		for(int i = -CHUNK_VISIBILITY_RANGE; i < (CHUNK_VISIBILITY_RANGE + 1); i++)
		{
			TrySpawnChunk(new Vector2(playerChunkX + xMoveDelta * CHUNK_VISIBILITY_RANGE, playerChunkY + i));
			TrySpawnChunk(new Vector2(playerChunkX + i, playerChunkY + yMoveDelta * CHUNK_VISIBILITY_RANGE));
		}
	}

	public void TrySpawnChunk(Vector2 coords)
	{
		//check if we don't already have something here
		foreach (var activeChunk in m_ActiveChunks)
		{
			if (activeChunk.CoordX == coords.x && activeChunk.CoordY == coords.y)
				return;
		}

		Chunk chunk;
		if (m_InactiveChunks.Count > 0)
		{
			var lastChunkIndex            = m_InactiveChunks.Count - 1;
			chunk                         = m_InactiveChunks[lastChunkIndex];
			ActivateChunk(chunk, coords, lastChunkIndex);
			return;
		}

		chunk  = new Chunk(m_CubePrefab, coords, m_PerlinOffset, m_CubeTypesData);
		ActivateChunk(chunk, coords);
	}

	private void ActivateChunk(Chunk chunk, Vector2 coords, int inactiveIndex = -1)
	{
		Footprints.TryGetValue(((int)coords.x, (int)coords.y), out var chunkFootprints);
		chunk.Activate(coords, chunkFootprints);
		m_ActiveChunks.Add(chunk);

		if(inactiveIndex >= 0)
			m_InactiveChunks.RemoveAt(inactiveIndex);
	}

	public void ReturnChunk(Chunk chunk)
	{
		chunk.Deactivate();
		m_ActiveChunks.Remove(chunk);
		m_InactiveChunks.Add(chunk);
	}

	public bool CanPlaceCube(Vector3 targetChunkWorldPos, Vector3 targetCubeLocalPos, Vector3 placingCubeLocalPos)
	{
		var chunkCoords = new Vector2(targetChunkWorldPos.x / Chunk.SIZE, targetChunkWorldPos.z / Chunk.SIZE);

		BorderCubeCompensation(ref placingCubeLocalPos, ref chunkCoords);

		for (int i = 0, count = m_ActiveChunks.Count; i < count; i++)
		{
			var chunk = m_ActiveChunks[i];
			if (chunk.CoordX == chunkCoords.x && chunk.CoordY == chunkCoords.y)
			{
				var cube = chunk[placingCubeLocalPos];
				return cube == null || cube.Status != Cube.E_Status.Active;
			}
		}
		return false;
	}

	public void PlaceCube(Cube.E_Type type, Vector3 chunkWorldPos, Vector3 cubeLocalPos)
	{
		var chunkCoords = new Vector2(chunkWorldPos.x / Chunk.SIZE, chunkWorldPos.z / Chunk.SIZE);

		BorderCubeCompensation(ref cubeLocalPos, ref chunkCoords);
		CubeTypeData cubeType = null;

		for (int i = 0, count = m_CubeTypesData.Length; i < count; i++)
		{
			var currentCubeType = m_CubeTypesData[i];
			if (currentCubeType.Type == type)
				cubeType = currentCubeType;
		}

		for (int i = 0, count = m_ActiveChunks.Count; i < count; i++)
		{
			var chunk = m_ActiveChunks[i];
			if (chunk.CoordX == chunkCoords.x && chunk.CoordY == chunkCoords.y)
			{
				var coord = (chunk.CoordX, chunk.CoordY);
				Footprints.TryGetValue(coord, out var footprints);
				chunk.PlaceCube(cubeLocalPos, cubeType, ref footprints);

				if (footprints != null)
					Footprints[coord] = footprints;

				return;
			}
		}
	}

	public void OnPlayerDestroyedCube(Vector3 chunkWorldPos, Vector3 cubeLocalPos)
	{
		var chunkCoordX = chunkWorldPos.x / Chunk.SIZE;
		var chunkCoordY = chunkWorldPos.z / Chunk.SIZE;

		for (int i = 0, count = m_ActiveChunks.Count; i < count; i++)
		{
			var chunk = m_ActiveChunks[i];
			if (chunk.CoordX == chunkCoordX && chunk.CoordY == chunkCoordY)
			{
				chunk.OnCubeDestroyed(cubeLocalPos);
				return;
			}
		}
	}

	public void Dispose()
	{
		for(int i = 0, count = m_InactiveChunks.Count; i < count; i++)
			m_InactiveChunks[i].Dispose();

		m_InactiveChunks.Clear();
		m_InactiveChunks = null;

		for(int i = 0, count = m_ActiveChunks.Count; i < count; i++)
			m_ActiveChunks[i].Dispose();

		m_ActiveChunks.Clear();
		m_ActiveChunks = null;


		foreach (var chunkFootprints in Footprints)
			chunkFootprints.Value?.Clear();

		Footprints.Clear();
		Footprints         = null;
		m_CubePrefab       = null;
		m_CubeTypesData    = null;
	}

	// PRIVATE METHODS

	private void BorderCubeCompensation(ref Vector3 cubeLocalPos, ref Vector2 chunkCoords)
	{
		if (cubeLocalPos.x == Chunk.SIZE)
		{
			chunkCoords.x++;
			cubeLocalPos.x = 0;
		}
		else if (cubeLocalPos.x == -1)
		{
			chunkCoords.x--;
			cubeLocalPos.x = Chunk.SIZE - 1;
		}

		if (cubeLocalPos.z == Chunk.SIZE)
		{
			chunkCoords.y++;
			cubeLocalPos.z = 0;
		}
		else if (cubeLocalPos.z == -1)
		{
			chunkCoords.y--;
			cubeLocalPos.z = Chunk.SIZE - 1;
		}
	}
}