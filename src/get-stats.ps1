$headers = @{}
$headers.accept = "application/json"
$waitingMessage = "waiting..."
$hostName = "RCM41VQPERAPP41"
# $hostName = "localhost"

while ($true)
{
    try
    {
        $router = Invoke-RestMethod "http://$hostName`:9090/api/MetricsEventTypes/Demo.ADTProcessing.Router" -Headers $headers
        $worker = Invoke-RestMethod "http://$hostName`:9090/api/MetricsEventTypes/Demo.ADTProcessing.Worker" -Headers $headers

        $avgRouterDelay = $router.avgDelayInMilliseconds
        $avgRouterExecution = $router.avgExecutionInMilliseconds
        $routerMessageRate = $router.messagesPerSecond
        $routerCount = [int]$router.count
        $avgDelay = $worker.avgDelayInMilliseconds
        $avgExecution = $worker.avgExecutionInMilliseconds
        $workerMessageRate = $worker.messagesPerSecond
        $workerCount = [int]$worker.count

        if ($avgDelay)
        {    
            #$worker 
            "router::$avgrouterExecution ms (dly=$avgRouterDelay ms, rt=$routerMessageRate/sec, ct={0:n0}) worker::$avgExecution ms (dly=$avgDelay ms, rt=$workerMessageRate/sec, ct={1:n0})" -f $routerCount, $workerCount       
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