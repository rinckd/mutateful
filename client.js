const net = require("net");
const maxApi = require("max-api");

var client = new net.Socket();
client.connect(13000, '127.0.0.1', function() {
	console.log('Connected');
	client.write('Hello, server! Love, Client.');
});
client.on('data', function(data) {
	console.log('Received: ' + data);
});
client.on('close', function() {
	console.log('Connection closed');
});

maxApi.addHandler("sendData", () => {
	client.write("Sending some data your way!");
})