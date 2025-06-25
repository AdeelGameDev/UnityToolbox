using System;
using System.Threading.Tasks;
using UnityEngine;

public class NetworkEventManager : SingletonPersistent<NetworkEventManager>
{
    public static event Action OnNetworkDisconnected;
    public static event Action OnNetworkConnected;

    [SerializeField] private string pingTarget = "8.8.8.8";
    [SerializeField] private float checkInterval = 5f;
    [SerializeField] private float maxPingTime = 1000f;
    [SerializeField] private bool isOnline = false;

    private bool manualCheck = false;
    private bool checkingNetwork = false;


    private async void Start()
    {
        await Task.Delay(1000);
        CheckNetworkStatusManual();
        InvokeRepeating(nameof(CheckNetworkStatus), checkInterval, checkInterval);
    }

    public void CheckNetworkStatusManual()
    {
        manualCheck = true;
        CheckNetworkStatus();
    }

    public async void CheckNetworkStatus()
    {
        if (checkingNetwork && !manualCheck)
            return; // Skip if already checking

        checkingNetwork = true;

        NetworkReachability reachability = Application.internetReachability;

        if (reachability == NetworkReachability.NotReachable)
        {
            HandleOffline();
        }
        else
        {
            await CheckNetworkQuality();
        }

        checkingNetwork = false;
    }

    private void HandleOffline()
    {
        if (isOnline || manualCheck)
        {
            Debug.Log("Offline");
            OnNetworkDisconnected?.Invoke();
            isOnline = false;
            manualCheck = false;
        }
    }

    private async Task CheckNetworkQuality()
    {
        Ping ping = new Ping(pingTarget);
        float startTime = Time.time;

        while (!ping.isDone)
        {
            if (Time.time - startTime > 2f) // Adjust timeout threshold as needed
            {
                Debug.Log("Ping timeout");
                HandleOffline();
                ping.DestroyPing();
                return;
            }
            await Task.Yield();
        }

        if (ping.time <= maxPingTime)
        {
            HandleOnline();
        }
        else
        {
            Debug.Log("Poor connection");
            HandleOffline();
        }

        ping.DestroyPing();
    }

    private void HandleOnline()
    {
        if (!isOnline || manualCheck)
        {
            Debug.Log("Online");
            OnNetworkConnected?.Invoke();
            isOnline = true;
            manualCheck = false;

        }
    }

    public bool IsOnline() => isOnline;
}


