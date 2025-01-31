using System.Reflection;

namespace SandFox;

public static class SandFoxSystem
{
    public static Assembly CurrentGameAssembly { get; private set; }

    public static void Init(Assembly gameAssembly)
    {
        CurrentGameAssembly = gameAssembly;

        try
        {
            AddConsoleCommands();
            InitializePatches();
        }
        catch (Exception e)
        {
            Log.Error(e);
            return;
        }
    }

    private static void AddConsoleCommands()
    {
        Log.Info($"Adding console commands from {nameof(ManagedPayload)}");
        Commands.ConsoleCommands.AddConsoleCommands(Assembly.GetExecutingAssembly());
    }

    private static void InitializePatches()
    {
        if (CurrentGameAssembly is null)
            throw new InvalidOperationException("A game assembly must be loaded before initializing patches");

        // Initialize MonoMod patches
        Patches.ZombieSpawnerPatch.Initialize();

        Log.Info("MonoMod patches initialized");
    }

    public static void Cleanup()
    {
        // Cleanup patches when system is shutting down
        Patches.ZombieSpawnerPatch.Cleanup();
    }
}