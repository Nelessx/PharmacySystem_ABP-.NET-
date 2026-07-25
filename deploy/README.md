# Deploying PharmacySystem with Docker (free hosting)

This folder contains everything needed to run the whole app — ABP .NET 10 API +
OpenIddict auth server, Angular SPA, PostgreSQL, and a one-shot DB migrator —
as one Docker Compose stack behind Caddy (automatic HTTPS).

```
Internet ──► Caddy (:80/:443, auto Let's Encrypt)
               ├─► api  (ABP host + OpenIddict)   http:80
               └─► web  (Angular SPA via nginx)   http:80
             db  (PostgreSQL, persistent volume)
             db-migrator  (runs once, then exits)
```

## Where to host it for free

Researched and verified (2026). This app is an **OAuth/OIDC auth server**, so it
needs **real HTTPS** and must **not scale-to-zero** (cold starts break token
flows). That rules out most "free" PaaS tiers.

| Option | Verdict |
| --- | --- |
| **Oracle Cloud "Always Free" VM** (recommended) | Only option that runs the full multi-container stack, always-on, with a persistent self-hosted Postgres. Free-forever. |
| Google Cloud `e2-micro` Always Free VM (fallback) | Also free-forever, more reliable to provision, but only 1 GB RAM → move Postgres to free **Neon**/**Supabase**. |
| Render / Koyeb / Fly free tiers | Not viable here: spin-down (breaks the auth server), no free one-shot job, and/or free Postgres that deletes after 30 days. |

### Oracle Cloud caveats — read before you start
- **A credit/debit card is required at signup** (~$1 auth hold, never charged unless you upgrade). It stays $0.
- Always-Free ARM (Ampere A1) is now **2 OCPU / 12 GB** (halved in 2026) — still plenty for this stack.
- **"Out of host capacity"** on ARM is common — retry across availability domains, or switch the account to **Pay-As-You-Go** (still $0 within free limits, and it also avoids idle-reclamation).
- **Idle reclamation:** a very quiet VM can be stopped. Pay-As-You-Go exempts you.
- **HTTPS is DIY:** you need a hostname. Use a real domain, or a free wildcard like `app.<VM-IP>.nip.io` / `api.<VM-IP>.nip.io` (Let's Encrypt issues for these).

## Prerequisites
- A host with Docker + Docker Compose (the Oracle VM below).
- Two DNS names pointing at the host's public IP — e.g. `api.example.com` and `app.example.com` (or `nip.io` names).
- Ports **80 and 443** open to the internet.

---

## Step-by-step (Oracle Cloud Always Free)

### 1. Create the VM
- Oracle Cloud → **Compute → Instances → Create**.
- Shape: **Ampere (VM.Standard.A1.Flex)**, 2 OCPU / 12 GB. Image: **Ubuntu 22.04/24.04**.
- Save the SSH key. Note the **public IPv4**.
- **Networking → Security List → add Ingress rules** for TCP **80** and **443** from `0.0.0.0/0`.

### 2. Open the OS firewall too (classic gotcha — ACME fails silently otherwise)
```bash
sudo iptables -I INPUT 6 -m state --state NEW -p tcp --dport 80 -j ACCEPT
sudo iptables -I INPUT 6 -m state --state NEW -p tcp --dport 443 -j ACCEPT
sudo netfilter-persistent save    # Ubuntu; persists the rules
```

### 3. Install Docker
```bash
curl -fsSL https://get.docker.com | sudo sh
sudo usermod -aG docker $USER && newgrp docker
```

### 4. Point DNS
Create **A records**: `api.example.com` → VM IP, `app.example.com` → VM IP.
(Or skip DNS and use `api.<VM-IP>.nip.io` / `app.<VM-IP>.nip.io`.)

### 5. Get the code
```bash
git clone https://github.com/Nelessx/PharmacySystem_ABP-.NET-.git
cd PharmacySystem_ABP-.NET-/deploy
```

### 6. Generate the OpenIddict certificate
```bash
openssl req -x509 -newkey rsa:2048 -keyout key.pem -out cert.pem \
  -days 3650 -nodes -subj "/CN=PharmacySystem"
openssl pkcs12 -export -out certs/openiddict.pfx -inkey key.pem -in cert.pem \
  -passout pass:YOUR_CERT_PASSWORD
rm key.pem cert.pem
```

### 7. Configure
```bash
cp .env.prod.example .env.prod
nano .env.prod   # set API_DOMAIN, APP_DOMAIN, DB password, CERT_PASSPHRASE
                 # (= YOUR_CERT_PASSWORD), STRING_ENCRYPTION_PASSPHRASE
```
Set the SPA's runtime API URLs (no Angular rebuild needed):
```bash
sed -i "s/REPLACE_API_DOMAIN/api.example.com/g; s/REPLACE_APP_DOMAIN/app.example.com/g" web/dynamic-env.json
```

### 8. Launch
```bash
docker compose -f docker-compose.prod.yml --env-file .env.prod up -d --build
docker compose -f docker-compose.prod.yml logs -f db-migrator   # watch it seed, then exit 0
```
First build takes a while (it compiles .NET + Angular on the VM). Caddy fetches
TLS certs automatically once DNS resolves and 80/443 are reachable.

### 9. Verify
- `https://app.example.com` → the app loads and you can log in.
- `https://api.example.com/health-status` → healthy.
- Log in with **`admin` / `1q2w3E*`** and **change the password immediately**.

---

## Updating to a new release
```bash
git pull
docker compose -f docker-compose.prod.yml --env-file .env.prod up -d --build
```
The migrator re-runs automatically (idempotent) and applies any new migrations
before the API restarts.

## Optional: test the whole stack locally first
On your dev machine you can smoke-test without a public domain by using Caddy's
internal CA. Create `deploy/docker-compose.override.yml`:
```yaml
services:
  caddy:
    environment:
      API_DOMAIN: localhost
      APP_DOMAIN: localhost
```
and a one-line `Caddyfile` using `tls internal`, then `docker compose ... up --build`.
(For a real deployment, use the committed `Caddyfile` + public domains.)

## Gotchas checklist
- ✅ Ports 80/443 open in **both** the OCI Security List **and** the OS firewall.
- ✅ DNS A records resolve to the VM **before** first start (Caddy needs it for ACME).
- ✅ `AuthServer__Authority` == `App__SelfUrl` == `https://<API_DOMAIN>` (already wired in compose).
- ✅ `SEED_DEMO_DATA=false` for real use.
- ✅ Keep `STRING_ENCRYPTION_PASSPHRASE` and `certs/openiddict.pfx` **stable** — changing them breaks decryption / invalidates tokens.
- ✅ Change the default `admin` password after first login.

## Fallback: Google Cloud `e2-micro` + Neon
If Oracle's ARM capacity blocks you: create a GCP **e2-micro** Always Free VM
(us-west1/us-central1/us-east1), install Docker, and use this same compose —
but because it has only 1 GB RAM, **remove the `db` service** and point
`ConnectionStrings__Default` at a free **Neon** Postgres (use its direct host for
the migrator, the `-pooler` host for the API). Everything else is identical.
