let _engines = engines.all();
if (app.versionName.startsWith("Pro 8")) {
    for (let i = 0; i < _engines.length; i++) {
        if (_engines[i].getSource().toString().indexOf("cap_script") != -1) {
            _engines[i].forceStop();
        }
    }
}
else if (app.versionName.startsWith("Pro 9")) {
    for (let i = 0; i < _engines.length; i++) {
        if (_engines[i].source.toString().indexOf("cap_script") != -1) {
            _engines[i].forceStop();
        }
    }
}

