using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Entities;

public class GameData : Singleton<GameData>
{
    public float spawnRate = 1;

    private Entity m_AgentEntityPrefab = Entity.Null;

    public void RegisterAgentEntityPrefab(Entity entityPrefab)
    {
        if (m_AgentEntityPrefab == Entity.Null)
            m_AgentEntityPrefab = entityPrefab;
    }

    public Entity AgentEntityPrefab { get { return m_AgentEntityPrefab; } }
}
