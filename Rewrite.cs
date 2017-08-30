// Essentials created by UserR00T
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

public class EssentialsPlugin
{

    #region Folder Locations
    //private static string DirectoryFolder = Directory.GetCurrentDirectory();
    private static string SettingsFile = "essentials_settings.txt";
    private static string LanguageBlockFile = "languageblock.txt";
    private static string ChatBlockFile = "chatblock.txt";
    private static string AnnouncementsFile = "announcements.txt";
    private static string IPListFile = "ip_list.txt";
    private static string AdminListFile = "admin_list.txt";
    private static string GodListFile = "godlist.txt";
    private static string AfkListFile = "afklist.txt";
    private static string MuteListFile = "mutelist.txt";
    #endregion

    #region predefining variables

    // General
    private static string version;
    private static bool msgUnknownCommand;
    private static bool ChatBlock;
    private static bool LanguageBlock;
    private static string command;
    private static SvPlayer player;
    private static string msgSayPrefix;
    private static ShPlayer Splayer;


    // Block arrays
    private static string[] ChatBlockWords;
    private static string[] LanguageBlockWords;
    private static string[] GodListPlayers;
    private static string[] AfkPlayers;
    private static string[] MutePlayers;
    //private static string[] admins;
    private static List<string> admins;


    // Messages
    private static string msgNoPerm;

    // Commands
    private static string cmdCommandCharacter;

    private static string cmdClearChat;
    private static string cmdClearChat2;

    private static string cmdSay;
    private static string cmdSay2;

    private static string cmdGodmode;
    private static string cmdGodmode2;

    private static string cmdMute;
    private static string cmdUnMute;

    private static string cmdReload;
    private static string cmdReload2;

    private static string arg1ClearChat;

    private static int announceIndex = 0;
    private static string[] announcements;
    private static int TimeBetweenAnnounce;
    private static string TimestampFormat;
    #endregion

    //Code below here, Don't edit unless you know what you're doing.
    //Information about the api @ https://github.com/deathbykorea/universalunityhooks

    [Hook("SvNetMan.StartServerNetwork")]
    public static void StartServerNetwork(SvNetMan netMan)
    {

        try
        {
            ReadFile(SettingsFile);


            //  Debug.Log("Manually setting variables");

            Debug.Log("[INFO] Essentials - version: " + version + " Loaded in successfully!");
        }
        catch (Exception ex)
        {
            Debug.Log("[WARNING] Essentials - Settings file does not exist! Make sure to place settings.txt in the game directory! pwd: " + Directory.GetCurrentDirectory() + "settings: " + SettingsFile);
            Debug.Log(ex); //remove when debugging is done
        }

        if (!File.Exists(AnnouncementsFile))
        {
            Debug.Log(SetTimeStamp() + "[WARNING] Annoucements file doesn't exist! Please create " + AnnouncementsFile + " in the game directory");
        }
        Thread thread = new Thread(new ParameterizedThreadStart(AnnounceThread));
        thread.Start(netMan);

        Debug.Log(SetTimeStamp() + "[INFO] Announcer started successfully!");
    }

    //Chat Events
    [Hook("SvPlayer.SvGlobalChatMessage")]
    public static bool SvGlobalChatMessage(SvPlayer player, ref string message)
    {
        MessageLog(message, player);


        // Checks if player is muted, if so, cancel message
        /*        if (MutePlayers.Contains(player.playerData.username))
                {
                    player.SendToSelf(Channel.Unsequenced, (byte)10, "You are muted now.");
                    return true;
                }*/
        // Checks if the message is a command, if not, log it

        //return false;
        // Checks if the message is a blocked one, if it is, block it.
        /*        if (ChatBlock)
                {
                    if (ChatBlockWords.Any(message.ToLower().Contains))
                    {
                        bool blocked = BlockMessage(message, player);
                        if (blocked)
                        {
                            return true;
                        }
                        else
                        {
                            return false;
                        }
                    }
                }
                // Check if the message is English, if not, block it
                if (LanguageBlock)
                {
                    if (LanguageBlockWords.Any(message.ToLower().Contains))
                    {
                        bool blocked = BlockMessage(message, player);
                        if (blocked)
                        {
                            return true;
                        }
                        else
                        {
                            return false;
                        }
                    }
                }*/


        // Clear chat command, self and global
        if (message.StartsWith(cmdClearChat) || message.StartsWith(cmdClearChat))
        {
            if (message == cmdClearChat || message == cmdClearChat2)
            {
                ClearChat(message, player, true);
                return true;
            }
            arg1ClearChat = message.Substring(11);
            if (arg1ClearChat == "all" || arg1ClearChat == "everyone")
            {
                ClearChat(message, player, false);
                return true;
            }
        }
        // Reload command
        if (message.StartsWith(cmdReload) || message.StartsWith(cmdReload2))
        {
            Reload(message, player);
            return true;
        }

        if (message.StartsWith(cmdMute) || message.StartsWith(cmdUnMute)) // <broke
        {
            return Mute(message, player);
        }
        if (message.StartsWith(cmdSay) || (message.StartsWith(cmdSay2))) // <broke
        {
            return say(message, player);
        }

        if (message.StartsWith(cmdGodmode) || message.StartsWith(cmdGodmode2))// <broke
        {
            return godMode(message, player);
        }

        // Info command
        if (message.StartsWith("/essentials") || message.StartsWith("/ess"))
        {
            essentials(message, player);
            return true;
        }
        return false;
        if (msgUnknownCommand)
        {
            player.SendToSelf(Channel.Unsequenced, (byte)10, "Unknown command");
            return true;
        }
        else
        {
            return false;
        }
    }

