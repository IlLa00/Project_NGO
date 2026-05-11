using System.Collections.Generic;
using System.Threading.Tasks;
using Unity.Services.Lobbies;
using Unity.Services.Lobbies.Models;
using UnityEngine;

namespace NGO.Lobby
{
    public class LobbyManager : MonoBehaviour
    {
        // Lobby 데이터 키: Relay Join Code 저장용
        const string KEY_RELAY_JOIN_CODE = "RelayJoinCode";

        public static LobbyManager Instance { get; private set; }

        Unity.Services.Lobbies.Models.Lobby _currentLobby;
        float _heartbeatTimer;
        const float HeartbeatInterval = 15f; // Lobby는 30초마다 heartbeat 필요

        [SerializeField] RelayManager relayManager;

        void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
        }

        void Update()
        {
            // Host만 heartbeat 전송 (Lobby 자동 삭제 방지)
            if (_currentLobby == null) return;

            _heartbeatTimer += Time.deltaTime;
            if (_heartbeatTimer >= HeartbeatInterval)
            {
                _heartbeatTimer = 0f;
                _ = LobbyService.Instance.SendHeartbeatPingAsync(_currentLobby.Id);
            }
        }

        /// <summary>
        /// Host: Relay 생성 후 Lobby 생성. Lobby 코드 반환.
        /// </summary>
        public async Task<string> CreateLobbyAsync(string lobbyName, int maxPlayers = 4)
        {
            string relayJoinCode = await relayManager.CreateRelayAsync(maxPlayers - 1);

            var options = new CreateLobbyOptions
            {
                IsPrivate = false,
                Data = new Dictionary<string, DataObject>
                {
                    {
                        KEY_RELAY_JOIN_CODE,
                        new DataObject(DataObject.VisibilityOptions.Member, relayJoinCode)
                    }
                }
            };

            _currentLobby = await LobbyService.Instance.CreateLobbyAsync(lobbyName, maxPlayers, options);

            Debug.Log($"[Lobby] 생성 완료 | LobbyCode: {_currentLobby.LobbyCode} | RelayCode: {relayJoinCode}");
            return _currentLobby.LobbyCode;
        }

        /// <summary>
        /// Client: Lobby 코드로 참가 후 Relay 연결.
        /// </summary>
        public async Task JoinLobbyByCodeAsync(string lobbyCode)
        {
            _currentLobby = await LobbyService.Instance.JoinLobbyByCodeAsync(lobbyCode);

            string relayJoinCode = _currentLobby.Data[KEY_RELAY_JOIN_CODE].Value;
            Debug.Log($"[Lobby] 참가 완료 | RelayCode: {relayJoinCode}");

            await relayManager.JoinRelayAsync(relayJoinCode);
        }

        public string GetLobbyCode() => _currentLobby?.LobbyCode;
    }
}
