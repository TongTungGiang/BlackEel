using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BE
{
    [RequireComponent(typeof(Camera))]
    public class CameraMovement : MonoBehaviour
    {
        public float m_CameraMoveSpeed;
        public float m_CameraZoomSpeed;
        public float m_MinHeight;
        public float m_MaxHeight;


        private void Update()
        {
            // Move
            Vector3 movement = new Vector3(Input.GetAxis("Horizontal"), 0, Input.GetAxis("Vertical"));
            float movementRate = (transform.position.y - m_MinHeight) / (m_MaxHeight - m_MinHeight);
            transform.position += movement.normalized * m_CameraMoveSpeed * Mathf.Abs(movementRate) * Time.deltaTime;

            // Zoom
            float scrollVal = Input.GetAxis("Mouse ScrollWheel");
            if (transform.position.y >= m_MinHeight && transform.position.y <= m_MaxHeight ||
                transform.position.y < m_MinHeight && scrollVal < 0 ||
                transform.position.y > m_MaxHeight && scrollVal > 0)
            {
                transform.position += transform.forward * scrollVal * m_CameraZoomSpeed * Time.deltaTime;
            }
        }
    }
}