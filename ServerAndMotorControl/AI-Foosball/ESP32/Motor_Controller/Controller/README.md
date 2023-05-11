# Controller.ino

The libraries used in **Controller.ino** are [CAN](https://www.arduino.cc/reference/en/libraries/can/) and [ESP-FlexyStepper](https://github.com/pkerspe/ESP-FlexyStepper).

**Controller.ino** works on every rod without modification due to the constants being selected by the board ID which comes from two pins soldered to ground or power (`board_ID = digitalRead(ID_2)*2 + digitalRead(ID_1)`). Each rod has a specific soldered ID which is commented in **Controller_Constants.h**.

Every arduino sketch always includes a `setup()` and `loop()`. All other functions are used inside those two functions. This specific sketch relies heavily on global constants and the following global variables:

- state
- board_ID
- receive_time
- send_time
- emergency_stop
- translation_measured
- rotation_measured
- translation_desired
- rotation_desired

The global constants are found in **Controller_Constants.h**.

**Note:** the majority of the bulk for this sketch is in serial print statements. These statements can be turned off by setting the three constants in different ways: `SERIAL_ON`, `SERIAL_MESSAGES` and `SERIAL_STATES`. If `SERIAL_ON = false` then no serial messages will be printed. If `SERIAL_ON = true` only a few essential messages will be printed. If `SERIAL_ON = true` and `SERIAL_MESSAGES = true` all CAN messages received and sent will be printed. If `SERIAL_ON = true` and `SERIAL_STATES = true` then all state transitions will be printed. If all are true, everything will be printed.

---

## `setup()`

In this setup, we set up the following:

1. Begin Serial communication at 115200bps
2. Set mode to input on both ID pins and the ENABLE pin
3. Connect RX and TX pins to the CAN object
4. Set mode of the "all good" led to output
5. Connect the pulse and direction pins to both stepper objects
6. Set the board ID based on digitally reading the ID pins as binary digits
7. Set the "steps per" values and the acceleration and deceleration of both stepper objects
8. Start can with mask based on board ID and with specific baud rate with `start_CAN()`
9. Set the start time and message time variables to the current time
10. Do the first state evaluation by calling `evaluateState()` (Note: this does not need to be done because it is the first thing done in the `loop()`)

Outside of `setup()`, two `Sensor_Debounce` objects are bound to each zeroing pin. This was to keep the objects global. For some reason, the pointer could not be declared null and then assigned in `setup()`.

**Note:** the stepper objects are ran on core 0 while everything else is ran on core 1.

---

## `loop()`

A sketches `loop()` function is an infinite loop. In this loop, four functions are called. 
1. `evaluateState()`
2. `CANReceiver()`
3. `setControl()`
4. `CANSender()`

However, `CANSender()` is only called at intervals based on the **COM Delay** variable.

---

## `evaluateState()`

In short, there are five major states: **Zero**, **Starting**, **Disabling**, **Running** and **Disabled**. However, there are many conditions in which **Disabled** can be triggered and released. Because of this, **Disabled** is split into the following five sub-states: **Short CAN Wait**, **Long CAN Wait**, **Emergency Stop**, **Stop Switch** and **Large CAN Delay**.

### ZERO

The zero state is the initial state of the system and is only entered on three conditions. 

The first is that the emergency stop switch has been released. This is because this switch bypasses the controller and disables the drivers. Because of this, the internal step count will get off and therefore needs to be reset.

The second is if a *zero* message is received during `CANReceiver()`.

The third is if the stepper objects ever go 1mm negative.

The state does three major things. The first is call `emergencyStop()` on both steppers. The second is disabling the stepper services if they are running. Finally, `zero()` is called. 

The only way to leave the state is if `zero()` returns true. Once true is returned, the speed of each stepper is reset, each stepper is started as a service again and due to zeroing taking a while, the `message_time` variable is set to the current time. The next state is always **Short CAN Wait** to prevent the motors from moving if there are no incoming messages.

### RUNNING

The **running** state is the primary state. This state is the only state to set the *all good led* to on. To leave the running state, an emergency stop command can be received, the emergency stop switch is pressed, or the received message time exceeds the timeout. If any off these occur, the next state is set as **disabling**.

### EMERGENCY_STOP

Emergency stop state is completely dependant on the global variable `emergency_stop`. This variable is set as true if the emergency stop message is received from the CAN bus in `CANReceiver()`. If any other message is received, `emergency_stop` is set to false. The state can only be left if `emergency_stop` is false. The next state is **Short CAN Wait** to prevent the motors from moving if there are no incoming messages.

Note: In the current system, the emergency stop command and state are not utilized. Instead a CAN timeout is used to stop the motors.

### STOP_SWITCH

As long as the pin on the enable line is grounded, the stop switch will be pressed. As long as the switch is pressed, the **stop switch** state will never be left.

Once the stop switched is released, the next state is set to **zero**.

### SHORT_CAN_WAIT

This state is the default disabled state. As long as no CAN message intended for the particular rod gets received, then this state will be active. If the stop switch is pressed or the emergency stop command is received, the next state will be **stop switch** or **emergency stop** respectively. If `millis() - message_time` is less than the timeout, the next state will be **starting**.

An issue arouse where the CAN bus object would stop it's connection with the CAN line. This issue occurred only when the emergency switch was pressed or the zero message was received. The root of this issue was never found. To fix it temporarily, the two states: **large CAN delay** and **long CAN wait** were added.

If the message time exceeds 10x the **MAX_COM_DELAY**, then the next state will be **large CAN delay**.

### LARGE_CAN_DELAY

This is an in-between state that restarts the CAN bus and then sets the next state as **long CAN wait**. This state is needed because constantly trying to restart the CAN module results in communication issues.

### LONG_CAN_WAIT

This state is identical to **short CAN wait**, but without the opportunity to go to **large CAN delay**.

---

The following two states are each evaluated independently from the others. This means that if the state is ever set to **starting** or **disabling** above, the following two states will process and set the next state without having to call `evaluateState()` again. Therefore, outside of `evaluateState()` the state should never be **starting** or **disabling**.

### STARTING

This state starts both stepper objects as services if they are not already started. Then the next state is set as **running**.

### DISABLING

This state calls `emergencyStop()` on both steppers and then determines which disabled state to go to.

---

## `zero()`

The zeroing function actually consists of two functions, `zeroRotation()` and `zeroTranslation()`. This is because while both are zeroing stepper motors, they each have different needs.

In `zero()`, `zeroTranslation()` is called first. If it fails, false is returned. If it succeeds, `zeroRotation()` is called. If it fails false is returned. If both succeed, true is returned.

**Note:** the steppers as a service are stopped in the **zero** state to allow for *ESP-Flexystepper's* `processMovement()` to work.

### `zeroRotation()`

To zero rotation, first the current location is set at home and stopped. Then the speed is set to the constant `HOME_SPEED_ROTATION` and the target set to three rotations in the direction from the direction array. 

Due to the zero button having a large range that it can be depressed, the motor must first rotate until the button is released and then fully rotate until the button is depressed again. This is done in to while loops.

The first uses `!rotation_stepper.processMovement() && rotational_zero.sensorActive()` and the second uses `!rotation_stepper.processMovement() && !rotational_zero.sensorActive()`.

To sense the button press, the `Sensor_Debounce` object associated with the zero pin is petted every millisecond. If the button is pressed then 10 pets will occur and then `sensorActive()` will return true or false if the button is released. If the emergency switch is ever pressed, false is returned. 

**Note:** the main need for the button debounce is that the button wires run next to the stepper motor wires which induce a current which will trigger a pressed if not debounced.

### `zeroTranslation()`

The translation zeroing works the same way as the rotation with three major exceptions. First, if the button is pressed, it will not try to release the button. The second is that once the button is pressed, the motor moves one millimeter away to relieve pressure on the rod's rubber stopper. The last one being, the target distance is set at the max displacement for the rod.

---

## `setControl()`

This function first checks if the state is the **running** state. If it is not the **running** state then nothing happens. But if it is  the **running** `translation_desired` is truncated to be between zero and the max translation . If the value of the current position is over 1mm negative, then the next state is set as the **zero** state. Otherwise, the direction from the direction array is multiplied by the desired positions and then set as the target for rotation and translation steppers.

---

## CAN communication
CAN (Controller Area Network) is a wire communication protocol originally designed for communication between many embedded micro controllers in the automotive industry. CAN allows for multiple devices in parallel to communicate to each other over two wires at a max speed of 1Mbps. This allows for cross communication between multiple devices without a need for a routing device and without the need for excess wires.

CAN functions on the idea that each device talks at the same time unless a more important device talks over it. To achieve this all devices send at the same time but if a device tries to send a **1** while another device is sending a **0**, the device sending **1** stops sending and instead listens. This means that a **ID** of `0b00` has a higher priority than an **ID** of `0b10`. With this in mind, all the **IDs** have been chosen following this priority hierarchy.

Messages from the *AI* will always have the highest priority so the first bit of every *AI* message *ID* will always be **0**. The second bit will indicate if the message is a **stop** command or not. Because **stop** commands are the most important, a stop command will be represented by a **0** as the second bit. The following four bits indicate which *controller* the message is from or going to.

The ID will only utilize the final 8-bits of the IDs 11-bits. The message data length is always 8 bytes. All bit data is using **Big-endian** meaning the bits on the left will be sent or received before the bits on the right.

**ID**: 8 bits

| **Bits**  | **0-1**                                                            | **2-3**                                                            | **4**            | **5**         | **6**         | **7**         |
| :-------: | :----------------------------------------------------------------: | :----------------------------------------------------------------: | :--------------: | :-----------: | :-----------: | :-----------: |
| `0`       | (`00`) message from *AI*         : (`01`) Zero command             | (`00`) stop\stopped message : (`01`) **NON** stop\stopped message  | **NOT** goal rod | **NOT** 2 rod | **NOT** 5 rod | **NOT** 3 rod |
| `1`       | (`11`) message from player poles : (`10`) message from controllers | (`11`) update goal counter  : (`10`) reset goal counter            | goal rod         | 2 rod         | 5 rod         | 3 rod         |

**NOTE**: Due to the PDP using high priority messages such as `0xA`, `0xC` and `0x2`, the stop command must be sent to all rods and not individual rods. Therefore, the only valid stop command is `0xF`

**DATA**: 8 bytes

| **Bytes**               | **0-3**           | **4-8**       |
| :---------------------: | :---------------: | :-----------: |
| ID bit 2:3 = 01         | Displacement Data | Angular Data  |
| ID bit 2:3 = 11         | Player goal count | AI goal count |


|    ID      | Description                                                 |
| :--------: | :---------------------------------------------------------: |
|`0b000XXXX1`| Message from *AI* to *controller 3 rod* **1**               |
|`0b000XXX1X`| Message from *AI* to *controller 5 rod* **2**               |
|`0b000XX1XX`| Message from *AI* to *controller 2 rod* **3**               |
|`0b000X1XXX`| Message from *AI* to *controller goal rod* **4**            |
|`0b010XXXX1`| Zero Message to *controller 3 rod* **1**                    |
|`0b010XXX1X`| Zero Message to *controller 5 rod* **2**                    |
|`0b010XX1XX`| Zero Message to *controller 2 rod* **3**                    |
|`0b010X1XXX`| Zero Message to *controller goal rod* **4**                 |
|`0b100XXXX1`| Message from *controller 3 rod* to *AI*                     |
|`0b100XXX1X`| Message from *controller 5 rod* to *AI*                     |
|`0b100XX1XX`| Message from *controller 2 rod* to *AI*                     |
|`0b100X1XXX`| Message from *controller goal rod* **4** to *AI*            |
|`0b110XXXX1`| Message from *player 3 rod* **1** to *AI*                   |
|`0b110XXX1X`| Message from *player 5 rod* **2** to *AI*                   | 
|`0b110XX1XX`| Message from *player 2 rod* **3** to *AI*                   |
|`0b110X1XXX`| Message from *player goal rod* **4** to *AI*                |
|`0b1011XXXX`| Message from *goal controller* to *AI* to update goal count |
|`0b0010XXXX`| Message from *AI* to *goal controller* to reset goal count  |


|    ID      | Displacement Data Length | Rotational Data Length | Description                            |
| :--------: | :----------------------: | :---------------------:| :------------------------------------: |
|`0b0X00XXXX`| **X**                    | **X**                  | **Stop** command from *AI*             |
|`0b1000XXXX`| **X**                    | **X**                  | **Stopped** message from *controller*  |
|`0b1100XXXX`| **X**                    | **X**                  | **Stopped** message from *player*      |
|`0b0001XXXX`| **4 Bytes**              | **4 Bytes**            | **Move** command from *AI*             |
|`0b1001XXXX`| **4 Bytes**              | **4 Bytes**            | **Location** message from *controller* |
|`0b1101XXXX`| **4 Bytes**              | **4 Bytes**            | **Location** message from *player*     |

### `CANSender()`

This function takes the current location of the stepper motors (rotations or mm), converts rotations into degrees . Then the value is written into the float part of a byte-float union. Then the bytes from the byte-float union are sent with a running or not running ID depending on state. Everything is sent **Big-Endian**.

### `CANReceiver()`

This function checks to see if a message is in the buffer and then processes the message. If there is no new message, nothing happens. 

The first thing done is the `receive_time` variable is set to the current time. Then the packet ID is checked to see if the message is an emergency stop, zero or movement command. If the message is either of the first two, `evaluateState()` is called to enact the message command after the pertinent variables are changed.

If the message is a movement command, the message is read into a bytes of a byte-float union using **Big-Endian**. Then the float is read and assigned to the two **desired** variables to be used in `setControl()`.

# Controller_Constants.h

This header file contains all constants for **Controller.ino**. These constants are all global to allow for easy changes to the header file to change **Controller.ino**.

All constants are explicitly named or labeled. Each constant was determined through calculations or extensive testing. The constants should only be changed if the mechanical components of the system change, the motors are replaced or the PCB is replaced.

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
