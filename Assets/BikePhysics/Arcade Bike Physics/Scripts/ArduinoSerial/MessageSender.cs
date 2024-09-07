using UnityEngine;
using System.Collections;

/**
 * Sample for reading using polling by yourself, and writing too.
 */
public class MessageSender: MonoBehaviour
{
    public ArcadeBP.ArcadeBikeController bikeController;
    public SerialController serialController;
    public MessageListener messageListener;
    [HideInInspector]
    public float current_time = 0f;
    public bool targetReached = false;
    public float currentAngle = 0f, targetAngle = 5f;
    public int rotationSpeed = 1000; //회전속도 0~1700

    // Initialization
    void Start()
    {
        serialController = GameObject.Find("SerialController").GetComponent<SerialController>();
        messageListener = GetComponent<MessageListener>();
    }

    // Executed each frame
    void Update()
    {
        currentAngle = messageListener.roll;
        string commandMessage = CheckTargetAngle(bikeController.targetDegree_scaled, currentAngle, rotationSpeed);
        Debug.Log("Message: " + commandMessage);
        serialController.SendSerialMessage(commandMessage);
        // CalculateAndSendTargetAngle();
        // if(Time.realtimeSinceStartup - current_time < 4f){
        //     // Debug.Log("Going Up!");
        //     targetAngle = 5.0f;
        // }else if(Time.realtimeSinceStartup - current_time < 8f){
        //     // Debug.Log("Going Down!");
        //     targetAngle = -5.0f;
        // }else if(Time.realtimeSinceStartup - current_time < 12f){
        //     // Debug.Log("Elevation Stop!");
        //     targetAngle = 0f;
        // }else if(Time.realtimeSinceStartup - current_time >= 12f){
        //     current_time = Time.realtimeSinceStartup;
        // }
    }

    void CalculateAndSendTargetAngle()
    {
        // Calculate the target angle using the formula: arctan(0.3 * hall_a - 0.4 * hall_b)
        float acceleration = 0.3f * messageListener.hall_a_normalizedValue - 0.4f * messageListener.hall_b_normalizedValue;
        float targetAngle = Mathf.Atan(acceleration) * Mathf.Rad2Deg; // Convert to degrees
        Debug.Log("Calculated Target Angle: " + targetAngle);

        // Send the target angle to Arduino
        SendTargetAngleToArduino(targetAngle);
    }

    void SendTargetAngleToArduino(float angle)
    {
        // Format the message to send to Arduino
        string message = $"t,{angle:F2}\n"; // 't' indicates target angle
        serialController.SendSerialMessage(message);
        Debug.Log("Sent target angle to Arduino: " + message);
    }

    //명령 string을 반환하는 함수
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
}
    
    