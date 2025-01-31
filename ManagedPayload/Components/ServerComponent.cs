using MightyBrick.GhoulGrounds;
using MightyBrick.GhoulGrounds.Player;
using Sandbox;
namespace SandFox.Components
{
    internal class ServerComponent : Component, Component.INetworkListener
    {


        protected override void OnUpdate()
        {


            if (Networking.IsHost)
            {
                Log.Info("Host");
            }
            if (Networking.IsClient)
            {
                Log.Info("Client");
            }
            if (Connection.All.Count > 1)
            {
                foreach (var connection in Connection.All)
                {
                    Log.Info(connection.DisplayName);
                }
            }
            else
            {
                Log.Info("No connections");
            }


        }


        [ConCmd("start_lobby")]
        public static void StartLobby()
        {
            Log.Info("Starting lobby");
            var GameManager = Game.ActiveScene.GetAllComponents<MightyBrick.GhoulGrounds.GameManager>().FirstOrDefault();
            if (!GameManager.IsValid())
            {
                Log.Info("GameManager not found");
                return;
            }

            //GameManager.GameObject.GetOrAddComponent<ServerComponent>();

            Networking.CreateLobby(new());
            if (PlayerPawn.LocalPlayer.IsValid())
            {
                Log.Info("Local player not found");
                return;
            }
            PlayerPawn.LocalPlayer.GameObject.NetworkMode = NetworkMode.Object;
            PlayerPawn.LocalPlayer.GameObject.NetworkSpawn();
            //netHelper.PlayerPrefab = PlayerPawn.LocalPlayer.GameObject;

            if (Game.ActiveScene.IsValid())
            {
                Log.Info("Active scene is null");
                return;
            }


            GameManager.GameObject.NetworkMode = NetworkMode.Object;
            GameManager.GameObject.NetworkSpawn();

            var spawner = Game.ActiveScene.GetAllComponents<ZombieSpawner>().FirstOrDefault();
            if (spawner.IsValid())
            {
                Log.Info("ZombieSpawner not found");
                return;
            }
            spawner.GameObject.NetworkMode = NetworkMode.Object;
            spawner.GameObject.NetworkSpawn();
            spawner.ZombiePrefab.NetworkMode = NetworkMode.Object;
            Log.Info(Networking.IsActive ? "Lobby started" : "Failed to start lobby");
        }

    }
}
