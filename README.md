# TMDL Tool

The TMDL Tool is a .NET Core console application that automates the process of deploying a Tabular Model to a Power BI workspace. The tool supports both pulling a Tabular Model from a workspace and deploying a Tabular Model to a workspace. The tool is designed to be used from CLI.

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

In this repository you can also find `example_settings.json` fill it in with your own settings and rename it to `settings.json` to use it with the tool.

```json
{
    "WorkspaceXMLA": "powerbi://api.powerbi.com/v1.0/myorg/MyWorkSpace",
    "DatasetName": "MyDatasetName",
    "TmdlFolderPath": "tmdl\\MyDatasetName",
    "Action": "pull",
    "AppId": "xxxxxxxx-xxxx-xxxx-xxxx-xxxxxxxxxxxx",
    "AppSecret": "xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx",
    "TenantId": "xxxxxxxx-xxxx-xxxx-xxxx-xxxxxxxxxxxx"
}
```


`tmdl_tool pull -s settings.json`

Example output of `tmdl_tool -h`

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
  -h    Show this help message
        --help


Examples:
  tmdl_tool --workspacexmla "powerbi://api.powerbi.com/v1.0/myorg/MyWorkspace" --datasetname "MyDataset" --tmdlfolderpath "C:\TMDL" --action "pull"
  tmdl_tool -w "powerbi://api.powerbi.com/v1.0/myorg/MyWorkspace" -d "MyDataset" -t "C:\TMDL" -a "pull"
  tmdl_tool -s "C:\settings.json" -a "deploy"
  tmdl_tool -ai "xxxxxxxx-xxxx-xxxx-xxxx-xxxxxxxxxxxx" -as "xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx" -ti "xxxxxxxx-xxxx-xxxx-xxxx-xxxxxxxxxxxx" -a "deploy"
  tmdl_tool deploy
  tmdl_tool pull
```

## Contributing

Pull requests are welcome. For major changes, please open an issue first to discuss what you would like to change.

## License

This project is licensed under the [MIT](https://choosealicense.com/licenses/mit/) license.
