const LOG_SERVER_URL = 'http://10.65.145.161:5000';

// Keep originals so we still see logs in the real console
const orig = {
    log: console.log,
    info: console.info,
    warn: console.warn,
    error: console.error,
    debug: console.debug,
};

const formatArg = (a) => {
    if (a instanceof Error) return a.stack || (a.name + ': ' + a.message);
    if (a === undefined) return 'undefined';
    if (a === null) return 'null';
    if (typeof a === 'object') {
        try { return JSON.stringify(a, (_k, v) => v, 2); }
        catch { return Object.prototype.toString.call(a); }
    }
    return String(a);
};

// Send log to server
const sendToServer = async (level, args) => {
    try {
        const message = args.map(formatArg).join(' ');
        const logData = {
            level: level,
            message: message,
            timestamp: new Date().toISOString(),
            source: 'benchmark'
        };

        const response = await fetch(`${LOG_SERVER_URL}/log`, {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json',
            },
            body: JSON.stringify(logData)
        });

        if (!response.ok) {
            console.warn('Failed to send log to server:', response.statusText);
        }
    } catch (error) {
        console.warn('Error sending log to server:', error);
    }
};

// Patch each console method
['log', 'info', 'warn', 'error', 'debug'].forEach((level) => {
    console[level] = (...args) => {
        // Send to log server
        sendToServer(level, args);
        // forward to the original console
        try { orig[level].apply(console, args); } catch { }
    };
});

// Window errors
window.addEventListener('error', (e) => {
    const errorMsg = [
        e.message,
        `@ ${e.filename}:${e.lineno}:${e.colno}`,
        e.error && (e.error.stack || e.error)
    ].filter(Boolean);
    sendToServer('error', errorMsg);
});

// Unhandled promise rejections
window.addEventListener('unhandledrejection', (e) => {
    const r = e.reason;
    const errorMsg = ['Unhandled promise rejection:', r && (r.stack || r)];
    sendToServer('error', errorMsg);
});
