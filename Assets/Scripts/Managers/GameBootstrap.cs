using System;
using System.IO;
using Unity.NetCode;
using Unity.Networking.Transport;
using Unity.Services.Multiplayer;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Scripting;
using Object = UnityEngine.Object;

namespace Unity.Template.CompetitiveActionMultiplayer
{
    [Preserve]
    public class GameBootstrap : ClientServerBootstrap
    {
        public override bool Initialize(string defaultWorldName)
        {
#if UNITY_SERVER && !UNITY_EDITOR
            AutoConnectPort = 0; // Disable the auto-connect
                                 //Create only a local simulation world without any multiplayer and netcode system in it.
            CreateLocalWorld(defaultWorldName); // Client & Server worlds are not created in that case,
                                                // we do so conditionally (depending on what the user chooses
                                                // via the MainMenu UI).
            return true;
#else
            // When in the main menu, we deactivate the bootstrap and let the user choose their connection.
            if (SceneManager.GetActiveScene().name == GameManager.MainMenuSceneName)
            {
                // Server only RequestedPlayType will prevent CreateClientWorld so the setting is forced back to ClientAndServer.
                if (RequestedPlayType == PlayType.Server)
                {
#if UNITY_EDITOR
                    MultiplayerPlayModePreferences.RequestedPlayType = PlayType.ClientAndServer;
#else
                    throw new System.NotSupportedException("Starting from the MainMenu scene is not supported when the Playmode Tools RequestedPlayType is Server.");
#endif
                }
                AutoConnectPort = 0; // Disable the auto-connect
                                     //Create only a local simulation world without any multiplayer and netcode system in it.
                CreateLocalWorld(defaultWorldName); // Client & Server worlds are not created in that case,
                                                    // we do so conditionally (depending on what the user chooses
                                                    // via the MainMenu UI).
                return true;
            }

            // The game is using the GameBoostrap to start.
            NetworkRole role = RequestedPlayType switch
            {
                PlayType.ClientAndServer => NetworkRole.Host,
                PlayType.Client => NetworkRole.Client,
                PlayType.Server => NetworkRole.Server,
                _ => throw new ArgumentOutOfRangeException()
            };
            NetworkStreamReceiveSystem.DriverConstructor = new EntityDriverConstructor(role);

            AutoConnectPort = ConnectionSettings.DefaultServerPort;
            DefaultConnectAddress = ResolveConnectAddress(role);
            DefaultListenAddress = NetworkEndpoint.AnyIpv4;
            if (base.Initialize(defaultWorldName))
            {
                // Load the GameManager.
                if (GameManager.Instance == null)
                {
                    GameManager gameManagerPrefab = Resources.Load<GameManager>("GameManager");
                    if (gameManagerPrefab != null)
                    {
                        Object.Instantiate(gameManagerPrefab);
                    }
                    else
                    {
                        Debug.LogError(
                            "Unable to find the GameManager prefab in the Resources folder. Has it been moved?");
                        return false;
                    }
                }

                // Setup the right connection endpoint so the ClientConnectionSystem is working.
                ConnectionSettings.Instance.ConnectionEndpoint = DefaultConnectAddress.WithPort(AutoConnectPort);
                Debug.Log($"LAN bootstrap role '{role}', endpoint '{ConnectionSettings.Instance.ConnectionEndpoint}'.");
                // Resume game start from the GameManager.
                GameManager.Instance.StartFromBootstrapAsync(ServerWorld, ClientWorld);
                return true;
            }

            // We're not starting from the main menu and the GameBootstrap has been deactivated,
            // let the current scene execute with a default setup and no GameManager.
            return false;
#endif
        }

        static NetworkEndpoint ResolveConnectAddress(NetworkRole role)
        {
            if (role != NetworkRole.Client)
                return DefaultConnectAddress;

            var lanIpFile = Path.Combine(Application.dataPath, "..", "lan-ip.txt");
            Debug.Log($"Looking for LAN server IP in '{lanIpFile}'.");
            if (File.Exists(lanIpFile))
            {
                var address = File.ReadAllText(lanIpFile).Trim();
                if (!string.IsNullOrEmpty(address) && NetworkEndpoint.TryParse(address, 0, out var endpoint))
                    return endpoint;

                Debug.LogWarning($"Invalid LAN server IP in '{lanIpFile}'. Falling back to {DefaultConnectAddress}.");
            }

            return DefaultConnectAddress;
        }
    }
}
