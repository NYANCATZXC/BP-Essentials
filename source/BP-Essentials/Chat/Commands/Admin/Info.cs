﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using static BP_Essentials.EssentialsVariablesPlugin;
using static BP_Essentials.EssentialsMethodsPlugin;

namespace BP_Essentials.Commands
{
    class Info
    {
        public static void Run(SvPlayer player, string message)
        {
            var arg1 = GetArgument.Run(1, false, true, message);
            var found = false;
            if (!String.IsNullOrEmpty(arg1))
            {
                foreach (var shPlayer in UnityEngine.Object.FindObjectsOfType<ShPlayer>())
                    if (shPlayer.username == arg1 || shPlayer.ID.ToString() == arg1.ToString())
                        if (!shPlayer.svPlayer.serverside)
                        {
                            player.Send(SvSendType.Self, Channel.Unsequenced, ClPacket.GameMessage, "Info about: '" + shPlayer.username + "'.");
                            string[] contentarray = {
                                    "Username:              " +  shPlayer.username,
                                    "",
                                    "",
                                    "Job:                         " + Jobs[shPlayer.job.jobIndex],
                                    "Health:                    " + Math.Floor(shPlayer.health),
                                    "OwnsApartment:   " + (bool)shPlayer.ownedApartment,
                                    "Position:                 " + shPlayer.GetPosition().ToString(),
                                    "WantedLevel:         " + shPlayer.wantedLevel,
                                    "IsAdmin:                 " + shPlayer.admin,
                                    "BankBalance:         " + shPlayer.svPlayer.bankBalance,
                                    "ChatEnabled:         " + playerList[shPlayer.ID].chatEnabled,
                                    "IP:                            " + shPlayer.svPlayer.connection.IP
                                };

                            var content = string.Join("\r\n", contentarray);

                            player.Send(SvSendType.Self, Channel.Reliable, ClPacket.ServerInfo, content);

                            found = true;
                        }
                if (!(found))
                    player.Send(SvSendType.Self, Channel.Unsequenced, ClPacket.GameMessage, NotFoundOnline);
            }
            else
                player.Send(SvSendType.Self, Channel.Reliable, ClPacket.GameMessage, ArgRequired);
        }
    }
}