    // These are the various functions for the commands.

    private static void AnnounceThread(object man)
    {
        SvNetMan netMan = (SvNetMan)man;
        while (true)
        {
            foreach (var player in netMan.players)
            {
                player.svPlayer.SendToSelf(Channel.Reliable, ClPacket.GameMessage, announcements[announceIndex]);
            }

            Debug.Log(SetTimeStamp() + "[INFO] Announcement made...");

            announceIndex += 1;
            if (announceIndex > announcements.Length - 1)
                announceIndex = 0;
            Thread.Sleep(TimeBetweenAnnounce * 1000);
        }
    }

    public static void MessageLog(string message, object oPlayer)
    {
        SvPlayer player = (SvPlayer)oPlayer;
        if (!message.StartsWith(cmdCommandCharacter))
        {
            Debug.Log(SetTimeStamp() + "[MESSAGE] " + player.playerData.username + ": " + message);
        }
        else if (message.StartsWith(cmdCommandCharacter))
        {
            Debug.Log(SetTimeStamp() + "[COMMAND] " + player.playerData.username + ": " + message);
        }
    }

    public static bool Mute(string message, object oPlayer)
    {
        SvPlayer player = (SvPlayer)oPlayer;

        string muteuser = message.Split(' ').Last();

        if (admins.Contains(player.playerData.username))
        {

            if (MutePlayers.Contains(muteuser))
            {
                ReadFile(MuteListFile);
                RemoveStringFromFile(MuteListFile, player.playerData.username);
                player.SendToSelf(Channel.Unsequenced, (byte)10, muteuser + " Unmuted");
                return true;

            }
            else
            {
                File.AppendAllText(MuteListFile, muteuser + Environment.NewLine);
                player.SendToSelf(Channel.Unsequenced, (byte)10, muteuser + " Unmuted");
                return true;
            }
        }
        else
        {
            player.SendToSelf(Channel.Unsequenced, (byte)10, msgNoPerm);
            return false;
        }
    }

    public static bool BlockMessage(string message, object oPlayer)
    {
        SvPlayer player = (SvPlayer)oPlayer;
        if (ChatBlock == true)
        {

            if (ChatBlockWords.Any(message.ToLower().Contains))
            {
                player.SendToSelf(Channel.Unsequenced, (byte)10, "Please don't say a blocked word, the message has been blocked.");
                Debug.Log(SetTimeStamp() + player.playerData.username + " Said a word that is blocked.");
                return true;
            }
        }

        if (admins.Contains(player.playerData.username))
        {
            player.SendToSelf(Channel.Unsequenced, (byte)10, "Because you are staff, your message has NOT been blocked.");
            return false;
        }
        else
        {
            player.SendToSelf(Channel.Unsequenced, (byte)10, "--------------------------------------------------------------------------------------------");
            player.SendToSelf(Channel.Unsequenced, (byte)10, "             ?olo ingl�s! Tu mensaje ha sido bloqueado.");
            player.SendToSelf(Channel.Unsequenced, (byte)10, "             Only English! Your message has been blocked.");
            player.SendToSelf(Channel.Unsequenced, (byte)10, "--------------------------------------------------------------------------------------------");
            return true;
        }
    }

