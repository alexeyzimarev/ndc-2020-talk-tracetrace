# Trace, Trace!

Code for the NDC Oslo 2020 Talk _OpenMetrics, OpenTracing, OpenTelemetry - are we there yet?_

## What does the code do?

There are two executable projects:
 - `TraceTrace`
 - `TraceTrace.Remote`
 
 Both applications are:
 - instrumented with Prometheus metrics (OpenMetrics)
 - expose the metrics via `/metrics` HTTP endpoint
 - instrumented with OpenTracing
 - send traced to Jaeger
 
 The `TraceTrace` project has an API that you can call using the embedded Swagger UI,
 it's available at http://localhost:5000/swagger/index.html when you run the application.
 
 The `TraceTrace.Remote` service consumes messages from the `TraceTarce` service.
 
 ## How to run it
 
 First, run the infrastructure using Docker Compose:
 
```
docker-compose up
```

The `docker-compose.yml` file is in the repo root.

Then, run both services and make some HTTP calls using `GET`, `POST` and `PUT` endpoints.

Check the metrics at http://localhost:5000/metrics and http://localhost:5001/metrics.

The `GET` call does everything in memory so it only produces HTTP metrics.

The `POST` call writes to MongoDB, so you should get metrics and traces for HTTP and Mongo.

Finally, the `PUT` call sends a message to `TraceTrace.Remote` via RabbitMQ using MassTransit.
So, you get metrics and traces for both services. Eventually, Jaeger will correlate traces and build a
distributed trace.

Check out traces in Jaeger by going to http://localhost:16686/ 