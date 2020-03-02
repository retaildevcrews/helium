# Helium Parameter Validation

Define valid Query String and URL parameters for the Helium API

## Common StatusCode rules

### Searches

- Valid input returns 200 with Array of `Movie` or `Actor`
- Valid input with no results returns 200 with empty array
- Invalid input returns 400 text/plain with Invalid parameter error

### Single Reads

- Valid single read returns 200 with `Movie` or `Actor`
- Valid single read with no results returns 404
- Invalid single read returns 400 with Invalid movieId | actorId message

### Error Handling

- Parameter validation fails on the first error

## Common Parameters

> /api/movies
>
> /api/actors

### Common Query String Parameters

- Name: q (search)
- Description: This is a case insensitive "contains" search string on Movie.Title or Actor.Name
- Type: string
- Parameter Validation:
  - Valid input: length [2, 20]
    - Status: 200
    - Content-Type: application/json
    - Filter: Movie.Title contains q
      - Actor.Name contains q
    - Response Body: Array of `Movie` or `Actor` filtered by search query
      - Empty array when no results
  - Invalid input: length [2, 20]
    - Status: 400
    - Content-Type: text/plain
    - Response Body: Invalid q parameter

- Name: pageSize
- Type: integer
- Description: limit the number of documents returned
- Parameter Validation:
  - Default value: 100
  - Valid input range: [1, 1000]
    - Status: 200
    - Content-Type: application/json
    - Response Body: Array of `Movie` or `Actor`
      - Empty array when no results
  - Invalid input: non-integer or out of range
    - Status: 400
    - Content-Type: text/plain
    - Response Body: Invalid pageSize parameter

- Name: pageNumber
- Type: integer
- Description: 1 based page number index
- Parameter Validation:
  - Default value: 1
  - Valid input range: [1, 10000]
    - Status: 200
    - Content-Type: application/json
    - Response Body: Array of `Movie` or `Actor`
      - Empty array when no results
  - Invalid input: non-integer or out of range
    - Status: 400
    - Content-Type: text/plain
    - Response Body: Invalid pageSize parameter

### Additional Movie Query String Parameters

> /api/movies

- Name: year
- Type: integer
- Parameter Validation:
  - Valid input range: [1874, currentYear + 5]
    - Status: 200
    - Content-Type: application/json
    - Filter: Movie.Year == year
    - Response Body: Array of `Movie`
      - Empty array when no results
  - Invalid input: input that does not parse or is out of range
    - Status: 400
    - Content-Type: text/plain
    - Response Body: Invalid year parameter

- Name: rating
- Type: double
- Parameter Validation:
  - Valid input range: [0.0, 10.0]
    - Status: 200
    - Content-Type: application/json
    - Filter: Movie.Rating >= rating
    - Response Body: Array of `Movie`
  - Invalid input: does not parse or out of range
    - Status: 400
    - Content-Type: text/plain
    - Response Body: Invalid rating parameter

- Name: actorId
- Type: string
- Parameter Validation
  - Valid input: starts with 'nm' (case sensitive)
    - followed by 5-9 digits
      - parses to int > 0
    - Status: 200
    - Content-Type: application/json
    - Filter: Movie.Roles contains actorId
    - Response Body: Array of `Movie`
      - Empty array when no results
  - Invalid input:
    - Status: 400
    - Content-Type: text/plain
    - Response Body: Invalid actorId parameter

- Name: genre
- Type: string
- Parameter Validation:
  - Valid input: length [3, 20]
    - Status: 200
    - Content-Type: application/json
    - Filter: Movie.Genres contains genre
    - Response Body: Array of `Movie`
      - Empty array when no results
  - Invalid input: length [3, 20]
    - Status: 400
    - Content-Type: text/plain
    - Response Body: Invalid genre parameter

### Movies Direct Read

> /api/movies/{movieId}

- Name: movieId
- Type: string
- Parameter Validation:
  - Valid input: starts with 'tt' (case sensitive)
    - followed by 5-9 digits
      - parses to int > 0
    - Status: 200
    - Content-Type: application/json
    - Filter: Movie.MovieId == movieId
    - Response Body: Single `Movie`
  - Valid input: movieId does not exist
    - Status: 404
    - Response Body: none
  - Invalid input:
    - Status: 400
    - Content-Type: text/plain
    - Response Body: Invalid movieId parameter

## Actors

### Query String Parameters

> /api/actors

- [Refer to the Common Query String Parameters](##-Common-Parameters)

### Actors Direct Read Parameter

> /api/actors/{actorId}

- Name: actorId
- Type: string
- Parameter Validation:
  - Valid input: starts with 'nm' (case sensitive)
    - followed by 5-9 digits
      - parses to int > 0
    - Status: 200
    - Content-Type: application/json
    - Filter: Actor.ActorId == actorId
    - Response Body: Single `Actor`
  - Valid input: actorId does not exist
    - Status: 404
    - Response Body: none
  - Invalid input:
    - Status: 400
    - Content-Type: text/plain
    - Response Body: Invalid actorId parameter
