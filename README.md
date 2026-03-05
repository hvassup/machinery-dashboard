# Machinery Dashboard

**Live demo: https://machinery-dashboard-zeta.vercel.app/**

A real-time factory equipment monitoring system. Supervisors schedule production orders, workers track machine states (red/yellow/green), and the dashboard provides live status and historical overviews.

## Architecture

Architecture Diagram can be found [here](https://mermaid.live/edit#pako:eNqVVF1v2jAU_SuWn_mIk5BCNFUCmnXToEsJK9KWPRjiQdYkzmynpSv8912bpmtot6p-iK59zzn3K7r3eMUThn28FrTcoPlZXCBzZLU8PI2zlBWqftYnXAy_xRi-75aie3rBtqrzUyLSM9crJlYsi_H3msGKJC6eqQafItAYLiJtQYxKKiaesBrgKM2rjCouJHCCX1Va5pASipi4SVdMHtH0CQggWY1sW8TkFmX8Fj3yzZNnSVQKDioyLdYvKdlNJdvQpixJq_xIi7yq5TS1HEN7T6U6UnL-p9RoqD6z6SXozuhymarp5UN-UtI1QyPBrx_6-pQxDD8CQyqqKtmmZWo4nYtgjoiFhlFozDEX7BkzPNej51KtBYsuJ4YYjCJ0xbMqfw6fjB6mPOE0QSOa0WLVmHOjmC9RMAP8ggtIGnVRVJV6xJILEweKuZVH1WgKardPdx_m8zDa6X-zdoH514O6UGYberq920FWNWYyeoQgv2_1yU73pvaCadwzRpPuQqSKQYDzY--YFxJqR-wGxid3ehzHkAjKRCO2TovwMNXPImFizPOcFkmDERBDCKtllsrNS5qB_SrCeQ0BpoH8O6WAvAFrvwHr4BbsmjTBvhIVa-GciZzqK77XKjFWG6Z_JB_MhIrrGMfFHjglLb5yntc0wav1Bvs_aCbhVpUJVewspbAv8sdXAV3XoatCYd8mPSOC_Xu8havdcT3H8lzXsVwY_EkL32GfuF6HWLbrugPSI7Y98PYt_NuEtToDl3i2bfXISb8_8Gy3hWEJwFKaHhao2aP7P3RgmhM)

## Services

| Service | Technology | Description |
|---|---|---|
| `pwa` | Next.js 15 | Worker/supervisor UI. Live machine status, order scheduling, push notifications. |
| `status-api` | .NET 10 ASP.NET Core | REST API. Consumes machine events, persists state and order history, sends push notifications. |
| `equipment-service` | .NET 10 Worker Service | Simulates a physical machine. One instance per machine. Receives commands, transitions through states, emits events. |

## Infrastructure

| Component | Purpose |
|---|---|
| RabbitMQ | Message broker. Commands routed 1:1 to machines, events broadcast via fanout exchange. |
| PostgreSQL | Persists machine state, order history, and event log. |
| Kubernetes (EKS) | Orchestrates all backend services. |
| Vercel | Hosts the PWA. |

## Running Locally

### Prerequisites
- Docker Desktop
- .NET 10 SDK
- Node.js 20+

### Backend

```bash
cp .env.example .env
# Edit .env with your values
docker compose up -d --build
```

The status-api will be available at `http://localhost:8081`.

### PWA

```bash
cd pwa
npm install
npm run dev
```

The dashboard will be available at `http://localhost:3000`.