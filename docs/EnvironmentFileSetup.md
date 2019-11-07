## Environment File (.env) Setup

To run the solutions, you will need the following `.env` file located in the directory of the project you are launching.

### Router

```
APPINSIGHTS_KEY={some key}
CACHE_GRPC_ENDPOINT={ip:port}
CACHE_GRPC_TIMEOUT={int, unit: milliseconds}
NUM_MESSAGES_EACH_GENERATION={int}
GENERATE_MESSAGES_EVERY={int, unit: milliseconds}
REST_SERVICE_URL={protocol://url:port}
RESTRICT_MESSAGES_AT_BUFFER_SIZE={int, dflt: 0}
MAX_WAIT_TO_ADD_MESSAGES={int, dflt: 30000}
PROTOCOL={REST or GRPC}
```

### Cache

```
PORT={int}
```
