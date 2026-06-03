echo "Configurando Observabilidade no Amazon CloudWatch..."

aws logs create-log-group --log-group-name /ecs/auth-service
aws logs create-log-group --log-group-name /ecs/upload-service
aws logs create-log-group --log-group-name /aws/lambda/processing-lambda
aws logs create-log-group --log-group-name /aws/lambda/notifications-lambda

aws logs put-metric-filter \
  --log-group-name /aws/lambda/processing-lambda \
  --filter-name ErrorFilter \
  --filter-pattern "ERROR" \
  --metric-transformations metricName=ProcessingErrors,metricNamespace=AWSLabPlatform,metricValue=1

aws cloudwatch put-metric-alarm \
  --alarm-name HighProcessingErrorRate \
  --alarm-description "Alarme de alta taxa de erro no processamento de arquivos" \
  --metric-name ProcessingErrors \
  --namespace AWSLabPlatform \
  --statistic Sum \
  --period 60 \
  --threshold 5 \
  --comparison-operator GreaterThanOrEqualToThreshold \
  --evaluation-periods 1 \
  --alarm-actions arn:aws:sns:us-east-1:123456789012:file-processed-topic

echo "Observabilidade provisionada com sucesso!"