# Java/Spring Boot Developer Documentation

## Index:
1. [Managed Identity and Key Vault](#managed-identity-and-key-vault)
    - [Key Vault](#key-vault)
2. [Key Rotation](#key-rotation) - WIP
3. [Cosmos DB](#cosmos-db)
    - [Spring Repository Pattern](#spring-repository-pattern)
    - [Partition Key Function](#partition-key-usage-in-spring)
4. [AKS Pod Identity Support](#aks-pod-identity-support) - WIP
5. [Versioning](#versioning)
6. [Application Insights](#application-insights)


## Managed Identity and Key Vault

After creating a Managed Identity for the Helium web app and assigning get and list secret permissions to Key Vault, the following code successfully authenticates using Managed Identity to create the Key Vault Client. Leveraging Managed Identity in this way eliminates the need to store any credential information in app code. 

### Key Vault 

If you need access to Key Vault in your app, you can retrieve the Key Vault Client from Spring framework's DI rather than have to track credentials and create a new connection.

#### Adding Key Vault via Spring Configuration

Add the dependency "azure-keyvault-secrets-spring-boot-starter" and "azure-client-authentication" to the maven POM file

[POM.xml] (https://github.com/microsoft/helium-java/blob/master/pom.xml) 

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

Open application.properties file and add below properties to specify your Azure Key Vault url, Azure service principal client id and client key. azure.keyvault.enabled is used to turn on/off Azure Key Vault Secret property source, default is true.

[application.properties](https://github.com/microsoft/helium-java/blob/master/src/main/resources/application.properties)

```properties

azure.keyvault.enabled=true
azure.keyvault.uri=https://${KeyVaultName}.vault.azure.net/
azure.keyvault.client-id=${client_id}
azure.keyvault.client-key=${client_key}

```

To use managed identities for App Services - please refer to [Using ManagedIdentities setup] (https://docs.microsoft.com/en-us/azure/app-service/overview-managed-identity?tabs=dotnet)

To use it in an App Service, add the below properties:

```properties
azure.keyvault.enabled=true
azure.keyvault.uri=https://${KeyVaultName}.vault.azure.net/
```

### JAVA-SPRINGBOOT-KEYVAULT-SDK-GAP : Local Development issue with spring-boot starter for key-vault with MSI
This does not works in the local development scenario as the spring-boot keyvault java sdk fails to get Key Vault access through MSI on local development environment​ 
This is a security hole in development environment which was uncovered here and now is seen with Walmart as well

Local development environments cannot access keyvault thru MSI as below
```properties
azure.keyvault.uri=https://gelato.vault.azure.net/
azure.keyvault.client-id=17305c4a-13a8-444b-bb88-1a7e184f6b52
azure.keyvault.client-key=
```

Local development environments can access keyvault with clear-text as below
```properties
azure.keyvault.uri=https://gelato.vault.azure.net/
azure.keyvault.client-id=17305c4a-13a8-444b-bb88-1a7e184f6b52
azure.keyvault.client-key=c5f6781e-8d02-47d3-8f79-cdf892590892
```
### Solution:
1. Use clear-text for testing locally 
2. Use service principal with client-key stored in the Azure DevOps as secret value - Samsclub is doing this

#### Now, you can get Azure Key Vault secret value as a configuration property in spring framework
[Application.java] (https://github.com/microsoft/helium-java/blob/master/src/main/java/com/microsoft/azure/helium/Application.java)

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

WIP

## Cosmos DB

### Spring Repository Pattern

### JAVA-SPRINGBOOT-COSMOSDB-SDK-GAP: Spring framework does not support different document types in the same collection 

This is limitation and needs a spring-boot patch. Spring-Boot entity framework works on entity<-> repo mapping.
Hence we cannot model movies, genre,actors in the same "movies" collection as recommended in this best practices [document](https://github.com/4-co/imdb)
spring-data-cosmosdb expects to have 3 separate collections as "movies", "genre", "actor"
However this is cost implication as "genre" collection having just 30 elements needs a separate collection and is 3 times the RU querying for 3 different collection.

### Solution:
spring-data-cosmosdb-starter is based Spring-Boot's Spring-Data framework .
Spring-Data Defines an entity-specific repository and is built on the following 3 principles:
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

### JAVA-SPRINGBOOT-SDK-GAP: spring-boot cosmosdb sdk does not support single document read with partition key​ as it internally calls QueryDocument and not a ReadDocument hence it is not a 1 RU 

To query by partition id annotate field partition column with @partitionkey in the document entity

(https://github.com/jyotsnaravikumar/helium-java/blob/CSE-feedbacks/src/main/java/com/microsoft/azure/helium/app/movie/Movie.java)

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

Then Query by field name example findByMovieId as below:

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

### Solution:
1. Customers are moving to Cosmos Mongo for easier developer experience with features on Native queries, Cross doc queries, paginations, flexibility support

## AKS Pod Identity Support

WIP 


## Versioning

Helium dynamically builds a version string based spring-boot snapshot version and date time of build. This is displayed in both the Healthz output as well as the Swagger UI. 

[BuildConfig.java](BuildConfig)


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

[POM.xml] (https://github.com/microsoft/helium-java/blob/master/pom.xml) 

```xml
    <dependency>
      <groupId>com.microsoft.azure</groupId>
      <artifactId>applicationinsights-web-auto</artifactId>
      <version>2.5.1</version>
    </dependency>
```

Add [ApplicationInsights.xml] (https://github.com/jyotsnaravikumar/helium-java/blob/CSE-feedbacks/src/main/resources/ApplicationInsights.xml)
Read the application insights from key-vault

```xml
    <InstrumentationKey>${APP_INSIGHTS_KEY}</InstrumentationKey>
```

Install the Java Agent(https://docs.microsoft.com/en-us/azure/azure-monitor/app/java-agent) to capture outgoing HTTP calls, JDBC queries, application logging, and better operation naming.

Configure the agent [AI-Agent.xml] https://github.com/jyotsnaravikumar/helium-java/blob/CSE-feedbacks/AI-Agent.xml

Run  application it in debug mode on your development machine, or publish to your server to view telemetry in Application Insights Resource


### NOTE JAVA-SPRINGBOOT-SDK-GAP: spring-boot sdk for Application Insights does not work thru Configuration based Injection, it works only thru XML based injection

[Configuration based Injection](https://docs.microsoft.com/en-us/java/azure/spring-framework/configure-spring-boot-java-applicationinsights?view=azure-java-stable#configure-springboot-application-to-send-log4j-logs-to-application-insights) does not work

[XML based injection](https://docs.microsoft.com/en-us/azure/azure-monitor/app/java-get-started) works

### Solution
Use XML Based injection for setting up application insights with App Service
