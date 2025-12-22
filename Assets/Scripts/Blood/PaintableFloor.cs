using UnityEngine;

public class PaintableFloor : MonoBehaviour
{
    [Header("Settings")]
    public int resolution = 512;
    public Texture2D initialSplatterMap; // Your Splatter PNG

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

    public void CleanAt(Vector2 uvPosition)
    {
        int x = (int)(uvPosition.x * maskTexture.width);
        int y = (int)(uvPosition.y * maskTexture.height);

        // Brush Size loop
        for (int i = -5; i <= 5; i++)
        {
            for (int j = -5; j <= 5; j++)
            {
                int pX = x + i;
                int pY = y + j;

                if (pX >= 0 && pX < maskTexture.width && pY >= 0 && pY < maskTexture.height)
                {
                    // Paint Black (Clean Wood)
                    maskTexture.SetPixel(pX, pY, Color.black);
                }
            }
        }
        maskTexture.Apply();
    }
}