using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ArcadeBP
{
    public class ArcadeBikeController : MonoBehaviour
    {
        public enum groundCheck { rayCast, sphereCaste };
        public enum MovementMode { Velocity, AngularVelocity };
        public enum TrafficMode {Road, Track, End}; //도로주행인지 인도주행인지 구분
        public MovementMode movementMode;
        public TrafficMode trafficMode;
        public groundCheck GroundCheck;
        public LayerMask drivableSurface;
        public Camera scooterCamera; //ScooterPreset의 카메라 객체

        public float MaxSpeed = 25f; // 킥보드의 최대 속도를 줄입니다.
        public float acceleration = 15f; // 가속도를 줄입니다.
        public float turn = 5f;
        public Rigidbody rb, bikeBody;
        public MessageListener messageListener;
        // public ArduinoSerialCommunication arduinoSerialCommunication;

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
        public bool isOnRoad = false, isOnBlock = false, isRedTrafficViolation = false, isGreenTrafficViolation = false, isRightDirection = false, isMoveRight = false;
        public GameObject hitObject = null;
        public GameObject parentObject = null;
        public GameObject childObject = null;
        public string groundType = null;
        public float trafficDetectionRadius = 15.0f, maxAngleForwardRoad = 20.0f, maxAngleForwardTrack = 20.0f; // ScooterCamera에서 주변 물체 감지 반지름과 신호등이 정면을 보는지를 인식하는 기준
        public float forwardVelocityThreshold = 0.3f, rightDirectionThreshold = 90.0f; //직진 여부 판단 속도
        public float waitForRedTrafficViolationRoad = 0.5f, waitForRedTrafficViolationTrack = 0.2f, waitForGreenTrafficViolation = 2.0f; //빨간 신호 진입 이후 정지 인정 시간, 즉, 0.5초 전에 정지해야 함. 
        public bool movedInRedLight = false, stoppedInGreenLight = false;
        private dynamic closestTrafficLight = null;
        private float redLightTimer = 0f, greenLightTimer = 0f;
        private bool isWaitingAtRedLight = false, isMovingAtGreenLight = false;
        //ZoneExplanation 띄우는 변수. bool 변수는 진입 여부를, count 변수는 창을 띄우는데에 사용됨. 
        [HideInInspector]
        public bool enterZone0 = false, enterZone1 = false, enterZone2 = false, enterZone3 = false, enterZone4 = false, enterZone5 = false, enterZone6 = false, enterZone7 = false, enterZone8 = false;
        public int enterZone0_Count = 0, enterZone1_Count = 0, enterZone2_Count = 0, enterZone3_Count = 0, enterZone4_Count = 0, enterZone5_Count = 0, enterZone6_Count = 0, enterZone7_Count = 0, enterZone8_Count = 0;
        public float steeringInput = 0, throttleInput = 0, brakeInput = 0, rollInput = 0;
        public bool isPause = false;
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

        public float radius, horizontalInput, verticalInput;
        private Vector3 origin;
        private Vector3 CurrentVelocity;
        public Vector3 PreviousVelocity;
        private float timeInterval = 0.3f;
        public float targetDegree = 0.0f;
        public float targetDegree_scaled = 0.0f;

        private void Start()
        {
            radius = rb.GetComponent<SphereCollider>().radius;
            if (messageListener == null)
            {
                Debug.LogError("MessageListener reference not set!");
            }
            trafficMode = TrafficMode.End; //시작은 인도주행
            if (movementMode == MovementMode.AngularVelocity)
            {
                Physics.defaultMaxAngularSpeed = 150;
            }
            rb.centerOfMass = Vector3.zero;
            rb.drag = 0.1f; // 드래그 값을 적절하게 설정합니다.
            rb.angularDrag = 0.1f; // 각 드래그 값을 적절하게 설정합니다.
            Debug.Log("ArcadeBikeController Start: Rigidbody and SphereCollider initialized.");
            //시작과 동시에 Zone0 설명 띄우기
            enterZone0 = true;
            enterZone0_Count = 1;

            CurrentVelocity = bikeBody.transform.InverseTransformDirection(bikeBody.velocity);
            PreviousVelocity = Vector3.zero;
            StartCoroutine(CalculateAcceleration());
        }

        private void Update()
        {
            //키보드 입력(센서 입력 시 주석)
            horizontalInput = Input.GetAxis("Horizontal"); // turning input
            verticalInput = Input.GetAxis("Vertical");     // acceleration input

            //센서 입력(키보드 입력 시 주석)
            if(messageListener.isReady){
                steeringInput = messageListener.handle_normalizedValue;
                throttleInput = messageListener.hall_a_normalizedValue;
                brakeInput = messageListener.hall_b_normalizedValue;
                rollInput = messageListener.roll;
            }
            // horizontalInput = steeringInput;
            // verticalInput = throttleInput;

            //Debug.Log("ArcadeBikeController Update: Horizontal Input - " + horizontalInput + ", Vertical Input - " + verticalInput);
            //빨간 신호 위반 여부 판단
            CheckRedLightViolation();
            CheckGreenLightViolation();
            Visuals();
            AudioManager();
        }

        IEnumerator CalculateAcceleration()
        {
            while (true)
            {
                if(!isPause){
                    CurrentVelocity = bikeBody.transform.InverseTransformDirection(bikeBody.velocity);
                
                    // Debug.Log("Velocity Magnitude: " + CurrentVelocity.magnitude * 3600/1000);
                    // Debug.Log("Velocity Magnitude: " + CurrentVelocity * 3600/1000);

                    Vector3 Acceleration = (CurrentVelocity - PreviousVelocity) / timeInterval;
                    float localAcceleration = Acceleration.z; // 주행방향 가속도
                    // Debug.Log(localAcceleration);
                    PreviousVelocity = CurrentVelocity;


                    targetDegree = Mathf.Atan(localAcceleration / 9.81f) * Mathf.Rad2Deg;
                    
                    if(targetDegree >= 0)
                    {
                        targetDegree_scaled = targetDegree * 4.5f / 16.69f; // arctan(0.3) = 16.69 (in degree)
                    }
                    else if (targetDegree < 0)
                    {
                        targetDegree_scaled = targetDegree * 5f / 21.80f; // arctan(0.4) = 21.80 (in degree)
                    }

                    //최대 각도 범위 내로 다시 조절
                    if(targetDegree_scaled >= 4.9f){
                        targetDegree_scaled = 4.9f;
                    }else if(targetDegree_scaled <= -4.4f){
                        targetDegree_scaled = -4.4f;
                    }

                    Debug.Log("Current Target Angle: " + targetDegree_scaled);
                    Debug.Log("Error of Angle: " + (targetDegree_scaled - rollInput));
                }
                // Wait for 0.1 seconds before measuring again
                yield return new WaitForSeconds(timeInterval);
            }
        }

        public void AudioManager()
        {
            engineSound.pitch = Mathf.Lerp(minPitch, MaxPitch, Mathf.Abs(bikeVelocity.z) / MaxSpeed);
            SkidSound.mute = !(Mathf.Abs(bikeVelocity.x) > 10 && grounded());
        }

        void FixedUpdate()
        {
            // Debug.Log("HorizontalInput: " + horizontalInput + ", VerticalInput: " + verticalInput);
            bikeVelocity = bikeBody.transform.InverseTransformDirection(bikeBody.velocity);
            //Debug.Log("ArcadeBikeController FixedUpdate: Bike Velocity - " + bikeVelocity);

            if (Mathf.Abs(bikeVelocity.x) > 0)
            {
                // frictionMaterial.dynamicFriction = frictionCurve.Evaluate(Mathf.Abs(bikeVelocity.x / 100));
                frictionMaterial.dynamicFriction = frictionCurve.Evaluate(Mathf.Abs(5.567f * bikeVelocity.x * bikeVelocity.x / 100 + 0.02704f * bikeVelocity.x / 100));

            }

            if (grounded())
            {
                float sign = Mathf.Sign(bikeVelocity.z);
                float TurnMultiplier = turnCurve.Evaluate(bikeVelocity.magnitude / MaxSpeed);

                // Calculate the desired acceleration based on verticalInput
                float g = 9.81f; // Gravity acceleration in m/s²
                float desiredAcceleration = 0;

                if (verticalInput > 0.1f || bikeVelocity.z > 1)
                {
                    bikeBody.AddTorque(Vector3.up * horizontalInput * sign * turn * 10 * TurnMultiplier);
                }
                else if (verticalInput < -0.1f || bikeVelocity.z < -1)
                {
                    bikeBody.AddTorque(Vector3.up * horizontalInput * sign * turn * 10 * TurnMultiplier);
                }

                //2.5f 곱하면 최대 속도 25km/h
                if (verticalInput >= 0)
                {
                    desiredAcceleration = 0.3f * g * verticalInput * 2.5f;
                }
                else
                {
                    desiredAcceleration = 0.4f * g * verticalInput * 2.5f;
                }

                // Velocity Mode
                if (movementMode == MovementMode.Velocity)
                {
                    if (Mathf.Abs(verticalInput) > 0.1f)
                    // if (Mathf.Abs(verticalInput) > 0.1f && messageListenerScript.hall_b_normalizedValue < 0.1f)
                    {
                        rb.velocity = Vector3.Lerp(rb.velocity, bikeBody.transform.forward * desiredAcceleration, acceleration * Time.deltaTime);
                    }
                }
                // Angular Velocity Mode
                else if (movementMode == MovementMode.AngularVelocity)
                {
                    if (Mathf.Abs(verticalInput) > 0.1f)
                    {
                        rb.angularVelocity = Vector3.Lerp(rb.angularVelocity, bikeBody.transform.right * desiredAcceleration / radius, acceleration * Time.deltaTime);
                    }
                }

                bikeBody.MoveRotation(Quaternion.Slerp(bikeBody.rotation, Quaternion.FromToRotation(bikeBody.transform.up, hit.normal) * bikeBody.transform.rotation, 0.09f));
            }
            else
            {
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
                //Debug.Log("ArcadeBikeController grounded: Using Raycast for ground check.");
                if(Physics.Raycast(rb.position, Vector3.down, out hit, maxDistance, drivableSurface)){
                    //도로와 인접하고 있는지를 판별하는 조건문, 여기에 도로에 해당하는 태그 다 추가해야 함.
                    hitObject = hit.collider.gameObject;
                    if(hitObject.transform.parent != null){
                        parentObject = hitObject.transform.parent.gameObject;
                        groundType = parentObject.tag;
                    }else{
                        parentObject = null; groundType = null;
                    }

                    if(hitObject.transform.childCount > 0){
                        if(hitObject.name.Contains("curve")){
                            rightDirectionThreshold = 105.0f;
                        }else{
                            rightDirectionThreshold = 92.5f;
                        }
                        childObject = hitObject.transform.GetChild(0).gameObject;
                        if(Vector3.Angle(scooterCamera.transform.forward, childObject.transform.right) < rightDirectionThreshold){
                            Debug.Log("AngleFront: " + Vector3.Angle(scooterCamera.transform.forward, childObject.transform.right));
                            isRightDirection = true;
                        }else{
                            isRightDirection = false;
                        }
                    }else{
                        childObject = null;
                        isRightDirection = true; //자식 객체가 없는 땅을 밟은 경우 맞는 방향으로 가고 있음
                    }
                    // Debug.Log("ArcadeBikeController FixedUpdate: Bike is grounded.");
                    // Debug.Log("Grounded On: " + zone);
                    //tag가 Road인 물체 위에 놓여 있을 때
                    if(groundType == "Road" || groundType == "road"){ 
                        // Debug.Log("ArcadeBikeController grounded: This is Road");
                        isOnRoad = true;
                        if(hitObject.name.Contains("curve")){ //커브 도로인 경우 중앙선 판별
                            if(childObject != null){
                                Vector3 transform_remove_y = transform.position;
                                transform_remove_y.y = 0;
                                if(Vector3.Distance(childObject.transform.position, transform_remove_y) < 28.0f){
                                    isMoveRight = false;
                                }else{
                                    isMoveRight = true;
                                }
                            }else{
                                isMoveRight = true;
                            }
                        }else if(!hitObject.transform.parent.parent.gameObject.name.Contains("Not") && !hitObject.name.Contains("NotCenter")){
                            if(childObject != null){
                                Vector3 transform_remove_y = transform.position;
                                transform_remove_y.y = 0;
                                if(childObject.transform.InverseTransformDirection(transform.position - childObject.transform.position).z > 0){
                                    isMoveRight = false;
                                }else{
                                    isMoveRight = true;
                                }
                            }else{
                                isMoveRight = true;
                            }
                        }else{
                            isMoveRight = true;
                        }
                    }else{
                        isOnRoad = false;
                        isMoveRight = true;
                    }
                    //tag가 Block인 물체 위에 놓여 있을 때
                    if(groundType == "Block" || groundType == "block"){ 
                        // Debug.Log("ArcadeBikeController grounded: This is Block");
                        isOnBlock = true;
                    }else{
                        isOnBlock = false;
                    }
                    return true;
                }else{
                    hitObject = null;
                    parentObject = null;
                    isOnRoad = false;
                    isMoveRight = true;
                    groundType = null;
                    isRightDirection = true; //grounded 아닌 경우에는 정주행이라고 설정
                    return false;
                }
            }
            else if (GroundCheck == groundCheck.sphereCaste)
            {
                //Debug.Log("ArcadeBikeController grounded: Using SphereCast for ground check.");
                if(Physics.SphereCast(origin, radius + 0.1f, direction, out hit, maxDistance, drivableSurface)){
                    hitObject = hit.collider.gameObject;
                    // Debug.Log("Hit: "+ hitObject.name);
                    if(hitObject.transform.parent != null){
                        parentObject = hitObject.transform.parent.gameObject;
                        groundType = parentObject.tag;
                    }else{
                        parentObject = null; groundType = null;
                    }

                    if(hitObject.transform.childCount > 0){
                        childObject = hitObject.transform.GetChild(0).gameObject;
                        if(Vector3.Angle(scooterCamera.transform.forward, childObject.transform.right) < 90){
                            // Debug.Log("AngleFront: " + Vector3.Angle(scooterCamera.transform.forward, childObject.transform.right));
                            isRightDirection = true;
                        }else{
                            isRightDirection = false;
                        }
                    }else{
                        childObject = null;
                        isRightDirection = true; //자식 객체가 없는 땅을 밟은 경우 맞는 방향으로 가고 있음
                    }
                    // Debug.Log("ArcadeBikeController FixedUpdate: Bike is grounded.");
                    // Debug.Log("Grounded On: " + zone);
                    //tag가 Road인 물체 위에 놓여 있을 때
                    if(groundType == "Road" || groundType == "road"){ 
                        // Debug.Log("ArcadeBikeController grounded: This is Road");
                        isOnRoad = true;
                        if(hitObject.name.Contains("curve")){ //커브 도로인 경우 중앙선 판별
                            if(childObject != null){
                                Vector3 transform_remove_y = transform.position;
                                transform_remove_y.y = 0;
                                if(Vector3.Distance(childObject.transform.position, transform_remove_y) < 28.0f){
                                    isMoveRight = false;
                                }else{
                                    isMoveRight = true;
                                }
                            }else{
                                isMoveRight = true;
                            }
                        }else if(!hitObject.transform.parent.parent.gameObject.name.Contains("Not") && !hitObject.name.Contains("NotCenter")){
                            if(childObject != null){
                                Vector3 transform_remove_y = transform.position;
                                transform_remove_y.y = 0;
                                if(childObject.transform.InverseTransformDirection(transform.position - childObject.transform.position).z > 0){
                                    isMoveRight = false;
                                }else{
                                    isMoveRight = true;
                                }
                            }else{
                                isMoveRight = true;
                            }
                        }else{
                            isMoveRight = true;
                        }
                    }else{
                        isOnRoad = false;
                        isMoveRight = true;
                    }
                    //tag가 Block인 물체 위에 놓여 있을 때
                    if(groundType == "Block" || groundType == "block"){ 
                        // Debug.Log("ArcadeBikeController grounded: This is Block");
                        isOnBlock = true;
                    }else{
                        isOnBlock = false;
                    }
                    return true;
                }else{
                    hitObject = null;
                    parentObject = null;
                    isOnRoad = false;
                    isMoveRight = true;
                    isRightDirection = true; //grounded 아닌 경우에는 정주행이라고 설정
                    groundType = null;
                    return false;
                }
            }else
            {
                hitObject = null;
                parentObject = null;
                isRightDirection = true; //grounded 아닌 경우에는 정주행이라고 설정
                isOnRoad = false;
                isMoveRight = true;
                groundType = null;
                return false;
            }
        }

        bool DetectTrafficLight()
        {
            closestTrafficLight = null;
            float closestDistance = Mathf.Infinity;
            float closestAngle = 0f;
            // Find all traffic light objects within detection radius
            Collider[] colliders = Physics.OverlapSphere(scooterCamera.transform.position, trafficDetectionRadius);

            foreach (Collider _collider in colliders)
            {
                dynamic trafficLight;
                if(trafficMode == TrafficMode.Road){
                    trafficLight = _collider.GetComponentInParent<TrafficLightController>();
                }else{
                    trafficLight = _collider.GetComponentInParent<TrafficPedLightController>();
                }
                
                if (trafficLight != null)
                {
                    foreach(GameObject trafficLight_gameObject in trafficLight._Signals){
                        Vector3 directionToLight = transform.forward;
                        Vector3 trafficLightNormal = trafficLight_gameObject.transform.forward;
                        Vector3 ped_position_vector = new Vector3(1.81f, -0.32f, 0.43f);
                        Vector3 positionToLight_forangle = trafficLight_gameObject.transform.position - scooterCamera.transform.position + trafficLight_gameObject.transform.TransformDirection(ped_position_vector);
                        Vector3 positionToLight = transform.InverseTransformDirection(positionToLight_forangle);
                        Vector3 positionToLight_plane = positionToLight_forangle;
                        positionToLight_plane.y = 0;
                        //신호의 법선 벡터와 scooter의 진행방향 사이 각도
                        float angleToCamera = Vector3.Angle(trafficLightNormal, -directionToLight);
                        //인도 신호 앞뒤 확인용
                        float angleWithRight = Vector3.Angle(trafficLight_gameObject.transform.right, directionToLight);
                        //화면 중앙으로부터 신호등이 벌어져 있는 각도
                        float angleFromGround = Vector3.Angle(positionToLight_plane, directionToLight);
                        float maxAngleForward;
                        if(trafficMode == TrafficMode.Road){
                            maxAngleForward = maxAngleForwardRoad;
                            if(angleToCamera >= 90.0f){
                                angleToCamera = 180.0f - angleToCamera;
                            }
                        }else{
                            maxAngleForward = maxAngleForwardTrack;
                        }
                        
                        //신호등이 정면에 놓이면서 scooter 앞에 있고, 그 중 켜져 있는 신호등 gameObject 탐지
                        if (positionToLight.z > 0 && trafficLight_gameObject.activeInHierarchy)
                        {
                            if((trafficMode == TrafficMode.Road && angleToCamera <= maxAngleForward) || (trafficMode == TrafficMode.Track && angleWithRight <= maxAngleForward && angleFromGround <= 15.0f)){
                                float distanceToLight = Vector3.Distance(scooterCamera.transform.position, trafficLight_gameObject.transform.position);
                                
                                if (distanceToLight < closestDistance)
                                {
                                    closestAngle = angleFromGround;
                                    closestDistance = positionToLight.z;
                                    closestTrafficLight = trafficLight;
                                }
                            }
                        }
                    }
                }
            }
            
            //감지되는 최소거리 신호 확인용
            // if(closestTrafficLight!= null){
            //     Debug.Log("Closest Angle: " + closestAngle);
            //     Debug.Log("Closest Traffic Light: " + closestTrafficLight.name);
            //     Debug.Log("Distance: " + closestDistance);
            // }
            //신호 인지의 기준(운전의 정지선 처럼)을 도로 신호의 경우 12.0f, 인도 신호의 경우 5.0f이 적당하다고 생각하여 그리 둠.
            if(trafficMode == TrafficMode.Road){
                return closestTrafficLight != null && closestDistance <= 12.0f;
            }else{
                return closestTrafficLight != null && closestDistance <= 8.0f;
            }
        }

        void CheckRedLightViolation()
        {
            bool isTrafficLight = DetectTrafficLight();
            float waitForRedTrafficViolation;
            if(trafficMode == TrafficMode.Road){
                waitForRedTrafficViolation = waitForRedTrafficViolationRoad;
            }else{
                waitForRedTrafficViolation = waitForRedTrafficViolationTrack;
            }
            if(isTrafficLight && closestTrafficLight.isRedLight){
                if(bikeVelocity.z < forwardVelocityThreshold){
                    isWaitingAtRedLight = true;
                    redLightTimer = 0f;
                    isRedTrafficViolation = false;
                }else{
                    isWaitingAtRedLight = false;
                }
                if(isWaitingAtRedLight){
                    redLightTimer = 0f;
                    isRedTrafficViolation = false;
                }else{
                    if(redLightTimer < 0.2f){
                        Debug.Log("Stop Right Now!");
                    }
                    redLightTimer += Time.deltaTime;
                    if(redLightTimer >= waitForRedTrafficViolation){
                        isRedTrafficViolation = true;
                        Debug.Log("ArcadeBikeController: Red Traffic Violation!");
                        isWaitingAtRedLight = true;
                    }
                }
            }else{
                isWaitingAtRedLight = true;
                redLightTimer = 0f;
                isRedTrafficViolation = false;
            }
        }

        void CheckGreenLightViolation()
        {
            bool isTrafficLight = DetectTrafficLight();
            if(isTrafficLight && closestTrafficLight.isGreenLight){
                if(bikeVelocity.z >= forwardVelocityThreshold){
                    isMovingAtGreenLight = true;
                    greenLightTimer = 0f;
                    isGreenTrafficViolation = false;
                }else{
                    isMovingAtGreenLight = false;
                }
                if(isMovingAtGreenLight){
                    greenLightTimer = 0f;
                    isGreenTrafficViolation = false;
                }else{
                    if(greenLightTimer < 1.0f){
                        Debug.Log("Move Right Now!");
                    }
                    greenLightTimer += Time.deltaTime;
                    if(greenLightTimer >= waitForGreenTrafficViolation){
                        isGreenTrafficViolation = true;
                        Debug.Log("ArcadeBikeController: Green Traffic Violation!");
                        isMovingAtGreenLight = true;
                    }
                }
            }else{
                isMovingAtGreenLight = true;
                greenLightTimer = 0f;
                isGreenTrafficViolation = false;
            }
        }

        //보이는 화면 안에 신호가 있는지 확인하는 함수
        bool IsTrafficLightInView(GameObject trafficLight)
        {
            // Get the screen point of the traffic light relative to the camera
            Vector3 screenPoint = scooterCamera.WorldToViewportPoint(trafficLight.transform.position);

            // Check if the traffic light is within the camera's field of view
            if (screenPoint.z > 0 && screenPoint.x >= 0 && screenPoint.x <= 1 && screenPoint.y >= 0 && screenPoint.y <= 1)
            {
                // Perform a raycast to ensure there are no obstacles blocking the view
                Ray ray = scooterCamera.ScreenPointToRay(scooterCamera.WorldToScreenPoint(trafficLight.transform.position));
                if (Physics.Raycast(ray, out RaycastHit hit))
                {
                    // Check if the hit object is the traffic light
                    if (hit.collider.gameObject == trafficLight)
                    {
                        return true;
                    }
                }
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
                Gizmos.color = Color.red;
                Gizmos.DrawWireSphere(scooterCamera.transform.position, trafficDetectionRadius);
            }
        }
    }
}


