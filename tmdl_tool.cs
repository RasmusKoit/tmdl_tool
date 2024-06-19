using Microsoft.AnalysisServices.Tabular;
using Newtonsoft.Json;

namespace tmdl_tool
{
    internal class tmdl_tool
    {
        private const int ERROR_SUCCESS = 0x0;
        private const int ERROR_INVALID_ACTION = 0x667;
        private const int ERROR_PATH_NOT_FOUND = 0x3;
        private const int ERROR_NETWORK_ACCESS = 0x44;
        private const int ERROR_UNEXPECTED_ERROR = 0x999;

        private static string VersionString = "";

        private static Settings runtimeSettings = new();

        static void Main(string[] args)
        {

            var assemblyInfo = System.Reflection.Assembly.GetExecutingAssembly().GetName();
            var tmdlLib = System.Reflection.Assembly.GetAssembly(typeof(Server));
            var libName = tmdlLib?.GetName()?.Name ?? "Unknown";
            var libVersion = tmdlLib?.GetName()?.Version?.ToString() ?? "Unknown";
            VersionString = $"{assemblyInfo.Name} v.{assemblyInfo.Version}, {libName}: {libVersion}";

            Environment.ExitCode = ERROR_UNEXPECTED_ERROR;

            PrintHelpIfRequested(args);

            GetArguments(args);

            GetSettings(runtimeSettings.SettingsFilePath);

            LogToConsole($"Starting with options:\n{JsonConvert.SerializeObject(runtimeSettings.GetSafe(), Formatting.Indented)}");

            PBI(runtimeSettings);

        }

        /// <summary>
        /// Prints the help message if the "--help" or "-h" option is present in the command-line arguments.
        /// </summary>
        /// <param name="args">The command-line arguments.</param>
        private static void PrintHelpIfRequested(string[] args)
        {
            if (args.Contains("--help") || args.Contains("-h"))
            {
                Console.WriteLine("Usage: tmdl_tool [arguments]");
                Console.WriteLine();
                Console.WriteLine("Arguments:");
                Console.WriteLine("  -w\tThe URL of the Power BI workspace\n\t--workspacexmla \"<url>\"");
                Console.WriteLine("  -d\tThe name of the Power BI dataset \n\t--datasetname \"<name>\"");
                Console.WriteLine("  -t\tThe path to the TMDL folder \n\t--tmdlfolderpath \"<path>\"");
                Console.WriteLine("  -a\tThe action to perform (pull or deploy) \n\t--action \"<pull|deploy>\"");
                Console.WriteLine("  -s\tThe path to the settings file (default: settings.json) \n\t--settingsfilepath \"<path>\"");
                Console.WriteLine("  -ai\tThe application ID of the Azure AD app \n\t--appid \"<id>\"");
                Console.WriteLine("  -as\tThe application secret of the Azure AD app \n\t--appsecret \"<secret>\"");
                Console.WriteLine("  -ti\tThe tenant ID of the Azure AD app \n\t--tenantid \"<id>\"");
                Console.WriteLine("  -at\tExternally aquired Access Token for Azure AD app \n\t--accessToken \"<AccessToken>\"");
                Console.WriteLine("  -v\tShow progress on STDOUT \n\t--verbose \"<true|false>\" (default: true)");
                Console.WriteLine();
                Console.WriteLine("  -h\tShow this help message \n\t--help\n");
                Console.WriteLine();
                Console.WriteLine("Environment:");
                Console.WriteLine("  Use an Environment Variable `tmdl_accessToken` to pass a Bearer Access Token to the PowerBI Server");
                Console.WriteLine();
                Console.WriteLine("Examples:");
                Console.WriteLine("  tmdl_tool --workspacexmla \"powerbi://api.powerbi.com/v1.0/myorg/MyWorkspace\" --datasetname \"MyDataset\" --tmdlfolderpath \"C:\\TMDL\" --action \"pull\"");
                Console.WriteLine("  tmdl_tool -w \"powerbi://api.powerbi.com/v1.0/myorg/MyWorkspace\" -d \"MyDataset\" -t \"C:\\TMDL\" -a \"pull\"");
                Console.WriteLine("  tmdl_tool -s \"C:\\settings.json\" -a \"deploy\"");
                Console.WriteLine("  tmdl_tool -ai \"xxxxxxxx-xxxx-xxxx-xxxx-xxxxxxxxxxxx\" -as \"xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx\" -ti \"xxxxxxxx-xxxx-xxxx-xxxx-xxxxxxxxxxxx\" -a \"deploy\"");
                Console.WriteLine("  tmdl_tool deploy");
                Console.WriteLine("  tmdl_tool pull");
                Environment.Exit(ERROR_SUCCESS);
            }
        }

