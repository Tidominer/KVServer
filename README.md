# KVServer

A secure, multi-tenant key-value store server built with .NET 10. Values are encrypted at rest using AES-256-GCM, and every update creates a versioned history entry.

## Features

- **Encrypted storage** — values encrypted with AES-256-GCM; keys derived via PBKDF2 (100,000 iterations, SHA-256)
- **Version history** — every write creates a new version; old versions are retrievable
- **Multi-tenant** — multiple named storages, each with its own access token and encryption salt
- **Token authentication** — all API requests require an `X-Access-Token` header
- **Rate limiting** — configurable failed-auth lockout per IP (default: 3 attempts/min)
- **Web UI** — built-in SPA with key management, version history, syntax-highlighted code samples, pagination, and search
- **CLI management** — create/delete storages, regenerate tokens, and manage keys from the command line
- **Export / import** — dump all keys to JSON and restore them, useful for backup and migration
- **Safe token rotation** — regenerating a token re-encrypts all stored values with the new key; no data is lost
- **Server flags** — tune behaviour at startup via flags or `appsettings.json`
- **SQLite backend** — single-file database, no separate database process needed

## Projects

| Project | Role |
|---|---|
| `KVServer.Core` | Domain models and service/repository interfaces |
| `KVServer.Infrastructure` | EF Core + SQLite, service implementations, encryption |
| `KVServer.Api` | ASP.NET Core Web API + optional SPA static files |
| `KVServer.Cli` | CLI tool for storage and key management (direct database access, no API calls) |

## Requirements

