# Lab 02 - Container Essentials

In this Lab we will explore the basics of containers. We will start by containerizing the following apps:

- Catalog Service
- Order Service
- Food Shop UI

## Task: Setup an Azure SQL Server and Database

>Note: We are not scripting this because at the moment there is no way to use the [new free Azure SQL tier](https://learn.microsoft.com/en-us/azure/azure-sql/database/free-offer?view=azuresql) using Azure CLI or Bicep.

- Create a new Azure SQL Server. Use a password that you can remember.

    ![sql server](_images/create-server.png)
  
- Create a new Database

    ![sql db](_images/create-db.png)

- Set the networking rules to allow you own ClientIP and Azure Services

    ![sql-networking](_images/sql-networking.png) 

- Get the connection string to your Database. You will need it later.

    ![sql-connection-string](_images/sql-connection-string.png)

    >Note: Use the ADO.NET authentication. Microsoft Entry Password (-less) Authentication is documented [here](https://learn.microsoft.com/en-us/azure/azure-sql/database/authentication-aad-configure?view=azuresql&tabs=azure-powershell). In production this should be the preferred way to authenticate. When using .NET you will have to use the [Microsoft.Data.SqlClient](https://www.nuget.org/packages/Microsoft.Data.SqlClient) package instead of [System.Data.SqlClient](https://www.nuget.org/packages/System.Data.SqlClient/). Find more information [here](https://learn.microsoft.com/en-us/sql/connect/ado-net/sql/azure-active-directory-authentication?view=sql-server-ver16)

- Save the connection string in Azure Key Vault. You will need it later:    

    ```bash
    env=dev
    vault=az-native-kv-$env
    sqlConnectionString=Server="tcp:aznativesql.database.windows.net,1433;Initial Catalog=aznative-food-catalog;Persist Security Info=False;User ID=aznativeadmin;Password=<PASSWORD>;MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;"

    az keyvault secret set --vault-name $vault --name "sqlConnectionString" --value $sqlConnectionString
    ```

    >Note: You will have to replace the password in the connection string with the password you used when creating the Azure SQL Server.

## Task: Containerizing the Catalog Service

- Add a docker file to Catalog Service build and test the container locally.
- Override values from appsettings.json using environment variables.
    - Set UseSQLite to false
    - Create a new Azure SQL Server & Azure SQL Database
    - Set the connection string to the new Azure SQL Database

    >Note: You can use the following module as a reference: 

    - [container build](/demos/02-containers/01-dev-workflow)    
    - [config management](/demos/02-containers/05-config-mgmt/)    
    
## Task: Containerizing the Food Shop UI

- Add a docker file to Shop UI build and test the container locally.
- Override values from appsettings.json using environment variables.
    - Set ENV_CATALOG_API_URL to the Catalog Service Url
    - Set ENV_ORDERS_API_URL to the Order Service Url

## Task: Containerizing the Order Service

- Add a docker file to Orders Api build and test the container locally.

## Task: Docker Compose

- Write a docker compose file to run the containers locally. Use the following [reference](/demos/02-containers/03-docker-dompose/docker-compose.yml)

## Task: Push to ACR

- Outsource the container build to ACR. Use the following [reference](/demos/02-containers/02-publish/publish-images.azcli)