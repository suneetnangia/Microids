# Instrumentation Design

For the initial iteration of the hack, we will use App Insights and verify that the instrumentation counts match up with what is expected (i.e. if we pump 1M messages through the MessageRouter does AI indicate that 1M requests to cache service were made?)

## Application Instrumentation

Message Router will report the following pieces of data:

- Count of total calls sent to Cache Service
- Count of calls resulting in errors to Cache Service
- Min/mean/max latency of calls to cache service (investigate whether percentiles are supported)

Cache Service will report the same data (the instrumentation should match up):

- Count of total calls received
- Count of calls returning in errors
- Min/mean/max latency of calls (investigate whether percentiles are supported)

## Platform Monitoring

We will also plug in AKS monitoring to App Insights to see when we hit CPU bottlenecks.

In the initial implementation, we will NOT include docker container metrics due to the overhead incurred by adding an additional process running in the docker container that's collecting data + using bandwidth to send the instrumentation to App Insights. 

VM-level resource monitoring will be emitted + pulled into AI by AKS.

## Concerns to Vet

1. App Insights dropping data -
    Ensure that the counts of calls seen in the instrumentation match the expected calls we attempted to execute
2. Incorrect local aggregation of data -
    App insights aggregates data locally (it doesn't send instrumentation for every request -> AI) Ensure that we don't do anything unexpected, average counts or sum latencies.
