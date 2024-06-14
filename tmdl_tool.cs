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
        private static string VersionString = "";

        static void Main(string[] args)
        {
            string workspaceXMLA;
            string datasetName;
            string tmdlfolderPath;
            string action;
            string settingsFilePath;
            string appId;
            string appSecret;
            string tenantId;

            // Setting tmdl_tool.VersionString
            var assemblyInfo = System.Reflection.Assembly.GetExecutingAssembly().GetName();
            var tmdlLib = System.Reflection.Assembly.GetAssembly(typeof(Server));
            var libName = tmdlLib?.GetName()?.Name ?? "Unknown";
            var libVersion = tmdlLib?.GetName()?.Version?.ToString() ?? "Unknown";
            VersionString = $"{assemblyInfo.Name} v.{assemblyInfo.Version}, {libName}: {libVersion}";
            Console.WriteLine($"Starting {VersionString}");

            Environment.ExitCode = ERROR_SUCCESS;

            PrintHelpIfRequested(args);

            GetArguments(args, out workspaceXMLA, out datasetName, out tmdlfolderPath, out action, out settingsFilePath, out appId, out appSecret, out tenantId);

            GetSettings(settingsFilePath, ref workspaceXMLA, ref datasetName, ref tmdlfolderPath, ref action, ref appId, ref appSecret, ref tenantId);

            PBI(workspaceXMLA, datasetName, tmdlfolderPath, action, appId, appSecret, tenantId);
        }

        /// <summary>
        /// Prints the help message if the "--help" or "-h" option is present in the command-line arguments.
        /// </summary>
        /// <param name="args">The command-line arguments.</param>
        static void PrintHelpIfRequested(string[] args)
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
                Console.WriteLine("  -h\tShow this help message \n\t--help\n");
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
        /// Parses the command-line arguments and sets the corresponding variables.
        /// </summary>
        /// <param name="args">The command-line arguments.</param>
        /// <param name="workspaceXMLA">The URL of the Power BI workspace.</param>
        /// <param name="datasetName">The name of the Power BI dataset.</param>
        /// <param name="tmdlfolderPath">The path to the TMDL folder.</param>
        /// <param name="action">The action to perform (pull or deploy).</param>
        /// <param name="settingsFilePath">The path to the settings file (default: settings.json).</param>
        private static void GetArguments(string[] args, out string workspaceXMLA, out string datasetName, out string tmdlfolderPath, out string action, out string settingsFilePath, out string appId, out string appSecret, out string tenantId)
        {
            workspaceXMLA = "";
            datasetName = "";
            tmdlfolderPath = "";
            action = "";
            settingsFilePath = "";
            appId = "";
            appSecret = "";
            tenantId = "";

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
        private static void GetSettings(string settingsFilePath, ref string workspaceXMLA, ref string datasetName, ref string tmdlfolderPath, ref string action, ref string appId, ref string appSecret, ref string tenantId)
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
            }
        }

        /// <summary>
        /// Performs the specified action (pull or deploy) on the specified Power BI dataset using the specified TMDL folder.
        /// </summary>
        /// <param name="workspaceXMLA">The URL of the Power BI workspace.</param>
        /// <param name="datasetName">The name of the Power BI dataset.</param>
        /// <param name="tmdlfolderPath">The path to the TMDL folder.</param>
        /// <param name="action">The action to perform (pull or deploy).</param>
        static void PBI(string workspaceXMLA, string datasetName, string tmdlfolderPath, string action, string appId, string appSecret, string tenantId)
        {
            string[] actions = { "pull", "deploy" };
            if (Array.IndexOf(actions, action) < 0)
            {
                Console.WriteLine("Please specify action as pull or deploy");
                Environment.ExitCode = ERROR_INVALID_ACTION;
                return;
            }
            if (action == "pull")
            {
                pull_tmdl(workspaceXMLA, datasetName, tmdlfolderPath, appId, appSecret, tenantId);
            }
            else if (action == "deploy")
            {
                deploy_tmdl(workspaceXMLA, datasetName, tmdlfolderPath, appId, appSecret, tenantId);
            }
        }

        /// <summary>
        /// Pulls the TMDL from the specified Power BI dataset and saves it to the specified TMDL folder.
        /// </summary>
        /// <param name="workspaceXMLA">The URL of the Power BI workspace.</param>
        /// <param name="datasetName">The name of the Power BI dataset.</param>
        /// <param name="tmdlfolderPath">The path to the TMDL folder.</param>
        static void pull_tmdl(string workspaceXMLA, string datasetName, string tmdlfolderPath, string appId, string appSecret, string tenantId)
        {
            try
            {
                using (var server = Connect(workspaceXMLA, appId, appSecret, tenantId))
                {
                    if (server == null) { return; }
                    var database = server.Databases.GetByName(datasetName);
                    TmdlSerializer.SerializeModelToFolder(database.Model, tmdlfolderPath);
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
        static void deploy_tmdl(string workspaceXMLA, string datasetName, string tmdlfolderPath, string appId, string appSecret, string tenantId)
        {
            try
            {
                var model = TmdlSerializer.DeserializeModelFromFolder(tmdlfolderPath);
                using (var server = Connect(workspaceXMLA, appId, appSecret, tenantId))
                {
                    if (server == null) { return; }
                    using (var remoteDatabase = server.Databases.GetByName(datasetName))
                    {
                        model.CopyTo(remoteDatabase.Model);
                        remoteDatabase.Model.SaveChanges();
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
        static Server? Connect(string workspaceXMLA, string appId, string appSecret, string tenantId)
        {

            try
            {
                var server = new Server();

                // Check if we have an access token in Environment variable
                var tokenString = Environment.GetEnvironmentVariable("tmdl_accesstoken");
                if (null != tokenString)
                {
                    Console.WriteLine($"Using Access Token from Environment Variable");
                    var accessTokenObj = new Microsoft.AnalysisServices.AccessToken(tokenString, DateTime.UtcNow + TimeSpan.FromHours(1));
                    server.AccessToken = accessTokenObj;
                };
                string connectionString = $"Data source={workspaceXMLA};User ID=app:{appId}@{tenantId};Password={appSecret}";
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

            public string? AppId { get; set; }
            public string? AppSecret { get; set; }
            public string? TenantId { get; set; }
        }
    }
}
