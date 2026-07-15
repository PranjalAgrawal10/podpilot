# Google Cloud Run

Deploy PodPilot API and web as Cloud Run services. Use Memorystore for Redis and Cloud SQL for MySQL.

## Prerequisites

- `gcloud` CLI
- Artifact Registry / GCR images
- Cloud SQL MySQL instance
- Memorystore Redis (or compatible)

## Secrets

Create Secret Manager secrets and grant the Cloud Run runtime SA access:

```bash
echo -n 'Server=...;Database=podpilot;User=...;Password=...;' | \
  gcloud secrets create mysql-connection --data-file=-

echo -n 'LONG_RANDOM_JWT_SECRET' | \
  gcloud secrets create jwt-secret --data-file=-
```

Wire them as env vars `ConnectionStrings__DefaultConnection` and `Jwt__Secret`.

## Deploy from YAML

```bash
# Edit PROJECT_ID and REDIS_HOST in cloudrun.yaml first.
gcloud run services replace deploy/gcp/cloudrun.yaml --region=us-central1
```

Or imperative:

```bash
gcloud run deploy podpilot-api \
  --image=gcr.io/PROJECT_ID/podpilot-api:latest \
  --region=us-central1 \
  --allow-unauthenticated \
  --port=8080 \
  --set-env-vars='ASPNETCORE_URLS=http://+:8080,ConnectionStrings__Redis=10.x.x.x:6379' \
  --set-secrets='ConnectionStrings__DefaultConnection=mysql-connection:latest,Jwt__Secret=jwt-secret:latest'
```

Set CORS origins to your Cloud Run web URL after the web service is live.
