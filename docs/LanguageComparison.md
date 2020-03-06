# Language Comparison

## Managed Identity + Key Vault (MIKV)

|   Resource/Set-up/Behavior    |   C-Sharp     |   Typescript      | Java |
| ------------- | ------------- | ------------- | ------------- |
| Repo | [mikv-csharp](https://github.com/Azure-Samples/app-service-managed-identity-key-vault-csharp) | not impl. | not impl. |
| **Key Vault Secrets** | | | |
| Secret Name: MySecret | Yes | | |
| Retrieved and used from Key Vault | Yes | | |
| Secret Name: AcrPassword | Yes | | |
| Retrieved and used from Key Vault | Yes | | |
| Secret Name: AcrUserId | Yes | | |
| Retrieved and used from Key Vault | Yes | | |
| Secret Name: AppInsightsKey | Yes | | |
| Retrieved and used from Key Vault | Yes | | |
| Runs successfully without App Insights Key secret set  | Yes | | |
| | | | |
| **Container Settings** | | | |
| KeyVaultName (env var) | Yes | | |
| KeyVaultName (cmd line) | Yes | | |
| APPINSIGHTS_INSTRUMENTATIONKEY | | | |
| | | | |
| **MSI to access Key Vault** | | | |
| App Services | Yes | | |
| Local | Yes | | |
| | | | |
| **Logging Behavior** | | | |
| Logging only Exceptions/Failures | Yes | | |
| | | | |
| **Observability/Testing** | | | |
| Metrics reporting to App Insights  | Yes | | |
| Unit Tests  | Yes | | |
| | | | |
| **Versioning** | | | |
| Version is set to 0.[Milestone].MmDd.Hhmm (UTC) | Yes | Yes | Yes |
| | | | |
| **API Spec** | | | |
| Endpoint /api/secret returns value of MySecret in KV | Yes | | |
| Endpoint /healthz returns "Healthy" | Yes | | |
| | | | |
| **CI-CD/Build/Deployment** | | | |
| Includes Dockerfile | Yes | Yes | Yes |
| Includes Dockerfile-Dev that runs locally with MSI | Yes | Yes | No |
| Can be deployed with App Services | Yes | Yes | Yes |
| | | | |
| **Linting** | | | |
| Linter being used | fxcop | tslint | |
| No lint errors | Yes | | |
| Linter running in Dockerfile, fails to build if errors | Needs Update | | |
| | | | |
| **Documentation** | | | |
| Readme specific to language | Needs update | | |
| Swagger Docs | Needs update | | |
| Publish and use shared Swagger | No | No | No |
| | | | |

## Helium (with Cosmos DB, Key Rotation, AKS)

|   Resource/Set-up/Behavior    |   C-Sharp     |   Typescript      | Java |
| ------------- | ------------- | ------------- | ------------- |
| Repo | [helium-csharp](https://github.com/retaildevcrews/helium-csharp) | [helium-typescript](https://github.com/retaildevcrews/helium-typescript) | [helium-java](https://github.com/retaildevcrews/helium-java) |
| **Key Vault Secrets** | | | |
| Secret Name: AcrPassword | Yes | Yes | Yes |
| Retrieved and used from Key Vault | Yes | Yes | Yes |
| Secret Name: AcrUserId | Yes | Yes | Yes |
| Retrieved and used from Key Vault | Yes | Yes | Yes |
| Secret Name: AppInsightsKey | Yes | Yes | Yes (but does not get used as AppInsights does not work with key-vault ) - we need to validate this - you should be able to pass as a parameter during startup |
| Retrieved and used from Key Vault | Yes | Yes | No |
| Secret Name: CosmosCollection | Yes | Yes | No: azure-cosmosdb-coll - this can be changed |
| Retrieved and used from Key Vault | Yes | Yes | No (will with coming changes) |
| Secret Name: CosmosDatabase | Yes | Yes | No: azure-cosmosdb-database - this can be changed  |
| Retrieved and used from Key Vault | Yes | Yes | No (will with coming changes) |
| Secret Name: CosmosKey | Yes | Yes | No: azure-cosmosdb-key - this can be changed  |
| Retrieved and used from Key Vault | Yes | Yes | Yes |
| Secret Name: CosmosUrl | Yes | Yes | No: azure-cosmosdb-uri - this can be changed  |
| Retrieved and used from Key Vault | Yes | Yes | Yes |
| Runs successfully without App Insights Key secret set  | Yes | No | ? |
| Runs as expected without Cosmos secrets provided  | Yes | (need to test) | ? |
| Key Rotation Handling Implemented  | Yes | No | No |
| | | | |
| **Cosmos DB** | | | |
| One Collection (movies) | Yes | Yes | Yes (not with repository pattern) |
| getAllGenres query uses "VALUE" to get array | Yes | Yes | No ([CSE Feedback]( https://csefy19.visualstudio.com/CSE%20Feedback/_workitems/edit/332438)) - workaround to run without "VALUE" and still return string array |
| | | | |
| **Container Settings** | | | |
| KeyVaultName (env var) | Yes | Yes | Yes |
| KeyVaultName (cmd line) | Yes | Yes | No |
| AUTH_TYPE (env var) | Yes | Yes | Yes |
| authtype (cmd line) | Yes | Yes | No |
| APPINSIGHTS_INSTRUMENTATIONKEY | No | No | Yes |
| | | | |
| **MSI to access Key Vault** | | | |
| App Services | Yes | Yes | Yes |
| Local | Yes | Yes | Yes (old SDK issue) |
| | | | |
| **Logging Behavior** | | | |
| Logging only Exceptions/Failures | Yes | Yes | No |
| | | | |
| **Observability/Testing** | | | |
| Unit Tests  | Yes | Yes | Yes |
| Metrics reporting to dashboard  | Yes | Yes | Yes |
| E2E integration testing running  | Yes | Yes | Yes |
| Alerts configured  | Yes | Yes | ? |
| | | | |
| **Versioning** | | | |
| Version is set to 0.[Milestone].MmDd.Hhmm (UTC) | Yes | Yes | Yes |
| | | | |
| **API Spec** | | | |
| Movie direct reads return expected values |  Yes |
| Actor direct reads return expected values |  Yes |
| Movie queries return expected values |  Yes |
| Actor queries return expected values |  Yes |
| Genre query returns expected value |  Yes |
| Featured query returns expected values |  Yes |
| | | | |
| **API Endpoints** | | | |
| Implemented /api/movies?q={}&genre={}&year={}&rating={}&actorId={}&pageNumber={}$pageSize={} | Yes | Yes | Yes |
| Implemented /api/movies/{movieId} | Yes | Yes | Yes |
| Implemented /api/actors?q={}&pageNumber={}$pageSize={} | Yes | Yes | Yes |
| Implemented /api/actors/{actorId} | Yes | Yes | Yes |
| Implemented /api/featured/movie | Yes | Yes | Yes |
| Implemented /api/genres | Yes | Yes | Yes |
| | | | |
| **Middleware** | | | |
| /robots**.txt is handled | Yes | Yes | Yes |
| /version returns app version | Yes | Yes | Yes |
| | | | |
| **Healthz** | | | |
| Endpoint /healthz returns pass/warn/fail | Yes | Yes | Yes |
| Endpoint /healthz/ietf returns expected ietf response | Yes | Yes | |
| Idiomatic healthz endpoint implemented | Yes | na | na |
| | | | |
| **CI-CD/Build/Deployment** | | | |
| Includes Dockerfile at root | Yes | Yes | Yes |
| Includes Dockerfile-Dev that runs locally with MSI | Yes | Yes | No |
| CI-CD is set up in repo | Yes | Yes | Yes |
| CI-CD includes e2e testing | No | No | No |
| Can be deployed with App Services | Yes | Yes | Yes |
| Can be deployed with AKS (using Pod Identity) | Yes | No | No |
| | | | |
| **Linting** | | | |
| Linter being used | fxcop | tslint |  |
| No lint errors | Yes | Yes |  |
| Linter running in Dockerfile, fails to build if errors (may not be possible in all 3) | Yes | Yes |  |
| | | | |
| **Documentation** | | | |
| Developer docs | Need updates | No | Need updates |
| Readme specific to language | Need updates | Need updates | Need updates |
| Swagger Docs | Yes | Yes | Need updates |
| | | | |

## I think everything below here can be removed

## Additional Java-specific requirements

NOTE: azure.cosmosdb.* can be changed to the standards - I have tested - bartr

### cosmos db application.properties setup for azure-keyvault-enabled=false  

azure.cosmosdb.uri=your-cosmosdb-uri
azure.cosmosdb.key=your-cosmosdb-key
azure.cosmosdb.database=your-cosmosdb-databasename
azure.keyvault.enabled=false

### cosmos db application.properties setup for azure-keyvault-enabled=true

azure.cosmosdb.uri=[leave-it-blank]
azure.cosmosdb.key=[leave-it-blank]
azure.cosmosdb.database=[leave-it-blank]
azure.keyvault.enabled=true
azure.keyvault.uri=https://${KeyVaultName}.vault.azure.net/

### key-vault application.properties setup for azure-keyvault-enabled=true without MSI

(I think this will be removed with Azure VM solution)

### this must be removed once we get the Azure VM solution working as it's a security issue

azure.keyvault.enabled=true
azure.keyvault.uri=put-your-azure-keyvault-uri-here
azure.keyvault.client-id=put-your-azure-client-id-here
azure.keyvault.client-key=put-your-azure-client-key-here

### key-vault application.properties setup for azure-keyvault-enabled=true with MSI

### enabled must be removed once we get the Azure VM developer experience working

azure.keyvault.enabled=true
azure.keyvault.uri= https://${KeyVaultName}.vault.azure.net/
