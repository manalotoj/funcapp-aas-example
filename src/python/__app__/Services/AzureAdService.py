import requests

class AzureAdService:
    def __init__(self, logging):
        self.logging = logging

    def getAccessToken(self, tenantId, clientId, clientSecret):
        pattern = 'https://login.microsoftonline.com/{0}/oauth2/token'
        url = pattern.format(tenantId)
        #headers = {'Content-Type': '}

        response = requests.post(
            url, 
            files=(
                ('client_id', (None, clientId)),
                ('client_secret', (None, clientSecret)),
                ('resource', (None, 'https://management.azure.com/')),
                ('grant_type', (None, 'client_credentials')) 
            )
        )
        if response.status_code == 200:
            self.logging.debug('successfully obtained access token from AAD')
            json = response.json()
            return json['access_token']
        else:
            self.logging.warning('failed to retrieve access token; status_code {0}, reason {1}'.format(json.status_code, json.reason))
            return None