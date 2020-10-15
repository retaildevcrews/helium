# Helium HTTP/400 Error Response Design

This design guide covers the types of responses and format used when an HTTP/400 error occurs. Aligning with [RFC 7807](https://tools.ietf.org/html/rfc7807), the error responses adhere to this specification with additional extension members containing a collection of parameter validation errors.

## Scope

The HTTP/400 error responses outlined in this document currently only cover API input parameter validation at this time.

## Problem Details Object

|   Property    |   Type    |   Required    |   Description                                         |
|:-------------:|:---------:|:-------------:|-------------------------------------------------------|
|   type        |   String  |      ✔       | An optional URI which identifies the problem type.    |
|   title       |   String  |      ✔       | Summary of the problem.                               |
|   status      |   Number  |      ✔       | Currently scoped to HTTP/400.                         |
|   detail      |   String  |      ✔       | A message specific to this type of problem.           |
|   instance    |   String  |      ✔       | Route path (may include query string parameters).     |

>Note: Object order in the response body and can vary.

## Extension Members

To provide more specific information about the error condition, the RFC permits extension members to be defined with no limit to the schema. As such, an object array called `validationErrors` is used to hold one or more objects with the following members:

|   Property    |   Type    |   Required    |   Description                                         |
|:-------------:|:---------:|:-------------:|-------------------------------------------------------|
|   code        |   String  |      ✔       | The error type specific to the validation issue in the collection.    |
|   target      |   String  |      ✔       | The name of the parameter which did not validate correctly.                               |
|   message     |   String  |      ✔       | A descriptive message outlining the constrains of the input.                         |

## Error Types

Given the above extension member called `validationErrors`, the `code` property can hold the following values:

1. **NullValue**: Used when the input parameter is null.
2. **InvalidValue**: Used when the input parameter is not the correct type or outside the bounds of the required value(s).

## Application Type

In accordance with [RFC 7807](https://tools.ietf.org/html/rfc7807), the application type, also known as the `Content-Type` property of the HTTP response header, shall be set to: `application/problem+json`

## Parameter Response Messages

|   Parameter    |  Message  |
|     :--:       |    --     |
|   q            |   The parameter 'q' should be between 2 and 20 characters. |
|   actorId      |   The parameter 'actorId' should start with 'nm' and be between 7 and 11 characters in total. |
|   movieId      |   The parameter 'movieId' should start with 'tt' and be between 7 and 11 characters in total. |
|   year         |   The parameter 'year' should be between 1874 and 2025. |
|   genre        |   The parameter 'genre' should be between 3 and 20 characters. |
|   rating       |   The parameter 'rating' should be between 0.0 and 10.0. |
|   pageSize     |   The parameter 'pageSize' should be between 1 and 1000. |
|   pageNumber   |   The parameter 'pageNumber' should be between 1 and 10000. |

## Examples

The following examples show multiple validation errors triggered on both the `Movies` and `Actors` APIs.

### Invalid `Movies` API Query Parameter Response

```json

{
    "type": "https://github.com/retaildevcrews/helium/blob/main/docs/ParameterValidation.md#movies",
    "title": "Parameter validation error",
    "status": 400,
    "detail": "One or more invalid parameters were specified.",
    "instance": "/api/movies?year=1800&genre=zz",
    "validationErrors": [
        {
            "code": "InvalidValue",
            "target": "year",
            "message": "The parameter 'year' should be between 1874 and 2025."
        },
                {
            "code": "InvalidValue",
            "target": "genre",
            "message": "The parameter 'genre' should be between 3 and 20 characters."
        }
    ]
}

```

### Invalid `Actors` API Query Parameter Response

```json

{
    "type": "https://github.com/retaildevcrews/helium/blob/main/docs/ParameterValidation.md#actors",
    "title": "Parameter validation error",
    "status": 400,
    "detail": "One or more invalid parameters were specified.",
    "instance": "/api/actors?q=a&pageSize=99999",
    "validationErrors": [
        {
            "code": "InvalidValue",
            "target": "q",
            "message": "The parameter 'q' should be between 2 and 20 characters."
        },
        {
            "code": "InvalidValue",
            "target": "pageSize",
            "message": "The parameter 'pageSize' should be between 1 and 1000."
        }
    ]
}

```

### Invalid `Movies` API Direct Read Response

```json

{
    "type": "https://github.com/retaildevcrews/helium/blob/main/docs/ParameterValidation.md#direct-read",
    "title": "Parameter validation error",
    "status": 400,
    "detail": "One or more invalid parameters were specified.",
    "instance": "/api/movies/tT0133093",
    "validationErrors": [
        {
            "code": "InvalidValue",
            "target": "movieId",
            "message": "The parameter 'movieId' should start with 'tt' and be between 7 and 11 characters in total."
        }
    ]
}

```
