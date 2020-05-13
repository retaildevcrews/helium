# Language Comparison

## Helium (with Cosmos DB, Key Rotation, AKS)

|   Repo    |   [csharp](https://github.com/retaildevcrews/helium-csharp) | [typescript](https://github.com/retaildevcrews/helium-typescript) | [java](https://github.com/retaildevcrews/helium-java) |
| ------------- | ------------- | ------------- | ------------- |
| **Key Vault Secrets** | | | |
| Secret Name: AcrPassword | Yes | Yes | Yes |
| Retrieved and used from Key Vault | Yes | Yes | Yes |
| Secret Name: AcrUserId | Yes | Yes | Yes |
| Retrieved and used from Key Vault | Yes | Yes | Yes |
| Secret Name: AppInsightsKey | Yes | Yes | Yes |
| Retrieved and used from Key Vault | Yes | Yes | Yes |
| Secret Name: CosmosCollection | Yes | Yes | Yes |
| Retrieved and used from Key Vault | Yes | Yes | Yes |
| Secret Name: CosmosDatabase | Yes | Yes | Yes |
| Retrieved and used from Key Vault | Yes | Yes | Yes |
| Secret Name: CosmosKey | Yes | Yes | Yes |
| Retrieved and used from Key Vault | Yes | Yes | Yes |
| Secret Name: CosmosUrl | Yes | Yes | Yes |
| Retrieved and used from Key Vault | Yes | Yes | Yes |
| Runs successfully without App Insights Key secret set  | Yes | Yes | Yes |
| Runs as expected without Cosmos secrets provided  | Yes | Yes | **No** |
| Key Rotation Handling Implemented  | Yes | **No** | **No**  |
| |
| **Cosmos DB** | | | |
| One Collection (movies) | Yes | Yes | Yes |
| getAllGenres query uses "VALUE" to get array | Yes | Yes | ([CSE Feedback]( https://csefy19.visualstudio.com/CSE%20Feedback/_workitems/edit/332438)) |
| Use prepared SQL queries | Yes | Yes | **No** |
| |
| **Container Settings** | | | |
| KEYVAULT_NAME (env var) | Yes | Yes | Yes |
| --keyvault-name (cmd line) | Yes | Yes | Yes |
| AUTH_TYPE (env var) | Yes | Yes | Yes |
| --auth-type (cmd line) | Yes | Yes | yes |
| LOG_LEVEL (env)  | Yes | Yes | **no** |
| --log-level (cmd line) | Yes | Yes | yes |
| |
| **MSI to access Key Vault** | | | |
| App Services | Yes | Yes | Yes |
| AKS | Yes | Yes | **No** |
| Local | Yes | Yes | Yes |
| |
| **Logging Behavior** | | | |
| Logging only Exceptions/Failures | Yes | Yes | **No** |
| |
| **Observability/Testing** | | | |
| Unit Tests  | Yes | Yes | Yes |
| Metrics reporting to dashboard  | Yes | Yes | Yes |
| E2E integration testing running  | Yes | Yes | Yes |
| Alerts configured  | Yes | Yes | Yes |
| |
| **Versioning** | | | |
| Version is set to [0,1].0.[Milestone]+MMdd.HHmm (UTC) | Yes | Yes | Yes |
| |
| **API Spec** | | | |
| Movie direct reads return expected values | Yes | Yes | Yes |
| Actor direct reads return expected values | Yes | Yes | Yes |
| Movie queries return expected values | Yes | Yes | Yes |
| Actor queries return expected values | Yes | Yes | Yes |
| Genre query returns expected value | Yes | Yes   | Yes |
| Featured query returns expected values | Yes | Yes | Yes |
| |
| **API Endpoints** | | | |
| Implemented /api/movies?q={}&genre={}&year={}&rating={}&actorId={}&pageNumber={}$pageSize={} | Yes | Yes | Yes |
| Implemented /api/movies/{movieId} | Yes | Yes | Yes |
| Implemented /api/actors?q={}&pageNumber={}$pageSize={} | Yes | Yes | Yes |
| Implemented /api/actors/{actorId} | Yes | Yes | Yes |
| Implemented /api/featured/movie | Yes | Yes | Yes |
| Implemented /api/genres | Yes | Yes | Yes |
| |
| **Middleware** | | | |
| /robots**.txt is handled | Yes | Yes | Yes |
| /version returns app version | Yes | Yes | Yes |
| |
| **Healthz** | | | |
| Endpoint /healthz returns pass/warn/fail | Yes | Yes | Yes |
| Endpoint /healthz/ietf returns expected ietf response | Yes | Yes | Yes |
| Idiomatic healthz endpoint implemented | Yes | na | na |
| |
| **CI-CD/Build/Deployment** | | | |
| Includes Dockerfile at root | Yes | Yes | Yes |
| Includes Dockerfile-Dev that runs locally with MSI | Yes | Yes | **No** |
| CI-CD is set up in repo | Yes | Yes | Yes |
| CI-CD includes e2e testing | **No** | **No** | **No** |
| Can be deployed with App Services | Yes | Yes | Yes |
| Can be deployed with AKS (using Pod Identity) | Yes | Yes | **No**  |
| |
| **Linting** | | | |
| Linter being used | fxcop | eslint | checkstyle, pmd, cpd and findbugs |
| **No** lint errors | Yes | Yes | Yes  |
| Linter running in Dockerfile, fails to build if errors | Yes | Yes | Yes |
| |
| **Documentation** | | | |
| Developer docs | Yes | Yes | **No** |
| Readme specific to language | Yes | Yes | Yes |
| Swagger Docs | Yes | Yes | Yes |
| Publish and use shared Swagger | Yes | Yes | Yes |
| |

## Managed Identity + Key Vault (MIKV)

|   Repo    | [csharp](https://github.com/Azure-Samples/app-service-managed-identity-key-vault-csharp) |  [typescript](https://github.com/retaildevcrews/mikv-typescript) |  |
| ------------- | ------------- | ------------- | ------------- |
| **Key Vault Secrets** |
| Secret Name: MySecret | Yes | Yes | |
| Retrieved and used from Key Vault | Yes | Yes | |
| Secret Name: AcrPassword | Yes | Yes | |
| Retrieved and used from Key Vault | Yes | Yes | |
| Secret Name: AcrUserId | Yes | Yes | |
| Retrieved and used from Key Vault | Yes | Yes | |
| |
| **Command Line and Environment Variables** | | | |
| KEYVAULT_NAME (env var) | ? | Yes | |
| --keyvault-name (cmd line) | ? | Yes | |
| AUTH_TYPE (env var) | ? | Yes | |
| --auth-type (cmd line) | ? | Yes | |
| LOG_LEVEL (env var) | ? | Yes | |
| --log-level (cmd line) | ? | Yes | |
| --help (cmd line) | ? | Yes | |
| --dry-run (cmd line) | ? | Yes | |
| |
| **MSI to access Key Vault** | | | |
| App Services | Yes | Yes | |
| Local | Yes | Yes | |
| |
| **Logging Behavior** | | | |
| Logging only Exceptions/Failures | Yes | Yes | |
| |
| **Observability/Testing** | | | |
| No App Insights (out of scope) | No | Yes | |
| No Unit Tests (out of scope) | No | Yes | |
| |
| **Versioning** | | | |
| Version is set to [0,1].0.[Milestone].MmDd.Hhmm (UTC) | ? | Yes | |
| |
| **API Spec** | | | |
| Endpoint /api/secret returns value of MySecret in KV | Yes | Yes | |
| |
| **CI-CD/Build/Deployment** | | | |
| Includes Dockerfile | Yes | Yes | |
| Can be deployed with App Services | Yes | Yes | |
| |
| **Linting** | | | |
| Linter being used | fxcop | eslint | |
| **No** lint errors | Yes | Yes | |
| Linter running in Dockerfile, fails to build if errors | **No** | | |
| |
| **Documentation** | | | |
| Readme specific to language | Yes | Yes | |
| Swagger Docs | Yes | Yes | |
| Use comment-generated Swagger | Yes | Yes |  |
