﻿using System;
using System.IO;
using System.Linq;
using System.Threading;
using UnityEngine;
using static BP_Essentials.EssentialsVariablesPlugin;
using System.Text.RegularExpressions;
using System.Collections.Generic;
using System.Text;
using System.Reflection;

namespace BP_Essentials
{
    public class EssentialsMethodsPlugin : EssentialsCorePlugin
    {

        [Hook("SvPlayer.SvSellApartment")]
        public static bool SvSellApartment(SvPlayer player)
        {
            player.Send(SvSendType.Self, Channel.Unsequenced, ClPacket.GameMessage, $"<color={warningColor}>Are you sure you want to sell your apartment? Type '</color><color={argColor}>{CmdCommandCharacter}{CmdConfirm}</color><color={warningColor}>' to confirm.</color>"); //softcode command
            return true;
        }

        [Hook("SvPlayer.Initialize")]
        public static void Initialize(SvPlayer player)
        {
            var shPlayer = player.player;
            if (!player.serverside)
            {
                new Thread(() => WriteIpToFile.Run(player)).Start();
                new Thread(() => CheckBanned.Run(player)).Start();
                new Thread(() => CheckAltAcc.Run(player)).Start();
                playerList.Add(shPlayer.ID, new _PlayerList { Shplayer = shPlayer });
            }
        }

        [Hook("SvPlayer.Destroy")]
        public static void Destroy(SvPlayer player)
        {
            foreach (KeyValuePair<int, _PlayerList> item in playerList)
                if (item.Value.Shplayer.svPlayer == player && !item.Value.Shplayer.svPlayer.serverside)
                {
                    Debug.Log(SetTimeStamp.Run() + "[INFO] [LEAVE] " + item.Value.Shplayer.username);
                    playerList.Remove(item.Key);
                    break;
                }
        }

        [Hook("SvPlayer.Damage")]
        public static bool Damage(SvPlayer player, ref DamageIndex type, ref float amount, ref ShPlayer attacker, ref Collider collider)
        {
            return CheckGodMode.Run(player, amount);
        }

        [Hook("SvPlayer.SpawnBot")]
        public static bool SpawnBot(SvPlayer player, ref Vector3 position, ref Quaternion rotation, ref Place place, ref Waypoint node, ref ShPlayer spawner, ref ShTransport transport, ref ShPlayer enemy)
        {
            var shPlayer = player.player;
            return EnableBlockSpawnBot == true && BlockedSpawnIds.Contains(shPlayer.spawnJobIndex);
        }

        [Hook("ShRestraint.HitEffect")]
        public static bool HitEffect(ShRestraint player, ref ShEntity hitTarget, ref ShPlayer source, ref Collider collider)
        {
            foreach (var shPlayer in UnityEngine.Object.FindObjectsOfType<ShPlayer>())
                if (!shPlayer.svPlayer.serverside)
                {
                    if (shPlayer != hitTarget) continue;
                    if (!GodListPlayers.Contains(shPlayer.username)) continue;
                    shPlayer.svPlayer.Send(SvSendType.Self, Channel.Unsequenced, ClPacket.GameMessage, "<color=#b7b5b5>Being handcuffed Blocked!</color>");
                    return true;
                }
            return false;
        }

        [Hook("SvPlayer.SvBan")]
        public static bool SvBan(SvPlayer player, ref int otherID)
        {
            if (BlockBanButtonTabMenu)
            {
                player.Send(SvSendType.Self, Channel.Unsequenced, ClPacket.GameMessage, $"<color={errorColor}>This button has been disabled. Please use the ban commands.</color>");
                return true;
            }
            foreach (var shPlayer in UnityEngine.Object.FindObjectsOfType<ShPlayer>())
                if (shPlayer.ID == otherID)
                    if (!shPlayer.svPlayer.serverside && !shPlayer.svPlayer.serverside)
                    {
                        LogMessage.LogOther($"{SetTimeStamp.Run()}[INFO] {shPlayer.username} Got banned by {player.playerData.username}");
                        player.Send(SvSendType.All, Channel.Unsequenced, ClPacket.GameMessage, $"<color={argColor}>{shPlayer.username}</color> <color={warningColor}>Just got banned by</color> <color={argColor}>{player.playerData.username}</color>");
                        SendDiscordMessage.BanMessage(shPlayer.username, player.playerData.username);
                    }
            return false;
        }

