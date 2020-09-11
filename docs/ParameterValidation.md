# Helium Parameter Validation

The following documentation describes the types of Query String and route parameters available in the Helium REST API. The categories are broken down by [Common Query Parameters](##Common-Query-Parameters), the [Movies API](##Movies-API), and the [Actors API](##Actors-API).

## Overview

The following section describes two types of interactions with the API; 'searching' or 'direct reads'. Searching involves the use of Query String parameters which can be specified independent of each other. A direct read involves specifying an `ActorId` or `MovieId` in the route path without Query String parameters.

### Searches (query string parameters)

- Valid input returns `HTTP/200` with array of `Movie` or `Actor` and content-type of `application/json`
- Valid input with no results returns `HTTP/200` with empty array and content-type of `application/json`
- Invalid input returns a `HTTP/400` error response and content-type of `application/problem+json`

### Direct Reads

- Valid single read returns `HTTP/200` with `Movie` or `Actor` and content-type of `application/json`
- Valid single read with no results returns `HTTP/404` and content-type of `application/json`
- Invalid single read returns a `HTTP/400` error response with a `application/problem+json` content type

### Error Handling

The error handling details including the response to parameter or route path validation errors uses a combination of RFC 7807 and the Microsoft REST API guidelines and can be found on the [HttpErrorResponses](HttpErrorResponses.md) page.

## Common Query Parameters

The following API routes use these common parameters:

- /api/movies
- /api/actors

|   Name        |  Description                                                |  Type    |  Valid Input         |  Response Body                                |  Notes                               |
|   ----        |  -----------                                                |  ----    |  -----------         |  -------------                                |  ------                              |
|   q (search)  |  Case insensitive search on 'Movie Title' or 'Actor Name'   |  string  |  between [2, 20]     |  Array of `Movie` or `Actor` or empty array   |  Movie.Title/Actor.Name contains q   |
|   pageSize    |  Limit the number of results                                |  integer |  between [1, 1000]   |  Array of `Movie` or `Actor` or empty array   |  n/a                                 |
|   pageNumber  |  Return a specific page from the results                    |  integer |  between [1, 10000]  |  Array of `Movie` or `Actor` or empty array   |  n/a                                 |

## Movies API

### Movies Query String Parameters

The following API route uses these additional parameters:

- /api/movies

|   Name     |  Description                                |  Type    |  Valid Input                           |  Response Body                     |  Notes                  |
|   ----     |  -----------                                |  ----    |  -----------                           |  -------------                     |  -----                  |
|   year     |  Get movies by year                         |  integer |  between [1874, currentYear + 5]       |  Array of `Movie` or empty array   |  n/a                    |
|   rating   |  Filter by Movie.Rating >= rating           |  double  |  between [0.0, 10.0]                   |  Array of `Movie` or empty array   |  n/a                    |
|   genre    |  Filter by Movie.Genre                      |  string  |  between [3, 20] characters            |  Array of `Movie` or empty array   |  n/a                    |
|   actorId  |  Return a specific page from the results    |  string  |  starts with 'nm' + [5, 9] characters  |  Array of `Movie` or empty array   |  'nm' must be lowercase |

### Movies Direct Read

This applies to the following API route:

- /api/movies/{movieId}

|   Name     |  Description                                |  Type    |  Valid Input                           |  Response Body     |  Notes                  |
|   ----     |  -----------                                |  ----    |  -----------                           |  -------------     |  -----                  |
|   movieId  |  Return a specific page from the results    |  string  |  starts with 'tt' + [5, 9] characters  |  Single `Movie`    |  'tt' must be lowercase |

## Actors API

### Actors Query String Parameters

The following API route uses these parameters:

- /api/actors

> NOTE: [Refer to the Common Query Parameters](##-Common-Query-Parameters)

### Actors Direct Read

This applies to the following API route:

- /api/actors/{actorId}

|   Name     |  Description                                |  Type    |  Valid Input                           |  Response Body    |  Notes                   |
|   ----     |  -----------                                |  ----    |  -----------                           |  -------------    |  -----                   |
|   actorId  |  Return a specific page from the results    |  string  |  starts with 'nm' + [5, 9] characters  |  Single `Actor`   |  'nm' must be lowercase  |
