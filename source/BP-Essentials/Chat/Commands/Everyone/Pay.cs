﻿using static BP_Essentials.EssentialsVariablesPlugin;
using System;
using System.Linq;
using UnityEngine;

namespace BP_Essentials.Commands
{
    public class Pay
    {
        public static void Run(SvPlayer player, string message)
        {
            string CorrSyntax = $"<color={argColor}>" + GetArgument.Run(0, false, false, message) + $"</color><color={errorColor}> [Player] [Amount]</color><color={warningColor}> (Incorrect or missing argument(s).)</color>";
            string arg1 = GetArgument.Run(1, false, true, message);
            string arg2 = message.Split(' ').Last().Trim();
            if (String.IsNullOrEmpty(GetArgument.Run(1, false, false, message)) || String.IsNullOrEmpty(arg2))
            {
                player.Send(SvSendType.Self, Channel.Unsequenced, ClPacket.GameMessage, CorrSyntax);
                return;
            }
            else
            {
                int lastIndex = arg1.LastIndexOf(" ");
                if (lastIndex != -1)
                    arg1 = arg1.Remove(lastIndex).Trim();
            }
            var isNumeric = int.TryParse(arg2, out int arg2Int);
            if (isNumeric)
            {
                if (arg2Int <= 0)
                {
                    player.Send(SvSendType.Self, Channel.Unsequenced, ClPacket.GameMessage, $"<color={errorColor}>Cannot transfer 0$.</color>");
                    return;
                }
                foreach (var shPlayer in UnityEngine.Object.FindObjectsOfType<ShPlayer>())
                    if (shPlayer.username == arg1 && !shPlayer.svPlayer.serverside || shPlayer.ID.ToString() == arg1.ToString() && !shPlayer.svPlayer.serverside)
                        foreach (var _shPlayer in UnityEngine.Object.FindObjectsOfType<ShPlayer>())
                            if (_shPlayer.svPlayer == player && !_shPlayer.svPlayer.serverside)
                            {
                                if (_shPlayer.MyMoneyCount() >= arg2Int)
                                {
                                    _shPlayer.TransferMoney(DeltaInv.RemoveFromMe, arg2Int, true);
                                    shPlayer.TransferMoney(DeltaInv.AddToMe, arg2Int, true);
                                    player.Send(SvSendType.Self, Channel.Unsequenced, ClPacket.GameMessage, $"<color={infoColor}>Successfully transfered</color> <color={argColor}>{arg2Int}</color><color={infoColor}>$ to </color><color={argColor}>{shPlayer.username}</color><color={infoColor}>!</color>");
                                    shPlayer.svPlayer.Send(SvSendType.Self, Channel.Unsequenced, ClPacket.GameMessage, $"<color={argColor}>{player.playerData.username}</color><color={infoColor}> gave you </color><color={argColor}>{arg2Int}</color><color={infoColor}>$!</color>");
                                }
                                else
                                    player.Send(SvSendType.Self, Channel.Unsequenced, ClPacket.GameMessage, $"<color={errorColor}>Cannot transfer money, do you have</color><color={argColor}> " + arg2Int + $"</color><color={errorColor}>$ in your inventory?</color>");
                                return;
                            }

                player.Send(SvSendType.Self, Channel.Unsequenced, ClPacket.GameMessage, NotFoundOnline);
            }
            else
                player.Send(SvSendType.Self, Channel.Unsequenced, ClPacket.GameMessage, CorrSyntax);
        }
    }
}
