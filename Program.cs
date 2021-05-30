using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using Microsoft.Win32;

namespace COM3D2_EN_DLC_Checker
{

    class Program
    {

        // Variables
        static readonly string GAME_NAME = "COM3D2_EN";
        static readonly string INI_FILE = GAME_NAME + "_DLC_Checker.ini";
        static readonly string MY_DLC_LST_FILE = "MY_COM_EN_NewListDLC.lst";
        static readonly string GAME_HEADER = "         COM3D2_EN_DLC_Checker   |   Github.com/MeidosFriend/COM3D2_EN_DLC_Checker";
        static readonly string DLC_URL = "https://raw.githubusercontent.com/MeidosFriend/COM3D2_EN_DLC_Checker/master/COM_EN_NewListDLC.lst";

        static readonly string DLC_LST_FILE = "COM_EN_NewListDLC.lst";
        static string DLC_LIST_PATH = Path.Combine(Directory.GetCurrentDirectory(), DLC_LST_FILE);

        // ini File default
        static string UseCurrentDir = "No";
        static string UpdateListFile = "Yes";
        static string MyDLCListFile = "No";

        const string GAME_REGISTRY = "SOFTWARE\\KISS\\CUSTOM ORDER MAID3D 2";
        static void Main(string[] args)
        {
            // Initialize ini File
            GetIniFile(ref UseCurrentDir, ref UpdateListFile, ref MyDLCListFile);

            // Write Header Lines to Console
            PRINT_HEADER();

            // Custom Listfile
            if (MyDLCListFile == "Yes")
            {
                DLC_LIST_PATH = Path.Combine(Directory.GetCurrentDirectory(), MY_DLC_LST_FILE);
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine("Using Custom Listfile {0}", MY_DLC_LST_FILE);
            }
            // Standard Listfile
            else
            {
                // Loading new ListFile from Internet or use Local File
                if (UpdateListFile == "Yes")
                {
                    // HTTP_RESOPOND
                    //  - Item1 = HTTP Status Code
                    //  - Item2 = Internet DLC List content
                    Tuple<HttpStatusCode, string> HTTP_RESPOND = CONNECT_TO_INTERNET(DLC_URL);
                    // Internet Connection OK
                    if (HTTP_RESPOND.Item1 == HttpStatusCode.OK)
                    {
                        Console.ForegroundColor = ConsoleColor.Cyan;
                        Console.WriteLine("Connected to {0}", DLC_URL);
                        //Console.WriteLine("Hint: You can preserve your current {0} by disable autoupdate in {1} with: UpdateListFile=No", DLC_LST_FILE, INI_FILE);
                        UPDATE_DLC_LIST(HTTP_RESPOND.Item2);
                    }
                    // Internet Connection NOK
                    else
                    {
                        Console.ForegroundColor = ConsoleColor.Yellow;
                        Console.WriteLine("Can't connect to internet, offline file will be used");
                    }
                }
                // Autoupdate disabled
                else
                {
                    Console.ForegroundColor = ConsoleColor.Yellow;
                    Console.WriteLine("Updating {0} disabled", DLC_LST_FILE);
                    //Console.ForegroundColor = ConsoleColor.Cyan;
                    //Console.WriteLine("Hint: You can enable autoupdate in {0} with: UpdateListFile=Yes", INI_FILE);
                }
            }

            // Print Game Directory
            if (UseCurrentDir=="No")
            {
                Console.ForegroundColor = ConsoleColor.Cyan;
                //Console.WriteLine("Game Dir: " + GET_GAME_INSTALLPATH());
            }
            else
            {
                Console.ForegroundColor = ConsoleColor.Yellow;
                //Console.WriteLine("Game Dir: " + GET_GAME_INSTALLPATH());
            }
            //Console.WriteLine("Game Dir: " + GET_GAME_INSTALLPATH());

            // DLC LIST = [DLC_FILENAME, DLC_NAME]
            IDictionary<string, string> DLC_LIST = READ_DLC_LIST();
            List<string> GAMEDATA_LIST = READ_GAMEDATA();

            // DLC LIST SORTED
            // Item 1 = INSTALLED_DLC
            // Item 2 = NOT_INSTALLED_DLC
            Tuple<List<string>, List<string>> DLC_LIST_SORTED = COMPARE_DLC(DLC_LIST, GAMEDATA_LIST);

            PRINT_DLC(DLC_LIST_SORTED.Item1, DLC_LIST_SORTED.Item2);

            EXIT_PROGRAM();
        }

