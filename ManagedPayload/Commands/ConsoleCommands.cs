using Sandbox;
using System.Collections;
using System.Reflection;

namespace SandFox.Commands;

public static class ConsoleCommands
{
    internal static void AddConsoleCommands(Assembly assembly)
    {
        try
        {
            // Find the ConVarSystem type
            Type conVarSystemType = null;
            foreach (var asm in AppDomain.CurrentDomain.GetAssemblies())
            {
                conVarSystemType = asm.GetType("Sandbox.ConVarSystem");
                if (conVarSystemType != null)
                {
                    Log.Info($"Found ConVarSystem in {asm.GetName().Name}");
                    break;
                }
            }

            if (conVarSystemType == null)
            {
                Log.Error("Could not find ConVarSystem type");
                return;
            }

            // Try to call the internal AddAssembly method
            var flags = BindingFlags.NonPublic | BindingFlags.Static;
            var addAssemblyMethod = conVarSystemType.GetMethod("AddAssembly", flags);

            if (addAssemblyMethod != null)
            {
                Log.Info("Found AddAssembly method, attempting to register commands...");
                addAssemblyMethod.Invoke(null, new object[] { assembly, "game", null });
                Log.Info("Successfully registered commands");
                return;
            }

            // Fallback: Get the command collection directly
            var members = conVarSystemType.GetField("Members", flags)?.GetValue(null) as IDictionary;
            if (members == null)
            {
                Log.Error("Could not find Members collection");
                return;
            }

            // Find and register all command methods
            var bindingFlags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static;
            foreach (var type in assembly.GetTypes())
            {
                foreach (var method in type.GetMethods(bindingFlags))
                {
                    var cmdAttr = method.GetCustomAttribute<ConCmdAttribute>();
                    if (cmdAttr == null) continue;

                    try
                    {
                        // Get the command name
                        string cmdName = cmdAttr.Name;
                        if (string.IsNullOrEmpty(cmdName))
                            cmdName = method.Name.ToLower();

                        // Check if command already exists
                        if (members.Contains(cmdName))
                        {
                            Log.Warning($"Command {cmdName} already exists - skipping");
                            continue;
                        }

                        var managedCmdType = conVarSystemType.Assembly.GetType("Sandbox.ManagedCommand");
                        if (managedCmdType == null)
                        {
                            Log.Error("Could not find ManagedCommand type");
                            continue;
                        }

                        var constructor = managedCmdType.GetConstructors(BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public)
                            .FirstOrDefault();

                        if (constructor == null)
                        {
                            Log.Error("Could not find ManagedCommand constructor");
                            continue;
                        }

                        // Create cookie container for command
                        var cookieContainerType = conVarSystemType.Assembly.GetType("Sandbox.CookieContainer");
                        var cookieContainer = Activator.CreateInstance(cookieContainerType, new object[] { $"convar/{cmdName}" });

                        // Create the command instance
                        var command = constructor.Invoke(new object[] { assembly, method, cmdAttr, cookieContainer });
                        if (command != null)
                        {
                            members[cmdName] = command;
                            Log.Info($"Successfully registered command: {cmdName}");
                        }
                    }
                    catch (Exception ex)
                    {
                        Log.Error($"Failed to register command {method.Name}: {ex.Message}");
                    }
                }
            }

            Log.Info("Finished registering commands");
        }
        catch (Exception ex)
        {
            Log.Error($"Error registering console commands: {ex}");
        }
    }

    [ConCmd("dump_console_commands")]
    public static void DumpAllConsoleCommands()
    {
        try
        {
            Type conVarSystemType = null;
            foreach (var asm in AppDomain.CurrentDomain.GetAssemblies())
            {
                conVarSystemType = asm.GetType("Sandbox.ConVarSystem");
                if (conVarSystemType != null) break;
            }

            if (conVarSystemType == null)
            {
                Log.Error("ConVarSystem not found");
                return;
            }

            var flags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static;
            var members = conVarSystemType.GetField("Members", flags)?.GetValue(null) as IDictionary;

            if (members == null)
            {
                Log.Error("Could not find command collection");
                return;
            }

            Log.Info($"Found {members.Count} commands:");
            foreach (var key in members.Keys)
            {
                var command = members[key];
                var cmdType = command.GetType();
                var name = cmdType.GetProperty("Name", flags)?.GetValue(command);
                var assembly = cmdType.GetField("assembly", flags)?.GetValue(command) as Assembly;
                Log.Info($"Command: {name} from {assembly?.GetName()?.Name}");
            }
        }
        catch (Exception ex)
        {
            Log.Error($"Error dumping console commands: {ex}");
        }
    }
}