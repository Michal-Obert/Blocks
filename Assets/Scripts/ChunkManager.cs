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
	// PRIVATE PROPERTIES

	private List<Chunk>    m_InactiveChunks = new List<Chunk>((2 * CHUNK_VISIBILITY_RANGE + 1) * 2 - 1);
	private List<Chunk>    m_ActiveChunks   = new List<Chunk>((2 * CHUNK_VISIBILITY_RANGE + 1) * (2 * CHUNK_VISIBILITY_RANGE + 1));
	private GameObject     m_CubePrefab;
	private int            m_PerlinOffset;
	private CubeTypeData[] m_CubeTypesData;

	// CONSTRUCTOR
	
	public ChunkManager(GameObject cubePrefab, CubeTypeData[] cubeTypesData)
	{
		m_CubePrefab    = cubePrefab;
		m_CubeTypesData = cubeTypesData;
		m_PerlinOffset  = Random.Range(0, 10000);
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
			chunk.Activate(coords);

			m_InactiveChunks.RemoveAt(lastChunkIndex);
			m_ActiveChunks.Add(chunk);
			return;
		}

		chunk  = new Chunk(m_CubePrefab, coords, m_PerlinOffset, m_CubeTypesData);

		m_ActiveChunks.Add(chunk);
	}

	public void ReturnChunk(Chunk chunk)
	{
		chunk.Deactivate();
		m_ActiveChunks.Remove(chunk);
		m_InactiveChunks.Add(chunk);
	}

	public bool CanPlaceCube(Vector3 targetChunkWorldPos, Vector3 targetCubeLocalPos, Vector3 placingCubeLocalPos)
	{
		if(targetCubeLocalPos.x == Chunk.SIZE - 1 || targetCubeLocalPos.y == Chunk.SIZE - 1 || targetCubeLocalPos.z == Chunk.SIZE - 1 ||
			targetCubeLocalPos.x == 0             || targetCubeLocalPos.z == 0)
		{
			//TODO: Handle border cubes
			return false;
		}
		var chunkCoordX = targetChunkWorldPos.x / Chunk.SIZE;
		var chunkCoordY = targetChunkWorldPos.z / Chunk.SIZE;

		for (int i = 0, count = m_ActiveChunks.Count; i < count; i++)
		{
			var chunk = m_ActiveChunks[i];
			if (chunk.CoordX == chunkCoordX && chunk.CoordY == chunkCoordY)
			{
				return chunk[placingCubeLocalPos].Status != Cube.E_Status.Active;
			}
		}
		return false;
	}

	public void PlaceCube(Cube.E_Type type, Vector3 chunkWorldPos, Vector3 cubeLocalPos)
	{
		var chunkCoordX = chunkWorldPos.x / Chunk.SIZE;
		var chunkCoordY = chunkWorldPos.z / Chunk.SIZE;

		for (int i = 0, count = m_ActiveChunks.Count; i < count; i++)
		{
			var chunk = m_ActiveChunks[i];
			if (chunk.CoordX == chunkCoordX && chunk.CoordY == chunkCoordY)
			{
				chunk.PlaceCube(cubeLocalPos, type);
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
		{
			m_InactiveChunks[i].Dispose();
		}
		m_InactiveChunks.Clear();
		m_InactiveChunks = null;

		for(int i = 0, count = m_ActiveChunks.Count; i < count; i++)
		{
			m_ActiveChunks[i].Dispose();
		}
		m_ActiveChunks.Clear();
		m_ActiveChunks = null;

		m_CubePrefab    = null;
		m_CubeTypesData = null;
}
}