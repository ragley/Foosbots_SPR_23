import socket
from GameState import GameState
from Message import Message

HOST = '127.0.0.1'  # The server's hostname or IP address
PORT = 5000       # The port used by the server

try:
    sock = socket.socket(socket.AF_INET, socket.SOCK_STREAM)
    sock.connect((HOST, PORT))

    while True:
        message = Message("POST")
        message.update_message_random_values()
        sock.sendall(message.encode_to_send(True))
        print(message.data)
        
except KeyboardInterrupt:
    print("caught keyboard interrupt, exiting")
finally:
    sock.send(b'')
    sock.close()