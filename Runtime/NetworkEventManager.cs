using System;
using System.Threading.Tasks;
using UnityEngine;

namespace Adeel.UnityToolbox
{
    /// <summary>
    /// Polls connectivity on an interval, verifies it with a ping, and raises
    /// <see cref="OnNetworkConnected"/> / <see cref="OnNetworkDisconnected"/> on
    /// transitions. Survives scene loads.
    /// </summary>
    /// <remarks>
    /// <see cref="UnityEngine.Ping"/> is not supported on every platform (WebGL in
    /// particular), so treat the result as a quality hint rather than a guarantee.
    /// </remarks>
    public class NetworkEventManager : SingletonPersistent<NetworkEventManager>
    {
        /// <summary>Raised when connectivity is lost.</summary>
        public static event Action OnNetworkDisconnected;

        /// <summary>Raised when connectivity is restored.</summary>
        public static event Action OnNetworkConnected;

        [Tooltip("Host used to measure round-trip time.")]
        [SerializeField] private string pingTarget = "8.8.8.8";

        [Tooltip("Seconds between automatic checks.")]
        [Min(1f)]
        [SerializeField] private float checkInterval = 5f;

        [Tooltip("Ping times above this (in milliseconds) count as a poor connection.")]
        [SerializeField] private float maxPingTime = 1000f;

        [SerializeField] private bool isOnline;

        private bool _manualCheck;
        private bool _checkingNetwork;

        private async void Start()
        {
            await Task.Delay(1000);
            CheckNetworkStatusManual();
            InvokeRepeating(nameof(CheckNetworkStatus), checkInterval, checkInterval);
        }

        /// <summary>Forces an immediate check, even if one is already running.</summary>
        public void CheckNetworkStatusManual()
        {
            _manualCheck = true;
            CheckNetworkStatus();
        }

        /// <summary>Runs a reachability + ping check. Safe to call repeatedly.</summary>
        public async void CheckNetworkStatus()
        {
            if (_checkingNetwork && !_manualCheck)
            {
                return;
            }

            _checkingNetwork = true;

            try
            {
                if (Application.internetReachability == NetworkReachability.NotReachable)
                {
                    HandleOffline();
                    return;
                }

                await CheckNetworkQuality();
            }
            finally
            {
                _checkingNetwork = false;
            }
        }

        private void HandleOffline()
        {
            if (!isOnline && !_manualCheck)
            {
                return;
            }

            Debug.Log("Network: offline");
            isOnline = false;
            _manualCheck = false;
            OnNetworkDisconnected?.Invoke();
        }

        private void HandleOnline()
        {
            if (isOnline && !_manualCheck)
            {
                return;
            }

            Debug.Log("Network: online");
            isOnline = true;
            _manualCheck = false;
            OnNetworkConnected?.Invoke();
        }

        private async Task CheckNetworkQuality()
        {
            Ping ping = new Ping(pingTarget);
            float startTime = Time.time;

            while (!ping.isDone)
            {
                if (Time.time - startTime > 2f)
                {
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
                Debug.Log($"Network: poor connection ({ping.time} ms)");
                HandleOffline();
            }

            ping.DestroyPing();
        }

        /// <summary>The last known connectivity state.</summary>
        public bool IsOnline() => isOnline;
    }
}
