using UnityEngine;

public enum BiomeType { Plains, Lake, Mountain, Volcano }

public class GenerateTerrainChunk : MonoBehaviour
{
    private int width, depth;
    private float heightMultiplier, heightNoiseScale, biomeNoiseScale;
    private int octaves;
    private float persistence, lacunarity;
    private AnimationCurve heightCurve;
    private int plainsPercent, lakePercent, mountainPercent, volcanoPercent;
    private Vector2 globalOffset;

    private float plainsThreshold, lakeThreshold, mountainThreshold;
    private Vector2 biomeNoiseOffset, heightNoiseOffset;
    private Vector2 volcanoCenter;

    private Mesh mesh;
    private Vector3[] vertices;
    private int[] triangles;
    private Color[] colors;

    public void Initialize(int size, float heightMult, float heightScale, float biomeScale,
        int oct, float pers, float lac, AnimationCurve curve,
        int plains, int lake, int mountain, int volcano, Vector2 offset)
    {
        width = size;
        depth = size;
        heightMultiplier = heightMult;
        heightNoiseScale = heightScale;
        biomeNoiseScale = biomeScale;
        octaves = oct;
        persistence = pers;
        lacunarity = lac;
        heightCurve = curve;
        plainsPercent = plains;
        lakePercent = lake;
        mountainPercent = mountain;
        volcanoPercent = volcano;
        globalOffset = offset;

        biomeNoiseOffset = new Vector2(Random.Range(0f, 1000f), Random.Range(0f, 1000f));
        heightNoiseOffset = new Vector2(Random.Range(0f, 1000f), Random.Range(0f, 1000f));
        volcanoCenter = new Vector2(Random.Range(width * 0.2f, width * 0.8f), Random.Range(depth * 0.2f, depth * 0.8f));
        CalculateBiomeThresholds();
        GenerateMesh();
    }

    void CalculateBiomeThresholds()
    {
        float total = plainsPercent + lakePercent + mountainPercent + volcanoPercent;
        plainsThreshold = plainsPercent / total;
        lakeThreshold = plainsThreshold + lakePercent / total;
        mountainThreshold = lakeThreshold + mountainPercent / total;
    }

    void GenerateMesh()
    {
        mesh = new Mesh();
        GetComponent<MeshFilter>().mesh = mesh;

        vertices = new Vector3[(width + 1) * (depth + 1)];
        colors = new Color[vertices.Length];

        for (int z = 0, i = 0; z <= depth; z++)
        {
            for (int x = 0; x <= width; x++, i++)
            {
                int worldX = x + (int)globalOffset.x;
                int worldZ = z + (int)globalOffset.y;

                BiomeType biome = GetBiomeType(worldX, worldZ);
                float y = GetHeightForBiome(biome, worldX, worldZ);
                vertices[i] = new Vector3(x, y, z);
                colors[i] = GetColorForBiome(biome);
            }
        }

        triangles = new int[width * depth * 6];
        int vert = 0;
        int tris = 0;

        for (int z = 0; z < depth; z++)
        {
            for (int x = 0; x < width; x++)
            {
                triangles[tris + 0] = vert + 0;
                triangles[tris + 1] = vert + width + 1;
                triangles[tris + 2] = vert + 1;
                triangles[tris + 3] = vert + 1;
                triangles[tris + 4] = vert + width + 1;
                triangles[tris + 5] = vert + width + 2;

                vert++;
                tris += 6;
            }
            vert++;
        }

        mesh.vertices = vertices;
        mesh.triangles = triangles;
        mesh.colors = colors;
        mesh.RecalculateNormals();
        mesh.RecalculateBounds();

        MeshCollider meshCollider = GetComponent<MeshCollider>();
        meshCollider.sharedMesh = null;
        meshCollider.sharedMesh = mesh;
    }

    BiomeType GetBiomeType(int x, int z)
    {
        float biomeNoise = Mathf.PerlinNoise(
            (x + biomeNoiseOffset.x) / biomeNoiseScale,
            (z + biomeNoiseOffset.y) / biomeNoiseScale
        );

        if (biomeNoise < plainsThreshold) return BiomeType.Plains;
        else if (biomeNoise < lakeThreshold) return BiomeType.Lake;
        else if (biomeNoise < mountainThreshold) return BiomeType.Mountain;
        else return BiomeType.Volcano;
    }

    float GetHeightForBiome(BiomeType biome, int x, int z)
    {
        float nx = (x + heightNoiseOffset.x) / heightNoiseScale;
        float nz = (z + heightNoiseOffset.y) / heightNoiseScale;

        switch (biome)
        {
            case BiomeType.Plains:
                return Mathf.PerlinNoise(nx, nz) * heightMultiplier * 0.3f;
            case BiomeType.Lake:
                return Mathf.PerlinNoise(nx, nz) * heightMultiplier * 0.1f;
            case BiomeType.Mountain:
                return heightCurve.Evaluate(GetFractalNoise(nx, nz)) * heightMultiplier * 1.2f;
            case BiomeType.Volcano:
                float dist = Vector2.Distance(new Vector2(x, z), volcanoCenter + globalOffset);
                float crater = Mathf.Clamp01(1f - dist / 20f);
                float baseNoise = GetFractalNoise(nx, nz);
                return heightCurve.Evaluate(baseNoise + crater) * heightMultiplier * 1.5f;
            default:
                return 0f;
        }
    }

    float GetFractalNoise(float x, float z)
    {
        float total = 0f, frequency = 9f, amplitude = 1f, maxValue = 0f;
        for (int i = 0; i < octaves; i++)
        {
            total += Mathf.PerlinNoise(x * frequency, z * frequency) * amplitude;
            maxValue += amplitude;
            amplitude *= persistence;
            frequency *= lacunarity;
        }
        return total / maxValue;
    }

    Color GetColorForBiome(BiomeType biome)
    {
        switch (biome)
        {
            case BiomeType.Plains: return Color.green;
            case BiomeType.Lake: return Color.cyan;
            case BiomeType.Mountain: return Color.gray;
            case BiomeType.Volcano: return new Color(0.5f, 0f, 0f);
            default: return Color.white;
        }
    }
}
