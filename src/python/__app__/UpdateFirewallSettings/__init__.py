import sys, os
import json, csv, io
import random
import logging

import azure.functions as func
from azure.common.client_factory import get_client_from_cli_profile
from azure.mgmt.resource import ResourceManagementClient
from azure.cosmosdb.table.tableservice import TableService
from azure.cosmosdb.table.models import Entity
from azure.mgmt.web import WebSiteManagementClient
from azure.mgmt.resource import SubscriptionClient
from __app__.Services import AzureAdService, WebAppService, AzureAnalysisService, AnalysisServerModel

def main(req: func.HttpRequest, ipListRowIn, ipListRowOut) -> func.HttpResponse:
    logging.info('Python HTTP trigger function processed a request.')

    # read request body
    reqMessage = req.get_json()
    logging.debug(reqMessage)

    # if available, load previous ip addressess from storage
    previousIpAddresses = None
    dataRow = json.loads(ipListRowIn) 
    if len(dataRow) > 0:    
        previousIpAddresses = json.loads(dataRow[0]['IpAddresses'])
    
    # read env vars
    tenantId = os.getenv("TENANT_ID")
    clientId = os.getenv("CLIENT_ID")
    clientSecret = os.getenv("CLIENT_SECRET")
    tableName = os.getenv("TABLE_NAME")
    entityRowKey = os.getenv("ENTITY_ROW_KEY")
    storageTableConnString = os.getenv("STORAGE_TABLE_CONN_STRING")

    # obtain access token address from Azure AD
    adservice = AzureAdService.AzureAdService(logging)
    accessToken = adservice.getAccessToken(tenantId, clientId, clientSecret)
    logging.debug('Azure AD access token: {0}'.format(accessToken))

    # obtain current possible IP addresses from web/func app
    appservice = WebAppService.WebAppService(logging)
    possibleOutboundIps = appservice.getPossibleIpAddresses(accessToken, reqMessage['webAppSubscriptionId'], reqMessage['webAppResourceGroup'], reqMessage['webAppName'])
    
    # convert to json
    possibleOutboundIps = possibleOutboundIps.split(',')
    logging.debug('ip addresses: {0}'.format(possibleOutboundIps))
    
    analysisservice = AzureAnalysisService.AzureAnalysisService(logging)
    analysisservice.updateFirewallSettings(
        accessToken, 
        previousIpAddresses, 
        possibleOutboundIps, 
        AnalysisServerModel.AnalysisServerModel(reqMessage['analysisServerSubscriptionId'], reqMessage['analysisServerResourceGroup'], reqMessage['analysisServerName']))
    
    # upsert table row
    if previousIpAddresses is None:
        data = {
            "PartitionKey": entityRowKey,
            "RowKey": entityRowKey,
            "IpAddresses": possibleOutboundIps
        }
        # insert using table storage output binding
        ipListRowOut.set(json.dumps(data))
    else:
        # update existing row using table storage client library
        table_service = TableService(connection_string=storageTableConnString)
        table_row = table_service.get_entity(tableName, entityRowKey, entityRowKey)
        table_row.IpAddresses = json.dumps(possibleOutboundIps)
        table_service.update_entity(tableName, table_row)
    
    return func.HttpResponse('done', status_code=200)

