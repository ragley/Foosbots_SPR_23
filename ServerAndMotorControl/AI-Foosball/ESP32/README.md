# Running ESP32 code
## Setting up Arduino IDE
This is written using Arduino 1.8.13 as a reference.

Download the Arduino IDE by clicking [here](https://www.arduino.cc/en/software). 

Once the installer is downloaded, launch it and follow the prompts for instillation. Open application once it is installed.

## Install Board
First thing to do is add the **ESP32 Dev Module** to your boards. To do this, first click `File->Preferences` then enter the URL:`https://dl.espressif.com/dl/package_esp32_index.json,  https://arduino.esp8266.com/stable/package_esp8266com_index.json` into the box labeled: Additional Boards Manager URLs`. Click OK. Next click on `Tools->Board->Boards Manager`. Then search for **esp32** in the *Filter your search* bar. **esp32** by **Espressif Systems** should appear. Select the newest version and click the install button. Then go back to `Tools->Board->ESP32 Arduino` and select the **ESP32 Dev Module**.

The essential settings under Tools should be:
- Board: "ESP32 Dev Module"
- Upload Speed: "921600"

All other setting should be correct.

Once the board is plugged in, make sure to select the correct port. You can check the port by using Windows Device Manager.
## Install Libraries
The Spring 2022 version of the controller code uses [CAN](https://www.arduino.cc/reference/en/libraries/can/) and [ESP-FlexyStepper](https://github.com/pkerspe/ESP-FlexyStepper).

Both libraries can be installed through Arduino IDE. To do this, navigate to `Tools->Manage Libraries`. Then search for **ESP-FlexyStepper** by **Paul Kerspe** and install newest version (current version 1.4.5). Do the same for **CAN**. Because "can" is such a pervasive word, you will have to scroll down quite a bit. Make sure to select the library only called **CAN** by **Sandeep Mistry** as there are many CAN libraries (current version 0.3.1).