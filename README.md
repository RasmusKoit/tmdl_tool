# TMDL Tool

The TMDL Tool is a .NET Core console application that automates the process of deploying a Tabular Model to a Power BI workspace. The tool supports both pulling a Tabular Model from a workspace and deploying a Tabular Model to a workspace. The tool is designed to be used from CLI. TMDL uses Azure Service Principal app authentication to authenticate with the Power BI API.

## About TMDL

[TMDL](https://learn.microsoft.com/en-us/analysis-services/tmdl/tmdl-overview?view=power-bi-premium-current) is Tabular Model Definition Language (TMDL) is an object model definition syntax for tabular data models at compatibility level 1200 or higher.

Key elements of TMDL include:

* Full compatibility with the entire Tabular Object Model (TOM).
* Text-based and optimized for human interaction and readability. TMDL uses a grammar syntax similar to YAML. Each TMDL object is represented in text with minimal delimiters and uses indentation to demark parent-child relationships.
* Better editing experience, especially on properties with embed expressions from different content-types, like Data Analysis Expression (DAX) and M.
* Better for collaboration because of its folder representation where each model object has an individual file representation, making it more source control friendly.

## Installation

To install `tmdl_tool`, simply clone the repository and build the solution using Visual Studio. Once the solution has been built, you can find the `tmdl_tool` executable in the `bin` folder. You can also download the latest release from the release section of this repository.

## Usage

To use `tmdl_tool`, simply run the executable from the command line and provide the required arguments. The following arguments are available:


- `WorkspaceXMLA`: The URL of the Power BI workspace.
- `DatasetName`: The name of the Power BI dataset.
- `TmdlFolderPath`: The path to the TMDL folder.
- `Action`: The action to perform (pull or deploy).
- `AppId`: The Application ID of the Azure AD Application.
- `AppSecret`: The Application Secret of the Azure AD Application.
- `TenantId`: The Tenant ID of the Azure AD Application.
- `AccessToken`: Externally aquired Oauth2/OpenId access token.
- `Verbose`: (default = true) Enable verbose output of the tool.

In this repository you can also find `example_settings.json` fill it in with your own settings and rename it to `settings.json` to use it with the tool.

```json
{
    "WorkspaceXMLA": "powerbi://api.powerbi.com/v1.0/myorg/MyWorkSpace",
    "DatasetName": "MyDatasetName",
    "TmdlFolderPath": "tmdl\\MyDatasetName",
    "Action": "pull",
    "AppId": "xxxxxxxx-xxxx-xxxx-xxxx-xxxxxxxxxxxx",
    "AppSecret": "xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx",
    "TenantId": "xxxxxxxx-xxxx-xxxx-xxxx-xxxxxxxxxxxx",
    "AccessToken": "zzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzz...",
    "Verbose": true
}
```


`tmdl_tool pull -s settings.json`

Output of `tmdl_tool -h`

```bash
Usage: tmdl_tool [arguments]

Arguments:
  -w    The URL of the Power BI workspace
        --workspacexmla "<url>"
  -d    The name of the Power BI dataset
        --datasetname "<name>"
  -t    The path to the TMDL folder
        --tmdlfolderpath "<path>"
  -a    The action to perform (pull or deploy)
        --action "<pull|deploy>"
  -s    The path to the settings file (default: settings.json)
        --settingsfilepath "<path>"
  -ai   The application ID of the Azure AD app
        --appid "<id>"
  -as   The application secret of the Azure AD app
        --appsecret "<secret>"
  -ti   The tenant ID of the Azure AD app
        --tenantid "<id>"
  -at   Externally aquired Access Token for Azure AD app
        --accessToken "<BearerTypeAccessToken>"
  -v    Show progress on STDOUT
        --verbose "<true|false>" (default: true)

  -h    Show this help message
        --help


Environment:
  Use an Environment Variable `tmdl_accessToken` to pass a Bearer Access Token to the PowerBI Server

Examples:
  tmdl_tool --workspacexmla "powerbi://api.powerbi.com/v1.0/myorg/MyWorkspace" --datasetname "MyDataset" --tmdlfolderpath "C:\TMDL" --action "pull"
  tmdl_tool -w "powerbi://api.powerbi.com/v1.0/myorg/MyWorkspace" -d "MyDataset" -t "C:\TMDL" -a "pull"
  tmdl_tool -s "C:\settings.json" -a "deploy"
  tmdl_tool -ai "xxxxxxxx-xxxx-xxxx-xxxx-xxxxxxxxxxxx" -as "xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx" -ti "xxxxxxxx-xxxx-xxxx-xxxx-xxxxxxxxxxxx" -a "deploy"
  tmdl_tool deploy
  tmdl_tool pull
```
Example verbose output of the tool:
```bash
1971-01-01 14:51:15.642 [tmdl_tool v.1.0.3.0, Microsoft.AnalysisServices.Tabular: 19.82.0.0]: Starting with options:
{
  "WorkspaceXMLA": "powerbi://api.powerbi.com/v1.0/myorg/myWorkspace",
  "DatasetName": "myDataset",
  "TmdlFolderPath": "../myDataset_pull",
  "Action": "pull",
  "AppId": "",
  "AppSecret": "",
  "TenantId": "",
  "AccessToken": "********",
  "Verbose": true,
  "SettingsFilePath": "settings.json"
}
1971-01-01 14:51:15.693 [tmdl_tool v.1.0.3.0, Microsoft.AnalysisServices.Tabular: 19.82.0.0]: Using Access Token from Settings / Command Line
1971-01-01 14:51:15.694 [tmdl_tool v.1.0.3.0, Microsoft.AnalysisServices.Tabular: 19.82.0.0]: Connecting to powerbi://api.powerbi.com/v1.0/myorg/myWorkspace
1971-01-01 14:51:19.195 [tmdl_tool v.1.0.3.0, Microsoft.AnalysisServices.Tabular: 19.82.0.0]: Getting Databases for myDataset
1971-01-01 14:51:19.196 [tmdl_tool v.1.0.3.0, Microsoft.AnalysisServices.Tabular: 19.82.0.0]: Serializing model from myDataset to ../myDataset_pull
1971-01-01 14:51:21.518 [tmdl_tool v.1.0.3.0, Microsoft.AnalysisServices.Tabular: 19.82.0.0]: Model pulled successfully to ../myDataset_pull
```

## Contributing

Pull requests are welcome. For major changes, please open an issue first to discuss what you would like to change.

## License

This project is licensed under the [MIT](https://choosealicense.com/licenses/mit/) license.

