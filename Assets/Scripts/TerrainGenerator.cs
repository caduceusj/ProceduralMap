using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Gera um terreno usando Perlin Noise, com múltiplos octaves (FBM).
/// Também demonstra onde criar objetos e entradas de cavernas.
/// Esse script é independente do MapGenerator / MeshGenerator e roda na cena externa.
/// Quando o jogador interagir com a entrada da caverna, você pode carregar a cena da caverna
/// onde o MapGenerator e o MeshGenerator serão executados.
/// </summary>
[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class TerrainGenerator : MonoBehaviour
{
    public int width = 100;
    public int height = 100;
    public float noiseScale = 0.02f;
    public int octaves = 4;
    public float persistence = 0.5f;
    public float amplitude = 10f;

    public GameObject objectPrefab;       // Prefab de objeto simples (ex: uma árvore)
    public float objectThreshold = 0.8f;  // Limite do noise para colocar o objeto

    public GameObject caveEntrancePrefab; // Prefab da entrada de caverna
    public float caveThreshold = 0.9f;    // Limite do noise para colocar a entrada

    private MeshFilter meshFilter;
    private MeshCollider meshCollider;
    private Mesh mesh;

    void Start()
    {
        meshFilter = GetComponent<MeshFilter>();
        meshCollider = GetComponent<MeshCollider>();

        GenerateTerrain();
        PlaceObjectsAndCaves();
    }

    void GenerateTerrain()
    {
        Vector3[] vertices = new Vector3[(width + 1) * (height + 1)];
        int[] triangles = new int[width * height * 6];
        Vector2[] uvs = new Vector2[vertices.Length];

        for (int z = 0; z <= height; z++)
        {
            for (int x = 0; x <= width; x++)
            {
                float y = PerlinFBM(x * noiseScale, z * noiseScale, octaves, persistence) * amplitude;
                int index = z * (width + 1) + x;
                vertices[index] = new Vector3(x, y, z);
                uvs[index] = new Vector2((float)x / width, (float)z / height);
            }
        }

        int t = 0;
        for (int z = 0; z < height; z++)
        {
            for (int x = 0; x < width; x++)
            {
                int i = z * (width + 1) + x;
                triangles[t] = i;
                triangles[t + 1] = i + width + 1;
                triangles[t + 2] = i + 1;

                triangles[t + 3] = i + 1;
                triangles[t + 4] = i + width + 1;
                triangles[t + 5] = i + width + 2;

                t += 6;
            }
        }

        mesh = new Mesh();
        mesh.vertices = vertices;
        mesh.triangles = triangles;
        mesh.uv = uvs;
        mesh.RecalculateNormals();

        meshFilter.sharedMesh = mesh;

        // Atualiza o collider sem criar novos repetidamente
        if (meshCollider == null)
            meshCollider = gameObject.AddComponent<MeshCollider>();
        else
            meshCollider.sharedMesh = null;

        meshCollider.sharedMesh = mesh;
    }

    void PlaceObjectsAndCaves()
    {
        // Exemplo simples: a cada 10 metros, verifique a posição
        // Utilize outro perlin noise ou o mesmo com outra escala
        float objNoiseScale = 0.1f;
        float caveNoiseScale = 0.05f;

        for (int z = 0; z < height; z += 10)
        {
            for (int x = 0; x < width; x += 10)
            {
                float objectNoise = Mathf.PerlinNoise(x * objNoiseScale, z * objNoiseScale);
                float caveNoise = Mathf.PerlinNoise(x * caveNoiseScale, z * caveNoiseScale);

                float terrainHeight = GetHeightAt(x, z);
                Vector3 pos = new Vector3(x, terrainHeight, z);

                if (objectNoise > objectThreshold && objectPrefab != null)
                {
                    Instantiate(objectPrefab, pos, Quaternion.identity);
                }

                if (caveNoise > caveThreshold && caveEntrancePrefab != null)
                {
                    // Cria uma entrada de caverna
                    Instantiate(caveEntrancePrefab, pos, Quaternion.identity);
                }
            }
        }
    }

    float GetHeightAt(int x, int z)
    {
        // Interpola a altura do mesh caso precise, aqui simplificado
        int index = z * (width + 1) + x;
        return mesh.vertices[index].y;
    }

    float PerlinFBM(float x, float y, int octaves, float persistence)
    {
        float total = 0f;
        float frequency = 1f;
        float amplitudeLocal = 1f;
        float maxValue = 0f;

        for (int i = 0; i < octaves; i++)
        {
            total += Mathf.PerlinNoise(x * frequency, y * frequency) * amplitudeLocal;
            maxValue += amplitudeLocal;
            amplitudeLocal *= persistence;
            frequency *= 2f;
        }

        return total / maxValue;
    }
}
