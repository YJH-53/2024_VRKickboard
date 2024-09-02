using UnityEngine;
using System.Collections;

/**
 * Sample for reading using polling by yourself, and writing too.
 */
public class MessageSender: MonoBehaviour
{
    public SerialController serialController;
    public MessageListener messageListener;
    [HideInInspector]
    public float current_time = 0f;

    // Initialization
    void Start()
    {
        serialController = GameObject.Find("SerialController").GetComponent<SerialController>();
        messageListener = GetComponent<MessageListener>();
    }

    // Executed each frame
    void Update()
    {
        // CalculateAndSendTargetAngle();
        if(Time.realtimeSinceStartup - current_time < 2f){
            Debug.Log("Going Up!");
            serialController.SendSerialMessage("a 1000\n");
        }else if(Time.realtimeSinceStartup - current_time < 4f){
            Debug.Log("Going Down!");
            serialController.SendSerialMessage("d 1500\n");
        }else if(Time.realtimeSinceStartup - current_time < 6f){
            Debug.Log("Elevation Stop!");
            serialController.SendSerialMessage("s 0\n");
        }else if(Time.realtimeSinceStartup - current_time >= 6f){
            current_time = Time.realtimeSinceStartup;
        }
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

}
    
    