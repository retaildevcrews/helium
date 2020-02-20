# Helium Parameter Specs Documentation

## Movies

   API       |   Params  |   Parameters                                                           | Status Code     |  Content-Type      | Response                           |
| ---------  | ----------| ----------------------------------------                               | ------------    |------------------  | ------------
| /api/movie |  year     |   1874 - 2020 (based on oldest movie available in IMDB db?)                     | 200             | application/json   | List of all movies filtered by year [{}, {}, {}]|
| /api/movie |  year     |   0, -1, numeric values outside of range 1874 -2020 , foo, anystring   | 400             | text/plain   | Invalid year parameter
| /api/movie |  rating   |   1 - 10 (based on rating scale from IMDB)   | 200             | application/json   | List of all movies filtered by rating [{}, {}, {}]|
| /api/movie |  rating   |   <1 or >10   | 400             | text/plain   | Invalid rating parameter|
| /api/movie |  toprated   |  true    | 200             | application/json   | List of top rated 10 movies [{}, {}, {}]|
| /api/movie |  toprated   |  false   | 200             | application/json   | List of all movies [{}, {}, {}]|
| /api/movie |  toprated   |  values other than true ,false , foo   | 400             | text/plain   | Invalid toprated parameter
| /api/movie |  actorid   |  valid actor id from IMDB db   | 200             | application/json   | List of all movies filtered by actorid as [{}, {}, {}]|
| /api/movie |  actorid   |  foo   | 400             | text/plain   | Invalid actorid parameter
| /api/movie |  genre   |  valid genre from genre list of IMDB db   | 200             | application/json   | List of all movies filtered by genre as [{}, {}, {}]|
| /api/movie |  genre   |  foo   | 400             | text/plain   | Invalid genre parameter
| /api/movie |  pagesize   |  numeric values from 1 - 1000   | 200             | application/json   | List of all movies filtered by pagesize as [{}, {}, {}]|
| /api/movie |  pagesize   |  numeric values < 1 or > 1000, foo   | 400             | text/plain   | Invalid pagesize parameter
| /api/movie |  pagenumber   | numeric values  1 thru total_count/pagesize     | 200             | application/json   | List of all movies filtered by pagenumber as [{}, {}, {}]|
| /api/movie |  pagenumber   | < 1 , foo      | 400             | text/plain   | Invalid pagenumber parameter
