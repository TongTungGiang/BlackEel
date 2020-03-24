using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Entities;

public class GameData : Singleton<GameData>
{
    public float spawnRate = 1;
    public float spawnRateNoise = 0.5f;
    public float spawnPositionNoise = 10;

    public int allySpawnBatchCountMin = 5;
    public int allySpawnBatchCountMax = 5;
    public int enemySpawnBatchCountMin = 1;
    public int enemySpawnBatchCountMax = 1;

    private Entity m_EnemyEntityPrefab = Entity.Null;

    public void RegisterEnemyEntityPrefab(Entity entityPrefab)
    {
        if (m_EnemyEntityPrefab == Entity.Null)
            m_EnemyEntityPrefab = entityPrefab;
    }

    public Entity EnemyEntityPrefab { get { return m_EnemyEntityPrefab; } }

    private Entity m_AllyEntityPrefab = Entity.Null;

    public void RegisterAllyEntityPrefab(Entity entityPrefab)
    {
        if (m_AllyEntityPrefab == Entity.Null)
            m_AllyEntityPrefab = entityPrefab;
    }

    public Entity AllyEntityPrefab { get { return m_AllyEntityPrefab; } }

    public float agentMoveSpeed;
    public float agentScanRadius;
    public float agentStoppingDistance;
    public float agentAttackRate;
    public float agentDamage;
    public float agentInitialHealth;
}