        [Hook("SvPlayer.SvStartVote")]
        public static bool SvStartVote(SvPlayer player, ref byte voteIndex, ref int ID)
        {
            if (voteIndex == VoteIndex.Kick)
            {
                if (!VoteKickDisabled)
                {
                    foreach (var shPlayer in UnityEngine.Object.FindObjectsOfType<ShPlayer>())
                        if (shPlayer.ID == ID)
                            foreach (var shIssuer in UnityEngine.Object.FindObjectsOfType<ShPlayer>())
                                if (shIssuer.svPlayer == player)
                                {
                                    if (player.svManager.vote != null || voteIndex >= shIssuer.manager.votes.Length || player.svManager.startedVote.Contains(shIssuer))
                                        return true;
                                    player.svManager.startedVote.Add(shIssuer);
                                    player.svManager.vote = shIssuer.manager.votes[voteIndex];
                                    if (player.svManager.vote.CheckVote(ID))
                                    {
                                        player.Send(SvSendType.All, Channel.Reliable, 60, voteIndex, ID);
                                        player.svManager.StartCoroutine(player.svManager.StartVote());
                                        Debug.Log($"{SetTimeStamp.Run()}[INFO] {player.playerData.username} Has issued a votekick against {shPlayer.username}");
                                        player.Send(SvSendType.All, Channel.Unsequenced, ClPacket.GameMessage, $"<color={argColor}>{player.playerData.username} </color><color={warningColor}>Has issued a vote kick against</color><color={argColor}> {shPlayer.username}</color>");
                                        LatestVotePeople.Clear();
                                    }
                                    else
                                        player.svManager.vote = null;
                                }
                }
                else
                    player.Send(SvSendType.Self, Channel.Unsequenced, ClPacket.GameMessage, $"<color={errorColor}>Vote kicking has been disabled on this server.</color>");
                return true;
            }
            else if (voteIndex == VoteIndex.Mission)
            {
                if (BlockMissions)
                {
                    player.Send(SvSendType.Self, Channel.Unsequenced, ClPacket.GameMessage, $"<color={errorColor}>All missions have been disabled on this server.</color>");
                    return true;
                }
            }
            return false;
        }

        [Hook("SvPlayer.SvVoteYes", true)]
        public static void SvVoteYes(SvPlayer player)
        {
            LatestVotePeople.Add(player.playerData.username);
        }

