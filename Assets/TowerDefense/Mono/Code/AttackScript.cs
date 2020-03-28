using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BE.Mono
{
    public class AttackScript : MonoBehaviour
    {
        [SerializeField] private LayerMask m_LayerToCheck;
        [SerializeField] private float m_CurrentHealth;
        [SerializeField] private float m_MaxHealth;

        private AttackScript m_CurrentTarget;
        private Collider[] m_NearbyColliders;
        private bool m_Occupied;
        private float m_LastAttack;

        // Start is called before the first frame update
        void Start()
        {
            m_NearbyColliders = new Collider[10];
        }

        public void SetupHealth(float maxHealth)
        {
            m_CurrentHealth = m_MaxHealth = maxHealth;
        }

        // Update is called once per frame
        void Update()
        {
            if (m_CurrentHealth <= 0)
            {
                StatDisplay.Instance.AgentCount -= 1;
                Destroy(gameObject);
                return;
            }

            if (m_CurrentTarget != null)
            {
                float distance = (m_CurrentTarget.transform.position - transform.position).magnitude;
                if (distance <= GameData.Instance.agentStoppingDistance)
                {
                    Attack();
                }
                else
                {
                    MoveToTarget();
                }
            }
            else
            {
                Find();
            }
        }

        void Find()
        {
            int nearbyCount = Physics.OverlapSphereNonAlloc(transform.position, GameData.Instance.agentScanRadius, m_NearbyColliders, m_LayerToCheck);
            if (nearbyCount > 0)
            {
                float closestDistance = float.MaxValue;
                for (int i = 0; i < nearbyCount; i++)
                {
                    float distance = (m_NearbyColliders[i].transform.position - transform.position).magnitude;
                    var enemyAttackScript = m_NearbyColliders[i].GetComponent<AttackScript>();
                    if (distance < closestDistance &&
                        !enemyAttackScript.m_Occupied)
                    {
                        closestDistance = distance;
                        m_CurrentTarget = enemyAttackScript;
                        m_CurrentTarget.m_Occupied = true;
                    }
                }
            }
        }

        void MoveToTarget()
        {
            Vector3 direction = m_CurrentTarget.transform.position - transform.position;
            float moveStep = GameData.Instance.agentMoveSpeed * Time.deltaTime;
            transform.position += direction.normalized * moveStep;
        }

        void Attack()
        {
            if (Time.time - m_LastAttack > GameData.Instance.agentAttackRate)
            {
                m_LastAttack = Time.time;
                m_CurrentTarget.m_CurrentHealth -= GameData.Instance.agentDamage;
                if (m_CurrentTarget.m_CurrentHealth <= 0)
                    OnTargetDie();
            }
        }

        void OnTargetDie()
        {
            m_CurrentTarget = null;
        }
    }
}