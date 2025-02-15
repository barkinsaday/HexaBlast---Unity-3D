using System.Collections.Generic;
using UnityEngine;

public class CylinderGrid : MonoBehaviour
{
    [SerializeField] private GameObject hexagonPrefab; // Prefab for the hexagon
    private GameObject[,] grid; // Will point to GM.grid 

    void Start()
    {
        if (GameManager.Instance.Level == -1)
        {
            Debug.LogWarning("CylinderGrid: No Level Selected. Waiting...");
            return;
        }

        float startY = 3f; // Initial Y position of the top hexagon
        float gapY = 1.25f; // Gap between hexagons in the Y-axis
        float columnOffsetY = 0.5f; // Alternating column offset
        int COL_COUNT = GameManager.Instance.COL_COUNT;
        int ROW_COUNT = GameManager.Instance.ROW_COUNT;
        float rotationStep = 360f / COL_COUNT;

        // Point to the GameManager grid
        grid = GameManager.Instance.grid_logical;

        // Ensure obstacles are placed according to the level
        Debug.LogWarning($"CylinderGrid: Generating Level {GameManager.Instance.Level}");

        CreateCylinder(ROW_COUNT, COL_COUNT, startY, gapY, columnOffsetY, rotationStep);
    }


    // Function to create the entire cylinder
    private void CreateCylinder(int rows, int columns, float startY, float gapY, float columnOffsetY, float rotationStep)
    {
        for (int col = 0; col < columns; col++)
        {
            // Calculate column offset for staggered height
            float columnYOffset = (col % 2 == 0) ? columnOffsetY : -columnOffsetY;

            // Create a column of hexagons at the initial position with the staggered offset
            CreateColumn(col, new Vector3(0, startY + columnYOffset * 0.5f, -2.7f), gapY, rows);

            // Rotate the column around the Z-axis to form the cylindrical surface
            RotateColumn(col, rotationStep * col, 0f, 0f, 0f);

            // Fix central rotation to make hexagons flat-down 
            for (int i = 0; i < rows; i++)
                grid[i, col].transform.Rotate(0, 0, 30, Space.Self);
        }

        SetTransfromGrid(rows, columns);
    }