        /// <summary>
        /// Log to console if verbose is set to true
        /// </summary>
        /// <param name="message"></param>
        private static void LogToConsole(string message)
        {
            if (!runtimeSettings.Verbose) return;
            var timeStampString = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff");
            Console.WriteLine($"{timeStampString} [{VersionString}]: {message}");
        }


        /// <summary>
        /// Parses the command-line arguments and sets the corresponding variables.
        /// </summary>
        /// <param name="args">The command-line arguments.</param>
        private static void GetArguments(string[] args)
        {
            for (int i = 0; i < args.Length; i++)
            {
                string arg = args[i];
                if (arg.StartsWith("-"))
                {
                    string value = "";
                    if (i + 1 < args.Length && !args[i + 1].StartsWith("-"))
                    {
                        value = args[i + 1];
                        i++;
                    }
                    switch (arg.ToLower())
                    {
                        case "--workspacexmla":
                        case "-w":
                            runtimeSettings.WorkspaceXMLA = value ?? "";
                            break;
                        case "--datasetname":
                        case "-d":
                            runtimeSettings.DatasetName = value ?? "";
                            break;
                        case "--tmdlfolderpath":
                        case "-t":
                            runtimeSettings.TmdlFolderPath = value ?? "";
                            break;
                        case "--action":
                        case "-a":
                            runtimeSettings.Action = value.ToLower() ?? "";
                            break;
                        case "--settingsfilepath":
                        case "-s":
                            runtimeSettings.SettingsFilePath = value ?? "";
                            break;
                        case "--appid":
                        case "-ai":
                            runtimeSettings.AppId = value ?? "";
                            break;
                        case "--appsecret":
                        case "-as":
                            runtimeSettings.AppSecret = value ?? "";
                            break;
                        case "--tenantid":
                        case "-ti":
                            runtimeSettings.TenantId = value ?? "";
                            break;
                        case "--accesstoken":
                        case "-at":
                            runtimeSettings.AccessToken = value ?? "";
                            break;
                        case "--verbose":
                        case "-v":
                            runtimeSettings.Verbose = bool.Parse(value);
                            break;
                        default:
                            Console.WriteLine($"Unknown argument: {arg}");
                            Environment.ExitCode = ERROR_INVALID_ACTION;
                            return;
                    }
                }
                else if (i == 0 && (arg == "pull" || arg == "deploy"))
                {
                    runtimeSettings.Action = arg;
                }
                else
                {
                    Console.WriteLine($"Unknown argument: {arg}");
                    Environment.ExitCode = ERROR_INVALID_ACTION;
                    return;
                }
            }
        }

        /// <summary>
        /// Reads the settings file and updates the workspaceXMLA, datasetName, tmdlfolderPath, and action variables with the values from the settings file.
        /// </summary>
        /// <param name="settingsFilePath">The path to the settings file (default: settings.json).</param>
        private static void GetSettings(string settingsFilePath)
        {
            settingsFilePath = string.IsNullOrEmpty(settingsFilePath) ? "settings.json" : settingsFilePath;
            if (File.Exists(settingsFilePath))
            {
                runtimeSettings.SettingsFilePath = settingsFilePath;
                var settings = JsonConvert.DeserializeObject<Settings>(File.ReadAllText(settingsFilePath));
                if (settings == null)
                {
                    Console.WriteLine("Error: Unable to read settings file.");
                    Environment.ExitCode = ERROR_PATH_NOT_FOUND;
                    return;
                }
                runtimeSettings.WorkspaceXMLA = string.IsNullOrEmpty(settings.WorkspaceXMLA) ? runtimeSettings.WorkspaceXMLA : settings.WorkspaceXMLA;
                runtimeSettings.DatasetName = string.IsNullOrEmpty(settings.DatasetName) ? runtimeSettings.DatasetName : settings.DatasetName;
                runtimeSettings.TmdlFolderPath = string.IsNullOrEmpty(settings.TmdlFolderPath) ? runtimeSettings.TmdlFolderPath : settings.TmdlFolderPath;
                runtimeSettings.Action = string.IsNullOrEmpty(settings.Action) ? runtimeSettings.Action : settings.Action.ToLower();
                runtimeSettings.AppId = string.IsNullOrEmpty(settings.AppId) ? runtimeSettings.AppId : settings.AppId;
                runtimeSettings.AppSecret = string.IsNullOrEmpty(settings.AppSecret) ? runtimeSettings.AppSecret : settings.AppSecret;
                runtimeSettings.TenantId = string.IsNullOrEmpty(settings.TenantId) ? runtimeSettings.TenantId : settings.TenantId;
                runtimeSettings.AccessToken = string.IsNullOrEmpty(settings.AccessToken) ? runtimeSettings.AccessToken : settings.AccessToken;
                runtimeSettings.Verbose = ! settings.Verbose ? false : runtimeSettings.Verbose;
            }
        }

