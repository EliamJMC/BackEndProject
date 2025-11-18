using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class EnemySpawner : MonoBehaviour
{
    [Header("Prefab & Limits")]
    public GameObject enemyPrefab;
    public int maxEnemies = 10;

    [Header("Area Settings")]
    public bool useRadius = true;                           // si false => usa rectángulo (bounds)
    public float radius = 15f;                              // para area circular
    public Vector3 boxSize = new Vector3(30f, 2f, 30f);     // para area rectangular (local space)

    [Header("Placement")]
    public float minDistanceBetweenEnemies = 1.5f; // evita spawn muy cerca
    public LayerMask placementObstacleMask;        // capas que bloquean el spawn (ej: paredes)
    public bool placeOnNavMesh = true;             // intenta ajustar la posición al NavMesh
    public float navMeshSampleDistance = 2f;

    [Header("Respawn (opcional)")]
    public bool maintainMax = false;               // si true, respawnea cuando mueren
    public float respawnDelay = 3f;

    // internal
    private List<GameObject> spawned = new List<GameObject>();

    void Start()
    {
        SpawnInitial();
        if (maintainMax)
            StartCoroutine(MaintainCountCoroutine());
    }

    // Spawn inicial hasta maxEnemies
    public void SpawnInitial()
    {
        int attempts = 0;
        int spawnedCount = 0;
        while (spawnedCount < maxEnemies && attempts < maxEnemies * 10)
        {
            Vector3 pos = GetRandomPointInArea();
            if (TryGetValidSpawnPosition(pos, out Vector3 validPos))
            {
                GameObject go = Instantiate(enemyPrefab, validPos, Quaternion.identity);
                spawned.Add(go);
                spawnedCount++;
            }
            attempts++;
        }
    }

    // Devuelve una posición aleatoria dentro del área (world space)
    Vector3 GetRandomPointInArea()
    {
        if (useRadius)
        {
            Vector2 circle = Random.insideUnitCircle * radius;
            Vector3 world = transform.position + new Vector3(circle.x, 0f, circle.y);
            return world;
        }
        else
        {
            Vector3 half = boxSize * 0.5f;
            Vector3 local = new Vector3(
                Random.Range(-half.x, half.x),
                Random.Range(-half.y, half.y),
                Random.Range(-half.z, half.z)
            );
            return transform.TransformPoint(local);
        }
    }

    // Valida la posición: no esté dentro de un obstáculo y no muy cerca de otros enemigos.
    bool TryGetValidSpawnPosition(Vector3 tryPos, out Vector3 result)
    {
        result = tryPos;

        // opcional: ajustar al NavMesh
        if (placeOnNavMesh)
        {
            NavMeshHit hit;
            if (NavMesh.SamplePosition(tryPos, out hit, navMeshSampleDistance, NavMesh.AllAreas))
            {
                result = hit.position;
            }
            else
            {
                return false; // no hay NavMesh cerca
            }
        }

        // chequea colisiones/obstáculos cercanos (paredes, etc)
        Collider[] hits = Physics.OverlapSphere(result, minDistanceBetweenEnemies * 0.5f, placementObstacleMask);
        if (hits.Length > 0)
            return false;

        // evita que quede muy cerca de otros spawns
        foreach (var e in spawned)
        {
            if (e == null) continue;
            if (Vector3.Distance(e.transform.position, result) < minDistanceBetweenEnemies)
                return false;
        }

        return true;
    }

    // Mantener siempre maxEnemies vivos (respawn si mueren)
    IEnumerator MaintainCountCoroutine()
    {
        while (true)
        {
            // limpia lista de nulos
            spawned.RemoveAll(x => x == null);

            if (spawned.Count < maxEnemies)
            {
                int need = maxEnemies - spawned.Count;
                for (int i = 0; i < need; i++)
                {
                    yield return new WaitForSeconds(respawnDelay);
                    int attempts = 0;
                    bool spawnedOne = false;
                    while (!spawnedOne && attempts < 20)
                    {
                        Vector3 pos = GetRandomPointInArea();
                        if (TryGetValidSpawnPosition(pos, out Vector3 validPos))
                        {
                            var go = Instantiate(enemyPrefab, validPos, Quaternion.identity);
                            spawned.Add(go);
                            spawnedOne = true;
                        }
                        attempts++;
                    }
                }
            }
            yield return new WaitForSeconds(1f);
        }
    }

    // Para debugging: dibuja el área en escena
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        if (useRadius)
        {
            Gizmos.DrawWireSphere(transform.position, radius);
        }
        else
        {
            Matrix4x4 old = Gizmos.matrix;
            Gizmos.matrix = transform.localToWorldMatrix;
            Gizmos.DrawWireCube(Vector3.zero, boxSize);
            Gizmos.matrix = old;
        }
    }
}
