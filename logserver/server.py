#!/usr/bin/env python3
"""
Log Server for .NET WebAssembly Benchmarks
Accepts logs via POST requests and writes them to a file
"""

from flask import Flask, request, jsonify
from flask_cors import CORS
import json
import logging
from datetime import datetime
import os
from pathlib import Path

app = Flask(__name__)
CORS(app)  # Enable CORS for all routes

# Configure logging
log_dir = Path("logs")
log_dir.mkdir(exist_ok=True)

# Create a rotating file handler
from logging.handlers import RotatingFileHandler
log_file = log_dir / "benchmark.log"
file_handler = RotatingFileHandler(log_file, maxBytes=10*1024*1024, backupCount=5)  # 10MB max, keep 5 backups

# Configure the logger
logger = logging.getLogger('benchmark')
logger.setLevel(logging.INFO)
formatter = logging.Formatter('%(asctime)s - %(levelname)s - %(message)s')
file_handler.setFormatter(formatter)
logger.addHandler(file_handler)

@app.route('/health', methods=['GET'])
def health():
    """Health check endpoint"""
    return jsonify({"status": "healthy", "timestamp": datetime.now().isoformat()})

@app.route('/log', methods=['POST'])
def log_message():
    """Accept log messages via POST request"""
    try:
        data = request.get_json()
        
        if not data:
            return jsonify({"error": "No JSON data provided"}), 400
        
        # Extract log information
        level = data.get('level', 'INFO').upper()
        message = data.get('message', '')
        timestamp = data.get('timestamp', datetime.now().isoformat())
        source = data.get('source', 'unknown')
        
        # Format the log message
        log_entry = f"[{timestamp}] {level} [{source}]: {message}"
        
        # Log to file
        if level == 'ERROR':
            logger.error(log_entry)
        elif level == 'WARN':
            logger.warning(log_entry)
        elif level == 'DEBUG':
            logger.debug(log_entry)
        else:
            logger.info(log_entry)
        
        # Also print to console for development
        print(log_entry)
        
        return jsonify({"status": "logged", "timestamp": timestamp})
        
    except Exception as e:
        logger.error(f"Error processing log request: {str(e)}")
        return jsonify({"error": str(e)}), 500

@app.route('/logs', methods=['GET'])
def get_logs():
    """Get recent logs (last 100 lines)"""
    try:
        if log_file.exists():
            with open(log_file, 'r', encoding='utf-8') as f:
                lines = f.readlines()
                # Return last 100 lines
                recent_logs = lines[-100:] if len(lines) > 100 else lines
                return jsonify({
                    "logs": recent_logs,
                    "total_lines": len(lines),
                    "recent_lines": len(recent_logs)
                })
        else:
            return jsonify({"logs": [], "total_lines": 0, "recent_lines": 0})
    except Exception as e:
        return jsonify({"error": str(e)}), 500

@app.route('/clear-logs', methods=['POST'])
def clear_logs():
    """Clear all logs"""
    try:
        if log_file.exists():
            # Clear the file
            open(log_file, 'w').close()
            logger.info("Logs cleared by user request")
            return jsonify({"status": "logs_cleared"})
        else:
            return jsonify({"status": "no_logs_to_clear"})
    except Exception as e:
        return jsonify({"error": str(e)}), 500

if __name__ == '__main__':
    print(f"Starting Log Server...")
    print(f"Log file: {log_file.absolute()}")
    print(f"Server will be available at: http://localhost:5000")
    print(f"Health check: http://localhost:5000/health")
    print(f"Log endpoint: http://localhost:5000/log")
    print(f"View logs: http://localhost:5000/logs")
    print(f"Clear logs: POST http://localhost:5000/clear-logs")
    print(f"Press Ctrl+C to stop the server")
    
    app.run(host='0.0.0.0', port=5000, debug=True)
