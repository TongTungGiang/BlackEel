using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BE.Mono
{
    public class FollowPathway : MonoBehaviour
    {
        private int m_CurrentIndex = 0;
        private Vector3 m_CurrentTarget;

        public Transform WaypointParent;

        private void Start()
        {
            OnReachTarget();
        }

        private void Update()
        {
            Vector3 direction = m_CurrentTarget - transform.position;
            float moveStep = GameData.Instance.agentMoveSpeed * Time.deltaTime;
            if (direction.magnitude < moveStep)
            {
                transform.position = m_CurrentTarget;
                OnReachTarget();
            }
            else
            {
                transform.position += direction.normalized * moveStep;
            }

            transform.forward = direction.normalized;
        }

        private void OnReachTarget()
        {
            if (m_CurrentIndex >= WaypointParent.childCount - 1)
            {
                return;
            }

            m_CurrentIndex++;
            m_CurrentTarget = WaypointParent.GetChild(m_CurrentIndex).position + new Vector3(
                Random.Range(-GameData.Instance.spawnPositionNoise, GameData.Instance.spawnPositionNoise),
                0,
                Random.Range(-GameData.Instance.spawnPositionNoise, GameData.Instance.spawnPositionNoise));
        }
    }
}
