import socket
import time
from Message import Message

#HOST = '127.0.0.1'  # The server's hostname or IP address
HOST = '192.168.0.1'
PORT = 5000       # The port used by the server
i = 1
cumulative_Vx = 0.0
cumulative_Vy = 0.0
max_Vx = 0.0
max_Vy = 0.0
standard_deviation_Vx = 0.0
standard_deviation_Vy = 0.0


try:
    sock = socket.socket(socket.AF_INET, socket.SOCK_STREAM)
    sock.connect((HOST, PORT))
    server_data = {"ball_x":0, "ball_y":0, "ball_Vx":0, "ball_Vy":0}

    while True:
        run = True
        message = Message("GET")
        message.data.update(server_data)
        sock.sendall(message.encode_to_send(True))
        recv_data = sock.recv(1024)
        received = Message("RECEIVED")
        received.decode_from_receive(recv_data)
        
        if "error" in received.data:
            print(received.data["error"])
            run = False
        else:
            for key in received.data:
                if received.data[key] == "not found" and key != "action":
                    print("Server did not return all required data. MISSING: %s"%key)
                    run = False

        if run:
            cumulative_Vx += abs(received.data["ball_Vx"])
            cumulative_Vy += abs(received.data["ball_Vy"])
            standard_deviation_Vx += abs(received.data["ball_Vx"] - (cumulative_Vx / i))
            standard_deviation_Vy += abs(received.data["ball_Vy"] - (cumulative_Vx / i))
            
            
            if abs(received.data["ball_Vx"]) > max_Vx:
                max_Vx = received.data["ball_Vx"]
            if abs(received.data["ball_Vy"]) > abs(max_Vy):
                max_Vy = received.data["ball_Vy"]
            
            #if i % 25 == 0:
            print("\033c")
            print("Average Vx: ",cumulative_Vx / i)
            print("Average Vy: ",cumulative_Vy / i)
            print("Standard Deviation Vx: ",standard_deviation_Vx / i)
            print("Standard Deviation Vy: ",standard_deviation_Vy / i)
            print("Max Vx: ",max_Vx)
            print("Max Vy: ",max_Vy)
    
            i += 1
            time.sleep(1)
                
except KeyboardInterrupt:
    print("caught keyboard interrupt, exiting")
finally:
    sock.send(b'')
    sock.close()