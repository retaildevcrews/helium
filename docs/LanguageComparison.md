# Language Comparison

## Helium (with Cosmos DB, Key Rotation, AKS)

|   Repo    |   [csharp](https://github.com/retaildevcrews/helium-csharp) | [typescript](https://github.com/retaildevcrews/helium-typescript) | [java](https://github.com/retaildevcrews/helium-java) |
| ------------- | ------------- | ------------- | ------------- |
| **Key Vault Secrets** | | | |
| Secret Name: AcrPassword | Yes | Yes |  |
| Retrieved and used from Key Vault | Yes | Yes |  |
| Secret Name: AcrUserId | Yes | Yes |  |
| Retrieved and used from Key Vault | Yes | Yes |  |
| Secret Name: AppInsightsKey | Yes | Yes |  |
| Retrieved and used from Key Vault | Yes | Yes |  |
| Secret Name: CosmosCollection | Yes | Yes |  |
| Retrieved and used from Key Vault | Yes | Yes | |
| Secret Name: CosmosDatabase | Yes | Yes |   |
| Retrieved and used from Key Vault | Yes | Yes |  |
| Secret Name: CosmosKey | Yes | Yes |   |
| Retrieved and used from Key Vault | Yes | Yes |  |
| Secret Name: CosmosUrl | Yes | Yes |  |
| Retrieved and used from Key Vault | Yes | Yes |  |
| Runs successfully without App Insights Key secret set  | Yes | Yes |  |
| Runs as expected without Cosmos secrets provided  | Yes | Yes |  |
| Key Rotation Handling Implemented  | Yes | **No** |  |
| |
| **Cosmos DB** | | | |
| One Collection (movies) | Yes | Yes |  |
| getAllGenres query uses "VALUE" to get array | Yes | Yes | ([CSE Feedback]( https://csefy19.visualstudio.com/CSE%20Feedback/_workitems/edit/332438)) |
| |
| **Container Settings** | | | |
| KeyVaultName (env var) | Yes | Yes |  |
| KeyVaultName (cmd line) | Yes | Yes |  |
| AUTH_TYPE (env var) | Yes | Yes |  |
| authtype (cmd line) | Yes | Yes |  |
| |
| **MSI to access Key Vault** | | | |
| App Services | Yes | Yes |  |
| Local | Yes | Yes |  |
| |
| **Logging Behavior** | | | |
| Logging only Exceptions/Failures | Yes | Yes |  |
| |
| **Observability/Testing** | | | |
| Unit Tests  | Yes | Yes |  |
| Metrics reporting to dashboard  | Yes | Yes |  |
| E2E integration testing running  | Yes | Yes |  |
| Alerts configured  | Yes | Yes |  |
| |
| **Versioning** | | | |
| Version is set to [0,1].0.[Milestone]+MmDd.Hhmm (UTC) | Yes | Yes |  |
| |
| **API Spec** | | | |
| Movie direct reads return expected values | Yes | **No** |  |
| Actor direct reads return expected values | Yes | **No** |  |
| Movie queries return expected values | Yes | **No** |  |
| Actor queries return expected values | Yes | **No** |  |
| Genre query returns expected value | Yes | **No** |  |
| Featured query returns expected values | Yes | **No** |  |
| |
| **API Endpoints** | | | |
| Implemented /api/movies?q={}&genre={}&year={}&rating={}&actorId={}&pageNumber={}$pageSize={} | Yes | Yes |  |
| Implemented /api/movies/{movieId} | Yes | Yes |  |
| Implemented /api/actors?q={}&pageNumber={}$pageSize={} | Yes | Yes |  |
| Implemented /api/actors/{actorId} | Yes | Yes |  |
| Implemented /api/featured/movie | Yes | Yes |  |
| Implemented /api/genres | Yes | Yes |  |
| |
| **Middleware** | | | |
| /robots**.txt is handled | Yes | Yes |  |
| /version returns app version | Yes | Yes |  |
| |
| **Healthz** | | | |
| Endpoint /healthz returns pass/warn/fail | Yes | Yes |  |
| Endpoint /healthz/ietf returns expected ietf response | Yes | Yes | |
| Idiomatic healthz endpoint implemented | Yes | na | na |
| |
| **CI-CD/Build/Deployment** | | | |
| Includes Dockerfile at root | Yes | Yes |  |
| Includes Dockerfile-Dev that runs locally with MSI | Yes | Yes |  |
| CI-CD is set up in repo | Yes | Yes |  |
| CI-CD includes e2e testing | **No** | **No** |  |
| Can be deployed with App Services | Yes | Yes |  |
| Can be deployed with AKS (using Pod Identity) | Yes | **No** |  |
| |
| **Linting** | | | |
| Linter being used | fxcop | eslint |  |
| **No** lint errors | Yes | Yes |  |
| Linter running in Dockerfile, fails to build if errors | Yes | Yes |  |
| |
| **Documentation** | | | |
| Developer docs | **No** | **No** |  |
| Readme specific to language | Yes | Yes |  |
| Swagger Docs | Yes | Yes |  |
| Publish and use shared Swagger | **No** | **No** |  |
| |
| ***Managed Identity + Key Vault (MIKV)*** |
|   Repo    | [csharp](https://github.com/Azure-Samples/app-service-managed-identity-key-vault-csharp) |         |  |
| **Key Vault Secrets** |
| Secret Name: MySecret | Yes | | |
| Retrieved and used from Key Vault | Yes | | |
| Secret Name: AcrPassword | Yes |  | |
| Retrieved and used from Key Vault | Yes | | |
| Secret Name: AcrUserId | Yes | | |
| Retrieved and used from Key Vault | Yes | | |
| Secret Name: AppInsightsKey | Yes | | |
| Retrieved and used from Key Vault | Yes | | |
| Runs successfully without App Insights Key secret set  | Yes | | |
| |
| **Container Settings** | | | |
| KeyVaultName (env var) | Yes | | |
| KeyVaultName (cmd line) | Yes | | |
| APPINSIGHTS_INSTRUMENTATIONKEY | | | |
| |
| **MSI to access Key Vault** | | | |
| App Services | Yes | | |
| Local | Yes | | |
| |
| **Logging Behavior** | | | |
| Logging only Exceptions/Failures | Yes | | |
| |
| **Observability/Testing** | | | |
| Metrics reporting to App Insights  | Yes | | |
| Unit Tests  | Yes | | |
| |
| **Versioning** | | | |
| Version is set to 0.[Milestone].MmDd.Hhmm (UTC) | Yes | | |
| |
| **API Spec** | | | |
| Endpoint /api/secret returns value of MySecret in KV | Yes | | |
| Endpoint /healthz returns "Healthy" | Yes | | |
| |
| **CI-CD/Build/Deployment** | | | |
| Includes Dockerfile | Yes | | |
| Includes Dockerfile-Dev that runs locally with MSI | Yes | | |
| Can be deployed with App Services | Yes | | |
| |
| **Linting** | | | |
| Linter being used | fxcop | | |
| **No** lint errors | Yes | | |
| Linter running in Dockerfile, fails to build if errors | **No** | | |
| |
| **Documentation** | | | |
| Readme specific to language | Yes | | |
| Swagger Docs | Yes | | |
| Publish and use shared Swagger | **No** |  |  |
