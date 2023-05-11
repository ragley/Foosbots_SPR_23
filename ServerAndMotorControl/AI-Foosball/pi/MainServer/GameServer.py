from json import JSONDecodeError
import socket
import selectors
import types
import time
from GameState import GameState
from Message import Message

sel = selectors.DefaultSelector()
game_state = GameState()  

def accept_wrapper(sock):
    conn, addr = sock.accept()  
    #print("accepted connection from", addr)
    conn.setblocking(False)
    data = types.SimpleNamespace(addr=addr, inb=b"", outb=b"")
    events = selectors.EVENT_READ | selectors.EVENT_WRITE
    sel.register(conn, events, data=data)


def service_connection(key, mask):
    sock = key.fileobj
    data = key.data
    if mask & selectors.EVENT_READ:
        try:
            recv_data = sock.recv(4)  
            if recv_data:
                data_size = int.from_bytes(recv_data, byteorder="big")
                recv_data = sock.recv(data_size)
                received = Message("RECEIVED")
                received.decode_from_receive(recv_data) 
                data.outb = handle_message(received.data)
            else:
                #print("closing connection to", data.addr)
                sel.unregister(sock)
                sock.close()
        except JSONDecodeError:
            print("JSONDecodeError from ",data.addr, " with data ", recv_data)
            sending = Message()
            sending.data = {"error":"JSONDecodeError"}
            data.outb = sending.encode_to_send(False)
        except UnicodeDecodeError:
            print("UnicodeDecodeError from ",data.addr, " with data ", recv_data)
            sending = Message()
            sending.data = {"error":"UnicodeDecodeError"}
            data.outb = sending.encode_to_send(False)
        except ConnectionResetError:
            print("Connection was closed forcibly by a remote host",data.addr)
            sel.unregister(sock)
    if mask & selectors.EVENT_WRITE:
        if data.outb:
            #print("Sending", repr(data.outb), "to", data.addr)
            #print("Sending to ", data.addr)
            sent = sock.send(data.outb)  # Should be ready to write
            data.outb = data.outb[sent:]

def handle_message(data):
    if "action" in data:
        if data["action"] == "POST":
            game_state.update_game_data(data)
            return ""
        elif data["action"] == "GET":
            return_data = game_state.get_game_data(data)
            sending = Message()
            sending.data = return_data
            return sending.encode_to_send(False)
        elif data["action"] == "DUMP":
            return_data = game_state.get_all_data()
            sending = Message()
            sending.data = return_data
            return sending.encode_to_send(False)
        else:
            sending = Message()
            sending.data = {"error":"Action not Understood"}
            return sending.encode_to_send(False)
    else:
        sending = Message()
        sending.data = {"error":"No Action in Data"}
        return sending.encode_to_send(False)
        
    

host = "192.168.0.1" #pi ip address
#host = "127.0.0.1" #localhost ip address
port = 5000

lsock = socket.socket(socket.AF_INET, socket.SOCK_STREAM)
lsock.bind((host, port))
lsock.listen()
#print("listening on", (host, port))
lsock.setblocking(False)
sel.register(lsock, selectors.EVENT_READ, data=None)

try:
    while True:
        events = sel.select(timeout=None)
        for key, mask in events:
            if key.data is None:
                accept_wrapper(key.fileobj)
            else:
                service_connection(key, mask)
except KeyboardInterrupt:
    print("caught keyboard interrupt, exiting")
finally:
    sel.close()
