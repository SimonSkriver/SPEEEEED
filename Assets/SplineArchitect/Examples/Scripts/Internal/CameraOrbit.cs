using UnityEngine;

namespace SplineArchitect.Examples
{
    [AddComponentMenu("")]
    public class CameraOrbit : MonoBehaviour
    {
        [Header("Orbit")]
        public Transform orbitCenter;
        public float radius = 20f;
        public float height = 10f;
        public float speed = 20f;

        [Header("Look At")]
        public Transform lookAtTarget;

        private float angle;

        private void Update()
        {
            angle += speed * Time.deltaTime;

            float radians = angle * Mathf.Deg2Rad;

            Vector3 position = orbitCenter.position;
            position.x += Mathf.Cos(radians) * radius;
            position.z += Mathf.Sin(radians) * radius;
            position.y += height;

            transform.position = position;
            transform.LookAt(lookAtTarget.position);
        }
    }
}