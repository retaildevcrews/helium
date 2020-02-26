# Helium Parameter Validation Documentation

This goals of this document is to define valid/invalid API and Query parameters and provide specifications on resulting behavior of the Helium API.

## Common Status code rules followed for all APIs :

### Searches

- Valid input returns 200 with Arraylist [{},{}...]
- Valid input with no results returns 200 with empty list []
- Invalid input with no results returns 400 with Invalid error response

### Single Reads

- Valid single reads returns 200 with JSON document
- Valid single reads with no results returns 400 with No {entity} found error response
- Invalid single reads like /api/movies/tt1234 , /api/actorid/nm1234 returns 400 with No {entity} found

## Multiple Error Scenarios

- Add each error to the errors array in alphabetical order.

## Movies

### API /api/movies

### Movies Query Parameters

- Name : q (search)
- Description : This a search query string on the title field.
- Type: string
- Parameter Validation:
    - Valid input : q length >= 3 characters
         - Status : 200
         - Content-Type: application/json
         - Response Body: List of movies filtered by search query on title field as [{}, {},..] , empty [] when no results
    - Invalid input : empty , q length < 3 characters
         - Status : 400
         - Content-Type: text/plain
         - Response Body: Invalid query parameter


- Name : year
- Type: integer
- Parameter Validation:
    - Valid input range : [1874 , currentyear+5]
         - Status : 200
         - Content-Type: application/json
         - Response Body: List of movies filtered by year as [{}, {},..], , empty [] when no results
    - Invalid input - empty, foo, values not in valid range
         - Status : 400
         - Content-Type: text/plain
         - Response Body: Invalid year parameter


- Name : rating
- Type: double
- Parameter Validation:
    - Valid input range : [>=0.0, <=10.0]
    - Round off more than single decimal to a single decimal. Example: round off 7.253896 to 7.2.
         - Status : 200
         - Content-Type: application/json
         - Response Body: List of movies filtered by rating as [{}, {},..], empty [] when no results
    - Invalid input - empty, foo, values not in valid range [<0.0 , >10.0]
         - Status : 400
         - Content-Type: text/plain
         - Response Body: Invalid rating parameter


- Name : actorId
- Type: string
- Parameter Validation
    - Valid input : well formed actorId rule - starts with 'nm' followed by upto a 9 digit integer converted to string. Example 'nm9877392'
         - Status : 200
         - Content-Type: application/json
         - Response Body: List of movies filtered by actorid as [{}, {},..] , empty [] when no results
    - Invalid input - empty, foo, values not conforming to wellformed actorid rule.  Example: 'nm1234', 'ab1234'
         - Status : 400
         - Content-Type: text/plain
         - Response Body: Invalid actorid parameter


- Name : genre
- Type: string
- Parameter Validation:
    - Valid input - genre length [>= 3, <=20>. Example 'War' , 'Documentary'
         - Status : 200
         - Content-Type: application/json
         - Response Body: List of movies filtered by actorid as [{}, {},..] , empty [] when no results
    - Invalid input - empty, genre length < 3 or >20 characters
         - Status : 400
         - Content-Type: text/plain
         - Response Body: Invalid genre parameter


- Name : pageSize
- Type: integer
- Description : pageSize limits the number of results to a (max - pageNumber skips (n-1) * pageSize)
- Parameter Validation:
    - Default value: 100
    - Valid input range : [1, 1000]
         - Status : 200
         - Content-Type: application/json
         - Response Body: List of movies limited to pagesize as [{}, {},..], empty [] when no results
    - Invalid input - Non integers are invalid. Example : 100.23
         - Status : 400
         - Content-Type: text/plain
         - Response Body: Invalid pagesize parameter


- Name : pageNumber
- Type: integer
- Parameter Validation:
    - Default value: 1
    - Valid input range : [1, 10000]
         - Status : 200
         - Content-Type: application/json
         - Response Body: List of movies limited to pageNumber as [{}, {},..], empty [] when no results
    - Invalid input - Non integers are invalid. Example : 100.23
         - Status : 400
         - Content-Type: text/plain
         - Response Body: Invalid pageNumber parameter

### API Parameters

- API /api/movies/{movieId}
- Name : movieId
- Type: string
- Parameter Validation:
    - Valid input - well formed movieId rule - starts with 'tt' followed by upto a 9 digit integer converted to string. Example 'tt0114746'
         - Status : 200
         - Content-Type: application/json
         - Response Body: Single movie document as JSON
    - Invalid input : empty, movieId not conforming to wellformed movieId rule example - 'tt1234', 'nm1234'
         - Status : 400
         - Content-Type: text/plain
         - Response Body: Invalid movieId parameter
    - Invalid input : movieId does not exist
         - Status : 404
         - Content-Type: text/plain
         - Response Body: movieId not found

## Actors

### API /api/actors

### Actors Query Parameters

- Query parameters for actors are q, pagesize, pagenumbers
- [Refer to the rules for Query Parameters for Movies](###movies-query-parameters)

### API Parameters

- API /api/actors/{actorId}
- Name : actorId
- Type: string
- Parameter Validation:
    - Valid input - well formed actorId rule - starts with 'nm' followed by upto a 9 digit integer converted to string. Example 'nm9877392'
         - Status : 200
         - Content-Type: application/json
         - Response Body: Single actor document as JSON
    - Invalid input : empty, actorId not conforming to wellformed movieid rule example - 'nm1234'
         - Status : 400
         - Content-Type: text/plain
         - Response Body: Invalid actorId parameter
    - Invalid input : actorId does not exist
         - Status : 404
         - Content-Type: text/plain
         - Response Body: actorId not found
