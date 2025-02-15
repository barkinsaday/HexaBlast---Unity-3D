using UnityEngine;

public class BombClickForwarder_util : MonoBehaviour
{
    private BombCell parent;

    void Start()
    {
        parent = GetComponentInParent<BombCell>(); // Find the parent HexagonCell
    }

    void OnMouseDown()
    {
        if (parent != null)
            parent.HandleClick(); // Forward the click to HexagonCell
    }
}
