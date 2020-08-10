import requests
import numpy as np

class AzureAnalysisService:
    def __init__(self, logging):
        self.logging = logging

    #
    # retrieve existing firewall rules; use as aseline
    #   remove what's no longer needed
    #   add anything that is new 

    def updateFirewallSettings(self, accessToken, previousIpAddresses, currentIpAddresses, analysisServer):

        # obtain existing firewall rules
        pattern = 'https://management.azure.com/subscriptions/{0}/resourceGroups/{1}/providers/Microsoft.AnalysisServices/servers/{2}?api-version=2017-08-01'
        url = pattern.format(analysisServer.subscriptionId, analysisServer.resourceGroup, analysisServer.name)
        headers = {'Authorization': 'bearer ' + accessToken}
        response = requests.get(url, headers=headers)

        if response.status_code == 200:
            analysisServerJson = response.json()
            self.logging.debug(analysisServerJson)

            firewallSettings = analysisServerJson['properties']['ipV4FirewallSettings']
            currentFirewallRules = firewallSettings['firewallRules']
            newFirewallRules = self.__buildNewFirewallRules(previousIpAddresses, currentIpAddresses, currentFirewallRules)
            firewallSettings['firewallRules'] = newFirewallRules

            updateBody = {
                "properties": firewallSettings
            }

            response = requests.patch(url, headers=headers, json=updateBody)
            if response.status_code != 200:
                message = 'failed to update analysis server firewall settings; status_code: {0}; reason: {1}'.format(response.status_code, response.reason)
                self.logging.warning(message)
                raise Exception(message)

        else:
            message = 'failed to retrieve Analysis server settings; status_code: {0}; reason: {1}'.format(response.status_code, response.reason)
            self.logging.warning(message)
            raise Exception(message)

    def __buildNewFirewallRules(self, previousIpAddresses, currentIpAddresses, currentFirewallRules):
        
        newFirewallRules = currentFirewallRules.copy()
        self.logging.debug('newFirewallRules: ')
        self.logging.debug(newFirewallRules)

        # find obsolete IP addresses and remove from newFirewallRules
        if previousIpAddresses is not None and previousIpAddresses != []:        
            ipAddressesToRemove = np.setdiff1d(previousIpAddresses, currentIpAddresses)
            for ipAddress in ipAddressesToRemove:
                rule = self.__findItem(newFirewallRules, ipAddress)
                if rule is not None: newFirewallRules.remove(rule)

        # add new IP addresses as-needed
        for ipAddress in currentIpAddresses:
            rule = self.__findItem(newFirewallRules, ipAddress) 
            if rule == None:
                self.__addFirewallRule(newFirewallRules, ipAddress)

        return newFirewallRules

    def __findItem(self, firewallRules, ipAddress):
        item = None
        for rule in firewallRules:
            if rule['rangeStart'] == ipAddress:
                item = rule
                break
        return item

    def __addFirewallRule(self, firewallRules, ipAddress):
        ruleName = ipAddress.replace('.', '-')
        firewallRules.add(
            {
                'firewallRuleName': ruleName,
                'rangeStart': ipAddress,
                'rangeEnd': ipAddress
            }
        )