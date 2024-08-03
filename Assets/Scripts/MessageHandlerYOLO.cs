/*
 * This script is handles detection results received from a ML model(e.g. YOLOV8) via a network connection.
 * It listens for JSON-encoded messages containing detection data, parses the data, and updates the UI accordingly.
 * The script is intended for use in Unity projects where object detection and visualization on a canvas are required.
 *
 * Key Features:
 * - Listens for detection messages from a NetworkingClient.
 * - Parses JSON messages into a list of detections.
 * - Dynamically updates the UI with bounding boxes and class names for detected objects.
 * - Utilizes TextMeshPro for displaying class names within the detection boxes.
 * - Automatically cleans up old UI elements to ensure only the latest detections are shown.
 * - Clears bounding boxes when the network connection is disconnected.
 */

using System;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

[Serializable]
public class Detection
{
    public string class_name;
    public List<int> bbox;
}

[Serializable]
public class DetectionWrapper
{
    public List<Detection> detections;
}

public class MessageHandlerYOLO : MonoBehaviour
{
    public NetworkingClient networkingClient;  
    public GameObject imagePrefab;  
    public RectTransform canvasRectTransform;  
    public int adjustWidth = 1024;  
    public int adjustHeight = 1024;  
    private readonly List<GameObject> activeImages = new();

    private void OnEnable()
    {
        networkingClient.OnMessageReceived += HandleMessage;  
        networkingClient.OnDisconnected += HandleDisconnection; 
    }

    private void OnDisable()
    {
        networkingClient.OnMessageReceived -= HandleMessage;  
        networkingClient.OnDisconnected -= HandleDisconnection; 
    }

    private void HandleMessage(string message)
    {
        if (string.IsNullOrEmpty(message))
        {
            Debug.LogError("Received an empty message.");
            return;
        }

        try
        {
            DetectionWrapper detectionWrapper = JsonUtility.FromJson<DetectionWrapper>(message);
            if (detectionWrapper != null && detectionWrapper.detections != null)
            {
                UpdateUI(detectionWrapper.detections);
            }
            else
            {
                Debug.LogError("Parsed JSON is null or does not contain detections.");
            }
        }
        catch (ArgumentException e)
        {
            Debug.LogError("JSON parse error: " + e.Message);
        }
    }

    private void UpdateUI(List<Detection> detections)
    {
        ClearActiveImages();

        foreach (var detection in detections)
        {   
            CreateImage(detection);
        }
    }

    private void CreateImage(Detection detection)
    {
        GameObject imageGO = Instantiate(imagePrefab, canvasRectTransform);
        RectTransform rectTransform = imageGO.GetComponent<RectTransform>();

        float x = detection.bbox[0];
        float y = detection.bbox[1];
        float x2 = detection.bbox[2];
        float y2 = detection.bbox[3];

        float width = x2 - x;
        float height = y2 - y;

        Vector2 canvasSize = canvasRectTransform.sizeDelta;
        float xPos = x / adjustWidth * canvasSize.x;
        float yPos = y / adjustHeight * canvasSize.y;
        float rectWidth = width / adjustWidth * canvasSize.x;
        float rectHeight = height / adjustHeight * canvasSize.y;

        rectTransform.anchoredPosition = new Vector2(xPos, canvasSize.y - yPos - rectHeight);
        rectTransform.sizeDelta = new Vector2(rectWidth, rectHeight);

        TextMeshProUGUI textMeshPro = imageGO.GetComponentInChildren<TextMeshProUGUI>();
        if (textMeshPro != null)
        {   
            textMeshPro.text = detection.class_name;
        }
        else
        {
            Debug.LogError("TextMeshProUGUI component not found in the image prefab.");
        }

        activeImages.Add(imageGO);
    }

    private void ClearActiveImages()
    {
        foreach (var image in activeImages)
        {
            Destroy(image);
        }
        activeImages.Clear();
    }

    private void HandleDisconnection()
    {
        ClearActiveImages();
    }
}
