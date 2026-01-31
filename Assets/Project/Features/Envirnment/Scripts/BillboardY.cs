using UnityEngine;

namespace MiniJam203
{
    public class BillboardY : MonoBehaviour
    {
        private Transform _camTransform;
        private float _zCash;

        private void Start()
        {
            _camTransform = Camera.main.transform;
            _zCash = transform.rotation.eulerAngles.z;
        }

        private void LateUpdate()
        {
            transform.LookAt(_camTransform);
            transform.rotation = Quaternion.Euler(0f, transform.rotation.eulerAngles.y, _zCash);
        }
    }
}