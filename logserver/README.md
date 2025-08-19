# Log Server for .NET WebAssembly Benchmarks

This Python Flask server accepts log messages via HTTP POST requests and writes them to a rotating log file.

## Features

- Accepts logs via POST requests to `/log` endpoint
- Writes logs to rotating files (10MB max per file, keeps 5 backups)
- CORS enabled for web applications
- Health check endpoint at `/health`
- View recent logs at `/logs` (last 100 lines)
- Clear logs endpoint at `/clear-logs`

## Installation

1. Install Python dependencies:
```bash
pip install -r requirements.txt
```

## Usage

### Start the server:
```bash
python server.py
```

The server will start on `http://localhost:5000`

### Send logs via POST request:

```bash
curl -X POST http://localhost:5000/log \
  -H "Content-Type: application/json" \
  -d '{
    "level": "INFO",
    "message": "Test log message",
    "source": "benchmark",
    "timestamp": "2024-01-01T12:00:00Z"
  }'
```

### Endpoints:

- `GET /health` - Health check
- `POST /log` - Submit a log message
- `GET /logs` - View recent logs
- `POST /clear-logs` - Clear all logs

### Log file location:

Logs are written to `logs/benchmark.log` in the current directory.

## Integration with HTML

The HTML file has been updated to send logs to this server instead of displaying them in a custom console element. Logs are sent via fetch requests to the `/log` endpoint. 