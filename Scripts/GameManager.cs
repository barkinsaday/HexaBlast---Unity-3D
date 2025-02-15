using System.Collections;
using System.Collections.Generic;
using System.Data;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    //Singleton Game Manager
    public static GameManager Instance { get; private set; }

    //Props
    public GridManager GridManager { get; private set; }
    public CylinderGrid CylinderGrid { get; private set; }
    public GameObject[,] grid_logical; //Logical Grid
    public Transform[,] grid_transforms; //Stores the static transforms of grid positions

    [Header("Level")]
    public int Level;

    [Header("Prefabs and Objects")]
    [SerializeField] public GameObject HexagonPrefab;
    [SerializeField] public GameObject ObstaclePrefab;
    [SerializeField] public GameObject BombPrefab;

    //Params
    [Header("Grid Settings - Do Not Change (Default: 0)")]
    [SerializeField] float initialX; 
    [SerializeField] float initialY;
    public int ROW_COUNT;
    public int COL_COUNT;
    public float scale;

    [Header("Animation Settings")]
    public float blastAnimationDuration; //0.6f
    public float fallAnimationDuration; //0.3f
    public float fallAnimationDelay; //0f
    public float spawnAnimationDelay; //0f

    public float spawnHeightOffset;

    [Header("Game Play Settings")]
    public int minimumMatchCount;
    public float obstacleProbability;
    public int obstacleDurability;
    public int bombDamage;

    private void Awake()
    {
        Debug.Log("GM: Awake");
        if (Instance == null){
            Instance = this;
            DontDestroyOnLoad(gameObject);
            Debug.LogWarning("GM: Do Not Destroy On Load");
            Init_Grid();
        }
        else{
            Destroy(gameObject);
            Debug.LogWarning("GM: Destroy GM");
        }
            
    }


    void Start()
    {
        Debug.LogWarning("GM: Start");
        GridManager = GetComponentInChildren<GridManager>();
        CylinderGrid = GetComponentInChildren<CylinderGrid>();
    }

    void Init_Grid()
    {
        if(ROW_COUNT == 0)
            ROW_COUNT = 6;
        if(COL_COUNT == 0)
            COL_COUNT = 20;

        grid_logical = new GameObject[ROW_COUNT, COL_COUNT];
        
        for (int i = 0; i < ROW_COUNT; i++)
        {
            for (int j = 0; j < COL_COUNT; j++){
                grid_logical[i, j] = null;
                grid_transforms = new Transform[ROW_COUNT, COL_COUNT];
            }
        }
        Debug.LogWarning($"GM: Init Null Grid ({ROW_COUNT}, {COL_COUNT})");
    }

    public void RestartLevel(int newLevel)
    {
        Debug.LogWarning($"Restarting Level: {newLevel}");

        // Reset the camera
        CameraController cameraController = FindObjectOfType<CameraController>();
        if (cameraController != null)
            cameraController.ResetCamera();
        else
            Debug.LogError("CameraController not found! Camera reset skipped.");

        // ✅ Destroy existing grid objects
        ClearExistingLevel();

        // ✅ Set new level
        Level = newLevel;

        // ✅ Reinitialize GridManager to reset all mechanics
        if (GridManager != null)
        {
            Destroy(GridManager.gameObject); // ✅ Fully reset GridManager
        }

        // ✅ Recreate GridManager as a child of GameManager
        GameObject gridManagerObj = new GameObject("GridManager");
        gridManagerObj.transform.SetParent(this.transform); // ✅ Reparent GridManager to GameManager
        GridManager = gridManagerObj.AddComponent<GridManager>();

        GridManager.Init(); // ✅ Initialize new GridManager

        // ✅ Reinitialize grid & objects
        Init_Grid();
        CylinderGrid.StartLevel();
    }



    private void ClearExistingLevel()
    {
        Debug.Log("Clearing Existing Level...");

        for (int i = 0; i < ROW_COUNT; i++)
        {
            for (int j = 0; j < COL_COUNT; j++)
            {
                if (grid_logical[i, j] != null)
                {
                    HexagonCell hex = grid_logical[i, j].GetComponent<HexagonCell>();
                    if (hex != null)
                    {
                        hex.OnHexagonClicked -= GameManager.Instance.GridManager.HandleHexagonClick; // ✅ Remove event subscriptions
                    }

                    Destroy(grid_logical[i, j]);
                    grid_logical[i, j] = null;
                }
            }
        }

        Debug.LogWarning("Grid Cleared.");
    }

    

}

/*
    NOTES:
    -Commentleri ve kod tarzını (parantez falan) düzelt
    
    -bombalar ve obtaclelardan oluşan bir bölüm yap
*/ 
