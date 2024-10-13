window.addEventListener('error', function (e) {
    var errorText = [
        e.message,
        'URL: ' + e.filename,
        'Line: ' + e.lineno + ', Column: ' + e.colno,
        'Stack: ' + (e.error && e.error.stack || '(no stack trace)'),
        '[~]'
    ].join('---');

    $.post(frontEndErrorLoggerUrl, { errorText });
});