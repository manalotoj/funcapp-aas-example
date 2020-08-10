import requests

class WebAppService:
    def __init__(self, logging):
        self.logging = logging

    def getPossibleIpAddresses(self, accessToken, subscriptionId, group, app):
        # '{1} {0}'.format('one', 'two')
        pattern = 'https://management.azure.com/subscriptions/{0}/resourceGroups/{1}/providers/Microsoft.Web/sites/{2}?api-version=2019-08-01'
        url = pattern.format(subscriptionId, group, app)
        headers = {'Authorization': 'bearer ' + accessToken}
        response = requests.get(url, headers=headers)
        json = response.json()
        ipAddresses = json['properties']['possibleOutboundIpAddresses']
        self.logging.info(ipAddresses)

        return ipAddresses

