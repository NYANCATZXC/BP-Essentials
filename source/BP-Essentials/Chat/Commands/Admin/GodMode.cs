﻿
using static BP_Essentials.EssentialsMethodsPlugin;
using static BP_Essentials.EssentialsVariablesPlugin;
using System;
using System.IO;
using UnityEngine;

namespace BP_Essentials.Commands {
    public class GodMode{
        public static void Run(SvPlayer player, string message) {
            {
                ReadFile.Run(GodListFile);
                string name = GetArgument.Run(1, false, true, message);
                string msg = String.Empty;
                if (String.IsNullOrEmpty(name))
                {
                    name = player.playerData.username;
                    msg = $"<color={infoColor}>Godmode </color><color={argColor}>{{0}}</color><color={infoColor}>.</color>";
                }
                else
                    foreach (var shPlayer in UnityEngine.Object.FindObjectsOfType<ShPlayer>())
                        if (shPlayer.username == name && !shPlayer.svPlayer.serverside || shPlayer.ID.ToString() == name && !shPlayer.svPlayer.serverside)
                        {
                            name = shPlayer.username;
                            msg = $"<color={infoColor}>Godmode </color><color={argColor}>{{0}}</color><color={infoColor}> for </color><color={argColor}>'{name}'</color><color={infoColor}>.</color>";
                            break;
                        }

                if (GodListPlayers.Contains(name))
                {
                    RemoveStringFromFile.Run(GodListFile, name);
                    ReadFile.Run(GodListFile);
                    player.Send(SvSendType.Self, Channel.Unsequenced, ClPacket.GameMessage, String.Format(msg, "disabled"));
                }
                else
                {
                    File.AppendAllText(GodListFile, name + Environment.NewLine);
                    GodListPlayers.Add(name);
                    player.Send(SvSendType.Self, Channel.Unsequenced, ClPacket.GameMessage, String.Format(msg, "enabled"));
                }

            }
        }
    }
}