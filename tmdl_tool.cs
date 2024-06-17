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

        private static string workspaceXMLA = "";
        private static string datasetName = "";
        private static string tmdlfolderPath = "";
        private static string action = "";
        private static string settingsFilePath = "";
        private static string appId = "";
        private static string appSecret = "";
        private static string tenantId = "";
        private static string accessToken = "";
        private static bool verbose = false;

        static void Main(string[] args)
        {

            // Setting tmdl_tool.VersionString
            var assemblyInfo = System.Reflection.Assembly.GetExecutingAssembly().GetName();
            var tmdlLib = System.Reflection.Assembly.GetAssembly(typeof(Server));
            var libName = tmdlLib?.GetName()?.Name ?? "Unknown";
            var libVersion = tmdlLib?.GetName()?.Version?.ToString() ?? "Unknown";
            VersionString = $"{assemblyInfo.Name} v.{assemblyInfo.Version}, {libName}: {libVersion}";

            Environment.ExitCode = ERROR_UNEXPECTED_ERROR;

            PrintHelpIfRequested(args);

            GetArguments(args);

            GetSettings(settingsFilePath);

            LogToConsole("Starting");

            PBI(workspaceXMLA, datasetName, tmdlfolderPath, action, appId, appSecret, tenantId, accessToken);

            Environment.ExitCode = ERROR_SUCCESS;
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
                Console.WriteLine("  -at\tExternally aquired Access Token for Azure AD app \n\t--accessToken \"<id>\"");
                Console.WriteLine("  -v\tShow progress on STDOUT \n\t--verbose\n");
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

        private static void LogToConsole(string message)
        {
            if (!verbose) return;
            var timeStampString = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff");
            Console.WriteLine($"{timeStampString} [{VersionString}]: {message}");
        }


        /// <summary>
        /// Parses the command-line arguments and sets the corresponding variables.
        /// </summary>
        /// <param name="args">The command-line arguments.</param>
        /// <param name="workspaceXMLA">The URL of the Power BI workspace.</param>
        /// <param name="datasetName">The name of the Power BI dataset.</param>
        /// <param name="tmdlfolderPath">The path to the TMDL folder.</param>
        /// <param name="action">The action to perform (pull or deploy).</param>
        /// <param name="settingsFilePath">The path to the settings file (default: settings.json).</param>
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
                            workspaceXMLA = value ?? "";
                            break;
                        case "--datasetname":
                            datasetName = value ?? "";
                            break;
                        case "--tmdlfolderpath":
                            tmdlfolderPath = value ?? "";
                            break;
                        case "--action":
                            action = value ?? "";
                            break;
                        case "--settingsfilepath":
                            settingsFilePath = value ?? "";
                            break;
                        case "--appid":
                            appId = value ?? "";
                            break;
                        case "--appsecret":
                            appSecret = value ?? "";
                            break;
                        case "--tenantid":
                            tenantId = value ?? "";
                            break;
                        case "--accesstoken":
                            accessToken = value ?? "";
                            break;
                        case "--verbose":
                            verbose = true;
                            break;
                        case "-w":
                            workspaceXMLA = value ?? "";
                            break;
                        case "-d":
                            datasetName = value ?? "";
                            break;
                        case "-t":
                            tmdlfolderPath = value ?? "";
                            break;
                        case "-a":
                            action = value ?? "";
                            break;
                        case "-s":
                            settingsFilePath = value ?? "";
                            break;
                        case "-ai":
                            appId = value ?? "";
                            break;
                        case "-as":
                            appSecret = value ?? "";
                            break;
                        case "-ti":
                            tenantId = value ?? "";
                            break;
                        case "-at":
                            accessToken = value ?? "";
                            break;
                        case "-v":
                            verbose = true;
                            break;
                        default:
                            Console.WriteLine($"Unknown argument: {arg}");
                            Environment.ExitCode = ERROR_INVALID_ACTION;
                            return;
                    }
                }
                else if (i == 0 && (arg == "pull" || arg == "deploy"))
                {
                    action = arg;
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
        /// <param name="workspaceXMLA">The URL of the Power BI workspace.</param>
        /// <param name="datasetName">The name of the Power BI dataset.</param>
        /// <param name="tmdlfolderPath">The path to the TMDL folder.</param>
        /// <param name="action">The action to perform (pull or deploy).</param>
        private static void GetSettings(string settingsFilePath)
        {
            settingsFilePath = string.IsNullOrEmpty(settingsFilePath) ? "settings.json" : settingsFilePath;
            if (File.Exists(settingsFilePath))
            {
                var settings = JsonConvert.DeserializeObject<Settings>(File.ReadAllText(settingsFilePath));
                workspaceXMLA = settings?.WorkspaceXMLA ?? workspaceXMLA;
                datasetName = settings?.DatasetName ?? datasetName;
                tmdlfolderPath = settings?.TmdlFolderPath ?? tmdlfolderPath;
                action = settings?.Action ?? action;
                appId = settings?.AppId ?? appId;
                appSecret = settings?.AppSecret ?? appSecret;
                tenantId = settings?.TenantId ?? tenantId;
                accessToken = settings?.AccessToken ?? accessToken;
                verbose = settings?.Verbose ?? false;
            }
        }

        /// <summary>
        /// Performs the specified action (pull or deploy) on the specified Power BI dataset using the specified TMDL folder.
        /// </summary>
        /// <param name="workspaceXMLA">The URL of the Power BI workspace.</param>
        /// <param name="datasetName">The name of the Power BI dataset.</param>
        /// <param name="tmdlfolderPath">The path to the TMDL folder.</param>
        /// <param name="action">The action to perform (pull or deploy).</param>
        private static void PBI(string workspaceXMLA, string datasetName, string tmdlfolderPath, string action, string appId, string appSecret, string tenantId, string accessToken)
        {
            string[] actions = { "pull", "deploy" };
            if (Array.IndexOf(actions, action) < 0)
            {
                Console.WriteLine("Please specify action as pull or deploy (-h | --help for Help)");
                Environment.ExitCode = ERROR_INVALID_ACTION;
                return;
            }

            var server = Connect(workspaceXMLA, appId, appSecret, tenantId, accessToken);
            if (server == null)
            {
                Environment.ExitCode = ERROR_NETWORK_ACCESS;
                return;
            }
            if (action == "pull")
            {
                Pull_TMDL(server, datasetName, tmdlfolderPath);
            }
            else if (action == "deploy")
            {
                Deploy_TMDL(server, datasetName, tmdlfolderPath);
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
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.GetType().ToString());
                Console.WriteLine(ex.ToString());
                Environment.ExitCode = ERROR_PATH_NOT_FOUND;
            }
        }

        /// <summary>
        /// Connects to the specified Power BI workspace using the provided URL.
        /// </summary>
        /// <param name="workspaceXMLA">The URL of the Power BI workspace.</param>
        /// <returns>A Server object representing the connected workspace, or null if the connection failed.</returns>
        static Server? Connect(string workspaceXMLA, string appId, string appSecret, string tenantId, string accessToken)
        {

            try
            {
                var server = new Server();

                // Check if we have an access token in Environment variable
                var tokenString = Environment.GetEnvironmentVariable("tmdl_accesstoken");
                if (! string.IsNullOrEmpty(accessToken))
                {
                    LogToConsole("Using Access Token from Settings / Command Line");
                    var accessTokenObj = new Microsoft.AnalysisServices.AccessToken(accessToken, DateTime.UtcNow + TimeSpan.FromHours(1));
                    server.AccessToken = accessTokenObj;
                }
                else if (! string.IsNullOrEmpty(tokenString))
                {
                    LogToConsole("Using Access Token from Environment Variable");
                    var accessTokenObj = new Microsoft.AnalysisServices.AccessToken(tokenString, DateTime.UtcNow + TimeSpan.FromHours(1));
                    server.AccessToken = accessTokenObj;
                };
                string connectionString = $"Data source={workspaceXMLA};User ID=app:{appId}@{tenantId};Password={appSecret}";
                LogToConsole($"Connecting to {workspaceXMLA}");
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
        public class Settings
        {
            /// <summary>
            /// The URL of the Power BI workspace.
            /// </summary>
            public string? WorkspaceXMLA { get; set; }

            /// <summary>
            /// The name of the Power BI dataset.
            /// </summary>
            public string? DatasetName { get; set; }

            /// <summary>
            /// The path to the TMDL folder.
            /// </summary>
            public string? TmdlFolderPath { get; set; }

            /// <summary>
            /// The action to perform (pull or deploy).
            /// </summary>
            public string? Action { get; set; }

            /// <summary>
            /// Application ID of the Azure AD app.
            /// </summary>
            public string? AppId { get; set; }

            /// <summary>
            /// Password of the Azure AD app.
            /// </summary>
            public string? AppSecret { get; set; }

            /// <summary>
            /// Tenant ID of the Azure AD app.
            /// </summary>
            public string? TenantId { get; set; }

            /// <summary>
            /// Externally aquired Access Token for Azure AD app.
            /// </summary>
            public string? AccessToken { get; set; }

            /// <summary>
            /// Verbose output
            /// </summary>
            public bool Verbose { get; set; }
        }
    }
}
