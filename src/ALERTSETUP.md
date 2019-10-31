# Using Alerts to Monitor your Application

You already have Application Insights set up to get insights on your app's performance, health, and usage.  Below are instructions to set up alerts so you get notified (via email) when the metrics reach a level you want to be aware of.

## Create an Action Group

Each alert can have an action.  In this case, the action we want is to email a list of one or more email addresses when an alert is fired. 

```bash

# create a new action group and add an email address
az monitor action-group create --name $He_Name-email-list --resource-group $He_Name-rg-app --action email {Name} {email address}

# update the group with as many email addresses as required (optional)
az monitor action-group update -n $He_Name-email-list -g $He_Name-rg-app --add-action email {Name} {email address}

```

## Create Alerts

Note: The alerts below assume there is a test server running that is driving traffic to your application.  You will likely want to observe the behavior of your app and adjust the alerts accordingly.

The format for the condition of the alerts is:<br>
* "{avg,min,max,total,count} METRIC {=,!=,>,>=,<,<=} THRESHOLD"<br>

You can also add additional filters (varies by metric) to the condition when applicable to the metric using a where clause:<br> 
* "{avg,min,max,total,count} METRIC {=,!=,>,>=,<,<=} THRESHOLD
where DIMENSION {includes,excludes} VALUE"

```bash

# get the full scope of the app insights instance
export He_App_Insights_Scope=$(az monitor app-insights component show -g $He_Name-rg-app --query [0].id -o tsv)

# create an alert for when the number of server requests per minute exceeds 600
# default severity is 2
az monitor metrics alert create -n $He_Name-Max-Requests -g $He_Name-rg-app \
--description "Requests over 600" \
--scopes $He_App_Insights_Scope \
--condition "count requests/count > 600" \
--window-size 1m \
--evaluation-frequency 1m \
-a $He_Name-email-list \

# create an alert for when the number of server requests per minute drops below 300
az monitor metrics alert create -n $He_Name-Min-Requests -g $He_Name-rg-app \
--description "Requests under 300" \
--scopes $He_App_Insights_Scope \
--condition "count requests/count < 300" \
--window-size 1m \
--evaluation-frequency 1m \
-a $He_Name-email-list

# run az monitor metrics alert create -h to see all available arguments.

```

## Additional Parameters to Customize Alerts

### Other metrics (See complete list [here](https://docs.microsoft.com/en-us/azure/azure-monitor/platform/app-insights-metrics)):
* requests/count (Total Server Requests)
* requests/failed (Total Failed Server Requests)
* requests/duration (Server Response Time)
* performanceCounters/processCpuPercentage (% total CPU used by process)


### Window Sizes (aggregation granularities):
* 1m
* 5m
* 15m
* 30m
* 1h
* 6h
* 12h
* 24h

### Evaluation Frequencies:<br>
Note: The evaluation frequency must be less than or equal to the window size.
* 1m
* 5m
* 15m
* 30m
* 1h