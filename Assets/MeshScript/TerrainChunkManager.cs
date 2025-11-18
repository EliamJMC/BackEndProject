using UnityEngine;

public class TerrainChunkManager : MonoBehaviour
{
    [Header("Configuración de chunks")]
    public int chunkSize = 50;
    public int chunksX = 4;
    public int chunksZ = 4;
    public GameObject chunkPrefab;

    [Header("Parámetros globales")]
    public float heightMultiplier = 10f;
    public float heightNoiseScale = 20f;
    public float biomeNoiseScale = 30f;
    public int octaves = 4;
    public float persistence = 0.5f;
    public float lacunarity = 2f;
    public AnimationCurve heightCurve;

    [Header("Porcentaje de biomas")]
    [Range(0, 100)] public int plainsPercent = 40;
    [Range(0, 100)] public int lakePercent = 20;
    [Range(0, 100)] public int mountainPercent = 30;
    [Range(0, 100)] public int volcanoPercent = 10;

    void Start()
    {
        for (int z = 0; z < chunksZ; z++)
        {
            for (int x = 0; x < chunksX; x++)
            {
                Vector3 position = new Vector3(x * chunkSize, 0, z * chunkSize);
                GameObject chunk = Instantiate(chunkPrefab, position, Quaternion.identity, transform);
                chunk.name = $"Chunk_{x}_{z}";

                var terrain = chunk.GetComponent<GenerateTerrainChunk>();
                terrain.Initialize(
                    chunkSize,
                    heightMultiplier,
                    heightNoiseScale,
                    biomeNoiseScale,
                    octaves,
                    persistence,
                    lacunarity,
                    heightCurve,
                    plainsPercent,
                    lakePercent,
                    mountainPercent,
                    volcanoPercent,
                    new Vector2(x * chunkSize, z * chunkSize) // offset global
                );
            }
        }
    }
}