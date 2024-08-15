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
    public float hall_b_normalizedValue = 0f;
    // Invoked when a line of data is received from the serial device.
    void OnMessageArrived(string msg)
    {
        ParseSensorData(msg, out float sensorValue, out string sensorTag);
        if(sensorTag == "hall_b"){
            Debug.Log("Hall Sensor Value: " + sensorValue); 
            if(sensorValue >= 600){ //490, 710 부근의 값을 최대치로 만듦.
                sensorValue = 710;
            }else if(sensorValue <= 600){
                sensorValue = 490;
            }
            hall_b_normalizedValue = 1 - Mathf.InverseLerp(490, 710, sensorValue); //710이면 0, 490이면 1 반환
            Debug.Log("Normalized Value: " + hall_b_normalizedValue);
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

    void ParseSensorData(string data, out float sensorValue, out string sensorTag)
    {
        // Split the received data by the comma
        string[] splitData = data.Split(',');

        // Initialize the output variables
        sensorValue = 0f;
        sensorTag = string.Empty;

        // Parse the split data
        if (splitData.Length == 2)
        {
            // Parse the first part as a float
            if (float.TryParse(splitData[0], out float parsedValue))
            {
                sensorValue = parsedValue;
            }
            else
            {
                Debug.LogWarning("Failed to parse sensor value.");
            }

            // The second part is the sensor tag
            sensorTag = splitData[1];
        }
        else
        {
            Debug.LogWarning("Unexpected data format received.");
        }
    }
}
