<div align="center">

<img src="https://img.shields.io/badge/.NET-10.0-512BD4?style=for-the-badge&logo=dotnet&logoColor=white"/>
<img src="https://img.shields.io/badge/AWS-Serverless-FF9900?style=for-the-badge&logo=amazonaws&logoColor=white"/>
<img src="https://img.shields.io/badge/Docker-Compose-2496ED?style=for-the-badge&logo=docker&logoColor=white"/>
<img src="https://img.shields.io/badge/LocalStack-Enabled-4CAF50?style=for-the-badge"/>
<img src="https://img.shields.io/badge/CI%2FCD-GitHub%20Actions-2088FF?style=for-the-badge&logo=githubactions&logoColor=white"/>

# 🚀 AWS Lab Platform
### Arquitetura de Microsserviços Multimodelo — .NET 10 & AWS

> Plataforma distribuída de alta escalabilidade para processamento assíncrono e orientado a eventos de arquivos, construída sob as premissas **Serverless-First** e **Container-Native** com o ecossistema moderno do **.NET 10**.

</div>

---

## 📋 Índice

- [Visão Geral](#-visão-geral)
- [Arquitetura](#️-arquitetura)
- [Microsserviços](#-microsserviços)
- [Stack Tecnológica](#️-stack-tecnológica)
- [Configuração e Execução Local](#-configuração-e-execução-local)
- [CI/CD — GitHub Actions](#️-cicd--github-actions)
- [Estrutura do Repositório](#-estrutura-do-repositório)
- [Contribuindo](#-contribuindo)

---

## 🎯 Visão Geral

Este laboratório demonstra a aplicação prática de padrões arquiteturais avançados combinados com a orquestração de serviços gerenciados da AWS — **sem a necessidade de gerenciamento de servidores físicos**.

O sistema é composto por **4 microsserviços independentes e desacoplados**, cada um utilizando o modelo de computação AWS mais adequado ao seu caso de uso específico: desde APIs Containerizadas no ECS Fargate até Funções Reativas no AWS Lambda.

**Principais características:**
- ✅ Processamento de arquivos 100% assíncrono e orientado a eventos
- ✅ Escalabilidade automática com custo proporcional ao uso
- ✅ Observabilidade completa com logs estruturados e alarmes no CloudWatch
- ✅ Ambiente local gratuito e fiel via LocalStack + Docker Compose
- ✅ Segurança por design: ECDSA, IAM de privilégio mínimo, CORS estrito

---

## 🏗️ Arquitetura

O fluxo completo da plataforma, desde a autenticação do usuário até a notificação do resultado do processamento:

```
┌─────────────────────────────────────────────────────────────────────────────┐
│                          Frontend SPA (S3 + CloudFront)                     │
└───────────────────────────────────┬─────────────────────────────────────────┘
                                    │ HTTPS / REST
                    ┌───────────────┼───────────────┐
                    ▼               ▼               ▼
           ┌─────────────┐  ┌─────────────┐        │
           │ Auth Service│  │Upload Service│        │
           │  (Fargate)  │  │  (Fargate)  │        │
           └──────┬──────┘  └──────┬───────┘        │
                  │                │                 │
                  ▼                ▼                 │
           ┌─────────┐      ┌─────────┐             │
           │ Auth DB │      │  AWS S3 │             │
           │(RDS/SQL)│      │ Bucket  │             │
           └─────────┘      └────┬────┘             │
                                 │ S3 Event Trigger  │
                                 ▼                   │
                          ┌────────────┐             │
                          │ Amazon SQS │             │
                          └──────┬─────┘             │
                     ┌───────────┴──────────┐        │
                     ▼                      ▼        │
              ┌────────────┐      ┌──────────────┐  │
              │   Lambda   │      │Fargate Worker │  │
              │ (Reactive) │      │(Long Polling) │  │
              └──────┬─────┘      └───────┬───────┘  │
                     └──────────┬──────────┘          │
                                ▼                     │
                         ┌────────────┐               │
                         │ Amazon SNS │               │
                         │  (Fanout)  │               │
                         └──────┬─────┘               │
                                ▼                     │
                        ┌──────────────┐              │
                        │Lambda Notif. │◄─────────────┘
                        │  (Reactive)  │
                        └──────┬───────┘
                               ▼
                        ┌────────────┐
                        │ Amazon SES │
                        │  (E-mail)  │
                        └────────────┘
```

### Fluxo de Dados

| Etapa | Evento | Responsável |
|-------|--------|-------------|
| 1 | Usuário autentica e recebe JWT | Auth Service |
| 2 | Client solicita Presigned URL para upload | Upload Service |
| 3 | Arquivo enviado diretamente ao S3 (bypass da API) | Cliente → S3 |
| 4 | S3 dispara evento → mensagem enfileirada no SQS | AWS S3 → SQS |
| 5 | Worker consome a fila e executa processamento pesado | Fargate Worker |
| 6 | Resultado publicado no tópico SNS | Processing Service |
| 7 | Lambda assina o SNS e envia e-mail via SES | Notification Lambda |

---

## 📦 Microsserviços

### 1. Auth Service — `ECS Fargate / Web API`

Gerencia a identidade dos usuários: cadastro, login e validação de tokens.

- **Autenticação:** Tokens JWT assinados com criptografia assimétrica **ECDSA**
- **Persistência:** Entity Framework Core com suporte a **Azure SQL Edge** (local) e **Amazon RDS** (cloud)
- **Padrão:** Clean Architecture + CQRS com MediatR
- **Validação:** Pipeline com FluentValidation integrado às Minimal APIs

### 2. Upload Service — `ECS Fargate / Minimal API`

Orquestra o fluxo seguro de ingestão de arquivos, protegendo a banda e memória das APIs.

- **Estratégia:** Geração de **Presigned URLs** via AWS SDK do S3
- **Benefício:** Upload de arquivos pesados realizado diretamente pelo cliente para o S3, sem tráfego pela API
- **Segurança:** Políticas de acesso temporário, escopo limitado por IAM Role

### 3. Processing Service — `Híbrido: Lambda + Fargate Worker`

Núcleo de processamento — arquitetura híbrida para máxima eficiência de custo e desempenho.

| Componente | Tipo | Caso de Uso |
|------------|------|-------------|
| Lambda Processor | Serverless Reativo | Respostas rápidas para metadados e eventos leves |
| Fargate Worker | Background Service | Filas de longa duração via Long Polling no SQS |

- **Resiliência:** Dead-Letter Queue (DLQ) para tratamento de falhas
- **Notificação:** Publicação do resultado no Amazon SNS ao final do fluxo

### 4. Notifications Service — `AWS Lambda / Serverless`

Função reativa, acionada por tópicos SNS, responsável pelo canal de comunicação com o usuário final.

- **Trigger:** Subscrição no tópico Amazon SNS
- **Entrega:** Envio de e-mails transacionais via **Amazon SES**
- **Modelo:** 100% assíncrono, sem servidor dedicado, custo por invocação

---

## 🛠️ Stack Tecnológica

### Linguagem e Framework

| Tecnologia | Versão | Uso |
|------------|--------|-----|
| C# | 13 | Linguagem principal |
| .NET | 10 | Runtime e SDK |
| ASP.NET Core Minimal APIs | 10 | Upload Service |
| ASP.NET Core Web API | 10 | Auth Service |
| AWS Lambda (.NET) | Runtime 10 | Processing & Notifications |

### Padrões Arquiteturais

- **Clean Architecture** — separação de responsabilidades em camadas (Domain, Application, Infrastructure, Presentation)
- **CQRS** (Command Query Responsibility Segregation) — segregação de leitura e escrita via **MediatR**
- **Event-Driven Architecture** — comunicação assíncrona entre serviços via SQS e SNS
- **Presigned URL Pattern** — upload direto ao storage sem intermediários

### AWS Services

| Serviço | Categoria | Função |
|---------|-----------|--------|
| ECS Fargate | Compute | Hospedagem dos containers de API |
| AWS Lambda | Serverless | Processamento reativo e notificações |
| Amazon S3 | Storage | Armazenamento de arquivos e hospedagem do SPA |
| CloudFront | CDN | Distribuição global do frontend |
| Amazon SQS | Messaging | Fila de processamento com DLQ |
| Amazon SNS | Pub/Sub | Fanout de eventos entre serviços |
| Amazon SES | Email | Notificações transacionais |
| Amazon RDS | Database | Banco relacional gerenciado |
| CloudWatch | Observability | Logs, métricas e alarmes |
| Amazon ECR | Registry | Repositório de imagens Docker |

### Observabilidade & Segurança

- **Logging:** Serilog com output estruturado em **JSON**, enriquecido com contexto de request
- **Monitoramento:** Provisionamento automático de Métricas e Alarmes no **Amazon CloudWatch**
- **Autenticação:** Criptografia assimétrica **ECDSA** para geração e validação de JWTs
- **Autorização:** IAM Roles com política de **privilégio mínimo** isoladas por serviço
- **API:** CORS configurado de forma estrita por ambiente

### DevOps & Infraestrutura

- **Containers:** Dockerfiles multi-stage otimizados para imagens mínimas de produção
- **IaC:** Infraestrutura como Código provisionada via **AWS CLI scripts**
- **CI/CD:** Automação completa com **GitHub Actions**

---

## 🚀 Configuração e Execução Local

O ambiente foi projetado para rodar localmente de forma **100% gratuita e offline** através do **LocalStack** + **Docker Compose**, emulando com precisão os serviços da AWS.

### Pré-requisitos

Certifique-se de ter as seguintes ferramentas instaladas:

| Ferramenta | Versão Mínima | Link |
|------------|---------------|------|
| Docker Desktop | 4.x | [Download](https://www.docker.com/products/docker-desktop/) |
| .NET SDK | 10.0 | [Download](https://dotnet.microsoft.com/download) |
| AWS CLI | 2.x | [Download](https://aws.amazon.com/cli/) |

### Passo a Passo

**1. Clone o repositório**

```bash
git clone https://github.com/seu-usuario/aws-lab-platform.git
cd aws-lab-platform
```

**2. Suba os containers locais (LocalStack + dependências)**

```bash
docker compose up -d
```

> O LocalStack iniciará emulando todos os serviços AWS necessários: S3, SQS, SNS, SES e DynamoDB.

**3. Provisione a infraestrutura local**

```bash
./infra/aws/setup-local.sh
```

> Este script cria automaticamente os buckets S3, filas SQS (incluindo DLQs), tópicos SNS e configura as permissões necessárias no ambiente LocalStack.

**4. Build das soluções .NET**

```bash
# Auth Service
dotnet build services/auth-service/AuthService.sln

# Processing Service
dotnet build services/processing-service/ProcessingService.slnx
```

**5. Verifique os serviços em execução**

```bash
docker compose ps
```

### Variáveis de Ambiente

Copie o arquivo de exemplo e configure as variáveis locais:

```bash
cp .env.example .env.local
```

> Para uso local com LocalStack, as credenciais AWS fictícias já estão pré-configuradas no `docker-compose.yml`.

---

## ⚙️ CI/CD — GitHub Actions

Os pipelines de integração e entrega contínua estão definidos em `.github/workflows/` e são acionados automaticamente a cada `push` nas branches configuradas.

### Auth Service Pipeline

**Arquivo:** `.github/workflows/auth-service.yml`

```
Push → Restore → Build → Unit Tests (xUnit) → Publish Image → ECR (simulado)
```

- Restauração de dependências NuGet isolada por pipeline
- Execução de testes unitários com **xUnit** em ambiente isolado
- Simulação de publicação da imagem no **Amazon ECR**

### Processing Service Pipeline

**Arquivo:** `.github/workflows/processing-service.yml`

```
Push → Restore → Build → Package Lambda (.zip) → Deploy Artifact
```

- Utiliza o **Amazon.Lambda.Tools** para compilar e empacotar
- Geração de pacotes `.zip` otimizados para a arquitetura Serverless da AWS

---

## 📁 Estrutura do Repositório

```
aws-lab-platform/
├── .github/
│   └── workflows/
│       ├── auth-service.yml
│       └── processing-service.yml
├── frontend/                          # SPA estático (S3/CloudFront)
├── infra/
│   └── aws/
│       └── setup-local.sh             # Script de provisionamento local
├── services/
│   ├── auth-service/                  # Web API — ECS Fargate
│   │   └── AuthService.sln
│   ├── upload-service/                # Minimal API — ECS Fargate
│   ├── processing-service/            # Lambda + Fargate Worker
│   │   └── ProcessingService.slnx
│   └── notifications-service/         # AWS Lambda
├── docker-compose.yml
├── .env.example
└── README.md
```

---

## 🤝 Contribuindo

Contribuições são bem-vindas! Siga os passos abaixo:

1. Faça um **fork** do repositório
2. Crie uma branch para sua feature: `git checkout -b feat/minha-feature`
3. Commit suas alterações: `git commit -m 'feat: adiciona minha feature'`
4. Faça o push: `git push origin feat/minha-feature`
5. Abra um **Pull Request**

---

<div align="center">

Desenvolvido com ☕ e **.NET 10**

</div>
