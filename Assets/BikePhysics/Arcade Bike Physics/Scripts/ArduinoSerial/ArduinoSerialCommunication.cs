using System.IO.Ports;
using System.Threading;
using UnityEngine;

public class ArduinoSerialCommunication : MonoBehaviour
{
    [Tooltip("Port name with which the SerialPort object will be created.")]
    public string portName = "COM14";
    public int baudRate = 115200;

    public ArcadeBP.ArcadeBikeController bikeController;
    private SerialPort serialPort;
    // Set the correct COM port and baud rate
    [HideInInspector]
    public float current_time = 0f;
    public bool targetReached = false;
    public float currentAngle = 0f, targetAngle = 5f;
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
        serialPort.ReadTimeout = 50;  // Timeout for serial reads
    }

    void Update()
    {
        lock(serialLock){
            //데이터를 보내는 부분
            if(Time.realtimeSinceStartup*1000 - lastCommandTime >= commandInterval && serialPort.IsOpen){
                currentAngle = bikeController.rollInput;
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
}
