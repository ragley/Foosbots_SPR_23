# Goal_Detection.ino

This script is separated into two tasks: CAN communication and goal sensing. These tasks are ran on two separate cores so that there is no delay in the goal sensor or in communications.

The goal sensor core is an infinite loop that monitors both goal sensors using the `Sensor_Debounce` object and then checks if either sensor is active. If a sensor is active, the goal count corresponding to that sensor is increased by one.

The communication core is also an infinite loop that checks if a CAN message has been received and sends the current score every 50ms. The current score has to be converted into bytes using long a long-byte union. The only message that is looked for is the **Goal Reset** message. If this message is received, both goal counts are set to zero. 

# Sensor_Debounce.h

This header file contains an object that can be used to do debounce on buttons or other sensors connected to a digital pin. This object does not use interrupts (adding interrupts could be a part of future development). Instead, it uses pin petting. 

---

## Constructor

The constructor takes in four values:
1. Pin number 
2. Pet Count
    - This is the amount of consecutive pets that would result in the sensor being active.
3. Pin Mode
    - Same as in `pinMode()`
4. Pressed value
    - The value that is considered pressed. Either **HIGH** or **LOW**

---

## `readSensor()`

This function is used to read if the sensor is active and if it has been read. This is used in functionality such as pressing a button and waiting for it to be released before it can be pressed again.

---

## `sensorActive()`

This function returns **true** if the sensor is active and **false** if the sensor is inactive.

---

## `sensorMonitor()`

This is the petting function. Whenever possible call this function. Whenever this function is called the **Pet Count** amount of times with the pin reading a consistent value, the internal `pressed` variable will be set as true or false, depending on the value on the pin.