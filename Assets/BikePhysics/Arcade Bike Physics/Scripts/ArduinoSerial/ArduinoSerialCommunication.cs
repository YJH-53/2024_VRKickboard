using System.IO.Ports;
using System.Threading;
using UnityEngine;

public class ArduinoSerialCommunication : MonoBehaviour
{
    [Tooltip("Port name with which the SerialPort object will be created.")]
    public string portName = "COM14";
    public int baudRate = 115200;

    private SerialPort serialPort;
    // Set the correct COM port and baud rate
    [HideInInspector]
    public float current_time = 0f;
    public bool targetReached = false;
    public float currentAngle = 0f, targetAngle = 5f;
    public int rotationSpeed = 500; //회전속도 0~1700
    [HideInInspector]
    public float hall_a_normalizedValue = 0f;
    public float hall_b_normalizedValue = 0f;
    public float handle_normalizedValue = 0f;
    public float roll = 0f, pitch = 0f;
    public float commandInterval = 100f;

    private Thread readThread;
    private bool isRunning = true;
    private float lastCommandTime = 0f;

    private string msg = "";  // Store received data from Arduino
    private object dataLock = new object();  // Lock for thread safety

    void Start()
    {
        serialPort = new SerialPort(portName, baudRate);
        serialPort.Open();
        serialPort.ReadTimeout = 50;  // Timeout for serial reads

        // Start the thread to read from Arduino
        readThread = new Thread(ReadFromArduino);
        readThread.Start();
    }

    void Update()
    {
        //데이터를 보내는 부분
        if(Time.realtimeSinceStartup*1000 - lastCommandTime >= commandInterval && serialPort.IsOpen){
            currentAngle = roll;
            string commandMessage = CheckTargetAngle(targetAngle, currentAngle, rotationSpeed);
            Debug.Log("Message: " + commandMessage);
            serialPort.WriteLine(commandMessage);
            lastCommandTime = Time.realtimeSinceStartup * 1000;
        }

        // Process data received from Arduino (in the main thread)
        lock (dataLock)
        {
            if (!string.IsNullOrEmpty(msg))
            {
                string[] values = msg.Split('/');
                

                if (values.Length == 5)
                {
                    // Debug.Log("Sensor Values: " + msg);
                    // 각 값을 순서대로 파싱하여 할당
                    if (float.TryParse(values[0], out float hall_b_value))
                    {
                        hall_b_normalizedValue = NormalizeHallBSensor(hall_b_value);
                    }

                    if (float.TryParse(values[1], out float hall_a_value))
                    {
                        hall_a_normalizedValue = NormalizeHallASensor(hall_a_value);
                    }

                    if (float.TryParse(values[2], out float rotation_x_value))
                    {
                        roll = rotation_x_value;
                        // Debug.Log("rollF: " + rotation_x_value);
                    }

                    if (float.TryParse(values[3], out float rotation_y_value))
                    {
                        pitch = rotation_y_value;
                        // Debug.Log("pitchF: " + rotation_y_value);
                    }

                    if (float.TryParse(values[4], out float potentiometer_value))
                    {
                        handle_normalizedValue = NormalizePotentiometer(potentiometer_value);
                    }
                }
                else if(values.Length == 1){
                    Debug.Log(msg);
                }
                else
                {
                    Debug.LogWarning("Received data does not match the expected format.");
                }
                msg = "";  // Clear the data after processing
            }
        }
    }

    void ReadFromArduino()
    {
        while (isRunning)
        {
            try
            {
                if(serialPort.BytesToRead > 0){
                    // Read data from Arduino
                    string data = serialPort.ReadLine();
                    lock (dataLock)
                    {
                        msg = data;  // Store received data for the main thread to process
                    }

                }
            }
            catch (System.Exception e)
            {
                Debug.LogError("Serial Read Error: " + e.Message);
            }
        }
    }

    void OnApplicationQuit()
    {
        // Clean up when the application is quitting
        isRunning = false;
        if (serialPort.IsOpen)
        {
            serialPort.Close();
        }
        if (readThread != null && readThread.IsAlive)
        {
            readThread.Join();  // Ensure the thread is stopped properly
        }
    }

    string CheckTargetAngle(float targetRoll, float roll, int speed){
        float error = targetRoll - roll;
        Debug.Log("Error of Angle: " + error);
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
        //accel 최대로 가속 465, 안 누를 때 491-492
        return 1 - Mathf.InverseLerp(465, 492, sensor_value);
    }

    float NormalizeHallBSensor(float sensor_value){
        //brake 최대로 누를 때 510, 안 누르면 835
        return 1 - Mathf.InverseLerp(490, 710, sensor_value); //710이면 0, 490이면 1 반환
    }

    float NormalizePotentiometer(float sensor_value){
        //좌측 끝 163, 우측 최대 553
        return 2*Mathf.InverseLerp(162, 555, sensor_value) - 1;
    }
}
