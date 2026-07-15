# AWS ECS / Fargate

Sample Fargate task definition for PodPilot API + web.

## Prerequisites

- ECS cluster (Fargate)
- VPC with private subnets + ALB (HTTP/HTTPS)
- ElastiCache Redis (or self-managed) — set `ConnectionStrings__Redis`
- RDS MySQL / Aurora MySQL — store connection string in Secrets Manager
- ECR or GHCR images for API and web

## Secrets

Store in AWS Secrets Manager / SSM Parameter Store and reference from the task definition:

| Secret | Env var |
|--------|---------|
| MySQL connection string | `ConnectionStrings__DefaultConnection` |
| JWT signing key | `Jwt__Secret` |

Redis is usually a non-secret hostname (`ConnectionStrings__Redis=mycache:6379`).

## Register & run

```bash
# Replace ACCOUNT_ID, REGION, and secret ARNs in ecs-task-definition.json first.
aws ecs register-task-definition --cli-input-json file://deploy/aws/ecs-task-definition.json

aws ecs create-service \
  --cluster podpilot \
  --service-name podpilot \
  --task-definition podpilot \
  --desired-count 1 \
  --launch-type FARGATE \
  --network-configuration "awsvpcConfiguration={subnets=[subnet-xxx],securityGroups=[sg-xxx],assignPublicIp=DISABLED}" \
  --load-balancers "targetGroupArn=arn:aws:elasticloadbalancing:...,containerName=podpilot-api,containerPort=8080"
```

Expose web on a separate target group / listener rule, or run web as its own service.
