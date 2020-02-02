﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Entities;

public class GameData : Singleton<GameData>
{
    public float spawnRate = 1;

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
}
