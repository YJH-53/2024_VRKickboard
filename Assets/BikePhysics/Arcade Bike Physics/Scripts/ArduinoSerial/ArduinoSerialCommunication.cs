using System.IO.Ports;
using System.Threading;
using System.Collections;
using UnityEngine;

public class ArduinoSerialCommunication : MonoBehaviour
{
    [Tooltip("Port name with which the SerialPort object will be created.")]
    public string portName = "COM15";
    public int baudRate = 115200;

    public ArcadeBP.ArcadeBikeController bikeController;
    private SerialPort serialPort;
    // Set the correct COM port and baud rate
    [HideInInspector]
    public float current_time = 0f;
    public bool targetReached = false;
    public float currentAngle = 0f, previousAngle = 0f, previousAngle2 = 0f, targetAngle = 5f, AngularVelocity = 0f;
    private float velocityAlpha = 1.0f, timeInterval = 200f;
    private float minSpeed = 500f, maxSpeed = 1700f, speedInterval = 50f;
    public int rotationSpeed = 500; //회전속도 0~1700
    [HideInInspector]
    public float commandInterval = 100f;

    // private bool isRunning = true;
    private float lastCommandTime = 0f;

    private object serialLock = new object();

    void Start()
    {
        serialPort = new SerialPort(portName, baudRate);
        serialPort.Open();
        StartCoroutine(GetRotationSpeed());
        serialPort.ReadTimeout = 50;  // Timeout for serial reads
    }

    void Update()
    {
        lock(serialLock){
            //데이터를 보내는 부분
            if(Time.realtimeSinceStartup*1000 - lastCommandTime >= commandInterval && serialPort.IsOpen){
                currentAngle = bikeController.rollInput;
                rotationSpeed = EvaluateRotationSpeed(AngularVelocity);
                string commandMessage = CheckTargetAngle(bikeController.targetDegree_scaled, currentAngle, rotationSpeed);
                // Debug.Log("Current TargetAngle: " + bikeController.targetDegree_scaled);
                // Debug.Log("Message: " + commandMessage);
                serialPort.WriteLine(commandMessage);
                lastCommandTime = Time.realtimeSinceStartup * 1000;
            }
        }
    }

    void OnApplicationQuit()
    {
        // Clean up when the application is quitting
        // isRunning = false;
        if (serialPort.IsOpen)
        {
            serialPort.Close();
        }
    }

    string CheckTargetAngle(float targetRoll, float roll, int speed){
        float error = targetRoll - roll;
        if(Mathf.Abs(error) <= 1.0f){
            targetReached = true;
            return "s 0";
        }else if(error > 0){
            return "u " + speed.ToString();
        }else{
            return "d " + speed.ToString();
        }
    }

    float NormalizeHallASensor(float sensor_value){
        //accel 최대로 가속 475, 안 누를 때 496
        return 1 - Mathf.InverseLerp(475, 496, sensor_value);
    }

    float NormalizeHallBSensor(float sensor_value){
        //brake 최대로 누를 때 510, 안 누르면 835
        if(sensor_value >= 780){
            sensor_value = 780;
        }
        return 1 - Mathf.InverseLerp(510, 780, sensor_value); //710이면 0, 490이면 1 반환
    }

    float NormalizePotentiometer(float sensor_value){
        //좌측 끝 214, 우측 최대 776
        return 2*Mathf.InverseLerp(213, 777, sensor_value) - 1;
    }

    IEnumerator GetRotationSpeed()
    {
        while (true){
            //Pause 상태인 경우 예전 각도를 0도, 아닌 경우 원래처럼 계산하여 각도 설정
            if(bikeController.isPause){
                previousAngle = 0f;
            }else{
                previousAngle = currentAngle;
            }
            AngularVelocity = (previousAngle - previousAngle2) / timeInterval;
                // Wait for 0.1 seconds before measuring again
            yield return new WaitForSeconds(timeInterval);
            previousAngle2 = previousAngle;
    
        }
    }

    public int EvaluateRotationSpeed(float angularVelocity){
        //실험을 통해 얻은 선형 관계
        float slope = 557.3571494f;
        float y_intercept = -7.995174601f;
        float speed =  Mathf.Abs(slope * angularVelocity + y_intercept);
        //최소, 최대 속도 조정 과정
        if(speed <= (minSpeed/velocityAlpha)){
            speed = (minSpeed/velocityAlpha);
        }else if(speed >= (maxSpeed/velocityAlpha)){
            speed = (maxSpeed/velocityAlpha);
        }
        float speed_quantized = speed * velocityAlpha;
        int speed_quantized_int = Mathf.RoundToInt(speed_quantized / 50f) * 50;
        return speed_quantized_int;
    }
}
