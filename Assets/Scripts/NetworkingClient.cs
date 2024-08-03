/*
 * This script handles TCP network connection on client side.
 * It allows the game object to connect to a TCP server, receive data asynchronously, 
 * and update the UI to reflect the connection status.
 * 
 * Key Features:
 * - Connects to a specified TCP server using an IP address and port.
 * - Asynchronously reads incoming messages from the server.
 * - Notifies other components of received messages through events.
 * - Updates the UI to indicate connection status using a color-coded image.
 * - Automatically disconnects from the server when the application quits.
 */

using System;
using System.Collections;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

public class NetworkingClient : MonoBehaviour
{
    public string host = "192.168.68.102";
    public int port = 8080;
    private TcpClient tcpClient;
    private NetworkStream networkStream;
    private bool attemptingConnection = false;
    private Coroutine readDataCoroutine;

    public delegate void MessageReceivedHandler(string message);
    public event MessageReceivedHandler OnMessageReceived;
    public delegate void DisconnectedHandler();
    public event DisconnectedHandler OnDisconnected;
    [SerializeField] private Image connectionStatusImage;
    [SerializeField] private Color connectedColor = Color.green;
    [SerializeField] private Color disconnectedColor = Color.red;

    void Start()
    {
        UpdateConnectionStatus(false);
    }

    public async void Connect()
    {
        if (attemptingConnection || (tcpClient != null && tcpClient.Connected)) return;
        attemptingConnection = true;

        try
        {
            tcpClient = new TcpClient();
            await tcpClient.ConnectAsync(host, port);
            networkStream = tcpClient.GetStream();
            UpdateConnectionStatus(true);
            readDataCoroutine ??= StartCoroutine(ReadDataAsync());
        }
        catch (Exception e)
        {
            Debug.LogError("Failed to connect to the TCP server: " + e.Message);
            UpdateConnectionStatus(false);
        }
        finally
        {
            attemptingConnection = false;
        }
    }

    public void Disconnect()
    {
        if (tcpClient != null)
        {
            if (tcpClient.Connected)
            {
                networkStream.Close();
                tcpClient.Close();
                UpdateConnectionStatus(false);
                OnDisconnected?.Invoke();
            }

            tcpClient = null;
            networkStream = null;

            if (readDataCoroutine != null)
            {
                StopCoroutine(readDataCoroutine);
                readDataCoroutine = null;
            }
        }
    }

    IEnumerator ReadDataAsync()
    {
        byte[] buffer = new byte[4096];

        while (true)
        {
            if (tcpClient != null && tcpClient.Connected && networkStream != null)
            {
                if (networkStream.DataAvailable)
                {
                    Task<int> readTask = networkStream.ReadAsync(buffer, 0, buffer.Length);
                    yield return new WaitUntil(() => readTask.IsCompleted);

                    if (readTask.Exception == null)
                    {
                        int bytesRead = readTask.Result;
                        string message = Encoding.UTF8.GetString(buffer, 0, bytesRead);
                        if (!string.IsNullOrEmpty(message))
                        {
                            OnMessageReceived?.Invoke(message);
                        }
                    }
                    else
                    {
                        Debug.LogError(readTask.Exception.ToString());
                    }
                }
            }
            yield return null;
        }
    }

    void OnApplicationQuit()
    {
        Disconnect();
    }

    public void UpdateHost(string newHost)
    {
        host = newHost;
    }

    private void UpdateConnectionStatus(bool isConnected)
    {
        if (connectionStatusImage != null)
        {
            connectionStatusImage.color = isConnected ? connectedColor : disconnectedColor;
        }
    }
}
