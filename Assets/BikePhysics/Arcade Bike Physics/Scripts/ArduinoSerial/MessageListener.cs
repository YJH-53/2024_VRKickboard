/**
 * Ardity (Serial Communication for Arduino + Unity)
 * Author: Daniel Wilches <dwilches@gmail.com>
 *
 * This work is released under the Creative Commons Attributions license.
 * https://creativecommons.org/licenses/by/2.0/
 */

using UnityEngine;
using System.Collections;

/**
 * When creating your message listeners you need to implement these two methods:
 *  - OnMessageArrived
 *  - OnConnectionEvent
 */
public class MessageListener : MonoBehaviour
{
    [HideInInspector]
    public float hall_a_normalizedValue = 0f;
    public float hall_b_normalizedValue = 0f;
    public float handle_normalizedValue = 0f;
    public float roll = 0f, pitch = 0f;
    public bool isReady =false;

    // Invoked when a line of data is received from the serial device.
    void OnMessageArrived(string msg)
    {
        // msg를 쉼표로 분리하여 각 값을 리스트에 저장

        // Debug.Log("Sensor Values: " + msg);
        string[] values = msg.Split('/');
        

        if (values.Length == 5)
        {
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
                Debug.Log("Roll: " + roll);
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
            isReady = true;
        }
        else if(values.Length == 1){
            Debug.Log(msg);
            isReady = false;
        }
        else
        {
            Debug.LogWarning("Received data does not match the expected format.");
            isReady = false;
        }

    }

    // Invoked when a connect/disconnect event occurs. The parameter 'success'
    // will be 'true' upon connection, and 'false' upon disconnection or
    // failure to connect.
    void OnConnectionEvent(bool success)
    {
        if (success)
            Debug.Log("Connection established");
        else
            Debug.Log("Connection attempt failed or disconnection detected");
    }

    float NormalizeHallASensor(float sensor_value){
        //accel 최대로 가속 473-5, 안 누를 때 494-6
        if(sensor_value <= 476f){
            sensor_value = 475f;
        }else if(476f <sensor_value && sensor_value <= 480f){
            sensor_value = 480.25f;
        }else if(480f < sensor_value && sensor_value <= 485f){
            sensor_value = 485.5f;
        }else if(485f < sensor_value && sensor_value <= 490f){
            sensor_value = 490.75f;
        }else if(490f < sensor_value && sensor_value < 500f){
            sensor_value = 496.0f;
        }
        Debug.Log("Accel Value: " +  (1 - Mathf.InverseLerp(475, 496, sensor_value)));
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
        //좌측 끝 188, 우측 최대 401
        return 2*Mathf.InverseLerp(188, 401, sensor_value) - 1;
    }
}

