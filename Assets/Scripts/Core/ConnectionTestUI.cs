using Unity.Netcode;
using UnityEngine;
using NGO.Lobby;

namespace NGO.Core
{
    // 1주차 전용 임시 테스트 UI. 2주차에 UGUI 로비 화면으로 교체 예정.
    public class ConnectionTestUI : MonoBehaviour
    {
        [SerializeField] LobbyManager lobbyManager;

        string _lobbyCodeInput = "";
        string _displayCode = "";
        string _statusText = "대기 중...";
        bool _isBusy;

        void OnGUI()
        {
            GUILayout.BeginArea(new Rect(20, 20, 320, 400));
            GUILayout.Label($"상태: {_statusText}");
            GUILayout.Space(10);

            int connected = NetworkManager.Singleton != null
                ? NetworkManager.Singleton.ConnectedClients.Count
                : 0;
            GUILayout.Label($"연결된 플레이어: {connected}");
            GUILayout.Space(20);

            GUI.enabled = !_isBusy;

            // Host
            if (GUILayout.Button("Host 시작", GUILayout.Height(40)))
                _ = StartHostAsync();

            GUILayout.Space(10);

            // Client
            GUILayout.Label("Lobby 코드 입력:");
            _lobbyCodeInput = GUILayout.TextField(_lobbyCodeInput, GUILayout.Height(30));

            if (GUILayout.Button("Client 참가", GUILayout.Height(40)))
                _ = StartClientAsync();

            GUILayout.Space(10);

            // 생성된 Lobby 코드 표시
            if (!string.IsNullOrEmpty(_displayCode))
            {
                GUILayout.Label($"내 Lobby 코드:\n{_displayCode}");
            }

            GUI.enabled = true;
            GUILayout.EndArea();
        }

        async System.Threading.Tasks.Task StartHostAsync()
        {
            _isBusy = true;
            _statusText = "Relay + Lobby 생성 중...";

            try
            {
                _displayCode = await lobbyManager.CreateLobbyAsync("TestRoom");
                _statusText = "Host 연결 완료";
            }
            catch (System.Exception e)
            {
                _statusText = $"오류: {e.Message}";
                Debug.LogException(e);
            }
            finally
            {
                _isBusy = false;
            }
        }

        async System.Threading.Tasks.Task StartClientAsync()
        {
            if (string.IsNullOrWhiteSpace(_lobbyCodeInput))
            {
                _statusText = "Lobby 코드를 입력하세요";
                return;
            }

            _isBusy = true;
            _statusText = "Lobby 참가 중...";

            try
            {
                await lobbyManager.JoinLobbyByCodeAsync(_lobbyCodeInput.Trim().ToUpper());
                _statusText = "Client 연결 완료";
            }
            catch (System.Exception e)
            {
                _statusText = $"오류: {e.Message}";
                Debug.LogException(e);
            }
            finally
            {
                _isBusy = false;
            }
        }
    }
}
