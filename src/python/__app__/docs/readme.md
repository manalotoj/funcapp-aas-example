## Contains example functions to manage an Azure Analysis service instance's firewall rules.

### UpdateFirewallSettings Function

An http triggered function that updates an Azure Analysis server's firewall rules using a web/functions app's <em>possible outbound IP addresses</em>. 

#### URL path: [root_url]/api/UpdateFirewallSettings
#### Supported request method: POST

#### Example request body
```json
{
    "analysisServerName":"<analysis server name>",
    "analysisServerResourceGroup":"<analysis server resource group name>",
    "analysisServerSubscriptionId":"<analysis server subscription Id>",
    "webAppName":"<webapp or functions app name>",
    "webAppResourceGroup":"<web app or functions app resource group name>",
    "webAppSubscriptionId":"<web app or functions app subscription Id>"
}
```

#### Configuration Settings
This section describes the required configuration settings. The settings can be defined in a local.settings.json within the __app__ directory. When running in Azure, the settings must be defined as application settings.
```json
    "FUNCTIONS_WORKER_RUNTIME": "python",
    "TENANT_ID": "<Azure AD tenant Id>",
    "CLIENT_ID": "<Azure AD application registration client Id>",
    "CLIENT_SECRET": "<Azure AD application registration client secret>",
    "STORAGE_TABLE_CONN_STRING": "<storage table connection string>",
    "TABLE_NAME": "possibleOutboundIpAddresses",
    "ENTITY_ROW_KEY": "196397ab-d9f3-45a7-b71b-4622bb4c8ef4"
```
Note: The TABLE_NAME and ENTITY_ROW_KEY values must be the same as the binding values in the /UpdateFirewallSettings/function.json file.

#### Bindings
The function defines the following bindings:
- Http Trigger: the function is triggered via HTTP request.
- Table storage input: used to read web/functions app IP address list from previous invocation.
- Table storage output: used to create a table entity upon initial execution. Updates are performed using the storage table client SDK as updates are not supported by the output binding.

##### functions.json file
```
{
  "scriptFile": "__init__.py",
  "bindings": [
    {
      "authLevel": "function",
      "type": "httpTrigger",
      "direction": "in",
      "name": "req",
      "methods": [
        "post"
      ]
    },
    {
      "type": "http",
      "direction": "out",
      "name": "$return"
    },
    {
      "name": "ipListRowIn",
      "type": "table",
      "tableName": "possibleOutboundIpAddresses",
      "rowKey": "196397ab-d9f3-45a7-b71b-4622bb4c8ef4",
      "connection": "STORAGE_TABLE_CONN_STRING",
      "direction": "in"
    },
    {
      "name": "ipListRowOut",
      "type": "table",
      "tableName": "possibleOutboundIpAddresses",
      "connection": "STORAGE_TABLE_CONN_STRING",
      "direction": "out"
    }
  ]
}

```
### Deployment

#### Prerequisites
- An app service or a functions app
- Azure storage table whose name corresponds to the name specified within the function.json file.
- Azure AD credentials (service principal) with permissions to manage the targeted Azure Analysis server (control plane read/write) and the source web/functions app (control plane read).

Deploy the functions app to Azure. This function app can be hosted on a consumption plan as the expected execution time will be far less than the default execution time of 5 mins.

Upon deployment, ensure that the required settings are configured as application settings.

### TODOs
- CICD pipeline
- Obtain access token using managed identity (refer to https://docs.microsoft.com/en-us/azure/app-service/overview-managed-identity?tabs=dotnet#obtain-tokens-for-azure-resources)
