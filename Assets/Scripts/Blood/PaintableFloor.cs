using UnityEngine;

public class PaintableFloor : MonoBehaviour
{
    // === PAINTABLE FLOOR SCRIPT === \\
    // This script allows a floor to be painted with
    // blood and cleaned by the player or Roomba. \\

    [Header("Settings")]
    public int resolution = 512;
    public Texture2D initialSplatterMap; // Your Splatter PNG
    [Range(2, 40)]
    public int brushRadius = 15;

    private Texture2D maskTexture;
    private Material floorMaterial;

    private void Start()
    {
        floorMaterial = GetComponent<MeshRenderer>().material;
        maskTexture = new Texture2D(resolution, resolution);

        if (initialSplatterMap != null)
        {
            // RESIZE if needed to prevent errors
            if (initialSplatterMap.width != resolution)
                maskTexture.Reinitialize(initialSplatterMap.width, initialSplatterMap.height);

            // === THE FIX: SANITIZE THE PIXELS ===
            // Get original pixels
            Color[] srcPixels = initialSplatterMap.GetPixels();
            // Create array for new clean mask
            Color[] destPixels = new Color[srcPixels.Length];

            for (int i = 0; i < srcPixels.Length; i++)
            {
                // Check the Alpha (Transparency) of the splatter image
                // If it's see-through (Alpha < 0.1), make it BLACK (Wood)
                // If it's visible, make it WHITE (Blood)
                if (srcPixels[i].a < 0.1f)
                {
                    destPixels[i] = Color.black;
                }
                else
                {
                    destPixels[i] = Color.white;
                }
            }

            // Apply our clean Black/White mask
            maskTexture.SetPixels(destPixels);
        }
        else
        {
            // Fallback: Full Blood if no image
            Color[] colors = new Color[resolution * resolution];
            for (int i = 0; i < colors.Length; i++) colors[i] = Color.white;
            maskTexture.SetPixels(colors);
        }

        maskTexture.Apply();
        floorMaterial.SetTexture("_BloodMask", maskTexture);
    }

    public int CleanAt(Vector2 uvPosition)
    {
        int cleanedCount = 0;

        int x = (int)(uvPosition.x * maskTexture.width);
        int y = (int)(uvPosition.y * maskTexture.height);

        int radiusSquared = brushRadius * brushRadius;

        // --- Brush Size loop --- \\
        for (int i = -brushRadius; i <= brushRadius; i++)
        {
            for (int j = -brushRadius; j <= brushRadius; j++)
            {
                // --- CIRCULAR BRUSH CHECK --- \\
                // Only paint if the distance from center is less than radius
                if (i * i + j * j <= radiusSquared)
                {
                    int pX = x + i;
                    int pY = y + j;

                    // --- BOUNDS CHECK --- \\
                    if (pX >= 0
                        && pX < maskTexture.width
                        && pY >= 0
                        && pY < maskTexture.height)
                    {
                        Color currentPixel = maskTexture.GetPixel(pX, pY); // Get current pixel color

                        if (currentPixel.r > 0.1f)
                        {
                            // Set pixel to BLACK (clean)
                            maskTexture.SetPixel(pX, pY, Color.black);
                            cleanedCount++;
                        }
                    }
                }
            }
        }

        if (cleanedCount > 0)
        {
            maskTexture.Apply();
        }

        return cleanedCount; // Return the score
    }
}