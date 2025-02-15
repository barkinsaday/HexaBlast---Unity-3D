using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GridManager : MonoBehaviour
{
    [SerializeField] private GameObject Hexagon_Cell_Prefab; // ✅ Assigned dynamically
    [SerializeField] private GameObject Obstacle_Cell_Prefab;
    GameObject[,] grid;

    void Start()
    {
        this.grid = GameManager.Instance.grid_logical;

        // ✅ Assign prefabs from GameManager to prevent null references after level change
        Hexagon_Cell_Prefab = GameManager.Instance.HexagonPrefab;
        Obstacle_Cell_Prefab = GameManager.Instance.ObstaclePrefab;

        if (Hexagon_Cell_Prefab == null || Obstacle_Cell_Prefab == null)
            Debug.LogError("[GridManager] Prefabs are not assigned properly!");
    }

    public void HandleHexagonClick(int x, int y)
    {
        Debug.Log("Grid Manager: Click Detected!!!");

        GameObject clickedObject = grid[x, y];
        if (clickedObject == null) return;

        // ✅ If the clicked object is a bomb, trigger explosion
        if (clickedObject.GetComponent<BombCell>() != null)
        {
            ActivateBomb(x, y);
            return;
        }

        HexagonCell clickedHex = clickedObject.GetComponent<HexagonCell>();
        if (clickedHex == null) return;

        // Find adjacent hexagons and obstacles
        List<GameObject> adjacentObstacles;
        List<GameObject> toDestroy = FindAdjacentOfType(x, y, clickedHex.hexagonType, out adjacentObstacles);

        if (toDestroy.Count >= GameManager.Instance.minimumMatchCount)
        {
            DestroyHexagons(toDestroy, x, y); // ✅ Pass click position for bomb placement
            HitObstacles(adjacentObstacles, toDestroy);
        }
        else
        {
            Debug.Log("Group too small to destroy.");
        }
    }


    List<GameObject> FindAdjacentOfType(int x, int y, int type, out List<GameObject> adjacentObstacles)
    {
        List<GameObject> found = new List<GameObject>(); // Hexagons of the same type
        adjacentObstacles = new List<GameObject>(); // Obstacles adjacent to the hexagons
        HashSet<Vector2Int> visited = new HashSet<Vector2Int>();
        Queue<Vector2Int> queue = new Queue<Vector2Int>();
        queue.Enqueue(new Vector2Int(x, y));

        int colCount = GameManager.Instance.COL_COUNT;
        int rowCount = GameManager.Instance.ROW_COUNT;

        while (queue.Count > 0)
        {
            Vector2Int current = queue.Dequeue();
            if (visited.Contains(current)) continue;

            int i = current.x, j = current.y;
            visited.Add(current);

            // Ensure valid index bounds for rows
            if (i < 0 || i >= rowCount) continue;

            GameObject cell = grid[i, j];

            // If it's an obstacle, add it to adjacentObstacles
            if (cell != null && cell.GetComponent<ObstacleCell>() != null)
            {
                adjacentObstacles.Add(cell);
                continue; // Stop further propagation for obstacles
            }

            // If it's a hexagon of the target type, add to found
            if (cell != null && cell.GetComponent<HexagonCell>()?.hexagonType == type)
            {
                found.Add(cell);

                // Add normal row neighbors
                queue.Enqueue(new Vector2Int(i - 1, j)); // Top
                queue.Enqueue(new Vector2Int(i + 1, j)); // Bottom

                // Column Wrapping for Cylindrical Behavior
                int leftColumn = (j == 0) ? colCount - 1 : j - 1;  // Wrap left
                int rightColumn = (j == colCount - 1) ? 0 : j + 1; // Wrap right

                queue.Enqueue(new Vector2Int(i, leftColumn));  // Left
                queue.Enqueue(new Vector2Int(i, rightColumn)); // Right

                // Diagonal Neighbors based on Even/Odd column
                bool isEvenColumn = (j % 2 == 0);
                if (isEvenColumn)
                {
                    queue.Enqueue(new Vector2Int(i - 1, leftColumn)); // Top-left
                    queue.Enqueue(new Vector2Int(i - 1, rightColumn)); // Top-right
                }
                else
                {
                    queue.Enqueue(new Vector2Int(i + 1, leftColumn)); // Bottom-left
                    queue.Enqueue(new Vector2Int(i + 1, rightColumn)); // Bottom-right
                }
            }
        }

        return found;
    }

    void DestroyHexagons(List<GameObject> hexagons, int clickedX, int clickedY)
    {
        bool spawnBomb = hexagons.Count >= 5;
        GameObject bomb = null;

        foreach (GameObject hex in hexagons)
        {
            if (hex == null) continue;

            HexagonCell hexCell = hex.GetComponent<HexagonCell>();
            int x = hexCell.GetX();
            int y = hexCell.GetY();

            grid[x, y] = null;
            StartCoroutine(DestroyHexagonCoroutine(hex));

            // ✅ Create the bomb AFTER hexagons are destroyed
            if (spawnBomb && x == clickedX && y == clickedY)
            {
                bomb = Instantiate(GameManager.Instance.BombPrefab, GameManager.Instance.grid_transforms[x, y].position,
                                    GameManager.Instance.grid_transforms[x, y].rotation, transform);
                BombCell bombCell = bomb.GetComponent<BombCell>();
                bombCell.SetGridPosition(x, y);
                grid[x, y] = bomb; // ✅ Bomb occupies grid space
            }
        }

        // ✅ Ensure the bomb falls if needed
        HandleFallingHexagons();
    }



    //New Coroutine to ensure destruction timing
    IEnumerator DestroyHexagonCoroutine(GameObject hex)
    {
        yield return hex.GetComponent<HexagonCell>().DestroyHexagonCell(GameManager.Instance.blastAnimationDuration);
        
        if (hex != null)
            Destroy(hex); //Ensure hexagon is destroyed properly after animation
    }


    public void HandleFallingHexagons()
    {
        List<(GameObject hex, int newRow, int col)> fallingHexagons = new List<(GameObject, int, int)>();
        int[] num_hex_to_spwan_on_column = new int[GameManager.Instance.COL_COUNT];

        for (int col = 0; col < GameManager.Instance.COL_COUNT; col++)
        {
            int gapCountOfColumn = 0;
            for (int row = GameManager.Instance.ROW_COUNT - 1; row >= 0; row--)
            {
                if (grid[row, col] == null) 
                {
                    gapCountOfColumn++;
                    continue;
                }

                // ✅ Ensure bombs and obstacles do not fall
                if (grid[row, col].GetComponent<ObstacleCell>() != null)
                {
                    gapCountOfColumn = 0; // Obstacle blocks falling
                    continue;
                }

                // ✅ If it's a hexagon or bomb, check if it needs to fall
                if (gapCountOfColumn > 0)
                {
                    GameObject cell = grid[row, col];
                    int newRow = row + gapCountOfColumn;

                    // ✅ Update logical grid
                    grid[newRow, col] = cell;
                    grid[row, col] = null;

                    // ✅ Store for movement
                    fallingHexagons.Add((cell, newRow, col));
                }
            }
            num_hex_to_spwan_on_column[col] = gapCountOfColumn;
        }

        // ✅ Process falling animations after updating grid
        foreach (var (cell, newRow, col) in fallingHexagons)
        {
            if (cell == null) continue;

            if (cell.GetComponent<HexagonCell>() != null || cell.GetComponent<BombCell>() != null)
            {
                cell.GetComponent<HexagonCell>()?.SetGridPosition(newRow, col);
                cell.GetComponent<BombCell>()?.SetGridPosition(newRow, col); // ✅ Update bomb's grid position

                Transform targetTransform = GameManager.Instance.grid_transforms[newRow, col];
                if (targetTransform != null)
                {
                    StartCoroutine(FallAnimation(cell, targetTransform.position,
                                                GameManager.Instance.fallAnimationDuration,
                                                GameManager.Instance.fallAnimationDelay));
                }
            }
        }

        // ✅ Spawn new hexagons only after falling completes
        StartCoroutine(SpawnNewHexagonsAfterFalling(num_hex_to_spwan_on_column));
    }



    IEnumerator SpawnNewHexagonsAfterFalling(int[] num_hex_to_spwan_on_column)
    {
        yield return new WaitForSeconds(GameManager.Instance.fallAnimationDuration); // Ensure all existing hexagons fall first

        for (int col = 0; col < GameManager.Instance.COL_COUNT; col++)
        {
            for (int i = 0; i < num_hex_to_spwan_on_column[col]; i++)
            {
                StartCoroutine(DelayedSpawnNewHexagon(col, i * GameManager.Instance.spawnAnimationDelay)); // Ensures ordered spawning
            }
        }
    }

    IEnumerator DelayedSpawnNewHexagon(int col, float delay)
    {
        yield return new WaitForSeconds(delay);
        SpawnNewHexagon(col);
    }


    void SpawnNewHexagon(int col)
    {
        int targetRow = -1;
        bool obstacleFound = false;

        // Find the lowest available row above the highest obstacle
        for (int row = 0; row < GameManager.Instance.ROW_COUNT; row++)
        {
            if (grid[row, col] != null && grid[row, col].GetComponent<ObstacleCell>() != null)
            {
                obstacleFound = true;
                break;
            }
            if (grid[row, col] == null)
            {
                targetRow = row;
            }
        }

        // If no obstacle was found, fall to the lowest null row (default behavior)
        if (!obstacleFound)
        {
            for (int row = GameManager.Instance.ROW_COUNT - 1; row >= 0; row--)
            {
                if (grid[row, col] == null)
                {
                    targetRow = row;
                    break;
                }
            }
        }

        if (targetRow == -1)
        {
            Debug.LogError($"[SpawnNewHexagon] No valid empty row found for column {col}");
            return;
        }

        Transform targetTransform = GameManager.Instance.grid_transforms[targetRow, col];

        if (targetTransform == null)
        {
            Debug.LogError($"[SpawnNewHexagon] No valid transform found for ({targetRow}, {col})");
            return;
        }

        // ✅ Ensure hexagon prefab is assigned before instantiating
        if (Hexagon_Cell_Prefab == null)
        {
            Debug.LogError("[SpawnNewHexagon] Hexagon_Cell_Prefab is not assigned!");
            return;
        }

        // Spawn the hexagon **above** the column
        Vector3 spawnPosition = targetTransform.position + new Vector3(0, GameManager.Instance.spawnHeightOffset, 0);
        GameObject newHexagon = Instantiate(Hexagon_Cell_Prefab, spawnPosition, targetTransform.rotation, transform);

        // Set a random type
        int randomType = Random.Range(1, 5);
        HexagonCell hexCell = newHexagon.GetComponent<HexagonCell>();
        hexCell.SetHexagonType(randomType);

        // Assign logical grid position before falling
        hexCell.SetGridPosition(targetRow, col);
        grid[targetRow, col] = newHexagon;

        // Start the falling animation
        StartCoroutine(FallAnimation(newHexagon, targetTransform.position, 
                                    GameManager.Instance.fallAnimationDuration, 
                                    GameManager.Instance.spawnAnimationDelay));
    }



    IEnumerator FallAnimation(GameObject hexagon, Vector3 targetPosition, float _duration, float _delay = -1)
    {
        if (hexagon == null) {Debug.LogError("Fall Animation: NULL Hexagon Obj"); yield break;} // Prevent error if the hexagon was destroyed

        if (_delay == -1) 
            _delay = GameManager.Instance.fallAnimationDelay; // Use default delay if none is provided

        yield return new WaitForSeconds(_delay); // Wait for the correct delay before falling starts

        float elapsed = 0f;
        Vector3 startPosition = hexagon.transform.position;

        while (elapsed < _duration)
        {
            elapsed += Time.deltaTime;
            if (hexagon != null)
                hexagon.transform.position = Vector3.Lerp(startPosition, targetPosition, elapsed / _duration);
            yield return null;
        }

        if (hexagon != null)
            hexagon.transform.position = targetPosition; // Snap to final position
    }


    public void ClearGridPosition(int x, int y)
    {
        grid[x, y] = null;
    }

    void HitObstacles(List<GameObject> obstacles, List<GameObject> blastedHexagons)
    {
        foreach (GameObject obstacle in obstacles)
        {
            ObstacleCell obstacleCell = obstacle.GetComponent<ObstacleCell>();
            if (obstacleCell != null)
            {
                int damage = 0;

                // Count how many blasted hexagons are directly adjacent to this obstacle
                foreach (GameObject hexagon in blastedHexagons)
                {
                    HexagonCell hexCell = hexagon.GetComponent<HexagonCell>();
                    if (IsDirectlyAdjacent(obstacleCell.GetX(), obstacleCell.GetY(), hexCell.GetX(), hexCell.GetY()))
                    {
                        damage++;
                    }
                }

                // Apply the calculated damage
                obstacleCell.ReduceDurability(damage);

                // If durability reaches 0, clear the grid position and destroy the obstacle
                if (obstacleCell.GetDurability() <= 0)
                {
                    ClearGridPosition(obstacleCell.GetX(), obstacleCell.GetY());
                    Destroy(obstacle);
                    HandleFallingHexagons();
                }
            }
        }
    }

    bool IsDirectlyAdjacent(int obstacleX, int obstacleY, int hexX, int hexY)
    {
        // Hexagonal grid adjacency logic
        bool isEvenColumn = (obstacleY % 2 == 0);

        // Direct neighbors
        Vector2Int[] directions = isEvenColumn
            ? new Vector2Int[]
            {
                new Vector2Int(-1, 0), // Top
                new Vector2Int(-1, -1), // Top-left
                new Vector2Int(-1, +1), // Top-right
                new Vector2Int(0, -1), // Left
                new Vector2Int(0, +1), // Right
                new Vector2Int(+1, 0), // Bottom
            }
            : new Vector2Int[]
            {
                new Vector2Int(-1, 0), // Top
                new Vector2Int(0, -1), // Left
                new Vector2Int(0, +1), // Right
                new Vector2Int(+1, -1), // Bottom-left
                new Vector2Int(+1, +1), // Bottom-right
                new Vector2Int(+1, 0), // Bottom
            };

        foreach (Vector2Int dir in directions)
        {
            if (obstacleX + dir.x == hexX && obstacleY + dir.y == hexY)
            {
                return true;
            }
        }

        return false;
    }

    public void ActivateBomb(int x, int y)
    {
        Debug.Log($"Activating Bomb at ({x}, {y})!");

        Queue<Vector2Int> bombQueue = new Queue<Vector2Int>(); // ✅ Queue for bombs to explode
        HashSet<Vector2Int> processedBombs = new HashSet<Vector2Int>(); // ✅ Prevent infinite loops

        bombQueue.Enqueue(new Vector2Int(x, y)); // Start with the first bomb

        StartCoroutine(TriggerBombsWithDelay(bombQueue, processedBombs)); // ✅ Start the delayed explosion sequence
    }


    IEnumerator TriggerBombsWithDelay(Queue<Vector2Int> bombQueue, HashSet<Vector2Int> processedBombs)
    {
        float delayBetweenExplosions = 0.15f; // ✅ Adjust this value for desired effect

        while (bombQueue.Count > 0)
        {
            Vector2Int currentBomb = bombQueue.Dequeue();
            if (processedBombs.Contains(currentBomb)) continue; // ✅ Prevent duplicate triggers
            processedBombs.Add(currentBomb);

            int bombX = currentBomb.x;
            int bombY = currentBomb.y;

            // ✅ Get all adjacent hexagons, obstacles, AND bombs
            List<GameObject> adjacentCells = GetAdjacentCells(bombX, bombY);
            List<GameObject> hexagonsToDestroy = new List<GameObject>();
            List<GameObject> obstaclesToHit = new List<GameObject>();

            foreach (GameObject cell in adjacentCells)
            {
                if (cell == null) continue;

                if (cell.GetComponent<HexagonCell>() != null)
                {
                    hexagonsToDestroy.Add(cell);
                }
                else if (cell.GetComponent<ObstacleCell>() != null)
                {
                    obstaclesToHit.Add(cell);
                }
                else if (cell.GetComponent<BombCell>() != null)
                {
                    Vector2Int bombPos = new Vector2Int(cell.GetComponent<BombCell>().x, cell.GetComponent<BombCell>().y);
                    if (!processedBombs.Contains(bombPos)) // ✅ Only enqueue if not already processed
                    {
                        bombQueue.Enqueue(bombPos);
                    }
                }
            }

            // ✅ Destroy adjacent hexagons
            foreach (GameObject hex in hexagonsToDestroy)
            {
                int cellX = hex.GetComponent<HexagonCell>().GetX();
                int cellY = hex.GetComponent<HexagonCell>().GetY();
                grid[cellX, cellY] = null;
                StartCoroutine(DestroyHexagonCoroutine(hex));
            }

            // ✅ Damage obstacles separately
            HitObstaclesWithBomb(obstaclesToHit);

            // ✅ Play explosion effect
            grid[bombX, bombY].GetComponent<BombCell>().ExplodeAnimation();

            // ✅ Remove the bomb from the grid and destroy it
            Debug.Log($"Destroying Bomb at ({bombX}, {bombY})");
            Destroy(grid[bombX, bombY]);
            grid[bombX, bombY] = null;

            // ✅ Wait before triggering the next bomb
            yield return new WaitForSeconds(delayBetweenExplosions);
        }

        // ✅ Trigger falling mechanic after all explosions are done
        HandleFallingHexagons();
    }





    void HitObstaclesWithBomb(List<GameObject> obstacles)
    {
        foreach (GameObject obstacle in obstacles)
        {
            ObstacleCell obstacleCell = obstacle.GetComponent<ObstacleCell>();
            if (obstacleCell != null)
            {
                int bombDamage = GameManager.Instance.bombDamage; // ✅ Bombs deal more damage than hexagons
                obstacleCell.ReduceDurability(bombDamage);

                // ✅ If destroyed, remove from grid
                if (obstacleCell.GetDurability() <= 0)
                {
                    grid[obstacleCell.GetX(), obstacleCell.GetY()] = null;
                    Destroy(obstacle);
                }
            }
        }
    }



    List<GameObject> GetAdjacentCells(int x, int y)
    {
        List<GameObject> adjacentCells = new List<GameObject>();
        int colCount = GameManager.Instance.COL_COUNT;
        int rowCount = GameManager.Instance.ROW_COUNT;

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

        Vector2Int[] directions = (y % 2 == 0) ? directionsEven : directionsOdd;

        foreach (Vector2Int dir in directions)
        {
            int neighborX = x + dir.x;
            int neighborY = y + dir.y;

            // ✅ Ensure the bomb does NOT wrap around the cylindrical grid incorrectly
            if (neighborY < 0) neighborY = colCount - 1;
            if (neighborY >= colCount) neighborY = 0;

            if (neighborX >= 0 && neighborX < rowCount && grid[neighborX, neighborY] != null)
            {
                adjacentCells.Add(grid[neighborX, neighborY]);
            }
        }

        return adjacentCells;
    }

    void OnDisable()
    {
        StopAllCoroutines(); // ✅ Ensures all coroutines are stopped when exiting play mode
    }

    public void Init()
    {
        Debug.Log("Initializing GridManager...");
        this.grid = GameManager.Instance.grid_logical;
    }


}


