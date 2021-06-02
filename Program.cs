using System;
//using System.Reflection;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using Microsoft.Win32;

namespace DLC_Checker
{
	class Program
	{

        // Class Gamedata init, sets the Values for the used Game
        static GameData MyData = new GameData();

        static readonly string GAME_NAME = MyData.GetGAME_NAME();
        static readonly string GAME_REGISTRY = MyData.GetGAME_REGISTRY();
        //static readonly string INI_FILE = MyData.GetINI_FILE();
        static readonly string DLC_LIST_FILE = MyData.GetDLC_LIST_FILE();
        static readonly string MY_DLC_LIST_FILE = MyData.GetMY_DLC_LIST_FILE();
        static readonly string DLC_LIST_PATH = MyData.GetDLC_LIST_PATH();
        static readonly string GAME_HEADER = MyData.GetGAME_HEADER();
        static readonly string DLC_URL = MyData.GetDLC_URL();
        
        static readonly string UseCurrentDir = MyData.GetUseCurrentDir();
        static readonly string UpdateListFile = MyData.GetUpdateListFile();
        static readonly string MyDLCListFile = MyData.GetMyDLCListFile();
        static readonly string UseMyURL = MyData.GetUseMyURL();
        
        static void Main(string[] args)
        {
            // Write Header Lines to Console
            PRINT_HEADER();

            //Console.WriteLine(VERSION);

            // update/get DLC Listfile 
            GET_DLC_LISTFILE();
            
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
		// End Main
	
		// Get DLC List File
		static void GET_DLC_LISTFILE()
		{
			// Custom Listfile
            if (MyDLCListFile == "Yes")
            {
                //DLC_LIST_PATH = Path.Combine(Directory.GetCurrentDirectory(), MyGameData.GetMY_DLC_LIST_FILE);
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine("Using Custom Listfile {0}", MY_DLC_LIST_FILE);
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
                        if (UseMyURL == "No")
                        {
                            Console.ForegroundColor = ConsoleColor.Cyan;
							Console.WriteLine("Using Standard URL");
                        }
                        else
                        {
                            Console.ForegroundColor = ConsoleColor.Yellow;
                            Console.WriteLine("Using Custom URL");
                        }
                        Console.WriteLine("Connected to {0}", DLC_URL);
                        //Console.WriteLine("Hint: You can preserve your current {0} by disable autoupdate in {1} with: UpdateListFile=No", DLC_LIST_FILE, INI_FILE);
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
                    Console.WriteLine("Updating {0} disabled", DLC_LIST_FILE);
                    //Console.ForegroundColor = ConsoleColor.Cyan;
                    //Console.WriteLine("Hint: You can enable autoupdate in {0} with: UpdateListFile=Yes", INI_FILE);
                }
                Console.ResetColor();
            }
			
		}
		// PRINT_HEADER
		static void PRINT_HEADER()
        {
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("===========================================================================================");
            Console.WriteLine(GAME_HEADER);
            Console.WriteLine("===========================================================================================");
            Console.ResetColor();
        }

        // CONNECT_TO_INTERNET
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
            catch (System.Net.WebException)
			{
                return new Tuple<HttpStatusCode, string>(HttpStatusCode.NotFound, null);
            }

        }

        // UPDATE_DLC_LIST
		static void UPDATE_DLC_LIST(string UPDATED_CONTENT)
        {
            using StreamWriter writer = new StreamWriter(DLC_LIST_PATH);
            writer.Write(UPDATED_CONTENT);
        }

        // Dictionary READ_DLC_LIST
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
                    CONSOLE_COLOR(ConsoleColor.Red, DLC_LIST_FILE + " file doesn't exist");
                }
                else
                {
                    CONSOLE_COLOR(ConsoleColor.Red, MY_DLC_LIST_FILE + " file doesn't exist");
                }
                EXIT_PROGRAM();
            }
            // DLC_LIST_FORMAT = [Keys = DLC_Filename, Value = DLC_Name]
            IDictionary<string, string> DLC_LIST_FORMATED = new Dictionary<string, string>();
            string sub;
            foreach (string DLC_LIST in DLC_LIST_UNFORMATED)
            {
                DLC_LIST.Trim();
                if (DLC_LIST != "")
                {     
                    sub = DLC_LIST.Substring(0, 1);
                    if (sub!="/" && sub != "*" && sub != ";")
                    { 
                        string[] temp_strlist = DLC_LIST.Split(',');
                        if (temp_strlist.Length > 2)
                        {
                            for (int i = 2; i < temp_strlist.Length; i++)
                                temp_strlist[1] = temp_strlist[1] + "," + temp_strlist[i];
                        }
                        DLC_LIST_FORMATED.Add(temp_strlist[0], temp_strlist[1]);
                    }
                }
            }
            return DLC_LIST_FORMATED;
        }

        static string GET_GAME_INSTALLPATH()
        {
            // Default: Current Directory of DLC_Checker
            // Will be replaced by Registry Entry
            
            string GAME_DIRECTORY_REGISTRY = (string)Registry.GetValue(GAME_REGISTRY, "InstallPath","");

            if (UseCurrentDir == "No")
            {
                if (GAME_DIRECTORY_REGISTRY != null)
                {
                    if (!Directory.Exists(GAME_DIRECTORY_REGISTRY))
            		{
                		CONSOLE_COLOR(ConsoleColor.Yellow, "Warning : " + GAME_NAME + "installation directory set in registry but doesn't exist. Will using work directory");
                		CONSOLE_COLOR(ConsoleColor.Yellow, "Current working directory: " + Directory.GetCurrentDirectory());
                    	return Directory.GetCurrentDirectory();
            		}
                    CONSOLE_COLOR(ConsoleColor.Cyan, GAME_NAME + " installation directory found in registry:");
                    CONSOLE_COLOR(ConsoleColor.Cyan, GAME_DIRECTORY_REGISTRY);
                    return GAME_DIRECTORY_REGISTRY;
                }
                else
                {
                    CONSOLE_COLOR(ConsoleColor.Yellow, "Warning : " + GAME_NAME + "installation directory not found in registry. Will using work directory");
					CONSOLE_COLOR(ConsoleColor.Yellow, "Current working directory: " + Directory.GetCurrentDirectory());
                    return Directory.GetCurrentDirectory();
                }
            }
            else
            {
                CONSOLE_COLOR(ConsoleColor.Yellow, "Set up to use current work directory: " + Directory.GetCurrentDirectory());
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
                CONSOLE_COLOR(ConsoleColor.Red, "GameData Directory doesn't exist, ");
                CONSOLE_COLOR(ConsoleColor.Red, "No valid Game Installation found");
                CONSOLE_COLOR(ConsoleColor.Red, "Exit Program. ");
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