    public static bool godMode(string message, object oPlayer)
    {
        try
        {
        SvPlayer player = (SvPlayer)oPlayer;

            if (admins.Contains(player.playerData.username))
            {
                if (GodListPlayers.Contains(player.playerData.username))
                {
                    ReadFile(GodListFile);
                    RemoveStringFromFile(GodListFile, player.playerData.username);
                    player.SendToSelf(Channel.Unsequenced, (byte)10, "Godmode disabled.");
                    return true;
                }
                else
                {
                    File.AppendAllText(GodListFile, player.playerData.username + Environment.NewLine);
                    player.SendToSelf(Channel.Unsequenced, (byte)10, "Godmode enabled.");
                    return true;
                }
            }
            else
            {
                player.SendToSelf(Channel.Unsequenced, (byte)10, msgNoPerm);
                return false;
            }

        }
        catch (Exception ex)
        {

            LogExpection(ex, "godMode");
            player.SendToSelf(Channel.Unsequenced, (byte)10, "Unknown error. Check console for more info");
            return true;
        }

    }
    public static void ClearChat(string message, object oPlayer, bool self)
    {
        
        try
        {
            SvPlayer player = (SvPlayer)oPlayer;
            if (self)
            {
                player.SendToSelf(Channel.Unsequenced, (byte)10, "Clearing the chat for yourself...");
                Thread.Sleep(250);
                for (int i = 0; i < 6; i++)
                {
                    player.SendToSelf(Channel.Unsequenced, (byte)10, " ");
                }
            }
            else if (!(self))
            {
                if (admins.Contains(player.playerData.username))
                {
                    SvPlayer svPlayer = (SvPlayer)player;
                    player.SendToAll(Channel.Unsequenced, (byte)10, "Clearing chat for everyone...");
                    Thread.Sleep(250);
                    for (int i = 0; i < 6; i++)
                    {
                        player.SendToAll(Channel.Unsequenced, (byte)10, " ");
                    }

                }
                else
                {
                    player.SendToSelf(Channel.Unsequenced, (byte)10, msgNoPerm);
                }
            }
            else
            {
                player.SendToSelf(Channel.Unsequenced, (byte)10, @"'" + arg1ClearChat + @"'" + " Is not a valid argument.");
            }
        }
        catch (Exception ex)
        {
            LogExpection(ex, "ClearChat");
            player.SendToSelf(Channel.Unsequenced, (byte)10, "Unknown error occured. Please check output_log.txt for more info.");
        }

    }
    public static void LogExpection(Exception ex, string Sender)
    {
        Debug.Log(SetTimeStamp() + "[ERROR] [" + Sender + "] An unknown error occured. Expection: " + ex.ToString());
        Debug.Log(SetTimeStamp() + "[ERROR] [" + Sender + "] Please post the error on GitHub please!");
        Debug.Log(SetTimeStamp() + "[ERROR] [" + Sender + "] Try reinstalling the newest version.");
    }
    public static void Reload(string message, object oPlayer)
    {
        SvPlayer player = (SvPlayer)oPlayer;
        if (admins.Contains(player.playerData.username))
        {
            player.SendToSelf(Channel.Unsequenced, (byte)10, "Reloading config files...");
            ReadFile(SettingsFile);
            player.SendToSelf(Channel.Unsequenced, (byte)10, "[OK] Config file reloaded");
            Thread.Sleep(50);
            player.SendToSelf(Channel.Unsequenced, (byte)10, "Reloading language and chat block files..");
            ReadFile(LanguageBlockFile);
            Thread.Sleep(10);
            ReadFile(ChatBlockFile);
            player.SendToSelf(Channel.Unsequenced, (byte)10, "[OK] Language and chat block files reloaded");
        }
        else
        {
            player.SendToSelf(Channel.Unsequenced, (byte)10, msgNoPerm);
        }
    }

