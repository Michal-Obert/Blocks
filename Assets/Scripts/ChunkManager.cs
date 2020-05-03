using System.Collections.Generic;
using UnityEngine;

public class ChunkManager
{
	// CONSTANTS

	private const int CHUNK_VISIBILITY_RANGE = 3;

	// PRIVATE PROPERTIES

	private List<Chunk> m_InactiveChunks = new List<Chunk>((2 * CHUNK_VISIBILITY_RANGE + 1) * 2 - 1);
	private List<Chunk> m_ActiveChunks   = new List<Chunk>((2 * CHUNK_VISIBILITY_RANGE + 1) * (2 * CHUNK_VISIBILITY_RANGE + 1));
	private GameObject  m_CubePrefab;
	private int         m_PerlinOffset;

	// CONSTRUCTOR

	public ChunkManager(GameObject cubePrefab, int perlinOffset)
	{
		m_CubePrefab   = cubePrefab;
		m_PerlinOffset = perlinOffset;
	}

	// PUBLIC METHODS

	public void GenerateInitialWorld()
	{
		for (int i = -CHUNK_VISIBILITY_RANGE; i < (CHUNK_VISIBILITY_RANGE + 1); i++)
		{
			for (int j = -CHUNK_VISIBILITY_RANGE; j < (CHUNK_VISIBILITY_RANGE + 1); j++)
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

		chunk  = new Chunk(m_CubePrefab, coords, m_PerlinOffset);

		m_ActiveChunks.Add(chunk);
	}

	public void ReturnChunk(Chunk chunk)
	{
		chunk.Deactivate();
		m_ActiveChunks.Remove(chunk);
		m_InactiveChunks.Add(chunk);
	}
}