# 🛡️ Real-Time Financial Fraud Detection System

A high-throughput, distributed, event-driven architecture designed to detect fraudulent financial transactions in real-time. This system ingests transaction streams via Kafka, processes them using a .NET 9 microservice, evaluates risk using a Python ML engine, and pushes live alerts to a React Dashboard via WebSockets.

![.NET](https://img.shields.io/badge/.NET-9.0-512BD4?style=flat&logo=dotnet)
![React](https://img.shields.io/badge/React-18-61DAFB?style=flat&logo=react)
![Python](https://img.shields.io/badge/Python-3.11-3776AB?style=flat&logo=python)
![Kafka](https://img.shields.io/badge/Apache%20Kafka-Streaming-231F20?style=flat&logo=apachekafka)
![Docker](https://img.shields.io/badge/Docker-Containerized-2496ED?style=flat&logo=docker)

## 🏗️ Architecture

The system follows a polyglot microservices architecture:

1.  **Transaction Producer:** Simulates high-velocity financial data (JSON) sent to **Kafka**.
2.  **Message Broker:** **Apache Kafka** buffers the stream of transactions.
3.  **Backend Processor (.NET 9):**
    * Consumes messages from Kafka.
    * Implements **Circuit Breaker** patterns (Polly) for fault tolerance.
    * Handles **Dead Letter Queues (DLQ)** for failed messages.
4.  **ML Engine (Python/FastAPI):** Receives transaction data and calculates a fraud risk score (0.0 - 1.0).
5.  **Real-Time Dashboard (React + SignalR):** Displays processed transactions and visualizes fraud risks instantly without page refreshes.

---

## 🚀 Tech Stack

| Component | Technology |
| :--- | :--- |
| **Backend** | C#, .NET 9 Web API, SignalR |
| **Frontend** | React 18, Vite, Bootstrap |
| **ML Service** | Python 3.11, FastAPI, Pydantic |
| **Messaging** | Apache Kafka, Zookeeper |
| **Resilience** | Polly (Circuit Breaker, Retry policies) |
| **DevOps** | Docker, Docker Compose |

---

## 🛠️ Getting Started (One-Click Run)

### Prerequisites
* Docker Desktop installed and running.

### Installation
1.  **Clone the repository**
    ```bash
    git clone [https://github.com/YOUR_USERNAME/FraudDetectionSystem.git](https://github.com/YOUR_USERNAME/FraudDetectionSystem.git)
    cd FraudDetectionSystem
    ```

2.  **Start the System**
    Run the entire stack (Kafka, Zookeeper, Backend, Frontend, ML API) with one command:
    ```bash
    docker compose up --build -d
    ```

3.  **Access the Dashboard**
    Open your browser to [http://localhost:3000](http://localhost:3000)

---

## 🧪 How to Test (Simulate Transactions)

Since this is an event-driven system, you need to feed data into Kafka to see the dashboard light up.

1.  **Open a terminal** and access the Kafka container:
    ```bash
    docker exec -it kafka kafka-console-producer --broker-list localhost:9092 --topic transactions
    ```

2.  **Paste a "Clean" Transaction (Low Amount)**
    *Result: Appears Green on Dashboard.*
    ```json
    {"TransactionId": "TXN-100", "Amount": 500, "MerchantId": "Amazon", "Timestamp": "2025-11-24T12:00:00"}
    ```

3.  **Paste a "Fraud" Transaction (Amount > 10,000)**
    *Result: Appears Red on Dashboard (High Risk).*
    ```json
    {"TransactionId": "TXN-999", "Amount": 15000, "MerchantId": "Unknown", "Timestamp": "2025-11-24T12:05:00"}
    ```

---

## 💥 Resilience Testing (Circuit Breaker)

The system is designed to survive failures.

1.  **Kill the ML Service:**
    ```bash
    docker stop mlapi
    ```
2.  **Send Messages:** Send 5+ messages via the Kafka producer terminal.
3.  **Observe Behavior:**
    * The .NET Backend logs will show failures.
    * After the threshold is reached, the **Circuit Breaker Opens**.
    * Messages are automatically routed to the **Dead Letter Queue (DLQ)** topic (`transactions-dlq`) to prevent data loss.

---

## 📂 Project Structure

```text
FraudDetectionSystem/
├── docker-compose.yml           # Orchestration
├── FraudDetection.Processor/    # .NET Backend
│   ├── FraudDetection.API/      # Web API & Kafka Consumer
│   └── FraudDetection.Worker/   # (Legacy Console Worker)
├── FraudDetection.MLApi/        # Python Fraud Detection Logic
└── FraudDetection.Web/          # React Frontend