    // Function to create a column of hexagons
  private void CreateColumn(int col, Vector3 startPosition, float gapY, int rows)
    {
        for (int i = 0; i < rows; i++)
        {
            // Calculate the position of each hexagon/obstacle in the column
            Vector3 position = startPosition + new Vector3(0, -i * gapY, 0);
            GameObject cell;

            if (GameManager.Instance.Level == 1 && i == 3) // ✅ Level 1: Row 3 obstacles
            {
                cell = Instantiate(GameManager.Instance.ObstaclePrefab, position, Quaternion.identity, transform);
                ObstacleCell obstacleCell = cell.GetComponent<ObstacleCell>();
                obstacleCell.SetGridPosition(i, col);
            }
            else if (GameManager.Instance.Level == 2 && islandMazeObstacles.Contains(new Vector2Int(i, col))) // ✅ Level 2: Island Maze obstacles
            {
                cell = Instantiate(GameManager.Instance.ObstaclePrefab, position, Quaternion.identity, transform);
                ObstacleCell obstacleCell = cell.GetComponent<ObstacleCell>();
                obstacleCell.SetGridPosition(i, col);
            }
            else if (GameManager.Instance.Level == 3) // ✅ Level 3: Bomb Grid
            {
                if (col == 0) // ✅ Column 0: All bombs
                {
                    cell = Instantiate(GameManager.Instance.BombPrefab, position, Quaternion.identity, transform);
                    BombCell bombCell = cell.GetComponent<BombCell>();
                    bombCell.SetGridPosition(i, col);
                }
                else if (col == 1 || col == 19) // ✅ Columns 1 & 19: All obstacles
                {
                    if (i == 0) // ✅ Row 0 has no obstacles
                    {
                        cell = Instantiate(hexagonPrefab, position, Quaternion.identity, transform);
                        HexagonCell hexCell = cell.GetComponent<HexagonCell>();
                        hexCell.SetHexagonType(Random.Range(1, 5));
                        hexCell.SetGridPosition(i, col);
                    }
                    else
                    {
                        cell = Instantiate(GameManager.Instance.ObstaclePrefab, position, Quaternion.identity, transform);
                        ObstacleCell obstacleCell = cell.GetComponent<ObstacleCell>();
                        obstacleCell.SetGridPosition(i, col);
                    }
                }
                else if ((col - 1) % 3 == 0 || (col - 2) % 3 == 0) // ✅ Hexagon columns
                {
                    cell = Instantiate(hexagonPrefab, position, Quaternion.identity, transform);
                    HexagonCell hexCell = cell.GetComponent<HexagonCell>();
                    hexCell.SetHexagonType(Random.Range(1, 5));
                    hexCell.SetGridPosition(i, col);
                }
                else // ✅ Alternating Obstacle columns
                {
                    if (i == 0) // ✅ Row 0 has no obstacles
                    {
                        cell = Instantiate(hexagonPrefab, position, Quaternion.identity, transform);
                        HexagonCell hexCell = cell.GetComponent<HexagonCell>();
                        hexCell.SetHexagonType(Random.Range(1, 5));
                        hexCell.SetGridPosition(i, col);
                    }
                    else
                    {
                        cell = Instantiate(GameManager.Instance.ObstaclePrefab, position, Quaternion.identity, transform);
                        ObstacleCell obstacleCell = cell.GetComponent<ObstacleCell>();
                        obstacleCell.SetGridPosition(i, col);
                    }
                }
            }

            else if (GameManager.Instance.Level == 4) // ✅ Level 4: Bomb Chambers
            {
                if (IsBombPosition(i, col)) // ✅ Place bomb in structured pattern
                {
                    cell = Instantiate(GameManager.Instance.BombPrefab, position, Quaternion.identity, transform);
                    BombCell bombCell = cell.GetComponent<BombCell>();
                    bombCell.SetGridPosition(i, col);
                }
                else if (IsObstaclePosition(i, col)) // ✅ Surround bombs with obstacles
                {
                    cell = Instantiate(GameManager.Instance.ObstaclePrefab, position, Quaternion.identity, transform);
                    ObstacleCell obstacleCell = cell.GetComponent<ObstacleCell>();
                    obstacleCell.SetGridPosition(i, col);
                }
                else // ✅ Fill remaining spaces with hexagons
                {
                    cell = Instantiate(hexagonPrefab, position, Quaternion.identity, transform);
                    HexagonCell hexCell = cell.GetComponent<HexagonCell>();
                    hexCell.SetHexagonType(Random.Range(1, 5));
                    hexCell.SetGridPosition(i, col);
                }
            }
            
            else // ✅ Default behavior for other levels
            {
                cell = Instantiate(hexagonPrefab, position, Quaternion.identity, transform);
                HexagonCell hexCell = cell.GetComponent<HexagonCell>();
                hexCell.SetHexagonType(Random.Range(1, 5));
                hexCell.SetGridPosition(i, col);
            }

            // Store it in the grid
            grid[i, col] = cell;
        }
    }



    // Function to rotate a column of hexagons around the Z-axis
    private void RotateColumn(int col, float degrees, float centerX, float centerY, float centerZ)
    {
        float radians = Mathf.Deg2Rad * degrees; // Convert degrees to radians

        for (int i = 0; i < grid.GetLength(0); i++) // Iterate over rows (first index)
        {
            GameObject hex = grid[i, col]; // Correct order (rows first, columns second)

            // Get the current position of the hexagon
            Vector3 currentPosition = hex.transform.position;

            // Translate position to be relative to the origin of rotation (Z-axis)
            float relativeX = currentPosition.x - centerX;
            float relativeY = currentPosition.y - centerY;
            float relativeZ = currentPosition.z - centerZ;

            // Perform 2D rotation in the XZ plane
            float rotatedX = relativeX * Mathf.Cos(radians) - relativeZ * Mathf.Sin(radians);
            float rotatedZ = relativeX * Mathf.Sin(radians) + relativeZ * Mathf.Cos(radians);

            // Calculate the new position after rotation
            Vector3 newPosition = new Vector3(rotatedX + centerX, relativeY, rotatedZ + centerZ);

            // Set the new position of the hexagon
            hex.transform.position = newPosition;

            // Calculate the rotation to ensure the hexagon still faces the Z-axis
            Vector3 directionToCenter = new Vector3(-rotatedX, 0, -rotatedZ); // Direction toward the Z-axis
            Quaternion newRotation = Quaternion.LookRotation(directionToCenter, Vector3.up);

            // Apply the new rotation
            hex.transform.rotation = newRotation;
        }
    }