- [.NET 10 SDK](https://dotnet.microsoft.com/download)

## Getting Started

### 1. Run the API server

```bash
kvserver
```

The server starts on `http://localhost:5205` by default (configured in `appsettings.json`). Open that URL in a browser to access the web UI.

### 2. Create a storage

Use the CLI to create a storage and receive its access token:

```bash
kvserver-cli storage create my-storage
```

Output:
```
Storage created successfully!
ID:           1
Name:         my-storage
Access Token: kv_1_a3f5b8c9d2e1f4a6

Save this token securely — it cannot be recovered.
```

### 3. Use the Web UI

Navigate to `http://localhost:5205`, enter the access token, and manage keys through the browser interface.

### 4. Read values via the API

Pass the token in every request via the `X-Access-Token` header.

```bash
# Get a key's current value
curl http://localhost:5205/api/keys/greeting \
  -H "X-Access-Token: <your-token>"

# Get a specific version
curl http://localhost:5205/api/keys/greeting/versions/1 \
  -H "X-Access-Token: <your-token>"

# View full version history
curl http://localhost:5205/api/keys/greeting/history \
  -H "X-Access-Token: <your-token>"
```

## Configuration

All server settings live under the `Server` key in `appsettings.json`. CLI flags override file-based settings. Environment variables (prefixed `KVSERVER_`) sit between the two.

Priority (highest wins): **CLI flags → env vars → appsettings.json**

### appsettings.json

```json
{
  "Server": {
    "Bind":      "localhost",
    "Port":      5205,
    "DbPath":    "./kvserver.db",
    "NoWeb":     false,
    "ReadOnly":  false,
    "NoCors":    false,
    "LogLevel":  "Warning",
    "RateLimit": 3
  }
}
```

### CLI flags

| Flag | Default | Description |
|---|---|---|
| `--bind <address>` | `localhost` | Network interface to listen on. Use `0.0.0.0` to listen on all interfaces. |
| `--port <n>` | `5205` | Port to listen on. |
| `--db <path>` | `./kvserver.db` | Path to the SQLite database file. |
| `--no-web` | off | Disable the web UI (static files, SPA routes, and `/api/storages/current`). API-only mode. |
| `--read-only` | off | Reject all write operations on keys (POST / PUT / DELETE). Reads still work. |
| `--no-cors` | off | Disable the permissive CORS policy. Enforces same-origin. |
| `--log-level <level>` | `Warning` | Minimum log level: `Trace`, `Debug`, `Information`, `Warning`, `Error`, `Critical`, `None`. |
| `--rate-limit <n>` | `3` | Max failed auth attempts per IP per minute before a 60-second lockout. |

#### Examples

```bash
# Public-facing API-only, read-only, behind a reverse proxy
kvserver --no-web --read-only --bind 0.0.0.0 --port 8080

# Verbose logging
kvserver --log-level Debug

# Custom database path with higher rate limit tolerance
kvserver --db /var/data/prod.db --rate-limit 10
```

### Environment variables

Each `Server` setting can be overridden with a `KVSERVER_` prefixed environment variable using double-underscore as the section separator:

```bash
KVSERVER_Server__Port=8080
KVSERVER_Server__Bind=0.0.0.0
KVSERVER_Server__ReadOnly=true
KVSERVER_Server__DbPath=/var/data/kvserver.db
```

## API Reference

All endpoints require `X-Access-Token: <token>` header.

### Storage

| Method | Path | Description |
|---|---|---|
| `POST` | `/api/storages` | Create a new storage |
| `GET` | `/api/storages/current` | Get the name and ID of the authenticated storage. |

### Keys

| Method | Path | Description |
|---|---|---|
| `GET` | `/api/keys` | List all keys in the storage |
| `POST` | `/api/keys` | Create a new key-value pair |
| `GET` | `/api/keys/{key}` | Get the current value of a key |
| `PUT` | `/api/keys/{key}` | Update a key's value (creates new version) |
| `DELETE` | `/api/keys/{key}` | Delete a key and all its versions |
| `GET` | `/api/keys/{key}/history` | Get the full version history of a key |
| `GET` | `/api/keys/{key}/versions/{version}` | Get a specific version |

### Health

| Method | Path | Description |
|---|---|---|
| `GET` | `/health` | Returns server status and UTC timestamp |

## CLI Reference

Pass `help` to any command for usage details.

### Storage commands

```bash
kvserver-cli storage create <name>        # Create a storage, prints ID and token
kvserver-cli storage list                 # List all storages with IDs and tokens
kvserver-cli storage delete <id|name>     # Delete a storage (with confirmation prompt)
```

### Token commands

```bash
kvserver-cli token regenerate <id|name>   # Rotate the access token; all values are
                                          # automatically re-encrypted with the new token
```

### Key commands

```bash
kvserver-cli key list    <token>                    # List all keys
kvserver-cli key get     <token> <key>              # Get the current value of a key
kvserver-cli key get     <token> <key> --version 2  # Get a specific version
kvserver-cli key set     <token> <key> <value>      # Create or update a key
kvserver-cli key set     <token> <key> -            # Read value from stdin
kvserver-cli key delete  <token> <key>              # Delete a key (with confirmation)
kvserver-cli key history <token> <key>              # Show full version history
kvserver-cli key export  <token>                    # Export all keys to JSON (stdout)
kvserver-cli key export  <token> --output file.json # Export to a file
kvserver-cli key import  <token> <file.json>        # Import keys from a JSON file
kvserver-cli key import  <token> -                  # Import from stdin
```

#### Export / import format

```json
{
  "storage": "my-storage",
  "exportedAt": "2026-06-20T12:00:00Z",
  "keys": [
    { "key": "db.host", "value": "localhost" },
    { "key": "db.port", "value": "5432" }
  ]
}
```

Import is idempotent — existing keys are updated, new keys are created. A summary of created/updated/failed counts is printed to stderr.

## Security Notes

- Encryption keys are never stored — they are derived at request time from the access token and a per-storage random salt using PBKDF2.
- If you lose an access token, you can recover access by running `kvserver-cli token regenerate <name>` — this issues a new token and re-encrypts all values without requiring the old token. Data is only unrecoverable if the database itself is lost.
- Token rotation (`token regenerate`) re-encrypts all version history entries before updating the stored token, so no data is lost.
- The API enforces IP-based rate limiting on authentication failures to mitigate brute-force attacks.
- For public deployments, run behind a TLS-terminating reverse proxy (nginx, Caddy, Traefik) rather than exposing the server directly. Use `--bind 127.0.0.1` to ensure the app is only reachable through the proxy.

## Building for Production (Linux x64)

Two PowerShell build scripts are provided. Both produce `kvserver` and `kvserver-cli` binaries under `builds/`, along with `wwwroot/` and `appsettings.json`.

| Script | Target machine requirement |
|---|---|
| `build-linux-x64-fdd.ps1` | .NET 10 runtime must be installed |
| `build-linux-x64-self-contained.ps1` | None — runtime is bundled (larger output) |

```powershell
# Framework-dependent (smaller, requires .NET 10 on the server)
.\build-linux-x64-fdd.ps1

# Self-contained (larger, no runtime needed on the server)
.\build-linux-x64-self-contained.ps1

# Either script accepts -Zip to produce a publish.zip archive
.\build-linux-x64-fdd.ps1 -Zip
```

Both scripts output to `builds/` and include `kvserver`, `kvserver-cli`, `wwwroot/`, `appsettings.json`, and `kvserver.service`.

> **Note:** A plain `dotnet publish` produces binaries named `KVServer.Api` and `KVServer.Cli`. The build scripts rename them to `kvserver` and `kvserver-cli`.

## Running as a systemd Service (Linux)

A `kvserver.service` unit file is included in every build output.

**1. Create a dedicated user and deploy the files:**

```bash
sudo useradd --system --no-create-home --shell /usr/sbin/nologin kvserver
sudo mkdir -p /opt/kvserver
sudo cp -r builds/kvserver\ linux-x64/* /opt/kvserver/
sudo chown -R kvserver:kvserver /opt/kvserver
sudo chmod +x /opt/kvserver/kvserver /opt/kvserver/kvserver-cli
```

**2. Install and start the service:**

```bash
sudo cp /opt/kvserver/kvserver.service /etc/systemd/system/
sudo systemctl daemon-reload
sudo systemctl enable kvserver
sudo systemctl start kvserver
sudo systemctl status kvserver
```

**3. View logs:**

```bash
journalctl -u kvserver -f
```

**Configuration via environment variables:**

Rather than editing `appsettings.json` on the server, uncomment and set the `Environment=` lines in the unit file before installing:

```ini
Environment=KVSERVER_Server__DbPath=/var/lib/kvserver/kvserver.db
Environment=KVSERVER_Server__Bind=127.0.0.1
Environment=KVSERVER_Server__Port=5205
```

After any change to the unit file:

```bash
sudo systemctl daemon-reload
sudo systemctl restart kvserver
```

## License

MIT
