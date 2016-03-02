$headers = @{}
$headers.accept = "application/json"
$waitingMessage = "waiting..."
$hostName = "RCM41VQPERAPP41"

while ($true)
{
    try
    {
        $router = Invoke-RestMethod "http://$hostName`:9090/api/MetricsEventTypes/Demo.ADTProcessing.Router" -Headers $headers
        $worker = Invoke-RestMethod "http://$hostName`:9090/api/MetricsEventTypes/Demo.ADTProcessing.Worker" -Headers $headers

        $avgDelay = $worker.avgDelayInMilliseconds
        $avgExecution = $worker.avgExecutionInMilliseconds

        if ($avgDelay)
        {     
            "$avgDelay - $avgExecution"       
        }
        else
        {
            $waitingMessage
        }
    } catch
    {
        $waitingMessage
    } 

    Start-Sleep -s 1
}