# Helium Parameter Specs Documentation

## System Overview

Helium is WebAPI reference application designed to "fork and code" in c#, java, typescript languages with the following features:

* Securely build, deploy and run an App Service (Web App for Containers) application
* Use Managed Identity to securely access resources
* Securely store secrets in Key Vault
* Securely build and deploy the Docker container from Container Registry or Azure DevOps
* Connect to and query CosmosDB
* Automatically send telemetry and logs to Azure Monitor
* Instructions for setting up Key Vault, ACR, Azure Monitor and Cosmos DB are in the Helium language specific repo readme.

## Design Considerations

## Assumptions and Dependencies

### General Constraints

### Goals and Guidelines

### Development Methods

## Architectural Strategies

### Developer Friendly

## Reusable/Adaptable

### Secure

## System Architecture
[Reference](https://github.com/retaildevcrews/helium)


## Design Considerations
Helium connects and queries the IMDB database. [Reference](https://github.com/retaildevcrews/imdb)
This repository contains an extract of 1300 movies and associated actors, producers, directors and genres from the IMDb public data available.
Based on design considerations explained in the repo there are 4 document types - Movies, Actors, Genres, FeaturedMovie stored in the one container 'Movies' which document type field one of movie, actor, genre, featuredmovie

## Parameter Validations

## Movies

### API /api/movies

### Query Parameters

* Name : q
* Description : query
* Type: string
* Parameter Validations:
    * Valid input containing results : q length >= 3 characters
         * Status : 200
         * Content-Type: application/json
         * Response Body: List of movies filtered by query as [{}, {},..]
    * Valid input and does not contain results
         * Status : 200
         * Content-Type: application/json
         * Response Body: Empty list []
    * Invalid input : empty , q length < 3 characters
         * Status : 400
         * Content-Type: text/plain
         * Response Body: Invalid query parameter


* Name : year
* Type: integer
* Parameter Validations:
    * Valid input range with results - [1874 , currentyear+5]
         * Status : 200
         * Content-Type: application/json
         * Response Body: List of movies filtered by year as [{}, {},..]
    * Valid input range and does not contain results
         * Status : 200
         * Content-Type: application/json
         * Response Body: Empty list []
    * Invalid input - empty, foo, values not in valid range
         * Status : 400
         * Content-Type: text/plain
         * Response Body: Invalid year parameter


* Name : rating
* Type: double
* Parameter Validations:
    * Valid input range containing results - [0.0, 10.0]
    * Round off more than single decimal to a single decimal. Example: round off 7.253896 to 7.2.
         * Status : 200
         * Content-Type: application/json
         * Response Body: List of movies filtered by rating as [{}, {},..]
    * Valid input range and does not contain results
         * Status : 200
         * Content-Type: application/json
         * Response Body: Empty list []
    * Invalid input - empty, foo, values not in valid range [<0.0 , >10.0]
         * Status : 400
         * Content-Type: text/plain
         * Response Body: Invalid rating parameter


* Name : actorid
* Type: string
* Parameter Validations:
    * Valid input - Wellformed actorid rule - starts with 'nm' followed by 9 digit integer converted to string. Example 'nm9877392'
         * Status : 200
         * Content-Type: application/json
         * Response Body: List of movies filtered by actorid as [{}, {},..]
    * Valid input - actorid conforms wellformed String rules and does not contain results
         * Status : 200
         * Content-Type: application/json
         * Response Body: Empty list []
    * Invalid input - empty, foo, values not conforming to wellformed actorid rule.  Example: 'nm1234', 'ab1234'
         * Status : 400
         * Content-Type: text/plain
         * Response Body: Invalid actorid parameter


* Name : genre
* Type: string
* Parameter Validations:
    * Valid input - genre length >= 3. Example 'War' , 'Horror'
         * Status : 200
         * Content-Type: application/json
         * Response Body: List of movies filtered by actorid as [{}, {},..]
    * Valid input - genre length > 3 characters and does not contain results, example = horro
         * Status : 200
         * Content-Type: application/json
         * Response Body: Empty list []
    * Invalid input - empty, genre length < 3 characters
         * Status : 400
         * Content-Type: text/plain
         * Response Body: Invalid genre parameter


* Name : pagesize
* Type: integer
* Parameter Validations:
    * Valid input range containing results - [1, 1000]
         * Status : 200
         * Content-Type: application/json
         * Response Body: List of movies filtered by pagesize as [{}, {},..]
    * Valid input range not containing results - [1, 1000]
         * Status : 200
         * Content-Type: application/json
         * Response Body: Empty list []
    * Invalid input - empty should picks defaults [1, 1000]
         * Status : 200
         * Content-Type: application/json
         * Response Body: Empty list []
    * Invalid input - 100.23
         * Status : 400
         * Content-Type: text/plain
         * Response Body: Invalid pagesize parameter


* Name : pagenumber
* Type: integer
* Parameter Validations:
    * Valid input range containing results - [1, 10000]
         * Status : 200
         * Content-Type: application/json
         * Response Body: List of movies filtered by pagenumber as [{}, {},..]
    * Valid input range not containing results - [1, 10000]
         * Status : 200
         * Content-Type: application/json
         * Response Body: Empty list []
    * Invalid input - empty values should pick defaults [1, 10000]
         * Status : 200
         * Content-Type: application/json
         * Response Body: Empty list []
    * Invalid input - 100.23
         * Status : 400
         * Content-Type: text/plain
         * Response Body: Invalid pagenumber parameter


## Multiple Error Scenarios (TBD)

* Options for handling multiple errors scenarios:
    * Consider simply adding each error to the errors array.
    * Throw the first error occured.
*   * Consider throwing a 422 Unprocessable Entity.


### API Parameters

* API /api/movies/{movieid}
* Name : movieid
* Type: string
* Parameter Validations:
    * Valid input - Wellformed movieid rule - starts with 'tt' followed by 9 digit integer converted to string. Example 'tt0114746'
         * Status : 200
         * Content-Type: application/json
         * Response Body: Single movie document as JSON
    * Invalid input - empty, movieid conforms wellformed String rules and does not exist, movieid not conforming to wellformed movieid rule example - 'tt1234', 'nm1234'
         * Status : 404
         * Content-Type: text/plain
         * Response Body: movieid not found


## Actors

### API /api/actors

### Query Parameters

* Name : q
* Description : query
* Type: string
* Parameter Validations:
    * Valid input containing results : q length >= 3 characters
         * Status : 200
         * Content-Type: application/json
         * Response Body: List of actors filtered by query as [{}, {},..]
    * Valid input and does not contain results
         * Status : 200
         * Content-Type: application/json
         * Response Body: Empty list []
    * Invalid input : empty , q length < 3 characters
         * Status : 400
         * Content-Type: text/plain
         * Response Body: Invalid query parameter

* Name : pagesize
* Type: integer
* Parameter Validations:
    * Valid input range containing results - [1, 1000]
         * Status : 200
         * Content-Type: application/json
         * Response Body: List of actors filtered by pagesize as [{}, {},..]
    * Valid input range not containing results - [1, 1000]
         * Status : 200
         * Content-Type: application/json
         * Response Body: Empty list []
    * Invalid input - empty should picks defaults [1, 1000]
         * Status : 200
         * Content-Type: application/json
         * Response Body: Empty list []
    * Invalid input - 100.23
         * Status : 400
         * Content-Type: text/plain
         * Response Body: Invalid pagesize parameter


* Name : pagenumber
* Type: integer
* Parameter Validations:
    * Valid input range containing results - [1, 10000]
         * Status : 200
         * Content-Type: application/json
         * Response Body: List of actors filtered by pagenumber as [{}, {},..]
    * Valid input range not containing results - [1, 10000]
         * Status : 200
         * Content-Type: application/json
         * Response Body: Empty list []
    * Invalid input - empty should picks defaults [1, 10000]
         * Status : 200
         * Content-Type: application/json
         * Response Body: Empty list []
    * Invalid input - 100.23
         * Status : 400
         * Content-Type: text/plain
         * Response Body: Invalid pagenumber parameter


### API Parameters

* API /api/actors/{actorid}
* Name : actorid
* Type: string
* Parameter Validations:
    * Valid input - Wellformed actorid rule - starts with 'nm' followed by 9 digit integer converted to string. Example 'nm0000246'
         * Status : 200
         * Content-Type: application/json
         * Response Body: Single movie document as JSON
    * Invalid input - empty, actorid conforms wellformed String rules and does not exist, actorid not conforming to wellformed actorid rule example - 'tt1234', 'nm1234'
         * Status : 404
         * Content-Type: text/plain
         * Response Body: actorid not found


### Common Status code rules followed:

## Searches

* Valid input returns 200 with Arraylist [{},{}...]
* Valid input with no movies/actors returns 200 with empty list []
* Invalid input with no movies/actors returns 400 with Invalid error response

## Single Reads

* Valid single reads like /api/movies/{movieid} , /api/actorid/{actorid} returns 200 with JSON document
* Invalid single reads like /api/movies/tt1234 , /api/actorid/nm1234 returns 404 with No {entity} found
