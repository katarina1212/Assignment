1. Open soluton in VS Studio
2. Build solution
3. Run API project
4. Open Postman or other software for API testing
5. URL: http://localhost:21625/api/Exchange/GetHistory

Body:

{
    "dates":["2018-02-01", "2018-02-15", "2018-03-01", "2020-02-03"],
    "baseCurrency":"SEK",
    "targetCurrency":"NOK"
}


6. For this input, response should be:

{
    "maximumRate": "Maximum rate 0.979845 on 2018-02-15",
    "minimumRate": "Minimum rate 0.952702 on 2018-03-01",
    "averageRate": "Average rate 0.96590925"
}
