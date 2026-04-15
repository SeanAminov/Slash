using UnityEngine;

namespace Slash
{
    // Camera that tracks the player on the X axis only. Y and Z are frozen
    // to their starting values so the view stays on a clean horizontal rail.
    public class CameraFollow : MonoBehaviour
    {
        public Transform target;
        public float smoothTime = 0.12f;
        public float offsetX = 0f;

        Vector3 _velocity;
        float _baseY;
        float _baseZ;

        void Start()
        {
            _baseY = transform.position.y;
            _baseZ = transform.position.z;
        }

        void LateUpdate()
        {
            if (target == null) return;

            Vector3 desired = new Vector3(target.position.x + offsetX, _baseY, _baseZ);
            transform.position = Vector3.SmoothDamp(transform.position, desired, ref _velocity, smoothTime);
        }
    }
}
