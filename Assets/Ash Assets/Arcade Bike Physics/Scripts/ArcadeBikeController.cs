using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ArcadeBP
{
    public class ArcadeBikeController : MonoBehaviour
    {
        public enum groundCheck { rayCast, sphereCaste };
        public enum MovementMode { Velocity, AngularVelocity };
        public MovementMode movementMode;
        public groundCheck GroundCheck;
        public LayerMask drivableSurface;

        public float MaxSpeed = 20f; // 킥보드의 최대 속도를 줄입니다.
        public float acceleration = 15f; // 가속도를 줄입니다.
        public float turn = 5f;
        public Rigidbody rb, bikeBody;

        [HideInInspector]
        public RaycastHit hit;
        public AnimationCurve frictionCurve;
        public AnimationCurve turnCurve;
        public AnimationCurve leanCurve;
        public PhysicMaterial frictionMaterial;
        [Header("Visuals")]
        public Transform BodyMesh;
        public Transform Handle;
        public Transform[] Wheels = new Transform[2];
        [HideInInspector]
        public Vector3 bikeVelocity;

        [Range(-70, 70)]
        public float BodyTilt;
        [Header("Audio settings")]
        public AudioSource engineSound;
        [Range(0, 1)]
        public float minPitch;
        [Range(1, 5)]
        public float MaxPitch;
        public AudioSource SkidSound;

        public float skidWidth;

        private float radius, horizontalInput, verticalInput;
        private Vector3 origin;

        private void Start()
        {
            radius = rb.GetComponent<SphereCollider>().radius;
            if (movementMode == MovementMode.AngularVelocity)
            {
                Physics.defaultMaxAngularSpeed = 150;
            }
            rb.centerOfMass = Vector3.zero;
            rb.drag = 0.1f; // 드래그 값을 적절하게 설정합니다.
            rb.angularDrag = 0.1f; // 각 드래그 값을 적절하게 설정합니다.
            Debug.Log("ArcadeBikeController Start: Rigidbody and SphereCollider initialized.");
        }

        private void Update()
        {
            horizontalInput = Input.GetAxis("Horizontal"); // turning input
            verticalInput = Input.GetAxis("Vertical");     // acceleration input
            Debug.Log("ArcadeBikeController Update: Horizontal Input - " + horizontalInput + ", Vertical Input - " + verticalInput);
            Visuals();
            AudioManager();
        }

        public void AudioManager()
        {
            engineSound.pitch = Mathf.Lerp(minPitch, MaxPitch, Mathf.Abs(bikeVelocity.z) / MaxSpeed);
            SkidSound.mute = !(Mathf.Abs(bikeVelocity.x) > 10 && grounded());
        }

        void FixedUpdate()
        {
            bikeVelocity = bikeBody.transform.InverseTransformDirection(bikeBody.velocity);
            Debug.Log("ArcadeBikeController FixedUpdate: Bike Velocity - " + bikeVelocity);

            if (Mathf.Abs(bikeVelocity.x) > 0)
            {
                frictionMaterial.dynamicFriction = frictionCurve.Evaluate(Mathf.Abs(bikeVelocity.x / 100));
            }

            if (grounded())
            {
                Debug.Log("ArcadeBikeController FixedUpdate: Bike is grounded.");
                float sign = Mathf.Sign(bikeVelocity.z);
                float TurnMultiplier = turnCurve.Evaluate(bikeVelocity.magnitude / MaxSpeed);
                if (verticalInput > 0.1f || bikeVelocity.z > 1)
                {
                    bikeBody.AddTorque(Vector3.up * horizontalInput * sign * turn * 10 * TurnMultiplier);
                }
                else if (verticalInput < -0.1f || bikeVelocity.z < -1)
                {
                    bikeBody.AddTorque(Vector3.up * horizontalInput * sign * turn * 10 * TurnMultiplier);
                }

                if (Input.GetAxis("Jump") > 0.1f)
                {
                    rb.constraints = RigidbodyConstraints.FreezeRotationX;
                }
                else
                {
                    rb.constraints = RigidbodyConstraints.None;
                }

                if (movementMode == MovementMode.AngularVelocity)
                {
                    if (Mathf.Abs(verticalInput) > 0.1f)
                    {
                        rb.angularVelocity = Vector3.Lerp(rb.angularVelocity, bikeBody.transform.right * verticalInput * MaxSpeed / radius, acceleration * Time.deltaTime);
                    }
                }
                else if (movementMode == MovementMode.Velocity)
                {
                    if (Mathf.Abs(verticalInput) > 0.1f && Input.GetAxis("Jump") < 0.1f)
                    {
                        rb.velocity = Vector3.Lerp(rb.velocity, bikeBody.transform.forward * verticalInput * MaxSpeed, acceleration * Time.deltaTime);
                    }
                }

                bikeBody.MoveRotation(Quaternion.Slerp(bikeBody.rotation, Quaternion.FromToRotation(bikeBody.transform.up, hit.normal) * bikeBody.transform.rotation, 0.09f));
            }
            else
            {
                Debug.Log("ArcadeBikeController FixedUpdate: Bike is not grounded.");
                bikeBody.MoveRotation(Quaternion.Slerp(bikeBody.rotation, Quaternion.FromToRotation(bikeBody.transform.up, Vector3.up) * bikeBody.transform.rotation, 0.02f));
            }
        }

        public void Visuals()
        {
            Handle.localRotation = Quaternion.Slerp(Handle.localRotation, Quaternion.Euler(Handle.localRotation.eulerAngles.x,
                                   20 * horizontalInput, Handle.localRotation.eulerAngles.z), 15f * Time.deltaTime);

            Wheels[0].localRotation = rb.transform.localRotation;
            Wheels[1].localRotation = rb.transform.localRotation;

            if (bikeVelocity.z > 1)
            {
                BodyMesh.localRotation = Quaternion.Slerp(BodyMesh.localRotation, Quaternion.Euler(0,
                                   BodyMesh.localRotation.eulerAngles.y, BodyTilt * horizontalInput * leanCurve.Evaluate(bikeVelocity.z / MaxSpeed)), 4f * Time.deltaTime);
            }
            else
            {
                BodyMesh.localRotation = Quaternion.Slerp(BodyMesh.localRotation, Quaternion.Euler(0, 0, 0), 4f * Time.deltaTime);
            }
        }

        public bool grounded()
        {
            origin = rb.position + rb.GetComponent<SphereCollider>().radius * Vector3.up;
            var direction = Vector3.down;
            var maxDistance = rb.GetComponent<SphereCollider>().radius + 0.2f;

            if (GroundCheck == groundCheck.rayCast)
            {
                Debug.Log("ArcadeBikeController grounded: Using Raycast for ground check.");
                return Physics.Raycast(rb.position, Vector3.down, out hit, maxDistance, drivableSurface);
            }
            else if (GroundCheck == groundCheck.sphereCaste)
            {
                Debug.Log("ArcadeBikeController grounded: Using SphereCast for ground check.");
                return Physics.SphereCast(origin, radius + 0.1f, direction, out hit, maxDistance, drivableSurface);
            }
            return false;
        }

        private void OnDrawGizmos()
        {
            radius = rb.GetComponent<SphereCollider>().radius;
            float width = 0.02f;
            if (!Application.isPlaying)
            {
                Gizmos.color = Color.yellow;
                Gizmos.DrawWireCube(rb.transform.position + ((radius + width) * Vector3.down), new Vector3(2 * radius, 2 * width, 4 * radius));
                if (GetComponent<BoxCollider>())
                {
                    Gizmos.color = Color.green;
                    Gizmos.DrawWireCube(transform.position, GetComponent<BoxCollider>().size);
                }
                if (GetComponent<CapsuleCollider>())
                {
                    Gizmos.color = Color.green;
                    Gizmos.DrawWireCube(transform.position, GetComponent<CapsuleCollider>().bounds.size);
                }
            }
        }
    }
}
