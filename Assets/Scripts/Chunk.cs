using System.Collections.Generic;
using UnityEngine;

using static Cube;

public class Chunk
{
	// CONSTANTS

	public  const int           SIZE         = 20;
	private const float         PERLIN_SCALE = 0.6f;

	// PUBLIC PROPERTIES

	public  int                 CoordX { get; private set; }
	public  int                 CoordY { get; private set; }

	// PRIVATE PROPERTIES

	private GameObject          m_Root;
	private Cube[][][]          m_Cubes;
	private List<(int,int,int)> m_CubesToDeactivate = new List<(int,int,int)>(SIZE * SIZE * SIZE / 4);
	private int                 m_PerlinOffset;
	private CubeTypeData[]      m_CubeTypesData;


	// CONSTRUCTOR

	public Chunk(GameObject cubePrefab, Vector2 coords, int perlinOffset, CubeTypeData[] cubeTypesData)
	{
		CoordX                    = (int)coords.x;
		CoordY                    = (int)coords.y;
		m_PerlinOffset            = perlinOffset;
		m_CubeTypesData           = cubeTypesData;
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
					var cubeObject                     = UnityEngine.Object.Instantiate(cubePrefab, m_Root.transform);
					cubeObject.transform.localPosition = new Vector3(x, y, z);
					var cube                           = cubeObject.GetComponent<Cube>();
					m_Cubes[x][y][z]                   = cube;
					if (SIZE * ComputePerlinNoise(new Vector2(CoordX * SIZE + x, CoordY * SIZE + z)) > y)
						InitCube(cube, y, true);
					else
						InitCube(cube, y, false);
				}
			}
		}

		DeactivateHiddenCubes();
	}


	// PUBLIC METHODS

	public Cube this[Vector3 cubeIndex]
	{
		get { return m_Cubes[(int)cubeIndex.x][(int)cubeIndex.y][(int)cubeIndex.z]; } 
	}

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
						InitCube(cube, y, true);
					else
						InitCube(cube, y, false);
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

	public void PlaceCube(Vector3 cubeLocalPos)
	{
		m_Cubes[(int)cubeLocalPos.x][(int)cubeLocalPos.y][(int)cubeLocalPos.z].SetStatus(E_Status.Active);

	//TODO: Only check neighbouring cubes
		DeactivateHiddenCubes();
	}

	//TODO: Only 6 cubes have to be checked, not all of them
	public void OnCubeDestroyed(Vector3 cubeLocalPos)
	{
		for (int x = 0; x < SIZE; x++)
		{
			for (int y = SIZE - 1; y >= 0; --y)
			{
				for (int z = 0; z < SIZE; z++)
				{
					var cube = m_Cubes[x][y][z];
					if (cube.Status == E_Status.Hidden && SIZE * ComputePerlinNoise(new Vector2(CoordX * SIZE + x, CoordY * SIZE + z)) > y)
						cube.SetStatus(E_Status.Active);
				}
			}
		}
		DeactivateHiddenCubes();
	}

	// PRIVATE METHODS

	private void InitCube(Cube cube, int height, bool activate)
	{
		var heightRatio  = height / (float)(SIZE - 1);
		var numOfCubes   = m_CubeTypesData.Length;
		var chosenType   = m_CubeTypesData[0]; //bedrock
		if (height != 0)
		{
			for (int i = numOfCubes - 2; i >= 0; --i)
			{
				var previousType = m_CubeTypesData[i+1];
				var currentType  = m_CubeTypesData[i];
				if (heightRatio >= currentType.MaxHeight)
				{
					chosenType = previousType;
					break;
				}
			}
			cube.Init(chosenType.Health, chosenType.Material, activate);
		}
		else
		{
			cube.Init(chosenType.Health, chosenType.Material, true);
		}
	}

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
						var topCubeCheck   =                           m_Cubes[x][y + 1][z].Status == E_Status.Active;
						var botCubeCheck   = (y == 0)        ? true  : m_Cubes[x][y - 1][z].Status == E_Status.Active; //Nobody can see bottom cube from bottom.
						var frontCubeCheck = (z == SIZE - 1) ? false : m_Cubes[x][y][z + 1].Status == E_Status.Active;
						var backCubeCheck  = (z == 0)        ? false : m_Cubes[x][y][z - 1].Status == E_Status.Active;
						var rightCubeCheck = (x == SIZE - 1) ? false : m_Cubes[x + 1][y][z].Status == E_Status.Active;
						var leftCubeCheck  = (x == 0)        ? false : m_Cubes[x - 1][y][z].Status == E_Status.Active;

						if (topCubeCheck && botCubeCheck && frontCubeCheck && backCubeCheck && rightCubeCheck && leftCubeCheck)
						{
							m_CubesToDeactivate.Add((x, y, z));
						}
					}
				}
			}
		}

		for (int i = 0, count = m_CubesToDeactivate.Count; i < count; i++)
		{
			var cubeCoords = m_CubesToDeactivate[i];
			var cube       = m_Cubes[cubeCoords.Item1][cubeCoords.Item2][cubeCoords.Item3];
			cube.SetStatus(E_Status.Hidden);
		}
		m_CubesToDeactivate.Clear();
	}
}