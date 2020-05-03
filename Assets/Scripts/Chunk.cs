using UnityEngine;

public class Chunk
{
	// CONSTANTS

	public const int   SIZE         = 12;
	public const float PERLIN_SCALE = 0.6f;

	// PUBLIC PROPERTIES

	public  int        CoordX { get; private set; }
	public  int        CoordY { get; private set; }

	// PRIVATE PROPERTIES

	private GameObject m_Root;
	private Cube[][][] m_Cubes;
	private int        m_PerlinOffset;

	// CONSTRUCTOR

	public Chunk(GameObject cubePrefab, Vector2 coords, int perlinOffset)
	{
		CoordX                    = (int)coords.x;
		CoordY                    = (int)coords.y;
		m_PerlinOffset            = perlinOffset;
		m_Cubes                   = new Cube[SIZE][][];
		m_Root                    = new GameObject();
		m_Root.transform.position = new Vector3(coords.x * SIZE, 0, coords.y * SIZE);
#if UNITY_EDITOR
		m_Root.name               = $"[{coords.x},{coords.y}]";
#endif
		//spawning loop
		for (int x = 0; x < SIZE; x++)
		{
			m_Cubes[x] = new Cube[SIZE][];
			for (int y = SIZE - 1; y >= 0; --y)
			{
				m_Cubes[x][y] = new Cube[SIZE];
				for (int z = 0; z < SIZE; z++)
				{
					var cubeObject                     = Object.Instantiate(cubePrefab, m_Root.transform);
					cubeObject.transform.localPosition = new Vector3(x, y, z);
					var cube                           = new Cube(cubeObject);
					m_Cubes[x][y][z]                   = cube;
					if(SIZE * ComputePerlinNoise(new Vector2(CoordX*SIZE + x, CoordY*SIZE + z)) > y)
						cube.SetActive(true);
					else
						cube.SetActive(false);
				}
			}
		}

		DeactivateHiddenCubes();
	}

	// PUBLIC METHODS

	public void Activate(Vector2 coords)
	{
		CoordX                    = (int)coords.x;
		CoordY                    = (int)coords.y;
		m_Root.transform.position = new Vector3(coords.x * SIZE, 0, coords.y * SIZE);
#if UNITY_EDITOR
		m_Root.name               = $"[{coords.x},{coords.y}]";
#endif

		for (int x = 0; x < SIZE; x++)
		{
			for (int y = SIZE - 1; y >= 0; --y)
			{
				for (int z = 0; z < SIZE; z++)
				{
					var cube = m_Cubes[x][y][z];
					if (SIZE * ComputePerlinNoise(new Vector2(CoordX * SIZE + x, CoordY * SIZE + z)) > y)
						cube.SetActive(true);
					else
						cube.SetActive(false);
				}
			}
		}

		DeactivateHiddenCubes();

		m_Root.SetActive(true);
	}

	public void Deactivate()
	{
		m_Root.SetActive(false);
	}

	// PRIVATE METHODS

	private float ComputePerlinNoise(Vector2 pos)
	{
		return Mathf.PerlinNoise((pos.x) / SIZE * PERLIN_SCALE + m_PerlinOffset, (pos.y) / SIZE * PERLIN_SCALE + m_PerlinOffset);
	}

	private void DeactivateHiddenCubes()
	{
		for (int x = 0; x < SIZE; x++)
		{
			for (int y = SIZE - 1; y > 0; --y)
			{
				for (int z = 0; z < SIZE; z++)
				{
					if (y < SIZE - 1) //top square always visible, no point in checking
					{
						var topCubeCheck   =                           m_Cubes[x][y + 1][z].IsActive;
						var botCubeCheck   = (y == 0)        ? true  : m_Cubes[x][y - 1][z].IsActive; //Nobody can see bottom cube from bottom.
						var frontCubeCheck = (z == SIZE - 1) ? false : m_Cubes[x][y][z + 1].IsActive;
						var backCubeCheck  = (z == 0)        ? false : m_Cubes[x][y][z - 1].IsActive;
						var rightCubeCheck = (x == SIZE - 1) ? false : m_Cubes[x + 1][y][z].IsActive;
						var leftCubeCheck  = (x == 0)        ? false : m_Cubes[x - 1][y][z].IsActive;

						if (topCubeCheck && botCubeCheck && frontCubeCheck && backCubeCheck && rightCubeCheck && leftCubeCheck)
						{
							var cube = m_Cubes[x][y][z];
							cube.SetActive(false);
						}
					}
				}
			}
		}
	}
}