	static void GetIniFile(ref string UseCurrentDir, ref string UpdateListFile, ref string MyDLCListFile)
	{
		// Creates or loads an INI file in the same directory as your executable
        // named EXE.ini (where EXE is the name of your executable)
		// Key, {Value}, Section            	

		var MyIni = new IniFile();
        if (!MyIni.KeyExists("UseCurrentDir", "GameDirectory"))
        {
           	MyIni.Write("UseCurrentDir", "No", "GameDirectory");
        }
		UseCurrentDir = MyIni.Read("UseCurrentDir","GameDirectory");

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
    }

        static void PRINT_HEADER()
        {
            CONSOLE_COLOR(ConsoleColor.Green, "===========================================================================================");
            CONSOLE_COLOR(ConsoleColor.Green, GAME_HEADER);
            CONSOLE_COLOR(ConsoleColor.Green, "===========================================================================================");
        }

        static Tuple<HttpStatusCode, string> CONNECT_TO_INTERNET(string DLC_URL)
        {
            HttpWebRequest httpWebRequest = (HttpWebRequest)WebRequest.Create(DLC_URL);
            HttpWebRequest request = httpWebRequest;
            request.AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate;

            try
            {
                using HttpWebResponse response = (HttpWebResponse)request.GetResponse() ;
                using Stream stream = response.GetResponseStream();
                using StreamReader reader = new StreamReader(stream);

                return new Tuple<HttpStatusCode, string>(response.StatusCode, reader.ReadToEnd());
            }
            catch (System.Net.WebException){
                return new Tuple<HttpStatusCode, string>(HttpStatusCode.NotFound, null);
            }

        }

        static void UPDATE_DLC_LIST(string UPDATED_CONTENT)
        {
            using StreamWriter writer = new StreamWriter(DLC_LIST_PATH);
            writer.Write(UPDATED_CONTENT);
        }

        static IDictionary<string, string> READ_DLC_LIST()
        {
            List<string> DLC_LIST_UNFORMATED = new List<string>();

            try
            {
                // Skip 1 = Remove version header
                DLC_LIST_UNFORMATED = File.ReadAllLines(DLC_LIST_PATH, Encoding.UTF8)
                    .Skip(1)
                    .ToList();

            }
            catch(FileNotFoundException)
            {
                if (MyDLCListFile == "No")
                {
                    CONSOLE_COLOR(ConsoleColor.Red, DLC_LST_FILE + " file doesn't exist");
                }
                else
                {
                    CONSOLE_COLOR(ConsoleColor.Red, MY_DLC_LST_FILE + " file doesn't exist");
                }
                EXIT_PROGRAM();
            }

            // DLC_LIST_FORMAT = [Keys = DLC_Filename, Value = DLC_Name]
            IDictionary<string, string> DLC_LIST_FORMATED = new Dictionary<string, string>();

            foreach (string DLC_LIST in DLC_LIST_UNFORMATED)
            {
                String[] temp_strlist = DLC_LIST.Split(',');
                DLC_LIST_FORMATED.Add(temp_strlist[0], temp_strlist[1]);
            }

            return DLC_LIST_FORMATED;

        }

        static string GET_GAME_INSTALLPATH()
        {
            // Default: Current Directory of DLC_Checker
            // Will be replaced by Registry Entry
            const string keyName = "HKEY_CURRENT_USER" + "\\" + GAME_REGISTRY;

            string GAME_DIRECTORY_REGISTRY = (string)Registry.GetValue(keyName,"InstallPath","");

            if (UseCurrentDir == "No")
            {
                if (GAME_DIRECTORY_REGISTRY != null)
                {
                    CONSOLE_COLOR(ConsoleColor.Cyan, "Game Dir: " + GAME_DIRECTORY_REGISTRY);
                    return GAME_DIRECTORY_REGISTRY;
                }
                else
                {
                    CONSOLE_COLOR(ConsoleColor.Yellow, GAME_NAME + " installation path not set in registry. Using working directory: " + Directory.GetCurrentDirectory());
                    return Directory.GetCurrentDirectory();
                }
            }
            else
            {
                CONSOLE_COLOR(ConsoleColor.Yellow, GAME_NAME + " installation directory set to current directory: " + Directory.GetCurrentDirectory());
                return Directory.GetCurrentDirectory();
            }
        }

