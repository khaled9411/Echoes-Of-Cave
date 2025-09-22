using UnityEngine;

[CreateAssetMenu(fileName = "RopeMaterialSetup", menuName = "Cave Puzzle/Rope Material Setup")]
public class RopeMaterialSetup : ScriptableObject
{
    [Header("Material Settings")]
    public Color ropeColor = new Color(0.4f, 0.3f, 0.2f, 1f);
    public Texture2D ropeTexture;
    public float metallic = 0f;
    public float smoothness = 0.2f;

    [Header("Animation Settings")]
    public bool animateTexture = true;
    public Vector2 textureScrollSpeed = new Vector2(0, -0.5f);

    public Material CreateRopeMaterial()
    {
        Material ropeMat = new Material(Shader.Find("Standard"));
        ropeMat.name = "RopeMaterial";

        ropeMat.color = ropeColor;
        ropeMat.SetFloat("_Metallic", metallic);
        ropeMat.SetFloat("_Glossiness", smoothness);

        if (ropeTexture != null)
        {
            ropeMat.mainTexture = ropeTexture;
        }

        return ropeMat;
    }
}