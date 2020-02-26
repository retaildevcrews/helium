# Helium Parameter Validation Documentation

This goals of this document is to define valid/invalid API and Query parameters and provide specifications on resulting behavior of the Helium API.

## Movies

### API /api/movies

### Query Parameters

- Name : q
- Description : query
- Type: string
- Parameter Validation:
    - Valid input : q length >= 3 characters
         - Status : 200
         - Content-Type: application/json
         - Response Body: List of movies filtered by query as [{}, {},..] , empty [] when no results
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
    - Valid input range : [0.0, 10.0]
    - Round off more than single decimal to a single decimal. Example: round off 7.253896 to 7.2.
         - Status : 200
         - Content-Type: application/json
         - Response Body: List of movies filtered by rating as [{}, {},..], empty [] when no results
    - Invalid input - empty, foo, values not in valid range [<0.0 , >10.0]
         - Status : 400
         - Content-Type: text/plain
         - Response Body: Invalid rating parameter


- Name : actorid
- Type: string
- Parameter Validations
    - Valid input : Wellformed actorid rule - starts with 'nm' followed by upto a 9 digit integer converted to string. Example 'nm9877392'
         - Status : 200
         - Content-Type: application/json
         - Response Body: List of movies filtered by actorid as [{}, {},..] , empty [] when no results
    - Invalid input - empty, foo, values not conforming to wellformed actorid rule.  Example: 'nm1234', 'ab1234'
         - Status : 400
         - Content-Type: text/plain
         - Response Body: Invalid actorid parameter


- Name : genre
- Type: string
- Parameter Validations:
    - Valid input - genre length >= 3. Example 'War' , 'Horror'
         - Status : 200
         - Content-Type: application/json
         - Response Body: List of movies filtered by actorid as [{}, {},..] , empty [] when no results
    - Invalid input - empty, genre length < 3 characters
         - Status : 400
         - Content-Type: text/plain
         - Response Body: Invalid genre parameter


- Name : pagesize
- Type: integer
- Parameter Validation:
    - Default value: 100
    - Valid input range : [1, 1000]
         - Status : 200
         - Content-Type: application/json
         - Response Body: List of movies filtered by pagesize as [{}, {},..], empty [] when no results
    - Invalid input - Non integers are invalid. Example : 100.23
         - Status : 400
         - Content-Type: text/plain
         - Response Body: Invalid pagesize parameter


- Name : pagenumber
- Type: integer
- Parameter Validations:
    - Default value: 1
    - Valid input range : [1, 10000]
         - Status : 200
         - Content-Type: application/json
         - Response Body: List of movies filtered by pagenumber as [{}, {},..], empty [] when no results
    - Invalid input - Non integers are invalid. Example : 100.23
         - Status : 400
         - Content-Type: text/plain
         - Response Body: Invalid pagenumber parameter

### API Parameters

- API /api/movies/{movieid}
- Name : movieid
- Type: string
- Parameter Validations:
    - Valid input - Wellformed movieid rule - starts with 'tt' followed by upto a 9 digit integer converted to string. Example 'tt0114746'
         - Status : 200
         - Content-Type: application/json
         - Response Body: Single movie document as JSON
    - Invalid input - empty, movieid does not exist, movieid not conforming to wellformed movieid rule example - 'tt1234', 'nm1234'
         - Status : 404
         - Content-Type: text/plain
         - Response Body: movieid not found

## Actors

### API /api/actors

### Query Parameters

- Query parameters for actors are q, pagesize, pagenumbers.
- Refer to the rules for Query Parameters for Movies.

### API Parameters

- API /api/actors/{actorid}
- Name : actorid
- Type: string
- Parameter Validations:
    - Refer to the "actorid" query parameter in the movies section

## Common Status code rules followed for all APIs :

### Searches

- Valid input returns 200 with Arraylist [{},{}...]
- Valid input with no movies/actors returns 200 with empty list []
- Invalid input with no movies/actors returns 400 with Invalid error response

### Single Reads

- Valid single reads like /api/movies/{movieid} , /api/actorid/{actorid} returns 200 with JSON document
- Invalid single reads like /api/movies/tt1234 , /api/actorid/nm1234 returns 404 with No {entity} found

## Multiple Error Scenarios

- Add each error to the errors array in alphabetical order.
