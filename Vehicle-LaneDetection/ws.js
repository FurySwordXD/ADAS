const WebSocket = require('ws');
const spawn = require("child_process").spawn;

const wss = new WebSocket.Server({ port: 8080 },()=>{
	console.log('server started')
})

wss.on('connection', function connection(ws) {
	const pythonProcess = spawn('python',["app.py"]);

	pythonProcess.stdout.on('data', (data) => {
		// Do something with the data returned from python script
		try{
			data = JSON.parse(data.toString());
			ws.send(JSON.stringify(data));
			console.log("Data: ", data);
		}		
		catch (err) {
			console.log(err);
		}
	});

	ws.on('message', (data) => {
		console.log('data received \n %o',data);
	})

	ws.on('close', ()=> {
		console.log('Connection closed');
		pythonProcess.kill();
	})
})

wss.on('listening',()=>{
	console.log('listening on 8080')
})