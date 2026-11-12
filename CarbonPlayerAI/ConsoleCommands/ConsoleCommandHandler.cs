using System;
using System.Collections.Generic;
using CarbonPlayerAI.Core;

// The ConsoleCommandHandler reads console commands from the F1 console for our own customs commmand. Then executes them.

namespace CarbonPlayerAI.ConsoleCommands
{
    public static class ConsoleCommandHandler
    {
        private static readonly Dictionary<ConsoleCommandEnum, IConsoleCommand> commandRegistry = new();

        // Static constructor ensures commands are registered at startup
        static ConsoleCommandHandler()
        {
            RegisterAllCommands();
        }

        //Umod doesn't allow reflection :skull:
        private static void RegisterAllCommands()
        {
            RegisterCommand(new SetCommand());
            RegisterCommand(new GetCommand());
            RegisterCommand(new BotCommand());
            RegisterCommand(new SummonLineCommand());
            RegisterCommand(new SummonLineAtCommand());
            RegisterCommand(new SpawnLineCommand());
            RegisterCommand(new MeleeLogCommand());

            Logger.Log($"Registered {commandRegistry.Count} predefined commands.", LogLevel.INFO);
        }


        public static void RegisterCommand(IConsoleCommand command)
        {
            if (!commandRegistry.ContainsKey(command.CommandName))
            {
                commandRegistry[command.CommandName] = command;
                Logger.Log($"Registered command: {command.CommandName}", LogLevel.INFO);
            }
            else
            {
                Logger.Log($"Command '{command.CommandName}' is already registered.", LogLevel.WARNING);
            }
        }

        public static void ProcessConsoleCommand(int playerId, string input, string output, bool success)
        {
            if (!CarbonPlayerAIMod.getIsServer())
            {
                return;
            }

            string[] parts = input.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length == 0) return;

            if (!Enum.TryParse(parts[0], true, out ConsoleCommandEnum commandName))
            {
                return; // Allow unknown commands to pass silently
            }

            if (!commandRegistry.TryGetValue(commandName, out var command))
            {
                Logger.Log($"Command '{commandName}' is not registered.", LogLevel.WARNING);
                return;
            }

            string[] parameters = parts.Length > 1 ? parts[1..] : Array.Empty<string>();

            if (!command.Validate(parameters, out string errorMessage))
            {
                if (!string.IsNullOrEmpty(errorMessage))
                {
                    Logger.Log($"Validation failed for command '{commandName}' from Player {playerId}: {errorMessage}", LogLevel.WARNING);
                    Feedback.Reply(playerId, errorMessage);
                }
                return;
            }

            command.Execute(playerId, parameters);
        }

    }
}
