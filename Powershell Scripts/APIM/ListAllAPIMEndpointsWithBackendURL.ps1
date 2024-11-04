#--------------------------------------------------------------------------------#
# Script to print or export(CSV) all endpoints from API management services
#--------------------------------------------------------------------------------#
#Pull list of APIs

#Uncomment the below two lines and login once
#Connect-AzAccount 
#Set-AzContext -Subscription "xxxxx...."

$outputFilePath    = "C:\apiEndpoints.csv"  # Define the path to the output CSV file

$apimName = "<servicename>"
$resourceGroup ="<ResourcegGroup>"

$ApiMgmtContext = New-AzApiManagementContext -ResourceGroupName $resourceGroup -ServiceName $apimName

$APIM= Get-AzApiManagement -ResourceGroupName $resourceGroup -Name $apimName

$Hostname = "https://"+$APIM.ProxyCustomHostnameConfiguration[0].Hostname

#write $Hostname

$Apis = Get-AzApiManagementApi -Context $ApiMgmtContext

#write @apis

$apiDetails = @()

foreach ($api in $Apis) {
    Write "----------------------------------------------------------------------------------"
    write "Api                 : $($api.ApiId)"
    write "APIM URL            : $($Hostname)$("/")$($api.Path)"
    write "Web Service URL     : $($api.ServiceUrl)"

        write "                Getting list of operations"

    $Operations = Get-AzApiManagementOperation -Context $ApiMgmtContext -ApiId $api.ApiId
    
    foreach ($oprtn in $Operations) {
        Write "                ----------------------------------------------            "
        write "    Operation         : $($oprtn.OperationId)"
        write "    Url Template      : $($oprtn.UrlTemplate)"
        write "    APIM URL          : $($Hostname)$("/")$($api.Path)$("/")$($oprtn.UrlTemplate)"
        write "    Backend URL       : $($api.ServiceUrl)$($oprtn.UrlTemplate)"

            $apiDetails += [PSCustomObject]@{
            "API Name"        = $api.Name
            "API ID"          = $api.ApiId
            "API Operation ID" = $oprtn.OperationId
            "APIM URL" = $Hostname + "/" + $api.Path + "/" + $oprtn.UrlTemplate
            "Backend Service URL" = $api.ServiceUrl + "/" + $oprtn.UrlTemplate
            }
        }
    }

$apiDetails | Export-Csv -Path $outputFilePath -NoTypeInformation