        [Hook("SvPlayer.SvFunctionKey")]
        public static bool SvFunctionKey(SvPlayer player, ref byte key)
        {
            try
            {
                if (key < 11)
                {
                    foreach (KeyValuePair<int, _PlayerList> item in playerList)
                    {
                        if (item.Value.Shplayer.svPlayer == player)
                        {
                            ShPlayer shPlayer = item.Value.Shplayer;

                            #region Report
                            if (item.Value.LastMenu == CurrentMenu.Report && key > 1 && key < 11)
                            {
                                player.Send(SvSendType.Self, Channel.Reliable, ClPacket.CloseFunctionMenu);
                                player.Send(SvSendType.Self, Channel.Reliable, ClPacket.GameMessage, $"<color={infoColor}>Reported \"</color><color={warningColor}>{item.Value.ReportedPlayer.username}</color><color={infoColor}>\" With the reason \"</color><color={warningColor}>{ReportReasons[key - 2]}</color><color={infoColor}>\".</color>");
                                item.Value.reportedReason = ReportReasons[key - 2];
                                item.Value.LastMenu = CurrentMenu.Main;
                                SendDiscordMessage.ReportMessage(item.Value.ReportedPlayer.username, player.player.username, ReportReasons[key - 2]);
                                ReportPlayer.Run(player.player.username, ReportReasons[key - 2], item.Value.ReportedPlayer);
                                return true;
                            }
                            #endregion

                            switch (key)
                            {
                                case 1:
                                    if (HasPermission.Run(player, AccessMoneyMenu) || HasPermission.Run(player, AccessItemMenu) || HasPermission.Run(player, AccessSetHPMenu) || HasPermission.Run(player, AccessSetStatsMenu) || HasPermission.Run(player, AccessCWMenu))
                                        player.Send(SvSendType.Self, Channel.Reliable, ClPacket.ShowFunctionMenu, "<color=#00ffffff>Main menu:</color>\n\n<color=#00ffffff>F3:</color> Server info menu\n<color=#00ffffff>F10:</color> Extras menu\n\n<color=#00ffffff>Press</color> <color=#ea8220>F11</color> <color=#00ffffff>To close this (G)UI</color>");
                                    else
                                        player.Send(SvSendType.Self, Channel.Reliable, ClPacket.ShowFunctionMenu, "<color=#00ffffff>Main menu:</color>\n\n<color=#00ffffff>F3:</color> Server info menu\n\n<color=#00ffffff>Press</color> <color=#ea8220>F11</color> <color=#00ffffff>To close this (G)UI</color>");
                                    item.Value.LastMenu = CurrentMenu.Main;
                                    break;
                                case 2:
                                    if (item.Value.LastMenu == CurrentMenu.ServerInfo)
                                    {
                                        player.Send(SvSendType.Self, Channel.Reliable, ClPacket.CloseFunctionMenu);
                                        player.Send(SvSendType.Self, Channel.Fragmented, ClPacket.ServerInfo, File.ReadAllText("server_info.txt"));
                                        item.Value.LastMenu = CurrentMenu.Main;
                                    }
                                    if (item.Value.LastMenu == CurrentMenu.Staff && HasPermission.Run(player, AccessMoneyMenu))
                                    {
                                        player.Send(SvSendType.Self, Channel.Reliable, ClPacket.ShowFunctionMenu, "<color=#00ffffff>Give Money menu:</color>\n\n<color=#00ffffff>F2:</color> Give <color=#ea8220>1.000 dollars (1k)</color>\n<color=#00ffffff>F3:</color> Give <color=#ea8220>10.000 dollars (10k)</color>\n<color=#00ffffff>F4:</color> Give <color=#ea8220>100.000 dollars (100k)</color>\n\n<color=#00ffffff>Press</color><color=#ea8220> F11 </color><color=#00ffffff>To close this (G)UI</color>");
                                        item.Value.LastMenu = CurrentMenu.GiveMoney;
                                    }
                                    else if (item.Value.LastMenu == CurrentMenu.GiveMoney)
                                    {
                                        player.Send(SvSendType.Self, Channel.Reliable, ClPacket.CloseFunctionMenu);
                                        shPlayer.TransferMoney(DeltaInv.AddToMe, 1000, true);
                                        player.Send(SvSendType.Self, Channel.Reliable, ClPacket.GameMessage, $"<color={infoColor}>You have given yourself 1.000 dollars.</color>");
                                        Debug.Log(SetTimeStamp.Run() + "[INFO] " + player.playerData.username + " Spawned in 1.000 dollars through the functionUI");
                                        item.Value.LastMenu = CurrentMenu.Main;
                                    }
                                    else if (item.Value.LastMenu == CurrentMenu.GiveItems)
                                    {
                                        player.Send(SvSendType.Self, Channel.Reliable, ClPacket.CloseFunctionMenu);
                                        shPlayer.TransferItem(1, CommonIDs[0], 500, true);
                                        player.Send(SvSendType.Self, Channel.Reliable, ClPacket.GameMessage, $"<color={infoColor}>You have given yourself 500 pistol ammo.</color>");
                                        Debug.Log(SetTimeStamp.Run() + "[INFO] " + player.playerData.username + " Spawned in 500 pistol ammo through the functionUI");
                                        item.Value.LastMenu = CurrentMenu.Main;
                                    }
                                    else if (item.Value.LastMenu == CurrentMenu.AdminReport && shPlayer.admin)
                                    {
                                        player.Send(SvSendType.Self, Channel.Reliable, ClPacket.CloseFunctionMenu);
                                        if (IsOnline.Run(item.Value.ReportedPlayer))
                                        {
                                            shPlayer.SetPosition(item.Value.ReportedPlayer.GetPosition());
                                            player.Send(SvSendType.Self, Channel.Reliable, ClPacket.GameMessage, $"<color={infoColor}>Teleported to \"</color><color=#ea8220>{item.Value.ReportedPlayer.username}</color><color={infoColor}>\".</color>");
                                        }
                                        else
                                            player.Send(SvSendType.Self, Channel.Reliable, ClPacket.GameMessage, "<color=#ff0000ff>Player not online anymore.</color>");
                                        item.Value.ReportedPlayer = null;
                                        item.Value.LastMenu = CurrentMenu.Main;
                                    }

                                    break;
                                case 3:
                                    if (item.Value.LastMenu == CurrentMenu.Staff && HasPermission.Run(player, AccessItemMenu))
                                    {
                                        player.Send(SvSendType.Self, Channel.Reliable, ClPacket.ShowFunctionMenu, "<color=#00ffffff>Give Items menu:</color>\n\n<color=#00ffffff>F2:</color> Give <color=#ea8220>500</color> Pistol Ammo\n<color=#00ffffff>F3:</color> Give <color=#ea8220>20</color> Handcuffs\n<color=#00ffffff>F4:</color> Give <color=#ea8220>10</color> Taser ammo\n<color=#00ffffff>F5:</color> Give <color=#ea8220>all</color> Licenses\n\n<color=#00ffffff>Press</color><color=#ea8220> F11 </color><color=#00ffffff>To close this (G)UI</color>");
                                        item.Value.LastMenu = CurrentMenu.GiveItems;
                                    }
                                    else if (item.Value.LastMenu == CurrentMenu.GiveMoney)
                                    {
                                        player.Send(SvSendType.Self, Channel.Reliable, ClPacket.CloseFunctionMenu);
                                        shPlayer.TransferMoney(DeltaInv.AddToMe, 10000, true);
                                        player.Send(SvSendType.Self, Channel.Reliable, ClPacket.GameMessage, $"<color={infoColor}>You have given yourself 10.000 dollars.</color>");
                                        Debug.Log(SetTimeStamp.Run() + "[INFO] " + player.playerData.username + " Spawned in 10.000 dollars through the functionUI");
                                        item.Value.LastMenu = CurrentMenu.Main;
                                        return true;
                                    }
                                    else if (item.Value.LastMenu == CurrentMenu.GiveItems)
                                    {
                                        player.Send(SvSendType.Self, Channel.Reliable, ClPacket.CloseFunctionMenu);
                                        shPlayer.TransferItem(1, CommonIDs[1], 20, true);
                                        player.Send(SvSendType.Self, Channel.Reliable, ClPacket.GameMessage, $"<color={infoColor}>You have given yourself 20 handcuffs.</color>");
                                        Debug.Log(SetTimeStamp.Run() + "[INFO] " + player.playerData.username + " Spawned in 20 handcuffs through the functionUI");
                                        item.Value.LastMenu = CurrentMenu.Main;
                                        return true;
                                    }
                                    else if (item.Value.LastMenu == CurrentMenu.AdminReport && shPlayer.admin)
                                    {
                                        player.Send(SvSendType.Self, Channel.Reliable, ClPacket.CloseFunctionMenu);
                                        item.Value.LastMenu = CurrentMenu.Main;
                                        return true;
                                    }
                                    if (item.Value.LastMenu == CurrentMenu.Main)
                                    {
                                        player.Send(SvSendType.Self, Channel.Reliable, ClPacket.ShowFunctionMenu, "<color=#00ffffff>Server info menu:</color>\n\n<color=#00ffffff>F2:</color> Show rules\n<color=#00ffffff>F3:</color> Show admins\n\n<color=#00ffffff>Press</color><color=#ea8220> F11 </color><color=#00ffffff>To close this (G)UI</color>");
                                        item.Value.LastMenu = CurrentMenu.ServerInfo;
                                    }
                                    else if (item.Value.LastMenu == CurrentMenu.ServerInfo)
                                    {
                                        player.Send(SvSendType.Self, Channel.Reliable, ClPacket.CloseFunctionMenu);

                                        var builder = new StringBuilder();
                                        builder.Append("All admins on this server:\n\n");
                                        foreach (var line in File.ReadAllLines("admin_list.txt"))
                                            if (line.Trim() != null && !line.Trim().StartsWith("#", StringComparison.OrdinalIgnoreCase))
                                                builder.Append(line + "\r\n");
                                        player.Send(SvSendType.Self, Channel.Fragmented, ClPacket.ServerInfo, builder.ToString());
                                        item.Value.LastMenu = CurrentMenu.Main;
                                    }

                                    break;
                                case 4:
                                    if (item.Value.LastMenu == CurrentMenu.GiveMoney)
                                    {
                                        item.Value.Shplayer.TransferMoney(DeltaInv.AddToMe, 100000, true);
                                        player.Send(SvSendType.Self, Channel.Reliable, ClPacket.GameMessage, $"<color={infoColor}>You have given yourself 100.000 dollars.</color>");
                                        Debug.Log(SetTimeStamp.Run() + "[INFO] " + player.playerData.username + " Spawned in 100.000 dollars through the functionUI");
                                        item.Value.LastMenu = CurrentMenu.Main;
                                    }
                                    else if (item.Value.LastMenu == CurrentMenu.Staff && HasPermission.Run(player, AccessSetHPMenu))
                                    {
                                        player.Heal(100);
                                        player.Send(SvSendType.Self, Channel.Reliable, ClPacket.GameMessage, $"<color={infoColor}>You've been healed.</color>");
                                        Debug.Log(SetTimeStamp.Run() + "[INFO] " + player.playerData.username + " healed himself through the functionUI");
                                        item.Value.LastMenu = CurrentMenu.Main;
                                    }
                                    else if (item.Value.LastMenu == CurrentMenu.GiveItems)
                                    {
                                        player.Send(SvSendType.Self, Channel.Reliable, ClPacket.CloseFunctionMenu);
                                        shPlayer.TransferItem(1, CommonIDs[2], ClPacket.GameMessage, true);
                                        player.Send(SvSendType.Self, Channel.Reliable, ClPacket.GameMessage, $"<color={infoColor}>You have given yourself 10 Taser ammo.</color>");
                                        Debug.Log(SetTimeStamp.Run() + "[INFO] " + player.playerData.username + " Spawned in 10 taser ammo through the functionUI");
                                        item.Value.LastMenu = CurrentMenu.Main;
                                    }
                                    player.Send(SvSendType.Self, Channel.Reliable, ClPacket.CloseFunctionMenu);
                                    break;
                                case 5:
                                    if (item.Value.LastMenu == CurrentMenu.Staff && HasPermission.Run(player, AccessSetStatsMenu))
                                    {
                                        player.UpdateStats(100F, 100F, 100F, 100F);
                                        player.Send(SvSendType.Self, Channel.Reliable, ClPacket.GameMessage, $"<color={infoColor}>Maxed out stats for yourself.</color>");
                                        Debug.Log(SetTimeStamp.Run() + "[INFO] " + player.playerData.username + " Maxed out stats through the functionUI");
                                        item.Value.LastMenu = CurrentMenu.Main;
                                    }
                                    else if (item.Value.LastMenu == CurrentMenu.GiveItems)
                                    {
                                        for (int i = 3; i < 7; i++)
                                            shPlayer.TransferItem(1, CommonIDs[i], 1, true);
                                        player.Send(SvSendType.Self, Channel.Reliable, ClPacket.GameMessage, $"<color={infoColor}>You have given yourself all licenses.</color>");
                                        Debug.Log(SetTimeStamp.Run() + "[INFO] " + player.playerData.username + " Spawned in all licenses through the functionUI");
                                        item.Value.LastMenu = CurrentMenu.Main;
                                    }

                                    player.Send(SvSendType.Self, Channel.Reliable, ClPacket.CloseFunctionMenu);
                                    break;
                                case 6:
                                    if (item.Value.LastMenu == CurrentMenu.Staff && HasPermission.Run(player, AccessCWMenu))
                                    {
                                        shPlayer.ClearCrimes();
                                        player.Send(SvSendType.Self, Channel.Reliable, 33, shPlayer.ID);
                                        player.Send(SvSendType.Self, Channel.Reliable, ClPacket.GameMessage, $"<color={infoColor}>Cleared wanted level.</color>");
                                        Debug.Log(SetTimeStamp.Run() + "[INFO] " + player.playerData.username + " Removed his wantedlevel through the functionUI");
                                        item.Value.LastMenu = CurrentMenu.Main;
                                    }

                                    player.Send(SvSendType.Self, Channel.Reliable, ClPacket.CloseFunctionMenu);
                                    break;
                                case 10:
                                    if (item.Value.LastMenu == CurrentMenu.Main)
                                    {
                                        var sb = new StringBuilder().Append("<color=#00ffffff>Staff menu:</color>\n\n");

                                        if (HasPermission.Run(player, AccessMoneyMenu))
                                            sb.Append("<color=#00ffffff>F2:</color> Give Money\n");
                                        if (HasPermission.Run(player, AccessItemMenu))
                                            sb.Append("<color=#00ffffff>F3:</color> Give Items\n");
                                        if (HasPermission.Run(player, AccessSetHPMenu))
                                            sb.Append("<color=#00ffffff>F4:</color> Set HP to full\n");
                                        if (HasPermission.Run(player, AccessSetStatsMenu))
                                            sb.Append("<color=#00ffffff>F5:</color> Set Stats to full\n");
                                        if (HasPermission.Run(player, AccessCWMenu))
                                            sb.Append("<color=#00ffffff>F6:</color> Clear wanted level\n\n");
                                        player.Send(SvSendType.Self, Channel.Reliable, ClPacket.ShowFunctionMenu, $"{sb}<color=#00ffffff>Press</color><color=#ea8220> F11 </color><color=#00ffffff>To close this (G)UI</color>");
                                        item.Value.LastMenu = CurrentMenu.Staff;
                                    }
                                    break;
                                default:
                                    break;
                            }
                        }
                    }
                    return true;
                }
                foreach (KeyValuePair<int, _PlayerList> item in playerList)
                    if (item.Value.Shplayer.svPlayer == player)
                        item.Value.LastMenu = CurrentMenu.Main;
                player.Send(SvSendType.Self, Channel.Reliable, ClPacket.CloseFunctionMenu);
            }
            catch (Exception ex)
            {
                ErrorLogging.Run(ex);
            }
            return true;
        }

