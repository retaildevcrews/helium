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
    * Valid input containing results : q length > 3 characters
         * Status : 200
         * Content-Type: application/json
         * Response Body: List of movies filtered by query as [{}, {},..]
    * Valid input with no results : q length > 3 characters
         * Status : 404
         * Content-Type: text/plain
         * Response Body: No movies found for {query}
    * Invalid input - empty , q length < 3 characters
         * Status : 400
         * Content-Type: text/plain
         * Response Body: Invalid query parameter


* Name : year
* Type: Integer
* Parameter Validations:
    * Valid input range with results - [1874 , currentyear+5]
         * Status : 200
         * Content-Type: application/json
         * Response Body: List of movies filtered by year as [{}, {},..]
    * Valid input range with no results
         * Status : 404
         * Content-Type: text/plain
         * Response Body: No movies found for {year}
    * Invalid input - empty, foo, values not in valid range
         * Status : 400
         * Content-Type: text/plain
         * Response Body: Invalid year parameter
