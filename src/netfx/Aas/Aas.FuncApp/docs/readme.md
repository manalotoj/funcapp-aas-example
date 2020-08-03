## Contains example functions to manage an Azure Analysis service instance's firewall rules.

### Aas.FuncApp Project

Defines Azure functions to set/update an Azure Analysis service's firewall rules.

#### [root_url]/api/UpdateFirewallSettings

#### Example request body
```json
{
    "analysisServerName":"<analysis server name>",
    "analysisServerResourceGroup":"<analysis server resource group name>",
    "analysisServerSubscriptionId":<subscriptionId>",
    "webAppName":"<webapp or functions app name>",
    "webAppResourceGroup":"<web app or functions app resource group name>",
    "webAppSubscriptionId":"<web app or functions app subscription Id>"
}
```

