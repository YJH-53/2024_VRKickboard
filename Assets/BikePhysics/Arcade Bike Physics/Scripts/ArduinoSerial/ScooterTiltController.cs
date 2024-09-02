using UnityEngine;

public class ScooterTiltController : MonoBehaviour
{
    public SerialController serialController; // Reference to the SerialController object
    public MessageListener messageListener;  // Reference to the MessageListener script

    private float targetAngle = 0f;

    void Start()
    {
        if (serialController == null)
        {
            serialController = FindObjectOfType<SerialController>();
        }

        if (messageListener == null)
        {
            messageListener = FindObjectOfType<MessageListener>();
        }
    }

    void Update()
    {
        // Calculate the acceleration and send the target angle to Arduino every frame
        CalculateAndSendTargetAngle();
    }

    void CalculateAndSendTargetAngle()
    {
        // Retrieve values from the MessageListener
        float hall_a_normalizedValue = messageListener.hall_a_normalizedValue;
        float hall_b_normalizedValue = messageListener.hall_b_normalizedValue;

        // Calculate the target angle based on acceleration
        float acceleration = 0.3f * hall_a_normalizedValue - 0.4f * hall_b_normalizedValue;
        targetAngle = Mathf.Atan(acceleration) * Mathf.Rad2Deg; // Convert radians to degrees
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
