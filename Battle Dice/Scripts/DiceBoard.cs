using UnityEngine;

public class DiceBoard : MonoBehaviour {

    // Const Definition
	public float cubesDistance = 0.072f;
    int TOTAL_NUM_OF_CUBES = 15;
    private static GameObject[] playerCubes;
    private static GameObject[] rivalCubes;
    private static Vector3[] playerVectors;
    private static Vector3[] rivalVectors;
   public GameObject cubeObject;
    [SerializeField] private GameObject playerStartCube;
    [SerializeField] private GameObject rivalStartCube;
	Vector3 playerCubePos, rivalCubePos;

    public static GameObject[] PlayerCubes
    {
        get
        {
            return playerCubes;
        }

        set
        {
            playerCubes = value;
        }
    }

    public static GameObject[] RivalCubes
    {
        get
        {
            return rivalCubes;
        }

        set
        {
            rivalCubes = value;
        }
    }

    public static Vector3[] PlayerVectors
    {
        get
        {
            return playerVectors;
        }

        set
        {
            playerVectors = value;
        }
    }

    public static Vector3[] RivalVectors
    {
        get
        {
            return rivalVectors;
        }

        set
        {
            rivalVectors = value;
        }
    }

    public float GetPLACE_FOR_CUBE1()
    {
        return cubesDistance;
    }

    // Use this for initialization
    void Start () {
        Camera.main.transform.LookAt(this.transform);
        playerCubePos = playerStartCube.transform.position;
        rivalCubePos = rivalStartCube.transform.position;
        PlayerCubes = new GameObject[TOTAL_NUM_OF_CUBES];
        RivalCubes = new GameObject[TOTAL_NUM_OF_CUBES];
        PlayerVectors = new Vector3[TOTAL_NUM_OF_CUBES];
        RivalVectors = new Vector3[TOTAL_NUM_OF_CUBES];
        CreateDice();
    }

    public void CreateDice()
    {
        for (int i = 0; i < TOTAL_NUM_OF_CUBES; i++)
        {
			PlayerCubes[i] = Instantiate(cubeObject, playerCubePos, cubeObject.transform.rotation) as GameObject;
            PlayerCubes[i].name = "player" + i.ToString();
			playerVectors[i] = playerCubePos;
			playerCubePos.x += GetPLACE_FOR_CUBE1();
        }

        for (int i = 0; i < TOTAL_NUM_OF_CUBES; i++)
        {
			RivalCubes[i] = Instantiate(cubeObject, rivalCubePos, cubeObject.transform.rotation) as GameObject;
            RivalCubes[i].name = "rival" + i.ToString();
			rivalVectors[i] = rivalCubePos;
			rivalCubePos.x -= GetPLACE_FOR_CUBE1();
        }

		playerCubePos = playerStartCube.transform.position;
		rivalCubePos = rivalStartCube.transform.position;
    }
}