        [Hook("SvPlayer.SvSuicide")]
        public static bool SvSuicide(SvPlayer player)
        {
            var shPlayer = player.player;
            if (BlockSuicide)
            {
                player.Send(SvSendType.Self, Channel.Unsequenced, ClPacket.GameMessage, $"<color={errorColor}>You cannot suicide on this server because the server owner disabled it.</color>");
                return true;
            }
            return false;
        }
        [Hook("SvPlayer.SvGetJob")]
        public static bool SvGetJob(SvPlayer player, ref int employerID)
        {
            try
            {
                var shPlayer = player.player;
                var shEmployer = shPlayer.manager.FindByID<ShPlayer>(employerID);
                if (WhitelistedJobs.ContainsKey(shEmployer.job.jobIndex))
                    if (!HasPermission.Run(player, WhitelistedJobs[shEmployer.job.jobIndex], false, shPlayer.job.jobIndex))
                    {
                        player.Send(SvSendType.Self, Channel.Unsequenced, ClPacket.GameMessage, MsgNoPermJob);
                        return true;
                    }
                return false;
            }
            catch (Exception ex)
            {
                ErrorLogging.Run(ex);
                return false;
            }
        }
        [Hook("SvPlayer.SvAddCrime")]
        public static bool SvAddCrime(SvPlayer player, ref byte crimeIndex, ref ShEntity victim)
        {
            try
            {
                if (GodModeLevel >= 1 && CheckGodMode.Run(player, null, "<color=#b7b5b5>Blocked crime and losing EXP!</color>"))
                    return true;
            }
            catch (Exception ex)
            {
                ErrorLogging.Run(ex);
            }
            return false;
        }
        [Hook("ShPlayer.TransferItem")]
        public static bool TransferItem(ShPlayer player, ref byte deltaType, ref int itemIndex, ref int amount, ref bool dispatch)
        {
            try
            {
                if (player != null && BlockedItems.Count > 0  && BlockedItems.Contains(itemIndex))
                {
                    player.svPlayer.Send(SvSendType.Self, Channel.Unsequenced, ClPacket.GameMessage, BlockedItemMessage);
                    return true;
                }
            }
            catch (Exception ex)
            {
                ErrorLogging.Run(ex);
            }
            return false;
        }


