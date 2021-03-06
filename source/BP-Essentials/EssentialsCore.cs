﻿/*


    BP:Essentials

    Created by UserR00T, DBK, and BP.
    Currently only being worked on by UserR00T unfortunately. :(

    License: GPLv3.


*/


using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using UnityEngine;
using static BP_Essentials.EssentialsVariablesPlugin;
namespace BP_Essentials
{
    public class EssentialsCorePlugin
    {

        //Initialization
        [Hook("SvManager.StartServer")]
        public static void StartServer(SvManager svManager)
        {
            try
            {
                SvMan = svManager;
                Kits.StartKitTimer();
                Reload.Run(true, null, true);
                CheckAutoReloadFile.Run(AutoReloader);
                if (EssentialsVariablesPlugin.Version != LocalVersion)
                {
                    Debug.Log("[ERROR] Essentials - Versions do not match!");
                    Debug.Log("[ERROR] Essentials - Essentials version: " + EssentialsVariablesPlugin.Version);
                    Debug.Log("[ERROR] Essentials - Settings file version: " + LocalVersion);
                    Debug.Log("");
                    Debug.Log("");
                    Debug.Log("[ERROR] Essentials - Recreating settings file!");
                    string date = DateTime.Now.ToString("yyyy_mm_dd_hh_mm_ss");
                    if (File.Exists(SettingsFile + "." + date + ".OLD"))
                        File.Delete(SettingsFile + "." + date + ".OLD");
                    File.Move(SettingsFile, $"{SettingsFile}.{date}.OLD");
                    Reload.Run(true);

                }
                Save.StartSaveTimer();
                Debug.Log("-------------------------------------------------------------------------------");
                Debug.Log($"[INFO] Essentials - version: {LocalVersion} {(isPreRelease ? "[PRE-RELEASE]" : "")} Loaded in successfully!");
                Debug.Log("-------------------------------------------------------------------------------");
            }
            catch (Exception ex)
            {
                Debug.Log("-------------------------------------------------------------------------------");
                Debug.Log("    ");
                Debug.Log("[ERROR]   Essentials - A file cannot be loaded in!");
                Debug.Log("[ERROR]   Essentials - Please check the error below for more info,");
                Debug.Log("[ERROR]   Essentials - And it would be highly appreciated if you would send the error to the developers of this plugin!");
                Debug.Log("    ");
                Debug.Log(ex);
                Debug.Log(ex.ToString());
                Debug.Log("-------------------------------------------------------------------------------");

            }
            if (Announcements.Length != 0)
            {
                Chat.Announce.Run();
                Debug.Log(SetTimeStamp.Run() + "[INFO] Announcer started successfully!");
            }
            else
                Debug.Log(SetTimeStamp.Run() + "[WARNING] No announcements found in the file!");
        }
    }
}