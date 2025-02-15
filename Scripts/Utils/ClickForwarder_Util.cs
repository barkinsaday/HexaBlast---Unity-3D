using UnityEngine;

public class HexagonClickForwarder : MonoBehaviour
{
    private HexagonCell parentHexagon;

    void Start()
    {
        parentHexagon = GetComponentInParent<HexagonCell>(); // Find the parent HexagonCell
    }

    void OnMouseDown()
    {
        if (parentHexagon != null)
        {
            parentHexagon.HandleClick(); // Forward the click to HexagonCell
        }
    }
}
