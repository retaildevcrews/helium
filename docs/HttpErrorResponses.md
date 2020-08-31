# Helium HTTP/400 Error Response Design

This design guide covers the types of responses and format used when an HTTP/400 error occurs. Aligning with [RFC 7807](https://tools.ietf.org/html/rfc7807), the error responses adhere to this specification with a additional extension members containing a collection of parameter validation errors.

## Scope

The HTTP/400 error responses outlined in this document currently only cover API input parameter validation at this time.

## Base Problem Details Object

The HTTP response contains a "Problem Details Object" which has the following members:

- "**type**" (string): An optional URI which identifies the problem type.
- "**title**" (string): Summary of the problem.
- "**status**" (number): Currently scoped to HTTP/400
- "**detail**" (string): A message specific to this type of problem.
- "**instance**" (string): A URI reference indicating the HTTP route referenced by the API (excluding query string parameters)

Note: Object order is not pre-defined or set and can vary.

## Extension Members

To provide more specific information about the error condition, the RFC permits extension members to be defined with no limit to the schema. As such, an object array called `validationErrors` is used to hold one or more objects with the following members:

- "**code**" (string): The error type specific to the validation issue in the collection
- "**target**" (string): The name of the parameter which did not validate correctly
- "**message**" (string): A descriptive message outlining the constrains of the input

## Error Types

Given the above extension member called `validationErrors`, the `code` property can hold the following values:

1. **NullValue**: Used when the input parameter is null
2. **InvalidValue**: Used when the input parameter is not the correct type or outside the bounds of the required value(s)

## Application Type

In accordance with RFC 7807, the application type, also known as the `Content-Type` property of the HTTP response header, shall be set to: `application/problem+json`

## Parameter Response Messages

|   Parameter    |  Message  |
|     :--:       |    --     |
|   q            |   The parameter 'q' should be between 2 and 20 characters. |
|   actorId      |   The parameter 'actorId' should start with 'nm' and be between 7 and 11 characters in total. |
|   movieId      |   The parameter 'movieId' should start with 'tt' and be between 7 and 11 characters in total. |
|   year         |   The parameter 'year' should be between 1874 and {Current Year + 5} |
|   genre        |   The parameter 'genre' should be between 3 and 20 characters. |
|   rating       |   The parameter 'rating' should be between 0 and 10.0 |
|   pageSize     |   The parameter 'pageSize' should be between 1 and 1000. |
|   pageNumber   |   The parameter 'pageNumber' should be between 1 and 10000 |

Note: The `year` parameter listed above uses a dynamic range for the maximum value which is today's year plus five (i.e. 2025 at current time of writing).

## Examples

The following examples show multiple validation errors be triggered on both the `Movies` and `Actors` APIs.

### Invalid `Movies` API Query Parameter Response

```json

{
    "type": "http://www.example.com/validation-error",
    "title": "Your request parameters did not validate.",
    "status": 400,
    "detail": "One or more invalid parameters were specified.",
    "instance": "/api/movies",
    "validationErrors": [
        {
            "code": "InvalidValue",
            "target": "Year",
            "message": "The parameter 'Year' should be between 1874 and 2025."
        },
                {
            "code": "InvalidValue",
            "target": "Genre",
            "message": "The parameter 'Genre' should be between 3 and 20 characters."
        }
    ]
}

```

### Invalid `Actors` API Query Parameter Response

```json

{
    "type": "http://www.example.com/validation-error",
    "title": "Your request parameters did not validate.",
    "status": 400,
    "detail": "One or more invalid parameters were specified.",
    "instance": "/api/actors",
        "validationErrors": [
        {
            "code": "InvalidValue",
            "target": "Q",
            "message": "The parameter 'q' should be between 2 and 20 characters."
        },
        {
            "code": "InvalidValue",
            "target": "PageSize",
            "message": "The parameter 'PageSize' should be between 1 and 1000."
        }
    ]
}

```

### Invalid `Movies` API Route Path Response

```json

{
    "type": "http://www.example.com/validation-error",
    "title": "Your request parameters did not validate.",
    "status": 400,
    "detail": "One or more invalid parameters were specified.",
    "instance": "/api/movies/tT0133093",
        "validationErrors": [
        {
            "code": "InvalidValue",
            "target": "MovieId",
            "message": "The parameter 'MovieId' should start with 'tt' and be between 7 and 11 characters in total"
        }
    ]
}

```
