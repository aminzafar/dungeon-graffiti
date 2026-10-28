using UnityEngine;

[CreateAssetMenu(fileName="New Spray Paint", menuName="Inventory/Spray Paint")]
public class SprayPaintData : ItemData
{
    public Texture brushTexture;
    public Color paintColor = Color.red;
    public float brushSize = 30f;
    public float[] brushSizes = {10f, 20f, 30f, 50f, 70f};
    private int currentBrushSizeIndex = 2;
    public float sprayRange = 5f;
    
    public float canvasSize = 320f; // gridSize * tileSpacing. change in shader graph asw

    private Material paintMaterial;

    public override void PrimaryActionHeld(InventoryManager inventoryManager)
    {
        if (Physics.Raycast(inventoryManager.playerCamera.transform.position, inventoryManager.playerCamera.transform.forward, out RaycastHit hit, sprayRange))
        {
            Renderer renderer = hit.collider.GetComponent<Renderer>();
            if (renderer == null) return;

            if (!renderer.material.HasProperty("_PaintMap")) return;
            
            RenderTexture paintMap = renderer.material.GetTexture("_PaintMap") as RenderTexture;
            if (paintMap == null) return;

            Vector3 absNormal = new Vector3(Mathf.Abs(hit.normal.x), Mathf.Abs(hit.normal.y), Mathf.Abs(hit.normal.z));

            float u, v;

            if (absNormal.y >= absNormal.x && absNormal.y >= absNormal.z)
            {
                u = hit.point.x;
                v = hit.point.z;
            }
            else if (absNormal.z >= absNormal.x && absNormal.z >= absNormal.y)
            {
                u = hit.point.x;
                v = hit.point.y;
            }
            else 
            {
                u = hit.point.z;
                v = hit.point.y;
            }

            u = Mathf.Repeat(u / canvasSize, 1f);
            v = Mathf.Repeat(v / canvasSize, 1f);

            float pixelX = u * paintMap.width;
            float pixelY = v * paintMap.height;

            DrawOntoTexture(paintMap, pixelX, pixelY);
        }
    }

    public override void SecondaryAction(InventoryManager inventoryManager)
    {
        currentBrushSizeIndex++;

        if (currentBrushSizeIndex >= brushSizes.Length)
        {
            currentBrushSizeIndex = 0;
        }

        brushSize = brushSizes[currentBrushSizeIndex];

        Debug.Log($"Brush Size changed to {brushSize}");
    }

    private void DrawOntoTexture(RenderTexture rt, float x, float y)
    {
        if (paintMaterial == null)
        {
            paintMaterial = new Material(Shader.Find("Sprites/Default"));
        }

        paintMaterial.color = paintColor;
        
        RenderTexture.active = rt;

        GL.PushMatrix();
        GL.LoadPixelMatrix(0, rt.width, rt.height, 0);

        Rect drawRect = new Rect(x - (brushSize/2), (rt.height - y) - (brushSize/2), brushSize, brushSize);

        Graphics.DrawTexture(drawRect, brushTexture, paintMaterial);

        GL.PopMatrix();
        RenderTexture.active = null;
    }
}