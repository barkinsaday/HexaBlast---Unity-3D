using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HexagonCell : MonoBehaviour
{
    // Properties
    public int hexagonType;
    private MeshFilter innerCellMeshFilter = null; // To manage the mesh of Inner_Hex

    [SerializeField] private Mesh[] meshArray; // Array of meshes for hexagon types

    public int x, y; // Grid coordinates of the cell

    public delegate void HexagonClickHandler(int x, int y);
    public event HexagonClickHandler OnHexagonClicked;

    void Awake()
    {
        // Find and initialize the inner hex mesh filter
        Transform childHex = transform.Find("Inner_Hex");
        if (childHex == null){
            Debug.LogError("Inner_Hex child object is missing!");
            return;
        }
        else
        {
            innerCellMeshFilter = childHex.GetComponent<MeshFilter>();
            if (innerCellMeshFilter == null){
                Debug.LogError("MeshFilter is missing on Inner_Hex!");
                return;
            }
        }
        hexagonType = 0; // Default type
    }

    void Start()
    {
        if (GameManager.Instance != null && GameManager.Instance.GridManager != null)
        {
            OnHexagonClicked += GameManager.Instance.GridManager.HandleHexagonClick;
            Debug.Log($"Subscribed Hexagon ({x}, {y}) to GridManager!");
        }
        else{
            Debug.LogError($"Subscription Failed for Hexagon ({x}, {y}) - GridManager not found.");
            return;
        }
    }


    void SetScale(GameObject obj, float scale)
    {
        obj.transform.localScale = new Vector3(scale, scale, scale);
    }

    // Method to set the type and mesh of the hexagon
    public void SetHexagonType(int type = 0)
    {
        hexagonType = type;
        if (type >= 0 && type < meshArray.Length)
            innerCellMeshFilter.mesh = meshArray[type];
        else
        {
            Debug.LogError("Invalid hexagon type or missing mesh in the array!");
            innerCellMeshFilter.mesh = null; // Fallback to no mesh
            return;
        }
        Debug.Log($"Hexagon ({x}, {y}) is set to type: {hexagonType}");
    }

    public void SetGridPosition(int x, int y)
    {
        this.x = x;
        this.y = y;
        this.name = $"Hexagon ({x}, {y}) - Color: {hexagonType}";
    }

    public int GetX()
    {
        return this.x;
    }

    public int GetY()
    {
        return this.y;
    }

    public IEnumerator DestroyHexagonCell(float duration)
    {
        float elapsed = 0f;
        Vector3 originalScale = gameObject.transform.localScale;
        MeshRenderer meshRenderer = gameObject.GetComponentInChildren<MeshRenderer>();

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;

            // Shrink hexagon
            float scaleFactor = Mathf.Lerp(1f, 0f, elapsed / duration);
            gameObject.transform.localScale = originalScale * scaleFactor;

            // Fade out
            if (meshRenderer != null)
            {
                Color color = meshRenderer.material.color;
                color.a = Mathf.Lerp(1f, 0f, elapsed / duration);
                meshRenderer.material.color = color;
            }

            yield return null;
        }

        // Ensure it's fully invisible and destroyed
        Destroy(gameObject);
    }

    public void HandleClick()
    {
        Debug.Log($"Hexagon clicked at grid position: ({x}, {y})");
        OnHexagonClicked?.Invoke(x, y);
    }
}
