# Helium Parameter Specs Documentation

## Movies

[Specification for building API and Parameters](https://jsonapi.org/examples/#error-objects)


   API       |   Params  |   Parameters                                                           | Status Code     |  Content-Type      | Response                           |
| ---------  | ----------| ----------------------------------------                               | ------------    |------------------  | ------------
| /api/movie?q= |  q (query)     |   Query parameter is free text              | 200             | application/json   | List of all movies filtered by year as [{}, {}, {}] or empty array if no results are returned as []|
| /api/movie?year= |  year     |   Valid parameters : 1874 - 2020 (Min date - should it be based on oldest movie available in IMDB db?)                     | 200             | application/json   | List of all movies filtered by year [{}, {}, {}]|
| /api/movie?year= |  year     |   Invalid parameters : 0, -1, numeric values outside of range 1874 -2020 , foo   | 400             | text/plain   | Invalid year parameter
| /api/movie?rating= |  rating   |   Valid parameters : 1 - 10 (based on rating scale from IMDB)   | 200             | application/json   | List of all movies filtered by rating [{}, {}, {}]|
| /api/movie?rating= |  rating   |   Invalid parameters : <1 or >10   | 400             | text/plain   | Invalid rating parameter|
| /api/movie?toprated= |  toprated   |  Valid parameters : true    | 200             | application/json   | List of top rated 10 movies [{}, {}, {}]|
| /api/movie?toprated= |  toprated   |  Valid parameters : false   | 200             | application/json   | List of all movies [{}, {}, {}]|
| /api/movie?toprated= |  toprated   |  Invalid parameters : values other than true ,false , foo   | 400             | text/plain   | Invalid toprated parameter
| /api/movie?actorid= |  actorid   |  Valid parameters : actor id from IMDB db   | 200             | application/json   | List of all movies filtered by actorid as [{}, {}, {}]|
| /api/movie?actorid= |  actorid   |  Invalid parameters : foo   | 400             | text/plain   | Invalid actorid parameter
| /api/movie?genre= |  genre   |  Valid parameters : valid genre from genre list of IMDB db   | 200             | application/json   | List of all movies filtered by genre as [{}, {}, {}]|
| /api/movie?genre= |  genre   |  Invalid parameters : foo   | 400             | text/plain   | Invalid genre parameter
| /api/movie?pagesize= |  pagesize   |  Valid parameters : numeric values from 1 - 1000   | 200             | application/json   | List of all movies filtered by pagesize as [{}, {}, {}]|
| /api/moviepagesize= |  pagesize   |  Invalid parameters : numeric values < 1 or > 1000, foo   | 400             | text/plain   | Invalid pagesize parameter
| /api/movie?pagenumber= |  pagenumber   | Valid parameters : numeric values  1 thru total_count/pagesize(hateos?)    | 200             | application/json   | List of all movies filtered by pagenumber as [{}, {}, {}]|
| /api/movie?pagenumber= |  Invalid parameters : pagenumber   | < 1 , foo      | 400             | text/plain   | Invalid pagenumber parameter
| /api/movie?actorid=nm0000246&pagenumber=1&pagesize=100 | actorid, pagenumber, pagesize   | Valid parameters : actor id from IMDB db   | 200             | application/json   | List of all movies filtered by actorid as [{}, {}, {}]|
| /api/movie?actorid=foo&pagenumber=1&pagesize=100 |  actorid, pagenumber, pagesize   | Invalid parameters : actorid      | 400             | text/plain   | Invalid actorid parameter
| api/movies?genre=horror&actorid=nm0000246&pagenumber=1&pagesize=100 |  genre, actorid, pagenumber, pagesize   | Valid parameters : actor id from IMDB db      | 200             | application/json   | List of all movies filtered by actorid as [{}, {}, {}]|
| /api/movie?genre=foo&actorid=foo&pagenumber=1&pagesize=100 |  genre, actorid, pagenumber, pagesize  | Invalid parameters : genre (throw the first invalid parameter)      | 400             | text/plain   | Invalid actorid parameter
| /api/movie/{movieid} |  movieid     |   Valid parameters : valid movieid from IMDB db                     | 200             | application/json   | Single movie object
| /api/movie/{movieid} |  movieid     |   Invalid parameters : foo   | 404             | text/plain   | Movie with {id} not found