    public Transform GetHexagonTransform(int row, int col)
    {
        if (grid == null)
        {
            Debug.LogError("[GetHexagonTransform] Grid is NULL in CylinderGrid!");
            return null;
        }
        
        if (row < 0 || row >= grid.GetLength(0) || col < 0 || col >= grid.GetLength(1))
        {
            Debug.LogError($"[GetHexagonTransform] Invalid indices: ({row}, {col})");
            return null;
        }

        if (grid[row, col] == null)
        {
            Debug.LogError($"[GetHexagonTransform] No hexagon exists at ({row}, {col})");
            return null;
        }

        return grid[row, col].transform;
    }

    void SetTransfromGrid(int rows, int columns){
        // Create a parent GameObject to hold all transform holders
        GameObject transformContainer = new GameObject("TransformHolders");
        transformContainer.transform.SetParent(this.transform); // Parent it to CylinderGrid

        // Set GM.Transforms (Look-Up Table)
        for (int i = 0; i < rows; i++)
        {
            for (int j = 0; j < columns; j++)
            {
                GameObject hexagon = grid[i, j];

                // Create an empty GameObject for the transform
                GameObject transformHolder = new GameObject($"TransformHolder_{i}_{j}");
                transformHolder.transform.SetPositionAndRotation(hexagon.transform.position, hexagon.transform.rotation);

                // Parent it under "TransformHolders" for organization
                transformHolder.transform.SetParent(transformContainer.transform);

                // Store the independent transform
                GameManager.Instance.grid_transforms[i, j] = transformHolder.transform;
            }
        }
    }
    
    HashSet<Vector2Int> islandMazeObstacles = new HashSet<Vector2Int>
    {
        new Vector2Int(3, 6), new Vector2Int(1, 2), new Vector2Int(1, 7), 
        new Vector2Int(2, 4), new Vector2Int(4, 1), new Vector2Int(4, 6), 
        new Vector2Int(5, 0), new Vector2Int(5, 8)
    };

    private bool IsBombPosition(int row, int col)
    {
        return (row % 3 == 0 && col % 3 == 0); // ✅ Bombs placed every 3rd row & column
    }

    private bool IsObstaclePosition(int row, int col)
    {
        Vector2Int[] directionsEven = {
            new Vector2Int(-1, 0), // Top
            new Vector2Int(0, -1), // Left
            new Vector2Int(0, +1), // Right
            new Vector2Int(+1, 0), // Bottom
            new Vector2Int(-1, -1), // Top-left
            new Vector2Int(-1, +1) // Top-right
        };

        Vector2Int[] directionsOdd = {
            new Vector2Int(-1, 0), // Top
            new Vector2Int(0, -1), // Left
            new Vector2Int(0, +1), // Right
            new Vector2Int(+1, 0), // Bottom
            new Vector2Int(+1, -1), // Bottom-left
            new Vector2Int(+1, +1) // Bottom-right
        };

        Vector2Int[] directions = (col % 2 == 0) ? directionsEven : directionsOdd;

        foreach (Vector2Int dir in directions)
        {
            if (IsBombPosition(row + dir.x, col + dir.y))
            {
                return true; // ✅ Place obstacle if adjacent to a bomb
            }
        }

        return false;
    }



    public void StartLevel()
    {
        Debug.LogWarning($"Generating Level {GameManager.Instance.Level}");

        float startY = 3f;
        float gapY = 1.25f;
        float columnOffsetY = 0.5f;
        int COL_COUNT = GameManager.Instance.COL_COUNT;
        int ROW_COUNT = GameManager.Instance.ROW_COUNT;
        float rotationStep = 360f / COL_COUNT;

        // ✅ Use updated grid
        grid = GameManager.Instance.grid_logical;

        // ✅ Create new grid objects based on level
        CreateCylinder(ROW_COUNT, COL_COUNT, startY, gapY, columnOffsetY, rotationStep);
    }



}
