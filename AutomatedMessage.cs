using System;
using System.Collections.Generic;
using System.Timers;
using System.Linq;
using System.IO;
using System.Text.Json;
using Vintagestory.API.Server;
using Vintagestory.API.Common;

namespace AutomatedMessageMod
{
    public class AutomatedMessageModSystem : ModSystem
    {
        private ICoreServerAPI? sapi;
        private System.Timers.Timer? checkTimer;
        private readonly List<ServerTimedMessage> messages = new List<ServerTimedMessage>();
        private string configFilePath = "";

        public override void StartServerSide(ICoreServerAPI api)
        {
            sapi = api;
            
            // Log immediately
            api.Logger.Notification("=== AutomatedMessage Mod Starting ===");

            // Set up config file path
            configFilePath = Path.Combine(api.GetOrCreateDataPath("ModConfig"), "AutomatedMessages.json");
            
            api.Logger.Notification($"Config file path will be: {configFilePath}");

            // Load saved messages from disk
            LoadMessages();

            // Register the command with enough optional word parsers for long messages
            api.ChatCommands.Create("servermsg")
                .WithDescription("Manage automated messages")
                .WithArgs(
                    api.ChatCommands.Parsers.Word("subcommand"),
                    api.ChatCommands.Parsers.OptionalWord("param1"),
                    api.ChatCommands.Parsers.OptionalWord("param2"),
                    api.ChatCommands.Parsers.OptionalWord("param3"),
                    api.ChatCommands.Parsers.OptionalWord("param4"),
                    api.ChatCommands.Parsers.OptionalWord("param5"),
                    api.ChatCommands.Parsers.OptionalWord("param6"),
                    api.ChatCommands.Parsers.OptionalWord("param7"),
                    api.ChatCommands.Parsers.OptionalWord("param8"),
                    api.ChatCommands.Parsers.OptionalWord("param9"),
                    api.ChatCommands.Parsers.OptionalWord("param10"),
                    api.ChatCommands.Parsers.OptionalWord("param11"),
                    api.ChatCommands.Parsers.OptionalWord("param12"),
                    api.ChatCommands.Parsers.OptionalWord("param13"),
                    api.ChatCommands.Parsers.OptionalWord("param14"),
                    api.ChatCommands.Parsers.OptionalWord("param15"),
                    api.ChatCommands.Parsers.OptionalWord("param16"),
                    api.ChatCommands.Parsers.OptionalWord("param17"),
                    api.ChatCommands.Parsers.OptionalWord("param18"),
                    api.ChatCommands.Parsers.OptionalWord("param19"),
                    api.ChatCommands.Parsers.OptionalWord("param20"),
                    api.ChatCommands.Parsers.OptionalWord("param21"),
                    api.ChatCommands.Parsers.OptionalWord("param22"),
                    api.ChatCommands.Parsers.OptionalWord("param23"),
                    api.ChatCommands.Parsers.OptionalWord("param24"),
                    api.ChatCommands.Parsers.OptionalWord("param25"),
                    api.ChatCommands.Parsers.OptionalWord("param26"),
                    api.ChatCommands.Parsers.OptionalWord("param27"),
                    api.ChatCommands.Parsers.OptionalWord("param28"),
                    api.ChatCommands.Parsers.OptionalWord("param29")
                )
                .RequiresPrivilege(Privilege.controlserver)
                .HandleWith(OnAutoMsgCommand);

            api.Logger.Notification("Command registered successfully");

            // Start the timer for scheduled messages
            checkTimer = new System.Timers.Timer(1000); // check every second
            checkTimer.Elapsed += (sender, e) => CheckSchedule();
            checkTimer.Start();
            
            api.Logger.Notification("=== AutomatedMessage Mod Started Successfully ===");
        }

        private void LoadMessages()
        {
            try
            {
                sapi?.Logger.Notification($"[LoadMessages] Attempting to load from: {configFilePath}");
                
                if (File.Exists(configFilePath))
                {
                    string json = File.ReadAllText(configFilePath);
                    sapi?.Logger.Notification($"[LoadMessages] File found, contents length: {json.Length}");
                    
                    var savedMessages = JsonSerializer.Deserialize<List<SavedMessage>>(json);

                    if (savedMessages != null)
                    {
                        messages.Clear();
                        foreach (var saved in savedMessages)
                        {
                            if (TimeSpan.TryParse(saved.Time, out var timeSpan))
                            {
                                messages.Add(new ServerTimedMessage(timeSpan, saved.Message));
                            }
                            else
                            {
                                sapi?.Logger.Warning($"[LoadMessages] Failed to parse time: {saved.Time}");
                            }
                        }
                        sapi?.Logger.Notification($"[LoadMessages] Successfully loaded {messages.Count} messages");
                    }
                }
                else
                {
                    sapi?.Logger.Notification($"[LoadMessages] No saved file found at {configFilePath}");
                }
            }
            catch (Exception ex)
            {
                sapi?.Logger.Error($"[LoadMessages] EXCEPTION: {ex.Message}");
                sapi?.Logger.Error($"[LoadMessages] Stack: {ex.StackTrace}");
            }
        }

