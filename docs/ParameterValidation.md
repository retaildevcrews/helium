# Helium Parameter Validation

Define valid Query String and URL parameters for the Helium API

## StatusCode

### Searches

- Valid input returns 200 with array of `Movie` or `Actor`
- Valid input with no results returns 200 with empty array
- Invalid input returns 400 text/plain with Invalid parameter error

### Direct Reads

- Valid single read returns 200 with `Movie` or `Actor`
- Valid single read with no results returns 404
- Invalid single read returns 400 with Invalid movieId | actorId error

### Error Handling

- Parameter validation fails on the first error
- Additional query string parameters are ignored
- Additional URL parameters result in 404 Not Found
  - example: /api/movies/tt12345/foo
- Specifying multiple instances of a query string is an error and results are unpredictable
  - Results are idiomatic to the language / framework used
    - i.e. /api/movies?year=1998&year=1999 will return 400 on some frameworks
    - while /api/movies?genre=action&genre=comedy will use the first value

### Error Response Format

The error response to parameter or route path validation errors uses a combination of RFC 7807 and the Microsoft REST API guidelines and can be found on the [HttpErrorResponses](HttpErrorResponses.md) page.

## Common Parameters

> /api/movies
>
> /api/actors

### Query String Parameters

- Name: q (search)
- Description: case insensitive contains search on Movie.Title or Actor.Name
- Type: string
- Parameter Validation:
  - Valid input: length [2, 20]
    - Status: 200
    - Content-Type: application/json
    - Filter: Movie.Title contains q
      - Actor.Name contains q
    - Response Body: array of `Movie` or `Actor` filtered by search query
      - Empty array when no results
  - Invalid input: length [2, 20]
    - Status: 400
    - Content-Type: application/problem+json
    - Response Body: [HttpErrorResponses](HttpErrorResponses.md)

- Name: pageSize
- Type: integer
- Description: limit the number of documents returned
- Parameter Validation:
  - Default value: 100
  - Valid input range: [1, 1000]
    - Status: 200
    - Content-Type: application/json
    - Response Body: array of `Movie` or `Actor`
      - Empty array when no results
  - Invalid input: non-integer or out of range
    - Status: 400
    - Content-Type: application/problem+json
    - Response Body: [HttpErrorResponses](HttpErrorResponses.md)

- Name: pageNumber
- Type: integer
- Description: 1 based page number
- Parameter Validation:
  - Default value: 1
  - Valid input range: [1, 10000]
    - Status: 200
    - Content-Type: application/json
    - Response Body: array of `Movie` or `Actor`
      - Empty array when no results
  - Invalid input: non-integer or out of range
    - Status: 400
    - Content-Type: application/problem+json
    - Response Body: [HttpErrorResponses](HttpErrorResponses.md)

## Movies

### Additional Query String Parameters

> /api/movies

- Name: year
- Type: integer
- Description: filter by year
- Parameter Validation:
  - Valid input range: [1874, currentYear + 5]
    - Status: 200
    - Content-Type: application/json
    - Filter: Movie.Year == year
    - Response Body: array of `Movie`
      - Empty array when no results
  - Invalid input: input that does not parse or is out of range
    - Status: 400
    - Content-Type: application/problem+json
    - Response Body: [HttpErrorResponses](HttpErrorResponses.md)

- Name: rating
- Type: double
- Description: filter by Movie.Rating >= rating
- Parameter Validation:
  - Valid input range: [0.0, 10.0]
    - Status: 200
    - Content-Type: application/json
    - Filter: Movie.Rating >= rating
    - Response Body: array of `Movie`
  - Invalid input: does not parse or out of range
    - Status: 400
    - Content-Type: application/problem+json
    - Response Body: [HttpErrorResponses](HttpErrorResponses.md)

- Name: actorId
- Type: string
- Description: filter by actorId in Movie.Roles
- Parameter Validation
  - Valid input: starts with 'nm' (case sensitive)
    - followed by 5-9 digits
      - parses to int > 0
    - Status: 200
    - Content-Type: application/json
    - Filter: Movie.Roles contains actorId
    - Response Body: array of `Movie`
      - Empty array when no results
  - Invalid input:
    - Status: 400
    - Content-Type: application/problem+json
    - Response Body: [HttpErrorResponses](HttpErrorResponses.md)

- Name: genre
- Type: string
- Description: filter by genre in Movie.Genres
- Parameter Validation:
  - Valid input: length [3, 20]
    - Status: 200
    - Content-Type: application/json
    - Filter: Movie.Genres contains genre
    - Response Body: array of `Movie`
      - Empty array when no results
  - Invalid input: length [3, 20]
    - Status: 400
    - Content-Type: application/problem+json
    - Response Body: [HttpErrorResponses](HttpErrorResponses.md)

### Direct Read

> /api/movies/{movieId}

- Name: movieId
- Type: string
- Description: `Movie` by movieId
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
    - Content-Type: application/problem+json
    - Response Body: [HttpErrorResponses](HttpErrorResponses.md)

## Actors

### Query String Parameters

> /api/actors

- [Refer to the Common Query String Parameters](##-Common-Parameters)

### Direct Read

> /api/actors/{actorId}

- Name: actorId
- Type: string
- Description: `Actor` by actorId
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
    - Content-Type: application/problem+json
    - Response Body: [HttpErrorResponses](HttpErrorResponses.md)
