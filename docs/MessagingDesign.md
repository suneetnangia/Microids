## Message choreography design

For REST, just need the API endpoint (e.g., /metadata/{id:string}). For gRPC, we just are returning a int32 metadata structure;

To get started - begin with the following tests:
* HTTP, single calls
    * `GET: /metadata/?id={id}`
* gRPC single calls

Following these results, we'll measure and evaluate. As needed, the 3rd step would be to test batching perf:
* HTTP, passing arrays
    * `GET: /metadata/?id[]=id1&id[]=id2`
    * HTTP has finite URL length (2048 chars, UTF-8 encoded)
* gRPC - leveraging streams.