        private void SaveMessages()
        {
            try
            {
                sapi?.Logger.Notification($"[SaveMessages] Attempting to save {messages.Count} messages to: {configFilePath}");
                
                var savedMessages = messages.Select(m => new SavedMessage
                {
                    Time = m.Time.ToString(@"hh\:mm"),
                    Message = m.Message
                }).ToList();

                var options = new JsonSerializerOptions { WriteIndented = true };
                string json = JsonSerializer.Serialize(savedMessages, options);
                
                sapi?.Logger.Notification($"[SaveMessages] Serialized JSON length: {json.Length}");

                // Ensure directory exists
                string? directory = Path.GetDirectoryName(configFilePath);
                
                if (!string.IsNullOrEmpty(directory))
                {
                    sapi?.Logger.Notification($"[SaveMessages] Directory: {directory}");
                    
                    if (!Directory.Exists(directory))
                    {
                        Directory.CreateDirectory(directory);
                        sapi?.Logger.Notification($"[SaveMessages] Created directory");
                    }
                }

                File.WriteAllText(configFilePath, json);
                sapi?.Logger.Notification($"[SaveMessages] File written successfully");
                
                // Verify
                if (File.Exists(configFilePath))
                {
                    var fileInfo = new FileInfo(configFilePath);
                    sapi?.Logger.Notification($"[SaveMessages] VERIFIED: File exists, size: {fileInfo.Length} bytes");
                }
                else
                {
                    sapi?.Logger.Error($"[SaveMessages] ERROR: File does not exist after write!");
                }
            }
            catch (Exception ex)
            {
                sapi?.Logger.Error($"[SaveMessages] EXCEPTION: {ex.Message}");
                sapi?.Logger.Error($"[SaveMessages] Stack: {ex.StackTrace}");
            }
        }

        private TextCommandResult OnAutoMsgCommand(TextCommandCallingArgs args)
        {
            // Collect all non-null arguments from the parsers
            List<string> allArgs = new List<string>();
            
            for (int i = 0; i < args.Parsers.Count; i++)
            {
                var value = args.Parsers[i].GetValue() as string;
                if (!string.IsNullOrEmpty(value))
                {
                    allArgs.Add(value);
                }
            }
            
            if (allArgs.Count == 0)
            {
                return TextCommandResult.Error("Available subcommands: add, list, remove");
            }

            string subcommand = allArgs[0].ToLower();

            switch (subcommand)
            {
                case "add":
                    {
                        if (allArgs.Count < 3)
                        {
                            return TextCommandResult.Error("Usage: /automsg add HH:mm message text");
                        }

                        string time = allArgs[1];
                        string message = string.Join(" ", allArgs.Skip(2));

                        if (TimeSpan.TryParse(time, out var scheduleTime))
                        {
                            messages.Add(new ServerTimedMessage(scheduleTime, message));
                            SaveMessages(); // Save to disk
                            return TextCommandResult.Success($"Added scheduled message at {time}: {message}");
                        }
                        else
                        {
                            return TextCommandResult.Error($"Invalid time format: {time}. Use HH:mm format (e.g., 16:00).");
                        }
                    }

                case "list":
                    {
                        if (messages.Count == 0)
                        {
                            return TextCommandResult.Success("No scheduled messages.");
                        }

                        string result = "Scheduled messages:";
                        for (int i = 0; i < messages.Count; i++)
                        {
                            var msg = messages[i];
                            result += $"\n{i + 1}: {msg.Time:hh\\:mm} - {msg.Message}";
                        }

                        return TextCommandResult.Success(result);
                    }

                case "remove":
                    {
                        if (allArgs.Count < 2 || !int.TryParse(allArgs[1], out int index))
                        {
                            return TextCommandResult.Error("Usage: /automsg remove <index>");
                        }

                        if (index >= 1 && index <= messages.Count)
                        {
                            var removed = messages[index - 1];
                            messages.RemoveAt(index - 1);
                            SaveMessages(); // Save to disk
                            return TextCommandResult.Success($"Removed message at {removed.Time:hh\\:mm}: {removed.Message}");
                        }
                        else
                        {
                            return TextCommandResult.Error($"Index out of range: {index}. Valid range: 1-{messages.Count}");
                        }
                    }

                default:
                    return TextCommandResult.Error("Available subcommands: add, list, remove");
            }
        }

        private void CheckSchedule()
        {
            if (sapi == null) return;

            var now = DateTime.Now.TimeOfDay;

            foreach (var msg in messages)
            {
                if (!msg.SentToday && msg.Time <= now)
                {
                    // Broadcast to all players using the server's player manager
                    foreach (var player in sapi.World.AllOnlinePlayers)
                    {
                        if (player is IServerPlayer serverPlayer)
                        {
                            serverPlayer.SendMessage(0, msg.Message, EnumChatType.Notification);
                        }
                    }
                    msg.SentToday = true;
                }

                // Reset at midnight (when current time is less than 1 second)
                if (now < TimeSpan.FromSeconds(1))
                {
                    msg.SentToday = false;
                }
            }
        }

        public override void Dispose()
        {
            checkTimer?.Stop();
            checkTimer?.Dispose();
            base.Dispose();
        }

        // Helper class for JSON serialization - stores time as string to avoid TimeSpan serialization issues
        private class SavedMessage
        {
            public string Time { get; set; } = "";
            public string Message { get; set; } = "";
        }

        private class ServerTimedMessage
        {
            public TimeSpan Time { get; }
            public string Message { get; }
            public bool SentToday { get; set; }

            public ServerTimedMessage(TimeSpan time, string message)
            {
                Time = time;
                Message = message;
                SentToday = false;
            }
        }
    }
}