using UnityEngine;

public class Chunk
{
	// CONSTANTS

	public const int SIZE = 7;

	// PUBLIC PROPERTIES

	public int              CoordX;
	public int              CoordY;

	public GameObject       Root;
	public GameObject[][][] Cubes;

	// CONSTRUCTOR

	public Chunk(GameObject cubePrefab, Vector2 spawnCoords)
	{
		CoordX                 = (int)spawnCoords.x;
		CoordY                 = (int)spawnCoords.y;
		Cubes                  = new GameObject[SIZE][][];
		Root                   = new GameObject();
		var offset             = new Vector3(spawnCoords.x * SIZE, 0, spawnCoords.y * SIZE);
		var rootTransform      = Root.transform;
		rootTransform.position = offset;
#if UNITY_EDITOR
		Root.name              = $"[{spawnCoords.x},{spawnCoords.y}]";
#endif

		//spawning loop
		for (int x = 0; x < SIZE; x++)
		{
			Cubes[x] = new GameObject[SIZE][];
			for (int y = SIZE - 1; y >= 0; --y)
			{
				Cubes[x][y] = new GameObject[SIZE];
				for (int z = 0; z < SIZE; z++)
				{
					if (Random.Range(0, 40) != 0 || y < 4)          //_!_hardcoded "noise" here
					{
						var cube                     = Object.Instantiate(cubePrefab, Root.transform);
						cube.transform.localPosition = new Vector3(x, y, z);
						Cubes[x][y][z]               = cube;
					}
				}
			}
		}

		//optimalization loop
		for (int x = 0; x < SIZE; x++)
		{
			for (int y = SIZE - 1; y > 0; --y)
			{
				for (int z = 0; z < SIZE; z++)
				{
					if (y < SIZE - 1) //top square always visible, no point in checking
					{
						var topCubeCheck   =                           Cubes[x][y + 1][z] != null;
						var botCubeCheck   = (y == 0)        ? true  : Cubes[x][y - 1][z] != null; //Nobody can see bottom cube from bottom.
						var frontCubeCheck = (z == SIZE - 1) ? false : Cubes[x][y][z + 1] != null;
						var backCubeCheck  = (z == 0)        ? false : Cubes[x][y][z - 1] != null;
						var rightCubeCheck = (x == SIZE - 1) ? false : Cubes[x + 1][y][z] != null;
						var leftCubeCheck  = (x == 0)        ? false : Cubes[x - 1][y][z] != null;

						if (topCubeCheck && botCubeCheck && frontCubeCheck && backCubeCheck && rightCubeCheck && leftCubeCheck)
						{
							var cube = Cubes[x][y][z];
							if (cube != null)
							{
								cube.SetActive(false);
							}
						}
					}
				}
			}
		}
	}

	// PUBLIC METHODS

	public void Activate(Vector2 coords)
	{
		CoordX                  = (int)coords.x;
		CoordY                  = (int)coords.y;
		Root.transform.position = new Vector3(coords.x * SIZE, 0, coords.y * SIZE);
		Root.SetActive(true);
	}

	public void Deactivate()
	{
		Root.SetActive(false);
	}
}