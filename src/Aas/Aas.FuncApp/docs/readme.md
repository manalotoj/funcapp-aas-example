## Contains example functions to manage an Azure Analysis service instance's firewall rules.

### Aas.FuncApp Project

Defines Azure functions to set/update an Azure Analysis service's firewall rules.

## UpdateFirewallSettings Function
Description: Updates an Azure Analysis server's firewall rules using a designated Azure web app, or, Azure functions app's <em>list of possible outbound IP addresses</em>. All firewall rules that are not associated with the designated web app or functions app are unchanged. If the list (of possible outbound IP addresses) changes between invocations, obsolete firewall rules are automatically deleted.

Path: [root_url]/api/UpdateFirewallSettings

Example request body:
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

