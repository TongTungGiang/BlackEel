using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BE.Mono
{
    public class AgentSpawner : MonoBehaviour
    {
        [SerializeField] Transform m_WaypointParent;
        [SerializeField] Transform m_AllySpawnPointsParent;
        [SerializeField] GameObject m_EnemyPrefab;
        [SerializeField] GameObject m_AllyPrefab;

        private float m_LastSpawn;

        // Update is called once per frame
        void Update()
        {
            if (Time.time - m_LastSpawn < GameData.Instance.spawnRate)
            {
                return;
            }

            m_LastSpawn = Time.time - Random.Range(-GameData.Instance.spawnRateNoise, GameData.Instance.spawnRateNoise);
            SpawnAlly();
            SpawnEnemy();
        }

        private void SpawnAlly()
        {
            Vector3 randomSpawnPointPosition = m_AllySpawnPointsParent.GetChild(Random.Range(0, m_AllySpawnPointsParent.childCount)).position;

            int batchCount = Random.Range(GameData.Instance.allySpawnBatchCountMin, GameData.Instance.allySpawnBatchCountMax);
            for (int i = 0; i < batchCount; i++)
            {
                Vector3 instanceSpawnPos = GetNoisedPosition(randomSpawnPointPosition);

                GameObject instance = Instantiate(m_AllyPrefab, instanceSpawnPos, Quaternion.identity);
                instance.GetComponent<AttackScript>().SetupHealth(Random.Range(GameData.Instance.agentInitialHealthMin, GameData.Instance.agentInitialHealthMax));
            }

            StatDisplay.Instance.AgentCount += batchCount;
        }

        private void SpawnEnemy()
        {
            Vector3 randomSpawnPointPosition = m_WaypointParent.GetChild(0).position;

            int batchCount = Random.Range(GameData.Instance.enemySpawnBatchCountMin, GameData.Instance.enemySpawnBatchCountMax);
            for (int i = 0; i < batchCount; i++)
            {
                Vector3 instanceSpawnPos = GetNoisedPosition(randomSpawnPointPosition);

                GameObject instance = Instantiate(m_EnemyPrefab, instanceSpawnPos, Quaternion.identity);
                instance.GetComponent<FollowPathway>().WaypointParent = m_WaypointParent;
                instance.GetComponent<AttackScript>().SetupHealth(Random.Range(GameData.Instance.agentInitialHealthMin, GameData.Instance.agentInitialHealthMax));
            }

            StatDisplay.Instance.AgentCount += batchCount;
        }

        private Vector3 GetNoisedPosition(Vector3 originalPosition)
        {
            return originalPosition + new Vector3(
                Random.Range(-GameData.Instance.spawnPositionNoise, GameData.Instance.spawnPositionNoise), 
                0, 
                Random.Range(-GameData.Instance.spawnPositionNoise, GameData.Instance.spawnPositionNoise));
        }
    }
}