        static List<string> READ_GAMEDATA()
        {
            string GAME_DIRECTORY = GET_GAME_INSTALLPATH();
            string GAMEDATA_DIRECTORY = GAME_DIRECTORY + "\\GameData";

            List<string> GAMEDATA_LIST = new List<string>();

            try
            {
                GAMEDATA_LIST.AddRange(Directory.GetFiles(@GAMEDATA_DIRECTORY, "*", SearchOption.TopDirectoryOnly).Select(Path.GetFileName));
            }
            catch (DirectoryNotFoundException)
            {
                CONSOLE_COLOR(ConsoleColor.Red, "GameData Directory doesn't exist, invalid Configuration Parameter in " + INI_FILE + " or Game not installed. Exit Program. ");
                EXIT_PROGRAM();
            }

            if (GAME_NAME == "COM3D2" || GAME_NAME == "COM3D2_EN")
            {
                string GAMEDATA_20_DIRECTORY = GAME_DIRECTORY + "\\GameData_20";
            
                try
                {
                    GAMEDATA_LIST.AddRange(Directory.GetFiles(@GAMEDATA_20_DIRECTORY, "*", SearchOption.TopDirectoryOnly).Select(Path.GetFileName));
                }
                catch (DirectoryNotFoundException)
                {
                    CONSOLE_COLOR(ConsoleColor.Yellow, "GameData_20 Directory doesn't exist, this might be ok with a fresh Installation");
                    //EXIT_PROGRAM();
                }                
            }
            return GAMEDATA_LIST;
        }
        static Tuple<List<string>,List<string>> COMPARE_DLC(IDictionary<string, string> DLC_LIST, List<string> GAMEDATA_LIST)
        {
            // DLC LIST = [DLC_FILENAME, DLC_NAME]
            List<string> DLC_FILENAMES = new List<string>(DLC_LIST.Keys);
            List<string> DLC_NAMES= new List<string>(DLC_LIST.Values);

            List<string> INSTALLED_DLC = new List<string>(); 
            foreach(string INSTALLED_DLC_FILENAMES in DLC_FILENAMES.Intersect(GAMEDATA_LIST).ToList())
            {
                // UNIT_DLC_LIST = [DLC_FILENAME, DLC_NAME]
                foreach (KeyValuePair<string,string> UNIT_DLC_LIST in DLC_LIST)
                {
                    if (INSTALLED_DLC_FILENAMES == UNIT_DLC_LIST.Key)
                    {
                        INSTALLED_DLC.Add(UNIT_DLC_LIST.Value);
                        DLC_LIST.Remove(UNIT_DLC_LIST);
                        break;
                    }
                }
            }
            
            List<string> NOT_INSTALLED_DLC = DLC_NAMES.Except(INSTALLED_DLC).ToList();
            INSTALLED_DLC.Sort();
            NOT_INSTALLED_DLC.Sort();
            return Tuple.Create(INSTALLED_DLC, NOT_INSTALLED_DLC);
        }

        static void PRINT_DLC(List<string> INSTALLED_DLC, List<string> NOT_INSTALLED_DLC)
        {
            CONSOLE_COLOR(ConsoleColor.Green, "\nAlready Installed:");
            foreach (string DLC in INSTALLED_DLC)
            {
                Console.WriteLine(DLC);
            }

            CONSOLE_COLOR(ConsoleColor.Yellow, "\nNot Installed :");
            foreach (string DLC in NOT_INSTALLED_DLC)
            {
                Console.WriteLine(DLC);
            }
        }

        static void EXIT_PROGRAM()
        {
            Console.WriteLine("\nPress 'Enter' to exit the process...");
            while (true)
            {
                if (Console.ReadKey().Key == ConsoleKey.Enter)
                {
                    System.Environment.Exit(0);
                }
            }
        }

        // Extension
        static void CONSOLE_COLOR(ConsoleColor color, string message)
        {
            Console.ForegroundColor = color;
            Console.WriteLine(message);
            Console.ResetColor();
        }
    }
}
