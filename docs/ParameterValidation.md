# Helium Parameter Validation Documentation

The goal of this document is to define valid/invalid APIs and Query parameters and provide specifications on resulting behavior of Helium API.

## Common Status code rules followed for all APIs

### Searches

- Valid input returns 200 with JSON Array [{},{}...]
- Valid input with no results returns 200 with empty array
- Invalid input returns 400 text/plain with Invalid error response

### Single Reads

- Valid single reads returns 200 with JSON document
- Valid single reads with no results returns 404 with No {entity} found error response
- Invalid single reads like /api/movies/tt1234 , /api/actors/nm1234 returns 400 with Invalid error response

### Multiple Error Scenarios

- Fail on first error

## Movies

### API /api/movies

### Movies Query Parameters


- Name: q (search)
- Description: This is a case insensitive search query (q) string on movie title or actor name field
- Type: string
- Parameter Validation:
  - Valid input: query length >= 3 non-whitespace characters
    - Example: 'ring', 'Ring'
    - Status: 200
    - Content-Type: application/json
    - Response Body: JSON Array of movies filtered by search query on title or actor name field as [{},{},..], empty array when no results
  - Invalid input: query length < 3 characters
    - Status: 400
    - Content-Type: text/plain
    - Response Body: Invalid query parameter

- Name: year
- Type: integer
- Parameter Validation:
  - Valid input range: [1874,currentYear+5]
    - Status: 200
    - Content-Type: application/json
    - Response Body: JSON Array of movies filtered by year as [{},{},..], empty array when no results
  - Invalid input: input that does not parse or is out of range
    - Status: 400
    - Content-Type: text/plain
    - Response Body: Invalid year parameter

- Name: rating
- Type: double
- Parameter Validation:
  - Valid input range: [0.0,10.0]
  - Round off more than single decimal to a single decimal based on round function for the language
    - Example: round off 7.253896 to 7.3
    - Status: 200
    - Content-Type: application/json
    - Response Body: JSON Array of movies filtered by rating as [{},{},..], empty array when no results, where movie.rating >= rating
  - Invalid input: input that does not parse or is out of range
    - Status: 400
    - Content-Type: text/plain
    - Response Body: Invalid rating parameter

- Name: actorId
- Type: string
- Parameter Validation
  - Valid input : well-formed actorId rule - starts with lower case 'nm' followed by 5-9 digits, parsed as an int. This is case sensitive input.
    - Example: 'nm1265067'
    - Status : 200
    - Content-Type: application/json
    - Response Body: JSON Array of movies filtered by actorid as [{}, {},..], empty array when no results, where actorId was in movie
  - Invalid input: values not conforming to well-formed actorid rule  
    - Example: 'nm1234', 'ab1234', 'NM1265067'
    - Status : 400
    - Content-Type: text/plain
    - Response Body: Invalid actorId parameter

- Name: genre
- Type: string
- Parameter Validation:
  - Valid input: case insensitive genre,length [3,20]
    - Example: 'War', 'war', 'Documentary', 'documentary'
    - Status: 200
    - Content-Type: application/json
    - Response Body: JSON Array of movies filtered by genre as [{},{},..], empty array when no results, where movie.genres contains genre
  - Invalid input: genre length < 3 or >20 characters
    - Status: 400
    - Content-Type: text/plain
    - Response Body: Invalid genre parameter

- Name: pageSize
- Type: integer
- Description: pageSize limits the number of results returned from the server to a specified number of results
- Parameter Validation:
  - Default value: 100
  - Valid input range: [1,1000]
    - Status: 200
    - Content-Type: application/json
    - Response Body: JSON Array of movies limited to pageSize as [{},{},..], empty array when no results
  - Invalid input: input that does not parse or is out of range
    - Example: 100.23
    - Status: 400
    - Content-Type: text/plain
    - Response Body: Invalid pageSize parameter

- Name: pageNumber
- Type: integer
- Description: pageNumber sets the first position to return from the results of the query
- Parameter Validation:
  - Default value: 1
  - Valid input range: [1,10000]
    - Status: 200
    - Content-Type: application/json
    - Response Body: JSON Array of movies based on pageNumber as [{},{},..], empty array when no results
  - Invalid input: input that does not parse or is out of range
    - Example: 100.23
    - Status: 400
    - Content-Type: text/plain
    - Response Body: Invalid pageNumber parameter

### Movies API Parameters

- API /api/movies/{movieId}
- Name: movieId
- Type: string
- Parameter Validation:
  - Valid input: well-formed movieId rule - starts with lower case 'tt' followed by 5-9 digits, parsed as an int. This is case sensitive input
    - Example 'tt0114746'
    - Status: 200
    - Content-Type: application/json
    - Response Body: Single movie document as JSON
  - Valid input: movieId does not exist
    - Example: 'tt123456'
    - Status: 404
    - Content-Type: text/plain
    - Response Body: movie not found
  - Invalid input: movieId not conforming to well-formed movieId rule
    - Example:'tt1234', 'nm1234', 'TT123456', 'nm1265067'
    - Status: 400
    - Content-Type: text/plain
    - Response Body: Invalid movieId parameter

## Actors

### API /api/actors

### Actors Query Parameters

- Query parameters for actors are q, pageSize, pageNumber
- [Refer to the rules for Query Parameters for Movies](###movies-query-parameters)

### Actors API Parameters

- API /api/actors/{actorId}
- Name: actorId
- Type: string
- Parameter Validation:
  - Valid input: well-formed actorId rule - starts with lower case 'nm' followed by 5-9 digits, parsed as an int
    - Example: 'nm1265067'
    - Status: 200
    - Content-Type: application/json
    - Response Body: Single actor document as JSON
  - Valid input: actorId does not exist
    - Example: 'nm123456'
    - Status: 404
    - Content-Type: text/plain
    - Response Body: actor not found
  - Invalid input: actorId not conforming to well-formed actorId rule
    - Example: 'nm1234', 'tt0114746', 'NM123456'
    - Status: 400
    - Content-Type: text/plain
    - Response Body: Invalid actorId parameter
