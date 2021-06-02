using System;
using System.IO;

namespace DLC_Checker
{
	public class GameData
	{
        static readonly System.Version VERSION = typeof(Program).Assembly.GetName().Version;
        public static readonly string GAME_NAME = "COM3D2_EN";
        public static readonly string INI_FILE = GAME_NAME + "_DLC_Checker.ini";
        public static readonly string DLC_LIST_FILE = "COM_EN_NewListDLC.lst";
        public static readonly string MY_DLC_LIST_FILE = "MY_" + DLC_LIST_FILE;
        public static readonly string GAME_HEADER = "         " + GAME_NAME + "_Checker Version " + VERSION + "     |   Github.com/MeidosFriend/" + GAME_NAME + "_Checker";
        public static readonly string GAME_REGISTRY = "HKEY_CURRENT_USER\\SOFTWARE\\KISS\\" + "CUSTOM ORDER MAID3D 2";
        
        public static string DLC_URL = "https://raw.githubusercontent.com/MeidosFriend/" + GAME_NAME + "_DLC_Checker/master/" + DLC_LIST_FILE;
        public static string DLC_LIST_PATH = Path.Combine(Directory.GetCurrentDirectory(), DLC_LIST_FILE);
        
        public static string UseCurrentDir;
        public static string UpdateListFile;
        public static string MyDLCListFile;
        public static string MyURL;
        public static string UseMyURL;

        public GameData()
		{
            IniFile MyIni = new IniFile();
            if (!MyIni.KeyExists("UseCurrentDir", "GameDirectory"))
            {
                MyIni.Write("UseCurrentDir", "No", "GameDirectory");
            }
            UseCurrentDir = MyIni.Read("UseCurrentDir", "GameDirectory");

            if (!MyIni.KeyExists("UpdateListFile", "DLCListFile"))
            {
                MyIni.Write("UpdateListFile", "Yes", "DLCListFile");
            }
            UpdateListFile = MyIni.Read("UpdateListFile", "DLCListFile");

            if (!MyIni.KeyExists("MyDLCListFile", "DLCListFile"))
            {
                MyIni.Write("MyDLCListFile", "No", "DLCListFile");
            }
            MyDLCListFile = MyIni.Read("MyDLCListFile", "DLCListFile");

            if (!MyIni.KeyExists("MyURL", "CustomURL"))
            {
                MyIni.Write("MyURL", DLC_URL, "CustomURL");
            }
            MyURL = MyIni.Read("MyURL", "CustomURL");

            if (!MyIni.KeyExists("UseMyURL", "CustomURL"))
            {
                MyIni.Write("UseMyURL", "No", "CustomURL");
            }
            UseMyURL = MyIni.Read("UseMyURL", "CustomURL");

            if (MyDLCListFile == "Yes")
            {
                DLC_LIST_PATH = Path.Combine(Directory.GetCurrentDirectory(), MY_DLC_LIST_FILE);
            }

            if (UseMyURL == "Yes")
            {
                DLC_URL = MyURL;
            }

        }
        public string GetUpdateListFile()
        {
            return UpdateListFile;
        }
        public string GetMyDLCListFile()
        {
            return MyDLCListFile;
        }
        public string GetUseCurrentDir()
        {
            return UseCurrentDir;
        }

        public string GetUseMyURL()
        {
            return UseMyURL;
        }

        public string GetMyURL()
        {
            return MyURL;
        }

        public string GetGAME_NAME()
        {
            return GAME_NAME;
        }
        public string GetINI_FILE()
        {
            return INI_FILE;
        }
        public string GetDLC_LIST_FILE()
        {
            return DLC_LIST_FILE;
        }

        public string GetMY_DLC_LIST_FILE()
        {
            return MY_DLC_LIST_FILE;
        }

        public string GetGAME_HEADER()
        {
            return GAME_HEADER;
        }
        public string GetDLC_URL()
        {
            return DLC_URL;
        }
        public string GetGAME_REGISTRY()
        {
            return GAME_REGISTRY;
        }

        public string GetDLC_LIST_PATH()
        {
            return DLC_LIST_PATH;
        }

        


    }
}