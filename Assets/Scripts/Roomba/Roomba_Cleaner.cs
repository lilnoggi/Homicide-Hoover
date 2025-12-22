using UnityEngine;

public class Roomba_Cleaner : MonoBehaviour
{
    void Update()
    {
        // Create a Mask that includes all layers EXCEPT the Player layer
        // [The ~ symbol meant "NOT" or "Inverse"]
        int layerMask = ~LayerMask.GetMask("Player");

        // Shoot a ray DOWN from the Roomba
        RaycastHit hit;

        Vector3 startPos = transform.position + Vector3.up;
        Vector3 direction = Vector3.down;

        Debug.DrawRay(startPos, direction * 2.0f, Color.green);

        if (Physics.Raycast(startPos, direction, out hit, 2.0f, layerMask))
        {
            Debug.Log("Laser hit: " + hit.collider.name);

            // Did we hit a Paintable Floor?
            PaintableFloor floor = hit.collider.GetComponent<PaintableFloor>();

            if (floor != null)
            {
                // Send the texture coordinate (UV) to the floor to be cleaned
                floor.CleanAt(hit.textureCoord);
            }
        }
    }
}
