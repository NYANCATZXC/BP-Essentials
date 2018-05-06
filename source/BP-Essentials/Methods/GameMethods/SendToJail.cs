﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using static BP_Essentials.EssentialsVariablesPlugin;
using static BP_Essentials.EssentialsMethodsPlugin;

namespace BP_Essentials
{
    class SendToJail : EssentialsChatPlugin
    {
        public static bool Run(ShPlayer shPlayer, float time)
        {
            if (shPlayer.IsDead())
                return false;
            Transform jailSpawn = shPlayer.manager.jail.jailSpawn;
            SetJob.Run(shPlayer, 2, true, false);
            shPlayer.svEntity.SvReset(jailSpawn.position, jailSpawn.rotation, 0);
            shPlayer.StartCoroutine(shPlayer.svPlayer.JailTimer(time));
            shPlayer.SendToJail();
            shPlayer.svPlayer.SendToSelf(Channel.Reliable, 39, time);
            return true;
        }
    }
}