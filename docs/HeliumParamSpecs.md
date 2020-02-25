# Helium Parameter Specs Documentation

## System Overview

[Reference](https://github.com/retaildevcrews/helium)

## Design Considerations

Helium connects and queries the IMDB database. For more details on data modelling designs refer to [Reference](https://github.com/retaildevcrews/imdb)

## Parameter Validations

This goals of this document is to define valid/invalid API/Query parameters and provide specifications on resulting behavior of the Helium API.

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
         * Response Body: List of movies filtered by query as [{}, {},..] , empty [] when no results
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
         * Response Body: List of movies filtered by year as [{}, {},..], , empty [] when no results
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
         * Response Body: List of movies filtered by rating as [{}, {},..], empty [] when no results
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
         * Response Body: List of movies filtered by actorid as [{}, {},..] , empty [] when no results
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
         * Response Body: List of movies filtered by actorid as [{}, {},..] , empty [] when no results
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
         * Response Body: List of movies filtered by pagesize as [{}, {},..], empty [] when no results
    * Invalid input - empty should picks defaults [1, 1000]
         * Status : 200
         * Content-Type: application/json
         * Response Body: List of movies filtered by default pagesize as [{}, {},..]
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
         * Response Body: List of movies filtered by pagenumber as [{}, {},..], empty [] when no results
    * Invalid input - empty values should pick defaults [1, 10000]
         * Status : 200
         * Content-Type: application/json
         * Response Body: List of movies filtered by default pagenumbers as [{}, {},..]
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


### Common Status code rules followed:

## Searches

* Specification applies for actor entity type.
* Valid input returns 200 with Arraylist [{},{}...]
* Valid input with no movies/actors returns 200 with empty list []
* Invalid input with no movies/actors returns 400 with Invalid error response

## Single Reads

* Valid single reads like /api/movies/{movieid} , /api/actorid/{actorid} returns 200 with JSON document
* Invalid single reads like /api/movies/tt1234 , /api/actorid/nm1234 returns 404 with No {entity} found