        /// <summary>
        /// Performs the specified action (pull or deploy) on the specified Power BI dataset using the specified TMDL folder.
        /// </summary>
        /// <param name="workspaceXMLA">The URL of the Power BI workspace.</param>
        /// <param name="datasetName">The name of the Power BI dataset.</param>
        /// <param name="tmdlfolderPath">The path to the TMDL folder.</param>
        /// <param name="action">The action to perform (pull or deploy).</param>
        /// <param name="appId">Application Id</param>
        /// <param name="appSecret">Application Secret</param>
        /// <param name="tenantId">Tenant Id</param>
        /// <param name="accessToken">Access Token</param>
        private static void PBI(Settings settings)
        {
            string[] actions = { "pull", "deploy" };
            if (Array.IndexOf(actions, settings.Action) < 0)
            {
                Console.WriteLine("Please specify action as pull or deploy (-h | --help for Help)");
                Environment.ExitCode = ERROR_INVALID_ACTION;
                return;
            }

            var server = Connect(settings);
            if (server == null)
            {
                Environment.ExitCode = ERROR_NETWORK_ACCESS;
                return;
            }
            if (settings.Action == "pull")
            {
                Pull_TMDL(server, settings.DatasetName, settings.TmdlFolderPath);
            }
            else if (settings.Action == "deploy")
            {
                Deploy_TMDL(server, settings.DatasetName, settings.TmdlFolderPath);
            }
        }

        /// <summary>
        /// Pulls the TMDL from the specified Power BI dataset and saves it to the specified TMDL folder.
        /// </summary>
        /// <param name="server">PBI Server</param>
        /// <param name="datasetName">The name of the Power BI dataset.</param>
        /// <param name="tmdlfolderPath">The path to the TMDL folder.</param>
        private static void Pull_TMDL(Server server, string datasetName, string tmdlfolderPath)
        {
            try
            {
                using (server)
                {
                    LogToConsole($"Getting Databases for {datasetName}");
                    var database = server.Databases.GetByName(datasetName);
                    LogToConsole($"Serializing model from {database.Name} to {tmdlfolderPath}");
                    TmdlSerializer.SerializeModelToFolder(database.Model, tmdlfolderPath);
                    LogToConsole($"Model pulled successfully to {tmdlfolderPath}");
                    Environment.ExitCode = ERROR_SUCCESS;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.GetType().ToString());
                Console.WriteLine(ex.GetBaseException().Message);
                Console.WriteLine(ex.ToString());
                Environment.ExitCode = ERROR_PATH_NOT_FOUND;
            }
        }

