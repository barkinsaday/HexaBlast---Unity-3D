using UnityEngine;

public class CameraController : MonoBehaviour
{
    [SerializeField] private Transform cylinderCenter; // The object to orbit around (CylinderGrid or an empty GameObject)
    [SerializeField] private float rotationSpeed = 5f; // Sensitivity of rotation
    [SerializeField] private LayerMask hexagonLayer; // Layer to check for hexagons

    private bool isDragging = false;
    private Vector3 lastMousePosition;

    private Vector3 initialPosition;
    private Quaternion initialRotation;

    void Start()
    {
        //Store initial camera transform
        this.initialPosition = transform.position;
        this.initialRotation = transform.rotation;
    }


    void Update()
    {
        HandleDragInput();
    }

    void HandleDragInput()
    {
        if (Input.GetMouseButtonDown(0)) // Left Click or Touch Start
        {
            lastMousePosition = Input.mousePosition;

            // Check if the click was on a hexagon
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity, hexagonLayer))
            {
                // Clicked on a hexagon, do NOT rotate
                isDragging = false;
                return;
            }

            // Clicked on empty space, allow rotation
            isDragging = true;
        }

        if (Input.GetMouseButton(0) && isDragging) // Holding Click or Dragging
        {
            Vector3 delta = Input.mousePosition - lastMousePosition; // Mouse movement
            lastMousePosition = Input.mousePosition;

            float rotationAmount = delta.x * rotationSpeed * Time.deltaTime; // Fixed rotation direction
            transform.RotateAround(cylinderCenter.position, Vector3.up, rotationAmount); // Rotate around Y-axis
        }

        if (Input.GetMouseButtonUp(0)) // Release Click or Touch End
        {
            isDragging = false;
        }
    }

    public void ResetCamera()
    {
        Debug.Log("Resetting Camera to Initial State");
        transform.position = initialPosition;
        transform.rotation = initialRotation;
    }
}
