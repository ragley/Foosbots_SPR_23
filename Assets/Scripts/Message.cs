using System.Collections;
using System.Collections.Generic;
using UnityEngine;
//using System.Text.Json;
using System;
using Newtonsoft.Json;
using System.Text;

public class Message
{
    public Dictionary<string, object> data;

    public Message(string action = "")
    {
        data = new Dictionary<string, object>();
        data["action"] = action;
    }

    public byte[] EncodeToSend(bool length)
    {
        string json_data = JsonConvert.SerializeObject(data);
        byte[] json_bytes = Encoding.UTF8.GetBytes(json_data);

        if (length)
        {
            byte[] length_bytes = BitConverter.GetBytes(json_bytes.Length);
            if (BitConverter.IsLittleEndian)
            {
                Array.Reverse(length_bytes);
            }
            return ConcatBytes(length_bytes, json_bytes);
        }
        else
        {
            return json_bytes;
        }
    }

    public void DecodeFromReceive(string convert)
    {
        data = JsonConvert.DeserializeObject<Dictionary<string, object>>(convert);
    }

    public void RequestRods()
    {
        Dictionary<string, object> rods = new Dictionary<string, object>()
        {
            { "robot_goal_rod_displacement", 0 },
            { "robot_goal_rod_angle", 0 },
            { "robot_2_rod_displacement", 0 },
            { "robot_2_rod_angle", 0 },
            { "robot_5_rod_displacement", 0 },
            { "robot_5_rod_angle", 0 },
            { "robot_3_rod_displacement", 0 },
            { "robot_3_rod_angle", 0 },
        };
        UpdateRods(rods);
    }

    // public void UpdateMessageRandomValues()
    // {
    //     int[] random = new int[8];
    //     Random rand = new Random();
    //     for (int i = 0; i < 8; i++)
    //     {
    //         random[i] = rand.Next(1, 101);
    //     }
    //     Dictionary<string, object> rods = new Dictionary<string, object>()
    //     {
    //         { "robot_goal_rod_displacement", random[0] },
    //         { "robot_goal_rod_angle", random[1] },
    //         { "robot_2_rod_displacement", random[2] },
    //         { "robot_2_rod_angle", random[3] },
    //         { "robot_5_rod_displacement", random[4] },
    //         { "robot_5_rod_angle", random[5] },
    //         { "robot_3_rod_displacement", random[6] },
    //         { "robot_3_rod_angle", random[7] },
    //     };
    //     UpdateRods(rods);
    // }

    public void UpdateRods(Dictionary<string, object> rods)
    {
        foreach (var item in rods)
        {
            data[item.Key] = item.Value;
        }
    }

    private byte[] ConcatBytes(byte[] a, byte[] b)
    {
        byte[] result = new byte[a.Length + b.Length];
        a.CopyTo(result, 0);
        b.CopyTo(result, a.Length);
        return result;
    }
}