        [Hook("ShPlayer.RemoveItemsDeath")]
        public static bool RemoveItemsDeath(ShPlayer player)
        {
            if (!blockLicenseRemoved)
                return false;
            foreach (InventoryItem inventoryItem in player.myItems.Values.ToArray())
            {
                if (blockLicenseRemoved && inventoryItem.item.name.StartsWith("License"))
                    continue;
                int extraCount = GetExtraCount.Run(player, inventoryItem);
                if (extraCount > 0)
                {
                    var shWearable = inventoryItem.item as ShWearable;
                    if (!shWearable || shWearable.illegal || player.curWearables[(int)shWearable.type].index != shWearable.index)
                        player.TransferItem(2, inventoryItem.item.index, extraCount, true);
                }
            }
            if (blockLicenseRemoved)
                player.svPlayer.Send(SvSendType.Self, Channel.Unsequenced, ClPacket.GameMessage, $"<color={warningColor}>This server disabled losing licenses on death.</color>");
            return true;
        }

        [Hook("ShMovable.ShDie")]
        public static void ShDie(ShMovable shMoveable)
        {
            shMoveable.CleanUp();
            shMoveable.ResetInputs();
            shMoveable.health = 0f;
            if (shMoveable.manager.isServer)
                if (!shMoveable.svEntity.respawnable)
                    shMoveable.Destroy();
                else
                    shMoveable.StartCoroutine(shMoveable.svMovable.RespawnDelay());
        }
        [Hook("SvPlayer.SvPlaceInJail")]
        public static void SvPlaceInJail(SvPlayer player, ref int criminalID)
        {
            var shPlayer = player.player;
            if (shPlayer.manager.jail && shPlayer.job is Police)
            {
                var crimShPlayer = player.entity.manager.FindByID<ShPlayer>(criminalID);
                if (!crimShPlayer)
                    return;
                if (player.serverside || crimShPlayer.DistanceSqr(player.player.manager.jail) < 14400f)
                {
                    var jailTime = 0f;
                    var Fine = 0;
                    foreach (var offense in crimShPlayer.offenses)
                    {
                        jailTime += offense.GetCrime().jailtime;
                        Fine += offense.GetCrime().fine;
                    }
                    SendToJail.Run(crimShPlayer, jailTime);
                    if (Fine > 0)
                        player.Reward(3, Fine);
                    if (ShowJailMessage)
                        player.Send(SvSendType.All, Channel.Unsequenced, ClPacket.GameMessage, $"<color={argColor}>{player.player.username}</color> <color={infoColor}>sent</color> <color={argColor}>{crimShPlayer.username}</color> <color={infoColor}>to jail{(Fine > 0 ? $" for a fine of</color> <color={argColor}>${Fine}</color>" : ".</color>")}");
                    return;
                }
                player.Send(SvSendType.Self, Channel.Unsequenced, ClPacket.GameMessage, "Confirm criminal is cuffed and near jail");
            }
        }
    }
}