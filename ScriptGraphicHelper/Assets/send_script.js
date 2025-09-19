importClass(java.io.OutputStream);
importClass(java.net.Socket);
importClass(java.net.ServerSocket);
importClass(java.nio.ByteBuffer);
importClass(java.nio.ByteOrder);

function int2Bytes(num) {
    let bytes = java.lang.reflect.Array.newInstance(java.lang.Byte.TYPE, 4);
    let arr = new Array(); arr[0] = (num >> 24) & 0xff; arr[1] = (num >> 16) & 0xff; arr[2] = (num >> 8) & 0xff; arr[3] = num & 0xff;
    bytes[0] = arr[0] < 127 ? arr[0] : arr[0] - 256; bytes[1] = arr[1] < 127 ? arr[1] : arr[1] - 256;
    bytes[2] = arr[2] < 127 ? arr[2] : arr[2] - 256; bytes[3] = arr[3] < 127 ? arr[3] : arr[3] - 256;
    return bytes;
}

function makePackData(key, desc, buffer) {
    var size = 4 + 4 + 256 + 4 + 256 + 4;
    if (buffer != null) {
        size += buffer.length;
    }

    var byteBuffer = ByteBuffer.allocate(size);
    byteBuffer.order(ByteOrder.BIG_ENDIAN);

    byteBuffer.put(int2Bytes(size - 4));

    var keyBytes = new java.lang.String(key).getBytes();
    byteBuffer.put(int2Bytes(keyBytes.length));
    byteBuffer.put(keyBytes);

    if (desc != null) {
        byteBuffer.position(4 + 4 + 256);
        var descBytes = new java.lang.String(desc).getBytes();
        byteBuffer.put(int2Bytes(descBytes.length));
        byteBuffer.put(descBytes);
    }

    if (buffer != null) {
        byteBuffer.position(4 + 4 + 256 + 4 + 256);
        byteBuffer.put(int2Bytes(buffer.length));
        byteBuffer.put(buffer);
    }

    return byteBuffer.array();
}

function send() {
    let socket;
    let stream;
    let remoteIP;

    let engine = null;
    let _engines = engines.all();
    if (app.versionName.startsWith("Pro 8")) {
        for (let i = 0; i < _engines.length; i++) {
            if (_engines[i].getSource().toString().indexOf("cap_script") != -1) {
                _engines[i].emit("send", remoteIP);
                return;
            }
        }
    }
    else if (app.versionName.startsWith("Pro 9")) {
        for (let i = 0; i < _engines.length; i++) {
            if (_engines[i].source.toString().indexOf("cap_script") != -1) {
                _engines[i].emit("send", remoteIP);
                return;
            }
        }
    }

    socket = new Socket(remoteIP, 5678);
    stream = socket.getOutputStream();

    if (engine == null) {
        var data = makePackData("screenShot_failed", "获取常驻截屏脚本对象失败, 请在图色助手重新连接aj!", null);
        stream.write(data);
        return;
    }
}

send();