using System;
using System.Collections.Generic;
using Vintagestory.API.Common;
using Vintagestory.API.Server;

namespace AutomatedMessageMod
{
    public class AutomatedMessageSystem : ModSystem
    {
        private ICoreServerAPI sapi;
        private List<MessageEntry> entries = new List<MessageEntry>();
        private const string ConfigFile = "AutomatedMessages.json";

        // Debounce so we only fire once per minute
        private string lastMinuteChecked = null;

        public override void StartServerSide(ICoreServerAPI api)
        {
            sapi = api;

            // Load persisted config
            entries = sapi.LoadModConfig<List<MessageEntry>>(ConfigFile) ?? new List<MessageEntry>();
            sapi.StoreModConfig(entries, ConfigFile); // ensure file exists

            // Commands
            sapi.RegisterCommand(
                "automsg",
                "Manage automated messages",
                "/automsg add <HH:mm[,HH:mm...]> <message> | /automsg list | /automsg remove <index> [HH:mm] | /automsg clear",
                CmdAutoMsg,
                Privilege.controlserver
            );

            sapi.RegisterCommand(
                "automsgtest",
                "Send a test broadcast",
                "/automsgtest",
                CmdTest,
                Privilege.controlserver
            );

            // Check every 2 seconds; we’ll only act on minute changes
            sapi.Event.RegisterGameTickListener(CheckSchedule, 2000);
        }

        private void CmdTest(IServerPlayer player, int groupId, CmdArgs args)
        {
            sapi.BroadcastMessageToAllGroups("[AutomatedMessage] Test", EnumChatType.Notification);
        }

        private void CheckSchedule(float dt)
        {
            string nowMinute = DateTime.Now.ToString("HH:mm");
            if (nowMinute == lastMinuteChecked) return;   // same minute, do nothing
            lastMinuteChecked = nowMinute;

            // Fire any entries matching this minute
            for (int i = 0; i < entries.Count; i++)
            {
                var entry = entries[i];
                for (int j = 0; j < entry.TriggerTimes.Count; j++)
                {
                    if (entry.TriggerTimes[j] == nowMinute)
                    {
                        sapi.BroadcastMessageToAllGroups(entry.Message, EnumChatType.Notification);
                        break;
                    }
                }
            }
        }

        private void CmdAutoMsg(IServerPlayer player, int groupId, CmdArgs args)
        {
            string mode = args.PopWord();
            if (mode == null)
            {
                player.SendMessage(groupId, "Usage: /automsg add <HH:mm[,HH:mm...]> <message> | /automsg list | /automsg remove <index> [HH:mm] | /automsg clear", EnumChatType.CommandError);
                return;
            }

            if (mode.Equals("add", StringComparison.OrdinalIgnoreCase))
            {
                string timesRaw = args.PopWord();
                string msg = args.PopAll();

                if (string.IsNullOrWhiteSpace(timesRaw) || string.IsNullOrWhiteSpace(msg))
                {
                    player.SendMessage(groupId, "Usage: /automsg add <HH:mm[,HH:mm...]> <message>", EnumChatType.CommandError);
                    return;
                }

                var times = new List<string>();
                foreach (var part in timesRaw.Split(','))
                {
                    var trimmed = part.Trim();
                    if (TimeSpan.TryParse(trimmed, out var ts))
                    {
                        times.Add(ts.ToString(@"hh\:mm")); // store canonical HH:mm
                    }
                }

                if (times.Count == 0)
                {
                    player.SendMessage(groupId, "No valid times found. Use 24h HH:mm (e.g., 05:00,17:30).", EnumChatType.CommandError);
                    return;
                }

                entries.Add(new MessageEntry { Message = msg, TriggerTimes = times });
                sapi.StoreModConfig(entries, ConfigFile);
                player.SendMessage(groupId, $"Added: \"{msg}\" at {string.Join(",", times)}", EnumChatType.Notification);
            }
            else if (mode.Equals("list", StringComparison.OrdinalIgnoreCase))
            {
                if (entries.Count == 0)
                {
                    player.SendMessage(groupId, "No automated messages set.", EnumChatType.Notification);
                    return;
                }

                for (int i = 0; i < entries.Count; i++)
                {
                    var e = entries[i];
                    player.SendMessage(groupId, $"{i + 1}. {e.Message} at {string.Join(",", e.TriggerTimes)}", EnumChatType.Notification);
                }
            }
            else if (mode.Equals("remove", StringComparison.OrdinalIgnoreCase))
            {
                int index = args.PopInt() ?? -1;
                if (index < 1 || index > entries.Count)
                {
                    player.SendMessage(groupId, "Invalid index. Use /automsg list to see valid numbers.", EnumChatType.CommandError);
                    return;
                }

                string timeArg = args.PopWord();
                if (timeArg == null)
                {
                    // Remove entire entry
                    var removed = entries[index - 1];
                    entries.RemoveAt(index - 1);
                    sapi.StoreModConfig(entries, ConfigFile);
                    player.SendMessage(groupId, $"Removed entry: \"{removed.Message}\"", EnumChatType.CommandSuccess);
                }
                else
                {
                    if (!TimeSpan.TryParse(timeArg, out var ts))
                    {
                        player.SendMessage(groupId, "Invalid time. Use HH:mm (24h).", EnumChatType.CommandError);
                        return;
                    }

                    string tcanon = ts.ToString(@"hh\:mm");
                    var entry = entries[index - 1];
                    if (entry.TriggerTimes.Remove(tcanon))
                    {
                        string msg = $"Removed time {tcanon} from entry {index}.";
                        if (entry.TriggerTimes.Count == 0)
                        {
                            entries.RemoveAt(index - 1);
                            msg += " (entry deleted)";
                        }
                        sapi.StoreModConfig(entries, ConfigFile);
                        player.SendMessage(groupId, msg, EnumChatType.CommandSuccess);
                    }
                    else
                    {
                        player.SendMessage(groupId, $"Time {tcanon} not found in entry {index}.", EnumChatType.CommandError);
                    }
                }
            }
            else if (mode.Equals("clear", StringComparison.OrdinalIgnoreCase))
            {
                entries.Clear();
                sapi.StoreModConfig(entries, ConfigFile);
                player.SendMessage(groupId, "All automated messages cleared.", EnumChatType.CommandSuccess);
            }
            else
            {
                player.SendMessage(groupId, "Unknown subcommand. Use add/remove/list/clear.", EnumChatType.CommandError);
            }
        }

        private class MessageEntry
        {
            public string Message { get; set; }
            public List<string> TriggerTimes { get; set; } = new List<string>();
        }
    }
}
