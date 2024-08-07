/*
 * This script handles user input for IP addresses in a Unity application.
 * It interacts with a TMP_InputField to allow users to enter a new IP address for network communication.
 * The script validates the IP address format and updates the NetworkingClient's host address if the input is valid.
 * It also displays the current IP address using a TMP_Text element.
 *
 * Functionality:
 * - Listens for changes in a TMP_InputField to capture new IP address input.
 * - Validates the format of the entered IP address.
 * - Updates the NetworkingClient's host address when a valid IP is entered.
 * - Displays the current IP address in a TMP_Text UI element.
 */

using TMPro;
using UnityEngine;

public class IPInputHandler : MonoBehaviour
{
    [SerializeField] private TMP_InputField ipInputField;
    [SerializeField] private TMP_Text currentIPText;
    [SerializeField] private NetworkingClient networkingClient;

    void Start()
    {
        UpdateCurrentIPText(networkingClient.host);
        ipInputField.onEndEdit.AddListener(OnIPInputChanged);
    }

    private void OnIPInputChanged(string newIP)
    {
        if (IsValidIPAddress(newIP))
        {
            networkingClient.UpdateHost(newIP);
            UpdateCurrentIPText(newIP);
        }
        else
        {
            Debug.LogError("Invalid IP address format: " + newIP);
        }
    }

    private void UpdateCurrentIPText(string ip)
    {
        currentIPText.text = ip;
    }

    private bool IsValidIPAddress(string ip)
    {
        if (string.IsNullOrWhiteSpace(ip))
        {
            return false;
        }

        string[] parts = ip.Split('.');
        if (parts.Length != 4) return false;

        foreach (string part in parts)
        {
            if (!int.TryParse(part, out int num) || num < 0 || num > 255)
            {
                return false;
            }
        }

        return true;
    }
}
