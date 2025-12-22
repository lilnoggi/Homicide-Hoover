using UnityEngine;

public class PaintableFloor : MonoBehaviour
{
    [Header("Settings")]
    public int resolution = 512; // Higher = crisper edges, but slower
    public Color floorBaseState = Color.white; // White = Bloody, Black = Clean

    private Texture2D maskTexture; // The texture that will hold the paint data
    private Material floorMaterial; // The material of the floor to apply the texture to

    private void Start()
    {
        // 1. Get the material from the renderer
        floorMaterial = GetComponent<MeshRenderer>().material;

        // 2. Create a new blank texture in memory
        maskTexture = new Texture2D(resolution, resolution);

        // 3. Fill it with the starting colour (Blood)
        // Loop through each pixel to set the colour
        Color[] colors = new Color[resolution * resolution];
        for (int i = 0; i < colors.Length; i++)
        {
            colors[i] = floorBaseState;
        }
        maskTexture.SetPixels(colors);
        maskTexture.Apply();

        // 4. Send this new texture to the Shader
        floorMaterial.SetTexture("_BloodMask", maskTexture);
    }

    // This function is called by the Roomba
    public void CleanAt(Vector2 uvPosition)
    {
        // Convert the 0-1 UV coordinates to pixel coordinates
        int x = (int)(uvPosition.x * resolution);
        int y = (int)(uvPosition.y * resolution);

        // Paint a small 3x3 patch of black (clean) pixels
        for (int i = -3; i <= 3; i++)
        {
            for (int j = -3; j <= 3; j++)
            {
                if (x + i >= 0
                    && x + i < resolution
                    && y + j >= 0
                    && y + j < resolution)
                {
                    maskTexture.SetPixel(x + i, y + j, Color.black);
                }
            }
        }

        // Apply the changes to the texture
        maskTexture.Apply();
    }
}
