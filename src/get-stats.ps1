$headers = @{}
$headers.accept = "application/json"

Invoke-RestMethod "http://localhost:9090/api/MetricsEventTypes/Demo.ADTProcessing.Router" -Headers $headers
Invoke-RestMethod "http://localhost:9090/api/MetricsEventTypes/Demo.ADTProcessing.Worker" -Headers $headers