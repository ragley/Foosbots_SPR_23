import json 
import numpy as np

class Message:

    def __init__(self, action = ""):
        self.data = {}
        self.data["action"] = action

    
    def encode_to_send(self, length):
        json_data = json.dumps(self.data)
        json_bytes = bytes(json_data,encoding="utf-8")
        if length:           
            return len(json_bytes).to_bytes(4, byteorder="big") + json_bytes
        else:
            return json_bytes


    def decode_from_receive(self, convert):
        self.data = json.loads(convert)
