using MonoMod.RuntimeDetour;
using Sandbox;
using System.Reflection;
using MightyBrick.GhoulGrounds;

namespace SandFox.Patches;

public static class ZombieSpawnerPatch
{
    private static Hook spawnHook;

    private static FieldInfo spawnsField = typeof(ZombieSpawner).GetField("Spawns",
        BindingFlags.NonPublic | BindingFlags.Instance);
    private static FieldInfo zombiesSpawnedField = typeof(ZombieSpawner).GetField("ZombiesSpawnedThisWave",
        BindingFlags.NonPublic | BindingFlags.Instance);

    public static void Initialize()
    {
        var original = typeof(ZombieSpawner).GetMethod("SpawnZombie",
            BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);

        if (original == null)
        {
            Log.Error("Could not find SpawnZombie method");
            return;
        }

        spawnHook = new Hook(
            original,
            new Action<Action<ZombieSpawner>, ZombieSpawner>((orig, self) =>
            {
                // Let the original method run first to maintain any important state
                if (Connection.All.Count <= 1)
                {
                    Log.Info("Skipping zombie spawn for clients");
                    orig(self);
                    return;
                }
                orig(self);
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
                Log.Info($" {zombies.Count} zombies");
                for (int i = zombies.Count - 1; i >= 0; i--)
                {
                    var zombie = zombies[i];
                    //zombie.NetworkSpawn();
                }
                
                Log.Info("[PATCH] Zombie spawn completed");
            })
        );

        Log.Info("ZombieSpawner patch initialized using MonoMod");
    }

    public static void Cleanup()
    {
        spawnHook?.Dispose();
    }
}