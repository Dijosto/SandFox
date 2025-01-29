using MightyBrick.GhoulGrounds;
using MightyBrick.GhoulGrounds.Player;
using Sandbox;
using System.Numerics;

namespace SandFox.Commands
{
    public static class PlayerCommands
    {
        [ConCmd("player_setpos")]
        public static void SetPlayerPosition(string x, string y, string z)
        {
            if (Game.ActiveScene is null)
                return;

            float ParsePosition(string input, float currentValue)
            {
                if (input.StartsWith("this") || input.StartsWith("this.") || input.StartsWith("p"))
                {
                    var offset = input.Substring(5);
                    if (string.IsNullOrEmpty(offset))
                    {
                        return currentValue;
                    }
                    else
                    {
                        if (float.TryParse(offset, out var offsetValue))
                        {
                            return currentValue + offsetValue;
                        }
                    }
                }
                else if (float.TryParse(input, out var value))
                {
                    return value;
                }
                throw new FormatException("Invalid position format");
            }

            var player = Game.ActiveScene
                .Directory
                .FindByName("player", false)
                .FirstOrDefault();

            if (player is null)
            {
                Log.Info($"player not found");
                return;
            }

            var currentPosition = player.WorldPosition;

            try
            {
                var xPos = ParsePosition(x, currentPosition.x);
                var yPos = ParsePosition(y, currentPosition.y);
                var zPos = ParsePosition(z, currentPosition.z);

                var position = new Vector3(xPos, yPos, zPos);
                player.WorldPosition = position;
                Log.Info($"Set player position to {position}");
            }
            catch (FormatException)
            {
                Log.Info("Invalid position format. Use: player_setpos x y z");
            }
        }




        [ConCmd("heal_player")]
        public static void HealPlayer(string amount)
        {
            if (!int.TryParse(amount, out var healAmount))
            {
                Log.Info("Invalid heal amount. Use: heal_player amount");
                return;
            }

            if (Game.ActiveScene is null)
                return;

            var player = PlayerPawn.LocalPlayer;
            if (!player.IsValid())
            {
                Log.Info($"player not found");
                return;
            }

            player.Health += healAmount;
            Log.Info($"Healed for {healAmount}");
        }
        [ConCmd("give_ammo")]
        public static void GiveAmmo(string amount)
        {
            if (!int.TryParse(amount, out var ammoAmount))
            {
                Log.Info("Invalid heal amount. Use: give_ammo amount");
                return;
            }

            if (Game.ActiveScene is null)
                return;

            var player = PlayerPawn.LocalPlayer;
            if (!player.IsValid())
            {
                Log.Info($"player not found");
                return;
            }

            Firearm firearm = (Firearm)PlayerPawn.LocalPlayer.Interaction.HeldItem;
            firearm.CurrentAmmo += ammoAmount;
            Log.Info($"Gave {ammoAmount} Ammo");
        }
        [ConCmd("kill_zombies")]
        public static void KillZombie()
        {
            if (Game.ActiveScene is null)
                return;
            var zombieSpawned = ZombieSpawner.ZombiesSpawnedThisWave;

            var zombies = new List<GameObject>();
            for (int i = 0; i < zombieSpawned; i++)
            {
                var zombieName = i == 0 ? "zombie" : $"zombie ({i})";
                var foundZombies = Game.ActiveScene.Directory.FindByName(zombieName, false);
                zombies.AddRange(foundZombies);
            }
            Log.Info($"Killed {zombies.Count} zombies");
            for (int i = zombies.Count - 1; i >= 0; i--)
            {
                var zombie = zombies[i];
                zombie.GetComponent<Zombie>().Hurt(1000);
                zombies.RemoveAt(i);
            }

           
        }

        [ConCmd("rapid_fire")]
        public static void RapidFire(string enabled)
        {
            if (!bool.TryParse(enabled, out var bEnabled))
            {
                Log.Info("Invalid: rapid_fire true");
                return;
            }
            if (Game.ActiveScene is null)
                return;
            var player = PlayerPawn.LocalPlayer;
            if (!player.IsValid())
            {
                Log.Info($"player not found");
                return;
            }
            Firearm firearm = (Firearm)PlayerPawn.LocalPlayer.Interaction.HeldItem;
            if (bEnabled)
            {
                firearm.FireRate = 0.0001f;
                firearm.CurrentAmmo = 10000000;
            }
            else
            {
                firearm.FireRate = 0.3f;
                firearm.CurrentAmmo = 7;
            }
            Log.Info($"Rapid fire is now {enabled}");
        }
    }
}