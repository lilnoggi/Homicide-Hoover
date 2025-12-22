using UnityEngine;

public class Roomba_Cleaner : MonoBehaviour
{
    void Update()
    {
        // Shoot a ray DOWN from the Roomba
        RaycastHit hit;
        if (Physics.Raycast(transform.position + Vector3.up, Vector3.down, out hit, 2.0f))
        {
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
