#!/bin/bash

echo "Iniciando provisionamento da infraestrutura no LocalStack..."

export AWS_ACCESS_KEY_ID="test"
export AWS_SECRET_ACCESS_KEY="test"
export AWS_DEFAULT_REGION="us-east-1"
export LOCALSTACK_URL="http://localhost:4566"

echo "------------------------------------------------"

echo "1. Criando S3 Bucket..."
aws --endpoint-url=$LOCALSTACK_URL s3 mb s3://aws-lab-platform-bucket

echo "Aplicando política de CORS no S3..."
cat <<EOF > cors.json
{
  "CORSRules": [
    {
      "AllowedHeaders": ["*"],
      "AllowedMethods": ["PUT", "POST", "GET"],
      "AllowedOrigins": ["*"],
      "ExposeHeaders": []
    }
  ]
}
EOF
aws --endpoint-url=$LOCALSTACK_URL s3api put-bucket-cors --bucket aws-lab-platform-bucket --cors-configuration file://cors.json
rm cors.json

echo "------------------------------------------------"

echo "2. Criando SQS Dead-Letter Queue (DLQ)..."

aws --endpoint-url=$LOCALSTACK_URL sqs create-queue --queue-name file-uploaded-dlq

echo "Criando SQS Standard Queue (Fila Principal)..."
aws --endpoint-url=$LOCALSTACK_URL sqs create-queue --queue-name file-uploaded-queue

echo "------------------------------------------------"

echo "3. Criando Tópico SNS..."
aws --endpoint-url=$LOCALSTACK_URL sns create-topic --name file-processed-topic

echo "------------------------------------------------"
echo "✅ Infraestrutura local provisionada com sucesso!"