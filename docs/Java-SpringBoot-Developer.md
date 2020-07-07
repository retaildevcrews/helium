<!-- markdownlint-disable MD033 -->
# Java Spring WebFlux Developer Documentation

## Index

1. [Frameworks and SDKs](#Frameworks-and-SDKs)
    - [Identity](#Identity)
    - [Key Vault](#key-vault)
    - [CosmosDB](#CosmosDB)
2. [Key Rotation](#key-rotation)
    - Work in progress
3. [Cosmos DB](#cosmos-db)
    - [Spring Repository Pattern](#spring-repository-pattern)
    - [Partition Key Function](#partition-key-usage-in-spring)
4. [AKS Pod Identity Support](#aks-pod-identity-support)
    - Work in progress
5. [Versioning](#versioning)
6. [Application Insights](#application-insights)

## Frameworks and SDKs

For the Java version of this project we chose to use Spring WebFlux.  This gives us an asynchronous framework on which to build and aligns with the latest version of the CosmosDB SDK for Java which also uses WebFlux.  

<img src="./images/CosmosDB-Flux.png" />

`Figure 01: CosmosDB & WebFlux`

As illustrated above it provides a natural flow within the code sinze the frameworks match.  Additionally, we used beta framewoks to provide the best security options available on the platform presently.  Those are covered in the subsequent sections.  

> **Important**
> At the time of release the version of the CosmosDB SDK relied on a version of Netty that had a bug which would cause closed connections to be used from the connection pool.  We observed this was a factor after 10 - 12 hours of running.  There is an upcoming change that will incorporate the Netty remediation.  We will update the dependency and remove this note once available.

### Identity

The identity SDK is a core component of this project.  It is via the credentials created using the SDK that the application accesses Key Vault to resolve runtime configuration and secrets.

```xml
<dependency>
    <groupId>com.azure</groupId>
    <artifactId>azure-identity</artifactId>
    <version>1.1.0-beta.5</version>
</dependency>
```

As can be seen in the dependency configuration, this is one of the framewoks that we are using which is in a pre-release ([beta-5](https://azuresdkdocs.blob.core.windows.net/$web/java/azure-identity/1.1.0-beta.5/index.html)) state. This version of the Azure Identity SDK came with the ability for us to create discrete credentials so that we could target Managed Service Identity (MSI) and Azure CLI (CLI).  Additionally, this version would allow for the creation of other identity types such as `IntelliJCredential` and `VSCodeCredential` should you want to support them.

The identiy SDK is used only in the `KeyVaultService` `@Service` class as that is the only place the code actively authenticates presently.

```java
public KeyVaultService(IEnvironmentReader environmentReader)
    throws HeliumException {

    if (this.authType.equals(Constants.USE_MSI)) {

        credential = new ManagedIdentityCredentialBuilder().build();

    } else if (this.authType.equals(Constants.USE_CLI)) {

        credential = new AzureCliCredentialBuilder().build();

    } else if (this.authType.equals(Constants.USE_MSI_APPSVC)) {
        try {
        credential = new ManagedIdentityCredentialBuilder().build();
        } catch (final Exception ex) {
        logger.error(ex.getMessage());
        throw new HeliumException(ex.getMessage());
        }
    } else {
        this.authType = Constants.USE_MSI;
        credential = new ManagedIdentityCredentialBuilder().build();
    }
```

The above code has been abridged, but shows the creation of the credential type in the `KeyVaultService` constructor based on either a command line flag or an environment variable.

### Key Vault

The April 2020 version [4.1.2](https://azuresdkdocs.blob.core.windows.net/$web/java/azure-security-keyvault-keys/4.1.2/index.html) of the Azure Key Vault Java SDK is used.  Key Vault + Managed Service Identity are the core enabling technologies in the secure by design pattern that we are advocating.  The only setting that is passed to the application is the name of the Key Vault.  All other endpoints and access secrets are stored as secrets in Key Vault.

The `KeyVaultService` class contains all of the Key Vault access code.  It is marked with an `@Service` attribute so that it may be construced and injected by Spring.  During construction the class creates a credential as illustrated in the Identity section.  After that it creates clients for Secrets, Certificates, and Keys.

```java
secretAsyncClient = new SecretClientBuilder()
    .vaultUrl(getKeyVaultUri())
    .credential(credential)
    .addPolicy(getKvLogPolicy())
    .buildAsyncClient();

//build key client
keyAsyncClient = new KeyClientBuilder()
    .vaultUrl(getKeyVaultUri())
    .credential(credential)
    .buildAsyncClient();

//build certificate client
certificateAsyncClient = new CertificateClientBuilder()
    .vaultUrl(getKeyVaultUri())
    .credential(credential)
    .buildAsyncClient();
```

While Certificates and Keys are not used in this implementation the class was built with them completed so that its use could be expanded with little to no modification.

> **Important Note**
> This version of the KeyVault SDK will log return values in plain text if the log level is set to **BODY_AND_HEADERS**.  For that reason, a custom loggin policy is used. That implementation may be found in the `KeyVaultSecretsLogPolicy` class. That class is responsible for redacting any values that come through the log so that they will not show up in log files or in the terminal window.  Additionally, the mapping of general log setting to the custom policy setting is shown in the following table.
>
KV Log Level | Description | App Log Level | Reason
-- | -- | -- | --
BASIC | Logs only URLs, HTTP methods, and time to finish the request. | WARN, ERROR, FATAL | Matching decreased information
BODY | Logs everything in BASIC, plus all the request and response body. Note that only payloads in plain text or plain text encoded in GZIP will be logged. | N/A | Unused as it will show plain text secrets
BODY_AND_HEADERS | Logs everything in HEADERS and BODY. | TRACE, DEBUG | A custom log policy redacts plain text secrets.
HEADERS | Logs everything in BASIC, plus all the request and response headers. | INFO | Matching increased amount of information
NONE | Logging is turned off. | OFF | This level is not currently used

### Configuration Cache

A `ConfigurationService` class has been defined within the project.  This class uses the `KeyVaultService` to retrieve all of the values from Key Vault needed to access data.  This information is fetched and cached at start-up.  Additionally, the `ConfigurationService` is injected into the `CosmosDbConfig` so that location and credentials for the CosmosDB configuration may be accessed.

> **NOTE on Key Rotation**
> Key rotation will need to be implemented in this class to ensure that it repopulates the cache.  Additionally, it will need to trigger clearing any constructed versions of the Data Access Classes as they will need to use the update configuration values.



### CosmosDB

```xml
<dependency>
    <groupId>com.microsoft.azure</groupId>
    <artifactId>spring-data-cosmosdb</artifactId>
    <version>2.2.2</version>
</dependency>
```

#### Adding Key Vault via Spring Configuration

Add the dependency "azure-keyvault-secrets-spring-boot-starter" and "azure-client-authentication" to the maven POM file

[POM.xml](https://github.com/microsoft/helium-java/blob/master/pom.xml)

```xml
<dependencies>
  <dependency>
      <groupId>com.microsoft.azure</groupId>
      <artifactId>azure-keyvault-secrets-spring-boot-starter</artifactId>
      <version>2.1.6</version>
    </dependency>
    <dependency>
      <groupId>com.microsoft.azure</groupId>
      <artifactId>azure-client-authentication</artifactId>
      <version>1.6.10</version>
    </dependency>
</dependencies>
```

To use Managed Identity with App Service - please refer to [Using Managed Identity](https://docs.microsoft.com/en-us/azure/app-service/overview-managed-identity?tabs=dotnet)

- Open the application.properties file and add below properties to specify your Azure Key Vault url
- azure.keyvault.enabled is used to turn on/off Azure Key Vault Secret property source, default is true.

[application.properties](https://github.com/microsoft/helium-java/blob/master/src/main/resources/application.properties)

```properties
azure.keyvault.enabled=true
azure.keyvault.uri=https://${KeyVaultName}.vault.azure.net/
```

### JAVA-SPRINGBOOT-KEYVAULT-SDK-GAP : Local Development issue with spring-boot starter for key-vault with MSI

- This does not works in the local development scenario as the spring-boot keyvault java sdk fails to get Key Vault access through MSI on local development environment​
- This is a security issue that affects the development environment

Local development environment can only access keyvault with clear-text credentials as below

```properties

azure.keyvault.uri=https://${KeyVaultName}.vault.azure.net/
azure.keyvault.client-id=${client_id}
azure.keyvault.client-key=${client_key}

```

#### Solution

1. Use clear-text for testing locally
2. Use service principal with client-key stored in the Azure DevOps as secret value

#### Now, you can get Azure Key Vault secret value as a configuration property in spring framework

[Application.java](https://github.com/microsoft/helium-java/blob/master/src/main/java/com/microsoft/azure/helium/Application.java)

```java

@EnableSwagger2
@SpringBootApplication
public class Application implements CommandLineRunner {
    private static final Logger logger = LoggerFactory.getLogger(Application.class);
    @Autowired
    Environment environment;

    @Value("${azure.keyvault.uri}")
    private String keyUri;

    @Value("${azure.keyvault.client-key}")
    private String key;

    public static void main(String[] args) {
        SpringApplication.run(Application.class, args);
    }

    public void run(String... varl) throws Exception {
        logger.info("keyUri: " + keyUri);
        logger.info("key: " + key);
    }
}

```

## Key Rotation

- Work in progress

## Cosmos DB

### Spring Repository Pattern

### JAVA-SPRINGBOOT-COSMOSDB-SDK-GAP: Spring framework does not support different document types in the same collection

This is limitation and needs a spring-boot patch. Spring-Boot entity framework works on entity<-> repo mapping.
Hence we cannot model movies, genre,actors in the same "movies" collection as recommended in this best practices [document](https://github.com/4-co/imdb)
spring-data-cosmosdb expects to have 3 separate collections as "movies", "genre", "actor"
However this is cost implication as "genre" collection having just 30 elements needs a separate collection and is 3 times the RU querying for 3 different collection.

#### Solution

- spring-data-cosmosdb-starter is based on Spring-Boot's Spring-Data framework
- Spring-Data Defines an entity-specific repository and is built on the following 3 principles:
  1. Repository pattern - No code repositories
  2. Reduced boiler plate code for CRUD operations
  3. Generated Queries ex: findBy

```java

@Document(collection = Constants.DEFAULT_MOVIE_COLLECTION_NAME)
@JsonPropertyOrder({"id","movieId", "partitionKey",  "type", "title", "textSearch", "year", "runtime", "rating", "votes", "totalScore", "genres", "roles" })
public class Movie  extends  MovieBase{

  @JsonIgnore
  @Id
  private String id;

  @PartitionKey
  private String partitionKey;

  private double rating;
  private long votes;
  private long totalScore;
  private String textSearch;
  private List<Role> roles;


    public Movie() {

    }
}

@Repository
public interface MoviesRepository extends DocumentDbRepository<Movie, String>  {
    List<Movie> findByMovieId(String movieId);
    List<Movie> findByTextSearchContaining(String movieName);

}

@Service
public class MoviesService {

    @Autowired
    MoviesRepository repository;

    private static Gson gson = new Gson();


    public List<Movie> getAllMovies(Optional<String> query, Sort sort) {
        if (query.isPresent() && !StringUtils.isEmpty(query.get())) {
            return repository.findByTextSearchContaining(query.get().toLowerCase());
        } else {
            return (List<Movie>) repository.findAll(sort);
        }
    }
}


```

### Partition Key usage in Spring

### JAVA-SPRINGBOOT-SDK-GAP: spring-boot cosmosdb sdk does not support single document read with partition key​ as it internally calls QueryDocument and not a ReadDocument hence it is not a 1 RU operation

To query by partition id annotate field partition column with @partitionkey in the [document entity](https://github.com/jyotsnaravikumar/helium-java/blob/CSE-feedbacks/src/main/java/com/microsoft/azure/helium/app/movie/Movie.java)

```java

@Document(collection = Constants.DEFAULT_MOVIE_COLLECTION_NAME)
@JsonPropertyOrder({"id","movieId", "partitionKey",  "type", "title", "textSearch", "year", "runtime", "rating", "votes", "totalScore", "genres", "roles" })
public class Movie  extends  MovieBase{

  @JsonIgnore
  @Id
  private String id;

  @PartitionKey
  private String partitionKey;

  private double rating;
  private long votes;
  private long totalScore;
  private String textSearch;
  private List<Role> roles;

}
```

Then query by field name example findByMovieId as below:

[MoviesService.java](https://github.com/jyotsnaravikumar/helium-java/blob/CSE-feedbacks/src/main/java/com/microsoft/azure/helium/app/movie/MoviesService.java)

A good CosmosDB best practice is to compute the partition key from the ID. In our case, we use the integer portion of the Movie or Actor document mod 10. This gives us 10 partitions ("0" - "9") with good distribution. For a deeper discussion on the document modeling decisions, please read this [document](https://github.com/4-co/imdb)

```java

@Service
public class MoviesService {

    @Autowired
    MoviesRepository repository;

    public Optional<Movie> getMovie(String movieId) {
        if (StringUtils.isEmpty(movieId)) {
            throw new NullPointerException("movieId cannot be empty or null");
        }
        //queries by partitionid - partitionkey is the field annotated with @partitionkey
        List<Movie> movies = repository.findByMovieId(movieId);
        //queries without partitionkey
        //repository.findById(movieId);
        if (movies.isEmpty()) {
            return Optional.empty();
        } else {
            return Optional.of(movies.get(0));
        }
    }
}

```

In order to directly read a document using 1 RU (assuming the document is 1K or less), you need the document's ID and partition key. A good CosmosDB best practice is to compute the partition key from the ID. In our case, we use the integer portion of the Movie or Actor document mod 10. This gives us 10 partitions ("0" - "9") with good distribution. For a deeper discussion on the document modeling decisions, please read this [document](https://github.com/4-co/imdb)

### JAVA-SPRINGBOOT-COSMOSDB-SDK-GAP : support for usage GetRequestCharge​ metrics

GetRequestCharge metrics are not supported in spring-boot-cosmosdb sdk.

### JAVA-SPRINGBOOT-COSMOSDB-SDK-GAP : No native query support with @Query Annotation

 ```java
 @Repository
public interface MoviesRepository extends DocumentDbRepository<Movie, String>  {
    List<Movie> findByMovieId(String movieId);
    List<Movie> findByTextSearchContaining(String movieName);
    //Native queries example commented below are not supported by spring-boot-cosmosdb sdk
    //@Query(“Select * from c where c.year= :1 order by order by c.year ”)
    //List<Movie> findMoviesByYear(int year);

}
 ```

## AKS Pod Identity Support

- Work in progress

## Versioning

Helium dynamically builds a version string based on spring-boot snapshot version and date time of build. This is displayed in both the Healthz output as well as the Swagger UI.

[Build Config](BuildConfig.java)

```xml
    <plugin>
        <groupId>org.springframework.boot</groupId>
        <artifactId>spring-boot-maven-plugin</artifactId>
        <executions>
          <execution>
            <id>build-info</id>
            <goals>
              <goal>build-info</goal>
            </goals>
          </execution>
        </executions>
      </plugin>

```

```java

public class BuildConfig {

    @Autowired
    BuildProperties buildProperties;

    public String getBuildVersion(){

        String buildName = buildProperties.getName();
        String buildVersion = buildProperties.getVersion();
        String buildTime =  String.valueOf(buildProperties.getTime().getEpochSecond());
        System.out.println("buildName " + buildName +"_"+ "buildVersion "+ buildVersion + "_" + "buildTime "+ buildTime );
        return buildVersion+"."+buildTime ;
    }
}

```

### Application Insights

Get an Application Insights instrumentation key by creating an Application Insights resource. Set the application type to Java web application.

Store the application insights instrumentation key in Key Vault to configure Helium to use Application Insights. When configured, the DI creates a singleton instance of TelemetryClient that can be used to track custom events and metrics.

#### Adding Application Insights from Spring framework DI

Add the dependency "azure-keyvault-secrets-spring-boot-starter" and "azure-client-authentication" to the maven POM file

[POM.xml](https://github.com/microsoft/helium-java/blob/master/pom.xml)

```xml
    <dependency>
      <groupId>com.microsoft.azure</groupId>
      <artifactId>applicationinsights-web-auto</artifactId>
      <version>2.5.1</version>
    </dependency>
```

Add [ApplicationInsights.xml](https://github.com/jyotsnaravikumar/helium-java/blob/CSE-feedbacks/src/main/resources/ApplicationInsights.xml)
Read the application insights from key-vault

```xml
    <InstrumentationKey>${APP_INSIGHTS_KEY}</InstrumentationKey>
```

Install the [Java Agent](https://docs.microsoft.com/en-us/azure/azure-monitor/app/java-agent) to capture outgoing HTTP calls, JDBC queries, application logging, and better operation naming.

Configure the agent [AI-Agent.xml](https://github.com/jyotsnaravikumar/helium-java/blob/CSE-feedbacks/AI-Agent.xml)

Run  application it in debug mode on your development machine, or publish to your server to view telemetry in Application Insights Resource

### NOTE JAVA-SPRINGBOOT-SDK-GAP: spring-boot sdk for Application Insights does not work thru Configuration based Injection, it works only thru XML based injection

[Configuration based Injection](https://docs.microsoft.com/en-us/java/azure/spring-framework/configure-spring-boot-java-applicationinsights?view=azure-java-stable#configure-springboot-application-to-send-log4j-logs-to-application-insights) does not work

[XML based injection](https://docs.microsoft.com/en-us/azure/azure-monitor/app/java-get-started) works

#### Solution

Use XML Based injection for setting up application insights with App Service
