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

    def request_rods(self):
        rods = {
            "robot_goal_rod_displacement": 0,
            "robot_goal_rod_angle": 0,
            "robot_2_rod_displacement": 0,
            "robot_2_rod_angle": 0,
            "robot_5_rod_displacement": 0,
            "robot_5_rod_angle": 0,
            "robot_3_rod_displacement": 0,
            "robot_3_rod_angle": 0,
        }
        self.data.update(rods)

    def update_message_random_values(self):
        random = np.random.randint(1,101,8)
        rods = {
            "robot_goal_rod_displacement": int(random[0]),
            "robot_goal_rod_angle": int(random[1]),
            "robot_2_rod_displacement": int(random[2]),
            "robot_2_rod_angle": int(random[3]),
            "robot_5_rod_displacement": int(random[4]),
            "robot_5_rod_angle": int(random[5]),
            "robot_3_rod_displacement": int(random[6]),
            "robot_3_rod_angle": int(random[7]),
        }
        self.data.update(rods)   


#test = Message("TEST")
#test.update_message_random_values()
#print(test.encode_to_send())