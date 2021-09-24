---
layout: default
title: Backlog
nav_order: 3
---

# Backlog

If you wish some feature to be added please contact us on [Slack channel](http://vektonn.slack.com/).

1. Splitting (several sub-indices within a single process)
    1. Split-key schema
    2. Search API
2. Sharding (several processes make up one index)
    1. Search Facade API
    2. Sharding schemas
        1. Explicit split-key mapping
        2. Split-key hashing
3. Persistent Data Sources
    1. Metadata schema
    2. Kafka
        1. Data Upload API
    3. DataSource abstraction 
        1. SQL Connector
        2. ... (If you have your own datastore, please contact us, we will add a new task to our inbox)
4. Deploy to Kubernetes
    1. Static helm charts (If you use it, please, tell us, cause we don't know, how popular is this case)
    2. Dynamic Index Scheduler
        1. Index Management API
            1. Authorization
        2. Desired Index State Storage
        3. Index Scheduler Daemon
    3. Deploy artifacts publication
        1. Docker-image
        2. Helm-charts
    4. Telemetry
        1. Logs
        2. Metrics
            1. Prometheus 
            2. ... (Tell us what system you use, we will add a new task to our inbox)
5. Integrations with other MLOps products
    1. kubeflow
    2. ... (Tell us what integration you need, and we will add a new task to our inbox)