        /// <summary>
        /// Deploys the TMDL from the specified TMDL folder to the specified Power BI dataset.
        /// </summary>
        /// <param name="workspaceXMLA">The URL of the Power BI workspace.</param>
        /// <param name="datasetName">The name of the Power BI dataset.</param>
        /// <param name="tmdlfolderPath">The path to the TMDL folder.</param>
        private static void Deploy_TMDL(Server server, string datasetName, string tmdlfolderPath)
        {
            try
            {
                var model = TmdlSerializer.DeserializeModelFromFolder(tmdlfolderPath);
                using (server)
                {
                    using (var remoteDatabase = server.Databases.GetByName(datasetName))
                    {
                        LogToConsole($"Deploying model to {datasetName}");
                        model.CopyTo(remoteDatabase.Model);
                        LogToConsole($"Saving changes to {datasetName}");
                        remoteDatabase.Model.SaveChanges();
                        LogToConsole($"Model deployed successfully to {datasetName}");
                        Environment.ExitCode = ERROR_SUCCESS;
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.GetType().ToString());
                Console.WriteLine(ex.ToString());
                if (ex.ToString().Contains("path does not exists"))
                {
                    Console.WriteLine("Error: The path does not exist.");
                    Console.WriteLine("Please check the path and try again.");
                    Environment.ExitCode = ERROR_PATH_NOT_FOUND;
                }
            }
        }

        /// <summary>
        /// Connects to the specified Power BI workspace using the provided URL.
        /// </summary>
        /// <param name="workspaceXMLA">The URL of the Power BI workspace.</param>
        /// <param name="appId">The application ID for authentication.</param>
        /// <param name="appSecret">The application secret for authentication.</param>
        /// <param name="tenantId">The tenant ID for authentication.</param>
        /// <param name="accessToken">The access token for authentication.</param>
        /// <returns>A Server object representing the connected workspace, or null if the connection failed.</returns>
        private static Server? Connect(Settings settings)
        {

            try
            {
                var server = new Server();

                // Check if we have an access token in Environment variable
                var tokenString = Environment.GetEnvironmentVariable("tmdl_accesstoken");
                if (! string.IsNullOrEmpty(settings.AccessToken))
                {
                    LogToConsole("Using Access Token from Settings / Command Line");
                    var accessTokenObj = new Microsoft.AnalysisServices.AccessToken(settings.AccessToken, DateTime.UtcNow + TimeSpan.FromHours(1));
                    server.AccessToken = accessTokenObj;
                }
                else if (! string.IsNullOrEmpty(tokenString))
                {
                    LogToConsole("Using Access Token from Environment Variable");
                    var accessTokenObj = new Microsoft.AnalysisServices.AccessToken(tokenString, DateTime.UtcNow + TimeSpan.FromHours(1));
                    server.AccessToken = accessTokenObj;
                };
                string connectionString = $"Data source={settings.WorkspaceXMLA};User ID=app:{settings.AppId}@{settings.TenantId};Password={settings.AppSecret}";
                LogToConsole($"Connecting to {settings.WorkspaceXMLA}");
                server.Connect(connectionString);
                return server;
            }
            catch (Exception ex)
            {
                string exType = ex.GetType().ToString();
                if (exType == "Microsoft.AnalysisServices.Authentication.AuthenticationException")
                {
                    Console.WriteLine("Error: " + ex.GetBaseException().Message);
                    Console.WriteLine(ex.ToString());
                    Environment.ExitCode = ERROR_NETWORK_ACCESS;
                }
                else if (exType == "Microsoft.AnalysisServices.ConnectionException")
                {
                    Console.WriteLine("Error: " + ex.GetBaseException().Message);
                    Console.WriteLine(ex.ToString());
                    Environment.ExitCode = ERROR_NETWORK_ACCESS;
                }
                else
                {
                    Console.WriteLine(exType);
                    Console.WriteLine(ex.GetBaseException().Message);
                    Console.WriteLine(ex.ToString());
                    Environment.ExitCode = ERROR_NETWORK_ACCESS;
                }
                return null;
            }
        }

        /// <summary>
        /// Represents the settings used by the tmdl_tool program.
        /// </summary>
        private class Settings
        {
            /// <summary>
            /// The URL of the Power BI workspace.
            /// </summary>
            public string WorkspaceXMLA { get; set; } = "";

            /// <summary>
            /// The name of the Power BI dataset.
            /// </summary>
            public string DatasetName { get; set; } = "";

            /// <summary>
            /// The path to the TMDL folder.
            /// </summary>
            public string TmdlFolderPath { get; set; } = "";

            /// <summary>
            /// The action to perform (pull or deploy).
            /// </summary>
            public string Action { get; set; } = "";

            /// <summary>
            /// Application ID of the Azure AD app.
            /// </summary>
            public string AppId { get; set; } = "";

            /// <summary>
            /// Password of the Azure AD app.
            /// </summary>
            public string AppSecret { get; set; } = "";

            /// <summary>
            /// Tenant ID of the Azure AD app.
            /// </summary>
            public string TenantId { get; set; } = "";

            /// <summary>
            /// Externally aquired Access Token for Azure AD app.
            /// </summary>
            public string AccessToken { get; set; } = "";

            /// <summary>
            /// Verbose output
            /// </summary>
            public bool Verbose { get; set; } = true;

            /// <summary>
            /// Settings file name
            /// </summary>
            public string SettingsFilePath { get; set; } = "";

            public Settings GetSafe()
            {
                return new Settings
                {
                    WorkspaceXMLA = WorkspaceXMLA,
                    DatasetName = DatasetName,
                    TmdlFolderPath = TmdlFolderPath,
                    Action = Action,
                    AppId = AppId,
                    AppSecret = string.IsNullOrEmpty(AppSecret) ? "" : "********",
                    TenantId = TenantId,
                    AccessToken = string.IsNullOrEmpty(AccessToken) ? "" : "********",
                    Verbose = Verbose,
                    SettingsFilePath = SettingsFilePath
                };
            }
        }
    }
}
