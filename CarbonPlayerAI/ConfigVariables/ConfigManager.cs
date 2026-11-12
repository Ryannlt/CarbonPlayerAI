using System;
using System.Collections.Generic;
using CarbonPlayerAI.Systems;

// The ConfigManager handles custom config variables by parsing data from method PassConfigVariables for our own custom commands.

namespace CarbonPlayerAI.ConfigVariables
{
    public static class ConfigManager
    {
        private static readonly Dictionary<ConfigCommandEnum, IConfigVariables> configCommands = new();

        static ConfigManager()
        {
            RegisterAllConfigVariables();
        }

        private static void RegisterAllConfigVariables()
        {
            RegisterConfigVariable(ConfigCommandEnum.EnableDebugLogging, new EnableDebugLogging());
            RegisterConfigVariable(ConfigCommandEnum.EnableMeleeLog, new EnableMeleeLog());
            RegisterConfigVariable(ConfigCommandEnum.SetBotDefaultAi, new SetBotDefaultAi());
            RegisterConfigVariable(ConfigCommandEnum.SetBotDefaultDeathPolicy, new SetBotDefaultDeathPolicy());
            RegisterConfigVariable(ConfigCommandEnum.SetBotKickDelay, new SetBotKickDelay());
            RegisterConfigVariable(ConfigCommandEnum.SetBotReplaceDelay, new SetBotReplaceDelay());
            RegisterConfigVariable(ConfigCommandEnum.SetGlobalAi, new SetGlobalAi());
            RegisterConfigVariable(ConfigCommandEnum.SpawnBot, new SpawnBot());
            RegisterConfigVariable(ConfigCommandEnum.SetLineBotCount, new SetLineBotCount());
            RegisterConfigVariable(ConfigCommandEnum.SetLineSpacing, new SetLineSpacing());
            RegisterConfigVariable(ConfigCommandEnum.SpawnLine, new SpawnLine());

            Logger.Log($"Registered {configCommands.Count} config variables.", LogLevel.DEBUG);
        }

        private static void RegisterConfigVariable(ConfigCommandEnum key, IConfigVariables command)
        {
            if (configCommands.ContainsKey(key))
            {
                Logger.Log($"Config variable {key} is already registered. Overwriting...", LogLevel.DEBUG);
            }

            configCommands[key] = command;
        }

        public static void ProcessConfigVariables(string[] configEntries)
        {
            if (configEntries == null || configEntries.Length == 0)
            {
                Logger.Log("Received empty config array. Ignoring.", LogLevel.DEBUG);
                return;
            }

            // Each non-empty config batch fully rebuilds the staged spawn lines, so re-passing the same
            // map config every round doesn't accumulate duplicate lines.
            LineManager.ClearStaged();

            foreach (string entry in configEntries)
            {
                if (string.IsNullOrWhiteSpace(entry))
                {
                    Logger.Log("Ignoring empty config entry.", LogLevel.DEBUG);
                    continue;
                }

                string[] parts = entry.Split(':');

                if (parts.Length < 2)
                {
                    Logger.Log($"Ignoring malformed config entry: {entry}", LogLevel.DEBUG);
                    continue;
                }

                string modId = parts[0];
                string key = parts[1];
                string data = parts.Length > 2 ? parts[2] : "";

                if (!modId.Equals("CarbonPlayerAI", StringComparison.OrdinalIgnoreCase))
                {
                    Logger.Log($"Ignoring non-CarbonPlayerAI config command: {modId}", LogLevel.DEBUG);
                    continue;
                }

                if (!Enum.TryParse(key, true, out ConfigCommandEnum parsedKey))
                {
                    Logger.Log($"Unknown config command key: {key}", LogLevel.WARNING);
                    continue;
                }

                if (!configCommands.TryGetValue(parsedKey, out IConfigVariables command))
                {
                    Logger.Log($"No handler registered for config variable: {parsedKey}", LogLevel.WARNING);
                    continue;
                }

                if (!command.Validate(data))
                {
                    Logger.Log($"Invalid value '{data}' for config variable '{parsedKey}'", LogLevel.WARNING);
                    continue;
                }

                command.Execute(data);
            }
        }
    }
}
