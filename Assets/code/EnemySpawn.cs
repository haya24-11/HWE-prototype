using UnityEngine;

public class EnemySpawner : MonoBehaviour
{
    [Header("Enemy Settings")]
    public GameObject enemyPrefab;      // 出現させる敵のPrefab
    public int maxEnemies = 10;         // 最大出現数
    public float spawnInterval = 2f;    // 敵を出現させる間隔（秒）

    [Header("Spawn Area")]
    public Vector2 spawnMin; // 出現範囲の最小座標
    public Vector2 spawnMax; // 出現範囲の最大座標

    [Header("Player Safe Area")]
    public float noSpawnRadius = 3.0f;  // ⭐ 플레이어 주변 스폰 금지 반경

    private float timer = 0f;
    private int currentEnemyCount = 0;
    private Transform player;

    void Start()
    {
        GameObject p = GameObject.FindGameObjectWithTag("Player");
        if (p != null)
            player = p.transform;
    }

    void Update()
    {
        timer += Time.deltaTime;

        if (timer >= spawnInterval && currentEnemyCount < maxEnemies)
        {
            SpawnEnemy();
            timer = 0f;
        }
    }

    void SpawnEnemy()
    {
        Vector3 spawnPos = Vector3.zero;
        bool validPos = false;

        // ⭐ 플레이어와 일정 거리 이상 떨어진 위치 찾기
        for (int i = 0; i < 20; i++) // 무한루프 방지
        {
            float x = Random.Range(spawnMin.x, spawnMax.x);
            float y = Random.Range(spawnMin.y, spawnMax.y);
            spawnPos = new Vector3(x, y, 0f);

            if (player == null ||
                Vector2.Distance(spawnPos, player.position) >= noSpawnRadius)
            {
                validPos = true;
                break;
            }
        }

        if (!validPos) return;

        GameObject enemy = Instantiate(enemyPrefab, spawnPos, Quaternion.identity);
        currentEnemyCount++;

        enemy.AddComponent<DestroyHandler>().spawner = this;
    }

    public void OnEnemyDestroyed()
    {
        currentEnemyCount--;
    }

    // ===== デバッグ表示（選択時）=====
    void OnDrawGizmosSelected()
    {
        GameObject p = GameObject.FindGameObjectWithTag("Player");
        if (p == null) return;

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(p.transform.position, noSpawnRadius);
    }
}

// 敵破壊時にSpawnerに通知するためのクラス
public class DestroyHandler : MonoBehaviour
{
    [HideInInspector] public EnemySpawner spawner;

    void OnDestroy()
    {
        if (spawner != null)
            spawner.OnEnemyDestroyed();
    }
}