    public static void essentials(string message, object oPlayer)
    {
        SvPlayer player = (SvPlayer)oPlayer;
        player.SendToSelf(Channel.Unsequenced, (byte)10, "Essentials Created by UserR00T");
        player.SendToSelf(Channel.Unsequenced, (byte)10, "Version " + version);

        //TODO: Subcommands like /essentials reload : executes cmdReload

    }
    public static bool say(string message, object oPlayer)
    {
        SvPlayer player = (SvPlayer)oPlayer;
        try
        {

            if (admins.Contains(player.playerData.username))
            {
                if ((message.Length == cmdSay.Length) || (message.Length == cmdSay2.Length))
                {
                    player.SendToSelf(Channel.Unsequenced, (byte)10, "An argument is required for this command.");
                    return true;
                }
                else
                {
                    string arg1 = null;
                    if (message.StartsWith(cmdSay))
                    {
                        arg1 = message.Substring(cmdSay.Length);
                    }
                    else if (message.StartsWith(cmdSay2))
                    {
                        arg1 = message.Substring(cmdSay2.Length);
                    }
                    player.SendToAll(Channel.Unsequenced, (byte)10, msgSayPrefix + player.playerData.username + ": " + arg1);
                    return true;
                }
            }
            else
            {
                player.SendToSelf(Channel.Unsequenced, (byte)10, msgNoPerm);
                return false;
            }

        }
        catch (Exception ex)
        {
            LogExpection(ex, "Say");
            player.SendToSelf(Channel.Unsequenced, (byte)10, "Unknown error. Check the log for more info");
            return true;
        }
    }

