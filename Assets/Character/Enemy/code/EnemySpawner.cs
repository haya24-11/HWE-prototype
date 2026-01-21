using System.Collections;
using UnityEngine;

public class EnemySpawner : MonoBehaviour
{
    [Header("Enemy Settings")]
    public GameObject enemyPrefab;
    public int maxEnemies = 10;
    public float spawnInterval = 2f;

    [Header("Spawn Area (World)")]
    public Vector2 spawnMin;
    public Vector2 spawnMax;

    [Header("Player Safe Area")]
    public float noSpawnRadius = 3.0f;

    private int currentEnemyCount = 0;
    private Transform player;
    private float noSpawnRadiusSqr;

    void Start()
    {
        var p = GameObject.FindGameObjectWithTag("Player");
        if (p != null) player = p.transform;

        noSpawnRadiusSqr = noSpawnRadius * noSpawnRadius;

        StartCoroutine(SpawnRoutine());
    }

    IEnumerator SpawnRoutine()
    {
        while (true)
        {
            if (currentEnemyCount < maxEnemies)
                TrySpawnEnemy();

            yield return new WaitForSeconds(spawnInterval);
        }
    }

    void TrySpawnEnemy()
    {
        // ﾇﾃｷｹﾀﾌｾ銧｡ ｾｸｸ・ｱﾗｳﾉ ｽｺﾆ・
        for (int i = 0; i < 30; i++)
        {
            Vector3 pos = new Vector3(
                Random.Range(spawnMin.x, spawnMax.x),
                Random.Range(spawnMin.y, spawnMax.y),
                0f
            );

            if (player != null)
            {
                Vector2 diff = (Vector2)pos - (Vector2)player.position;
                if (diff.sqrMagnitude < noSpawnRadiusSqr)
                    continue; // ﾇﾃｷｹﾀﾌｾ・ﾁﾖｺｯﾀｺ ｽｺﾆ・ｱﾝﾁ・
            }

            GameObject go = Instantiate(enemyPrefab, pos, Quaternion.identity);
            currentEnemyCount++;

            // Enemyｿ｡ｰﾔ ｽｺﾆﾊ ｿｬｰ・(Enemy.csｿ｡ SetSpawnerｰ｡ ﾀﾖｾ﨨ﾟ ﾇﾔ)
            var enemy = go.GetComponent<Enemy>();
            if (enemy != null)
                enemy.SetSpawner(this);

            return;
        }

        // 30ｹ・ｽﾃｵｵﾇﾟｴﾂｵ･ｵｵ ｾﾈﾀ・・ｶｧｹｮｿ｡ ｽﾇﾆﾐﾇﾏｸ・ｱﾗｳﾉ ﾆ・
    }

    // Enemyｰ｡ ﾁﾗﾀｻ ｶｧ ﾈ｣ﾃ・
    public void OnEnemyDestroyed()
    {
        currentEnemyCount = Mathf.Max(0, currentEnemyCount - 1);
    }

#if UNITY_EDITOR
    // ｽｺﾆ・ｹ・ｧ ｽﾃｰ｢ﾈｭ(ｼｱﾅﾃ)
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Vector3 a = new Vector3(spawnMin.x, spawnMin.y, 0);
        Vector3 b = new Vector3(spawnMax.x, spawnMin.y, 0);
        Vector3 c = new Vector3(spawnMax.x, spawnMax.y, 0);
        Vector3 d = new Vector3(spawnMin.x, spawnMax.y, 0);
        Gizmos.DrawLine(a, b);
        Gizmos.DrawLine(b, c);
        Gizmos.DrawLine(c, d);
        Gizmos.DrawLine(d, a);

        if (player != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(player.position, noSpawnRadius);
        }
    }
#endif
}
