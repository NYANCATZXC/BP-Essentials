﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using static BP_Essentials.EssentialsVariablesPlugin;
using static BP_Essentials.EssentialsMethodsPlugin;
using System.Threading;

namespace BP_Essentials.Chat
{
    class Announce : EssentialsCorePlugin
    {
        public static void Run()
        {
            try
            {
                _Timer.Elapsed += (sender, e) => OnTime();
                _Timer.Interval = TimeBetweenAnnounce * 1000;
                _Timer.Enabled = true;
            }
            catch (Exception ex)
            {
                ErrorLogging.Run(ex);
            }
        }

        private static void OnTime()
        {
            if (!string.IsNullOrWhiteSpace(Announcements[AnnounceIndex]))
            {
                foreach (var player in SvMan.players)
                    foreach (var line in Announcements[AnnounceIndex].Split(new[] { "\\r\\n", "\\r", "\\n" }, StringSplitOptions.None))
                        player.Value.svPlayer.Send(SvSendType.Self, Channel.Reliable, ClPacket.GameMessage, line);
                Debug.Log($"{SetTimeStamp.Run()}[INFO] Announcement made...");
            }
            if (++AnnounceIndex > Announcements.Length - 1)
                AnnounceIndex = 0;
        }
    }
}