    [Hook("SvPlayer.Initialize")]
    public static void Initialize(SvPlayer player)
    {
        if (player.playerData.username != null)
        {
            Thread thread = new Thread(new ParameterizedThreadStart(WriteIPToFile));
            thread.Start(player);
        }
    }
    private static void WriteIPToFile(object oPlayer)
    {
        Thread.Sleep(500);
        SvPlayer player = (SvPlayer)oPlayer;
        Debug.Log(SetTimeStamp() + "[INFO] " + "[JOIN] " + player.playerData.username + " IP is: " + player.netMan.GetAddress(player.connection));
        try
        {
            if (!File.ReadAllText(IPListFile).Contains(player.playerData.username + ": " + player.netMan.GetAddress(player.connection)))
            {
                File.AppendAllText(IPListFile, player.playerData.username + ": " + player.netMan.GetAddress(player.connection) + Environment.NewLine);
            }
        }
        catch (Exception ex)
        {
            LogExpection(ex, "WriteIPToFile");
        }

    }
    public static void RemoveStringFromFile(string FileName, string RemoveString)
    {
        try
        {
            var tempFile = Path.GetTempFileName();
            var linesToKeep = File.ReadLines(FileName).Where(l => l != RemoveString);

            File.WriteAllLines(tempFile, linesToKeep);

            File.Delete(FileName);
            File.Move(tempFile, FileName);
        }
        catch (Exception ex)
        {
            LogExpection(ex, "RemoveStringFromFile");
        }

    }
    public static string SetTimeStamp()
    {
        try
        {
            var src = DateTime.Now;
            var hm = new DateTime(src.Year, src.Month, src.Day, src.Hour, src.Minute, src.Second);
            string PlaceHolderText = TimestampFormat;
            string Hours = hm.ToString("HH");
            string Minutes = hm.ToString("mm");
            string Seconds = hm.ToString("ss");

            if (TimestampFormat.Contains("{H}"))
            {
                PlaceHolderText = PlaceHolderText.Replace("{H}", hm.ToString("HH"));
            }
            if (TimestampFormat.Contains("{h}"))
            {
                PlaceHolderText = PlaceHolderText.Replace("{h}", hm.ToString("hh"));
            }
            if (TimestampFormat.Contains("{M}") || TimestampFormat.Contains("{m}"))
            {
                PlaceHolderText = PlaceHolderText.Replace("{M}", Minutes);
            }
            if (TimestampFormat.Contains("{S}") || TimestampFormat.Contains("{s}"))
            {
                PlaceHolderText = PlaceHolderText.Replace("{S}", Seconds.ToString());
            }

            PlaceHolderText = PlaceHolderText + " ";
            return PlaceHolderText;
        }
        catch (Exception)
        {
            return "[Failed] ";
        }
    }
    static void ReadFile(string FileName)
    {
        Debug.Log("ReadFileStarted");
        if (FileName == SettingsFile)
        {
            foreach (var line in File.ReadAllLines(SettingsFile))
            {
                if (line.StartsWith("#") || line.Contains("#"))
                {
                    continue;
                }
                else
                {
                    // TODO: make this better/compacter
					if (line.Contains ("version: ")) {
						version = line.Substring (9);
						Debug.Log ("Version: " + version);
					} else if (line.Contains ("CommandCharacter: ")) {
						cmdCommandCharacter = line.Substring (17);
						Debug.Log ("CmdChar: " + cmdCommandCharacter);

					} else if (line.Contains ("noperm: ")) {
						msgNoPerm = line.Substring (8);
						Debug.Log ("NoPerm: " + msgNoPerm);
					} else if (line.Contains ("ClearChatCommand: ")) {
						cmdClearChat = cmdCommandCharacter + line.Substring (18);
						Debug.Log ("ClearChat: " + cmdClearChat);
					} else if (line.Contains ("ClearChatCommand2: ")) {
						cmdClearChat2 = cmdCommandCharacter + line.Substring (19);
						Debug.Log ("ClearChat2: " + cmdClearChat2);

					} else if (line.Contains ("SayCommand: ")) {	
						cmdSay = line.Substring (12);
						Debug.Log ("CmdSay: " + cmdSay);

					} else if (line.Contains ("SayCommand2:")) {
						cmdSay2 = line.Substring (13);
						Debug.Log ("CmdSay2: " + cmdSay2);
					} else if (line.Contains ("msgSayPrefix: ")) {
						msgSayPrefix = line.Substring (14);
						Debug.Log ("MsgSayPrefix: " + msgSayPrefix);
					} else if (line.Contains ("GodmodeCommand: ")) {
						cmdGodmode = line.Substring (16);
						Debug.Log ("GodmodeCommand: " + cmdGodmode); 
					} else if (line.Contains ("GodmodeCommand2: ")) {
						cmdGodmode2 = line.Substring (17);
						Debug.Log ("GodmodeCommand2: " + cmdGodmode2);
					} else if (line.Contains ("MuteCommand: ")) {
						cmdMute = line.Substring (13);
						Debug.Log ("MuteCommand: " + cmdMute);
					} else if (line.Contains ("UnMuteCommand: ")) {
						cmdUnMute = line.Substring (15);
						Debug.Log ("UnMuteCommand: " + cmdUnMute);
					}
                    else if (line.Contains("UnknownCommand: "))
                    {
                        msgUnknownCommand = Convert.ToBoolean(line.Substring(16));
						Debug.Log ("Unknown: " + msgUnknownCommand);
                    }
                    else if (line.Contains("enableChatBlock: "))
                    {
                        ChatBlock = Convert.ToBoolean(line.Substring(17));
						Debug.Log ("ChatBlock?: " + ChatBlock);
                    }
                    else if (line.Contains("enableLanguageBlock: "))
                    {
                        LanguageBlock = Convert.ToBoolean(line.Substring(21));
						Debug.Log ("LangBlock?: " + LanguageBlock);
                    }
                    else if (line.Contains("ReloadCommand: "))
                    {
                        cmdReload = cmdCommandCharacter + line.Substring(15);
						Debug.Log ("Reload: " + cmdReload);

                    }
                    else if (line.Contains("ReloadCommand2: "))
                    {
                        cmdReload2 = cmdCommandCharacter + line.Substring(16);
						Debug.Log ("Reload2: " + cmdReload2);
                    }
                    else if (line.Contains("TimeBetweenAnnounce: "))
                    {
                        TimeBetweenAnnounce = Int32.Parse(line.Substring(21));
						Debug.Log ("AnnounceTime: " + TimeBetweenAnnounce);
                    }
                    else if (line.Contains("TimestapFormat: "))
                    {
                        TimestampFormat = line.Substring(16);
						Debug.Log ("TimestampFormat: " + TimestampFormat);
                    }


                }

                //else if (line.Contains(""))
                //{
                //    = line.Substring();
                //}

            }
        }
        else if (FileName == LanguageBlockFile)
        {
            LanguageBlockWords = File.ReadAllLines(FileName);
        }
        else if (FileName == ChatBlockFile)
        {
            ChatBlockWords = File.ReadAllLines(FileName);
        }
        else if (FileName == AnnouncementsFile)
        {
            announcements = File.ReadAllLines(AnnouncementsFile);
        }
        else if (FileName == AdminListFile)
        {
            foreach (var line in File.ReadAllLines(AdminListFile))
            {
                if (line.StartsWith("#") || line.Contains("#"))
                {
                    continue;
                }
                else
                {
                    admins.Add(line);
                }
            }
        }
        else if (FileName == GodListFile)
        {
            GodListPlayers = File.ReadAllLines(FileName);
        }
        else if (FileName == AfkListFile)
        {
            AfkPlayers = File.ReadAllLines(FileName);
        }
        else if (FileName == MuteListFile)
        {
            MutePlayers = File.ReadAllLines(FileName);
        }

    }
}