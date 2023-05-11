const net = require("net")

let client = new net.Socket();
client.connect(5000, '127.0.0.1', () => {
		console.log("connected");
		// write data here
});

client.on('data', (data) => {
	console.log("Received:" + data);
	client.destroy(); // kill the client after the server's response
});
