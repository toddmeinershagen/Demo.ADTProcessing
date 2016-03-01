$headers = @{}
$headers.accept = "application/json"

Invoke-RestMethod "http://RCM41VQPERAPP41:9090/api/MetricsEventTypes/Demo.ADTProcessing.Router" -Headers $headers
Invoke-RestMethod "http://RCM41VQPERAPP41:9090/api/MetricsEventTypes/Demo.ADTProcessing.Worker" -Headers $headers