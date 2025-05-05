# 🛠️ Observability OTEL

In this ecosystem, we have four .NET applications simulating a real-world scenario where multiple services are responsible for different tasks. Each application exports its telemetry using the OpenTelemetry standard through the OTEL Collector. Logs, metrics, and traces are collected and routed by the OTEL Collector to Loki (logs), Tempo/Jaeger (traces), and Prometheus (metrics). All data is visualized in Grafana.
Details: https://medium.com/@faulycoelho/implementing-observability-in-a-net-applications-logging-tracing-and-metrics-67fe5b58312d

![image](https://github.com/user-attachments/assets/87d54006-ecea-4d13-ad54-30e737cadb01)

---

## 🚀 Technologies Used

- [.NET 8](https://dotnet.microsoft.com/)
- ASP.NET Core
- OpenTelemetry (Logging, Tracing & Metrics)
- Docker & Docker Compose
- Grafana, Tempo, Loki, Prometheus
- Jaeger
- RabbitMQ (Messaging)
- PostgreSQL / SQL Server / Redis

---

## 🧰 Prerequisites

- [.NET SDK](https://dotnet.microsoft.com/en-us/download) installed
- [Docker](https://www.docker.com/) running locally
- Visual Studio or VS Code (optional but recommended)

---

## 🐳 Running the Project

1. **Clone the repository**

```bash
git clone https://github.com/faulycoelho/observability-otel-multiple-exporters.git
cd observability-otel-multiple-exporters
```
2. **Start dependencies with Docker**
```docker-compose up```

---
## 📊 Results

Below are some example results and dashboards generated:
### Loki (Logs)
![image](https://github.com/user-attachments/assets/a5f1ded6-590d-4d19-afab-8a074a10281c)

### Jaeger/Tempo (Traces)
![image](https://github.com/user-attachments/assets/4f66d9eb-79e2-43c5-a296-4769823198a9)
![image](https://github.com/user-attachments/assets/9487fc43-b644-437b-8f6e-23f1553b154b)

### Prometheus (Metrics)
![image](https://github.com/user-attachments/assets/fce195e1-856a-4c01-aa72-12fb3ad56de6)

