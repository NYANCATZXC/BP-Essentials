﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using static BP_Essentials.EssentialsVariablesPlugin;
using static BP_Essentials.EssentialsMethodsPlugin;
using System.Threading;
using System.IO;

namespace BP_Essentials
{
    class CheckBanned : EssentialsCorePlugin
    {
        public static void Run(object oPlayer)
        {
            Thread.Sleep(3000);
            try
            {
                var player = (SvPlayer)oPlayer;
                if (File.ReadAllText(BansFile).Contains(player.playerData.username))
                {
                    Debug.Log(SetTimeStamp.Run() + "[WARNING] " + player.playerData.username + " Joined while banned! IP: " + player.netMan.GetAddress(player.connection));
                    foreach (var shPlayer in UnityEngine.Object.FindObjectsOfType<ShPlayer>())
                    {
                        if (shPlayer.svPlayer == player)
                        {
                            if (shPlayer.IsRealPlayer())
                            {
                                player.netMan.AddBanned(shPlayer);
                                player.netMan.Disconnect(player.connection);
                            }
                        }
                    }

                }
            }
            catch (Exception ex)
            {
                ErrorLogging.Run(ex);
            }
        }
    }
}