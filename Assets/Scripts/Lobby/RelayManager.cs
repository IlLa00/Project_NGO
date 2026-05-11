using System.Threading.Tasks;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using Unity.Networking.Transport.Relay;
using Unity.Services.Authentication;
using Unity.Services.Core;
using Unity.Services.Relay;
using Unity.Services.Relay.Models;
using UnityEngine;

namespace NGO.Lobby
{
    public class RelayManager : MonoBehaviour
    {
        public static RelayManager Instance { get; private set; }

        void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
        }

        // UGS 초기화 + 익명 로그인 (중복 호출 안전)
        async Task InitializeUGSAsync()
        {
            if (UnityServices.State == ServicesInitializationState.Initialized)
                return;

            await UnityServices.InitializeAsync();

            if (!AuthenticationService.Instance.IsSignedIn)
                await AuthenticationService.Instance.SignInAnonymouslyAsync();

            Debug.Log($"[Relay] UGS 초기화 완료 | PlayerId: {AuthenticationService.Instance.PlayerId}");
        }

        /// <summary>
        /// Host: Relay 할당 후 NGO StartHost. Join Code 반환.
        /// </summary>
        public async Task<string> CreateRelayAsync(int maxConnections = 3)
        {
            await InitializeUGSAsync();

            Allocation allocation = await RelayService.Instance.CreateAllocationAsync(maxConnections);
            string joinCode = await RelayService.Instance.GetJoinCodeAsync(allocation.AllocationId);

            Debug.Log($"[Relay] 할당 완료 | JoinCode: {joinCode}");

            var transport = NetworkManager.Singleton.GetComponent<UnityTransport>();
            transport.SetRelayServerData(new RelayServerData(allocation, "dtls"));

            NetworkManager.Singleton.StartHost();
            Debug.Log("[Relay] Host 시작됨");

            return joinCode;
        }

        /// <summary>
        /// Client: Join Code로 Relay 참가 후 NGO StartClient.
        /// </summary>
        public async Task JoinRelayAsync(string joinCode)
        {
            await InitializeUGSAsync();

            JoinAllocation joinAllocation = await RelayService.Instance.JoinAllocationAsync(joinCode);

            Debug.Log($"[Relay] 참가 완료 | JoinCode: {joinCode}");

            var transport = NetworkManager.Singleton.GetComponent<UnityTransport>();
            transport.SetRelayServerData(new RelayServerData(joinAllocation, "dtls"));

            NetworkManager.Singleton.StartClient();
            Debug.Log("[Relay] Client 시작됨");
        }
    }
}
