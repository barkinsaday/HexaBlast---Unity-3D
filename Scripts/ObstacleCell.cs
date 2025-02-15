using System;
using UnityEngine;

public class ObstacleCell : MonoBehaviour
{
    [SerializeField] private MeshRenderer innerHexRenderer; // ✅ Assign Inner_Hex's MeshRenderer in Inspector
    [SerializeField] private Color fullHealthColor = Color.white; 
    [SerializeField] private Color damagedColor = Color.red; 

    private int durability;
    private int maxDurability;
    private int x, y;
    private GridManager gridManager;

    void Awake()
    {
        durability = GameManager.Instance.obstacleDurability;
        maxDurability = durability; // Store the initial durability to scale color changes
    }

    void Start()
    {
        gridManager = GameManager.Instance.GridManager;
    }

    public void SetGridPosition(int x, int y)
    {
        this.x = x;
        this.y = y;
        this.name = $"Obstacle ({x}, {y})";
    }

    public void ReduceDurability(int damage)
    {
        durability -= damage;
        durability = Mathf.Max(0, durability);

        Debug.Log($"Damaged: {damage} to {this.name}, Remaining Durability: {durability}");

        if (durability <= 0)
            DestroyObstacle();
        else
            UpdateVisual();
    }

    private void UpdateVisual()
    {
        if (innerHexRenderer != null)
        {
            float healthPercentage = (float)durability / maxDurability;
            innerHexRenderer.material.color = Color.Lerp(damagedColor, fullHealthColor, healthPercentage);
        }
    }

    private void DestroyObstacle()
    {
        gridManager.ClearGridPosition(x, y);
        Destroy(gameObject);
    }

    public int GetDurability(){return this.durability;}

    public int GetX(){return this.x;}

    public int GetY(){return this.y;}
}
