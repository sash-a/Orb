using UnityEngine;

    [RequireComponent(typeof(Rigidbody))]
    public class NetPlayerActions : MonoBehaviour
    {
        Vector3 velocity;
        Vector3 rotation;

        private float camRotationX;
        [SerializeField] private float camRotLimitX = 30f;
        private float currentCamRotX = 0f;

        [SerializeField] private Camera cam;


        private Rigidbody rb;

        void Start()
        {
            velocity = Vector3.zero;
            rb = GetComponent<Rigidbody>();
        }

        void FixedUpdate()
        {
            doMovement();
            doRotations();
        }

        public void move(Vector3 _velocity)
        {
            velocity = _velocity;
        }

        void doMovement()
        {
            rb.MovePosition(rb.position + velocity * Time.fixedDeltaTime);
        }

        public void rotate(Vector3 _rotation, float _camRot)
        {
            rotation = _rotation;
            camRotationX = _camRot;
        }

        void doRotations()
        {
            rb.MoveRotation(rb.rotation * Quaternion.Euler(rotation));

            currentCamRotX -= camRotationX;
            currentCamRotX = Mathf.Clamp(currentCamRotX, -camRotLimitX, camRotLimitX);
            cam.transform.localEulerAngles = new Vector3(currentCamRotX, 0, 0);
        }
    }
