## Deployment

Deployment leverages kubernetes w helm to execute charts. In addition Terraform is utilized to setup the initial AKS cluster, node pools & ACR.

* Be sure to run:

kubectl create secret generic appinsightskey --from-literal=APPINSIGHTS_KEY=<insert_insights_key_here>

Where you replace the above with the instrumentation key you plan to use.

Environment Variables Include:

GENERATE_MESSAGES_EVERY (int in milliseconds)
NUM_MESSAGES_EACH_GENERATION (int)
CACHE_GRPC_ENDPOINT
APPINSIGHTS_KEY