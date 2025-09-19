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

events.on("send", function (remoteIP) {
    let socket;
    let stream;
    log("1111");
    try {
        let img = images.captureScreen();
        socket = new Socket(remoteIP, 5678);
        stream = socket.getOutputStream();
        if (img != null) {
            let img_data = images.toBytes(img);
            var data = makePackData("screenShot_successed", null, img_data);
            stream.write(data);
        }
        else {
            var data = makePackData("screenShot_failed", "获取截图失败!", null);
            stream.write(data);
        }
    }
    catch (error) {
        console.log(error);
    }
    finally {
        if (stream != null) {
            stream.close();
        }
        if (socket != null) {
            socket.close();
        }
    }
});




let _engines = engines.all();

if (app.versionName.startsWith("Pro 8")) {
    threads.start(function () {
        if (!requestScreenCapture()) {
            alert("请求截图权限失败"); exit();
        }
        else {
            toastLog("请求截图权限成功");
        }
    });
}
else if (app.versionName.startsWith("Pro 9")) {
    threads.start(function () {
        if (!requestScreenCapture()) {
            alert("请求截图权限失败");
            exit();
        }
        else {
            toastLog("请求截图权限成功");
        }
    });
}

setInterval(() => { }, 1000);


