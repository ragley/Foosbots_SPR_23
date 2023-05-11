#include <CAN.h>
#include <SpeedyStepper.h>
#include "Arduino_Motor_Control_Constants.h"
#include "Sensor_Debounce.h"







unsigned char len = 0;
unsigned char buf[8];






/*If device memory is too low for program use System.println(F("TEXT HERE"));
E-STOP is now hardware based, will cut connection to ground if pressed, This does not work if a serial monitor cord is plugged into arduino and computer, 
that will provide ground for all of the boards and allow them to cont. running*/






//States
#define ZERO 0
#define RUNNING 1
#define SHORT_CAN_WAIT 2
#define EMERGENCY_STOP 3
#define DISABLING 4
#define STARTING 5
#define STOP_SWITCH 6
#define LARGE_CAN_DELAY 7
#define LONG_CAN_WAIT 8

const bool SERIAL_ON = true;
const bool SERIAL_MESSAGES = true;
const bool SERIAL_STATES = true;


SpeedyStepper translation_stepper;
SpeedyStepper rotation_stepper;
Sensor_Debounce rotational_zero = Sensor_Debounce(ROTATION_DRIVER_ZERO, SENSOR_DEBOUNCE, INPUT_PULLUP, LOW);
Sensor_Debounce translational_zero = Sensor_Debounce(TRANSLATION_DRIVER_ZERO, SENSOR_DEBOUNCE, INPUT_PULLUP, LOW);

bool zero();
void setControl();
void CANSender();
void CANReceiver();
void evaluateState();

int board_ID = 0;         //Change this for different rods, will change direction for zeroing based on this 

int state = ZERO;

double translation_measured = 0;
double rotation_measured = 0;
double translation_desired = 0;
double rotation_desired = 0;

unsigned long receive_time = 0;
double send_time = 0;
int received_zero = false;
typedef union {
  float value;
  byte bytes[sizeof(float)];
} FLOAT_BYTE_UNION;

void setup() {

  if (SERIAL_ON) {
    Serial.begin(115200);
  }
 
  CAN.setPins(SPI_CS_PIN); //set to pin 9 for CAN shields, could be changed through physically altering CAN shields if needed

  if(!start_CAN(1000E3)){ //starting CAN
    Serial.println("STARTING CAN FAILED");
  while(1);  
  }
  

  SERIAL_PORT_MONITOR.println(F("CAN init ok!"));
  
  pinMode(ENABLE, INPUT);
  pinMode(ROTATION_DRIVER_ZERO,INPUT); //for some reason this pin had to be declared as input and we had to add voltage to set this as high when rot switch is not pressed
                                      //Translation zero did not have this issue so we left it alone
  
  //declaring motors and wires used for them
  translation_stepper.connectToPins(TRANSLATION_DRIVER_PULSE, TRANSLATION_DRIVER_DIR);
  rotation_stepper.connectToPins(ROTATION_DRIVER_PULSE, ROTATION_DRIVER_DIR);


  if (SERIAL_ON) {
    Serial.print(F("Board ID: "));
    Serial.println(board_ID);
  }
  //sets how many motor steps per millimeter of movement based on board ID
  translation_stepper.setStepsPerMillimeter(STEP_PULSE_TRANSLATION_CONVERSION[board_ID]);
  translation_stepper.setAccelerationInMillimetersPerSecondPerSecond(MAX_ACCELERATION_TRANSLATION);

  rotation_stepper.setStepsPerRevolution(STEP_PULSE_ROTATION_CONVERSION);
  rotation_stepper.setAccelerationInRevolutionsPerSecondPerSecond(MAX_ACCELERATION_ROTATION);


  delay(3000);
  receive_time = millis();
  send_time = millis();
  evaluateState();
}

void loop() {

  
  evaluateState();
  CANReceiver();
  setControl();
  if (millis() - send_time > COM_DELAY) {
    send_time = millis();
    CANSender();  
  }
}

bool start_CAN(int BAUD) {
  if (SERIAL_ON) Serial.println(F("Starting CAN"));
  while (!CAN.begin(1000E3)) {
    if (SERIAL_ON) Serial.println(F("failed!"));
    digitalWrite(ALL_GOOD_LED, LOW);
    delay(1000);
    return false;
  }
  //Starts filter for each board
  while (!CAN.filter(0b1 << board_ID, 0b10100000 + (1 << board_ID) + 0x1fffff00)) {   

    if (SERIAL_ON) Serial.println(F("filter failed!"));
    digitalWrite(ALL_GOOD_LED, LOW);
    delay(1000);
    return false;
  }
  if (SERIAL_ON) Serial.println(F("Starting CAN success"));
  return true;
}

bool zero() {
  //need to change ALL_Good_LED to Arduino LED in constants
  digitalWrite(ALL_GOOD_LED, LOW);
  if (SERIAL_ON) Serial.print(F("Zeroing Translation"));
  bool success_translation = zeroTranslation();
  if (!success_translation) {
    //checks if enable wire is plugged into board
    if (SERIAL_ON && digitalRead(ENABLE) == LOW) Serial.println(F(" Failed due to ENABLE being low | 0"));
    else if (SERIAL_ON) Serial.println(F(" Failed"));
    return success_translation;
  }
  if (SERIAL_ON) {
    Serial.println(F(" Success"));
    Serial.println(F("Zeroing Rotation"));
  }
  Serial.println("Zero rot being called");
  delay(1000);
  bool success_rotation = zeroRotation();
  if (!success_rotation) {
    if (SERIAL_ON && digitalRead(ENABLE) == LOW) Serial.println(F(" Failed due to ENABLE being low | 0"));
    else if (SERIAL_ON) Serial.println(F(" Failed"));
    return success_rotation;
  }
  bool success = success_rotation && success_translation;
  if (SERIAL_ON) Serial.println(F(" Success"));
  digitalWrite(ALL_GOOD_LED, HIGH);
  return success;
}

bool zeroRotation() {
  
  int timer_serial = 0;
  int timer_bounce = 0;
  int count = 0;
  //lower rotation speed for homing
  rotation_stepper.setSpeedInRevolutionsPerSecond(HOME_SPEED_ROTATION);
  //Moves in direction if switch is NOT pressed
  while (digitalRead(ROTATION_DRIVER_ZERO) == HIGH) {
    Serial.println("FIRST LOOP high ROT");     
    rotation_stepper.moveRelativeInSteps(-5);  
                                               
      

  }
  //switch is pressed, motor will move until it is NOT pressed
  while (digitalRead(ROTATION_DRIVER_ZERO) == LOW) {

    rotation_stepper.moveRelativeInSteps(5);
    Serial.println("ROT DRIVE zero LOW");
    Serial.print("switch: ");
    Serial.println(digitalRead(ROTATION_DRIVER_ZERO));
    Serial.print("SENSOR: ");
    Serial.println(rotational_zero.sensorActive());

  }
  //sets zero location when switch is NOT pressed
  rotation_stepper.setCurrentPositionInSteps(0.0);
  while (digitalRead(ROTATION_DRIVER_ZERO) == HIGH) {

    rotation_stepper.moveRelativeInSteps(-5);
  }
  //sets zero location when switch is pressed
    rotation_stepper.setCurrentPositionInSteps(0.0);
    delay(500);
  Serial.print("Rotational Sensor Active Result is: ");
  Serial.println(rotational_zero.sensorActive());

  //checks if switch is pressed
  if (digitalRead(ROTATION_DRIVER_ZERO) == LOW) {
    return true;
} else {
   Serial.println(F("Failed due to translation_zero.sensorActive() being false | 0 "));
   return false;
  }
    








}

bool zeroTranslation() {
  for (int i = 0; i < SENSOR_DEBOUNCE * 2; i++) translational_zero.sensorMonitor();
  int timer_serial = millis();
  int timer_bounce = millis();
  translation_stepper.setSpeedInMillimetersPerSecond(MAX_SPEED_TRANSLATION);
 translation_stepper.setAccelerationInMillimetersPerSecondPerSecond(MAX_ACCELERATION_TRANSLATION);
  while (digitalRead(TRANSLATION_DRIVER_ZERO) == HIGH) {
    
    translation_stepper.moveRelativeInMillimeters(DIRECTIONS[board_ID][TRANSLATION] * 1);  //MOVES towards switch, direction based on board ID

  }

  while (digitalRead(TRANSLATION_DRIVER_ZERO) == LOW) {  //first click of switch, should stop then reverse
    
    delay(750);
    Serial.println("I am now moving toward the motor and away from switch in 5mm steps");



    translation_stepper.moveRelativeInMillimeters(DIRECTIONS[board_ID][TRANSLATION] * -5);  //moves in 5 mm steps away from motor
  }

  while (digitalRead(TRANSLATION_DRIVER_ZERO) == HIGH) {
      
    translation_stepper.moveRelativeInMillimeters(DIRECTIONS[board_ID][TRANSLATION] * 1);    //MOVES
    Serial.println("I am now moving back towards switch to finish zero");
            


 }
//sets zero location for translation
  translation_stepper.setCurrentPositionInMillimeters(0.00);

  delay(500);
  Serial.print(F("Transaltional Sensor Active Result is: "));
  Serial.println(translational_zero.sensorActive());

  if (digitalRead(TRANSLATION_DRIVER_ZERO) == LOW || translational_zero.sensorActive()) {
     translation_stepper.moveRelativeInMillimeters(DIRECTIONS[board_ID][TRANSLATION] * -1 * MAX_TRANSLATIONS[board_ID]);
     delay(3000);
    translation_stepper.setCurrentPositionInMillimeters(0.00);

    delay(1000);
    Serial.println(F("About to move to middle"));
    translation_stepper.moveRelativeInMillimeters(DIRECTIONS[board_ID][TRANSLATION] * (MAX_TRANSLATIONS[board_ID] / 2));
    Serial.println(F("Moved to middle"));
    Serial.println(translation_stepper.getCurrentPositionInMillimeters());
    return true;
  } else {
   Serial.println(F("Failed due to translation_zero.sensorActive() being false | 0 "));
    return false;
 }

}

void setControl() {
  if (state == RUNNING) {
    if (translation_desired > MAX_TRANSLATIONS[board_ID]) translation_desired = MAX_TRANSLATIONS[board_ID];
    if (translation_desired < 0) translation_desired = 1;
    if (translation_stepper.getCurrentPositionInMillimeters() * DIRECTIONS[board_ID][TRANSLATION] < -1) {
      if (SERIAL_ON && SERIAL_STATES) {
        Serial.print(F("Went Negative: "));
        Serial.println(translation_stepper.getCurrentPositionInMillimeters() * DIRECTIONS[board_ID][TRANSLATION]);
      }
      state = ZERO;
    } else {
       //sets target rot and translation movement 
     translation_stepper.setupMoveInMillimeters(DIRECTIONS[board_ID][TRANSLATION] * translation_desired);
     rotation_stepper.setupMoveInRevolutions(DIRECTIONS[board_ID][ROTATION] * rotation_desired);
      //processes movement that is targeted in code above, this makes sure that both rotation and linear commands are completed
     while((!translation_stepper.motionComplete())||(!rotation_stepper.motionComplete())){
          translation_stepper.processMovement();
          rotation_stepper.processMovement();

     }
         
    }
  }
}

void CANSender() {
  FLOAT_BYTE_UNION translation_measured_f;
  FLOAT_BYTE_UNION rotation_measured_f;
  //sends current translation and rot pos. 
  translation_measured_f.value = (float)(DIRECTIONS[board_ID][TRANSLATION] * translation_stepper.getCurrentPositionInMillimeters());
  rotation_measured_f.value = (float)(DIRECTIONS[board_ID][ROTATION]*rotation_stepper.getCurrentPositionInRevolutions()*DEGREES_PER_REVOLUTION);
  if (SERIAL_ON && SERIAL_MESSAGES) Serial.print(F("Sent: (packet: 0b"));
  if (state == RUNNING) {
    CAN.beginPacket(0b10010000 + (1 << board_ID));
    for (int i = sizeof(float) - 1; i >= 0; i--) {
      CAN.write(translation_measured_f.bytes[i]);
    }
    for (int i = sizeof(float) - 1; i >= 0; i--) {
      CAN.write(rotation_measured_f.bytes[i]);
    }
    CAN.endPacket();
    if (SERIAL_ON && SERIAL_MESSAGES) Serial.print(0b10010000 + (1 << board_ID), BIN);
  } else if (state == SHORT_CAN_WAIT || state == EMERGENCY_STOP || state == STOP_SWITCH || state == ZERO) {
    CAN.beginPacket((1 << board_ID) + 0b10000000);
    for (int i = sizeof(float) - 1; i >= 0; i--) {
      CAN.write(translation_measured_f.bytes[i]);
    }
    for (int i = sizeof(float) - 1; i >= 0; i--) {
      CAN.write(rotation_measured_f.bytes[i]);
    }
    CAN.endPacket();
    if (SERIAL_ON && SERIAL_MESSAGES) Serial.print((1 << board_ID) + 0b10000000, BIN);
  } else {
    if (SERIAL_ON && SERIAL_MESSAGES) Serial.println(F("NON PRINT STATE"));
  }
  if (SERIAL_ON && SERIAL_MESSAGES) {
    Serial.print(F(" ROTATION: "));
    Serial.print(rotation_measured_f.value);
    Serial.print(F(" Translation: "));
    Serial.println(translation_measured_f.value);
  }
}

void CANReceiver() {
  if (CAN.parsePacket()) {
    receive_time = millis();
    FLOAT_BYTE_UNION translation_desired_f;
    FLOAT_BYTE_UNION rotation_desired_f;
    if (SERIAL_ON && SERIAL_MESSAGES) {
      Serial.print(F("Recv: (packet: 0b"));
      Serial.print(CAN.packetId(), BIN);
      Serial.print(" ");
    }

    //EMERGENCY STOP
    if ((CAN.packetId() & (0x1fffff00 + 0b11111111)) == 0b00001111) {
      if (SERIAL_ON) Serial.println(F("E STOP message received"));
      emergency_stop = true;
      evaluateState();
      return;
    } else {
      emergency_stop = false;
    }

    //zero
    if ((CAN.packetId() & (0x1fffff00 + 0b11100000)) == 0b01000000) {
      if (SERIAL_ON) Serial.println(F("Zero message received"));
      state = ZERO;
      if (received_zero) state = LARGE_CAN_DELAY;
      received_zero = true;
      evaluateState();
      return;
    }

    if ((CAN.packetId() & (0x1fffff00 + 0b11110000)) == 0b10000) {
      received_zero = false;
      int rotation_index = sizeof(float) - 1;
      int translation_index = sizeof(float) - 1;
      bool data_received = false;
      while (CAN.available()) {
        data_received = true;
        if (translation_index >= 0) {
          translation_desired_f.bytes[translation_index--] = (byte)CAN.read();
        } else if (rotation_index >= 0) {
          rotation_desired_f.bytes[rotation_index--] = (byte)CAN.read();
        } else {
          if (SERIAL_ON && SERIAL_MESSAGES) Serial.println(F("DATA DROPPED)"));
          return;
        }
      }
      if (data_received) {
        if (!((translation_index < 0) && (rotation_index < 0))) {
          if (SERIAL_ON && SERIAL_MESSAGES) Serial.println(F("LESS THAN 8 BYTES RECEIVED)"));
          return;
        }
        rotation_desired = (double)rotation_desired_f.value / DEGREES_PER_REVOLUTION;
        translation_desired = (double)translation_desired_f.value;
        if (SERIAL_ON && SERIAL_MESSAGES) {
          Serial.print(" Rotation: ");
          Serial.print(rotation_desired * DEGREES_PER_REVOLUTION);
          Serial.print(" Translation: ");
          Serial.print(translation_desired);
        }
      }
      if (SERIAL_ON && SERIAL_MESSAGES) Serial.println(F(")"));
    }
  }
}

void evaluateState() {
  if (SERIAL_ON && SERIAL_STATES) {
      
    Serial.print(F("State: "));
    Serial.println(state);
  }
  if (state == ZERO) {
    digitalWrite(ALL_GOOD_LED, LOW);
    Serial.println("CALLED AGAIN ZERO IS");
   
    if (zero()) {
      translation_stepper.setSpeedInMillimetersPerSecond(MAX_SPEED_TRANSLATION);
      rotation_stepper.setSpeedInRevolutionsPerSecond(MAX_SPEED_ROTATION);
      receive_time = millis();
      state = SHORT_CAN_WAIT;
    }
  } else if (state == RUNNING) {
    digitalWrite(ALL_GOOD_LED, HIGH);
    if (emergency_stop || (millis() - receive_time > MAX_COM_DELAY) || (digitalRead(ENABLE) == LOW)) {
      state = DISABLING;
      if (SERIAL_ON && SERIAL_STATES && emergency_stop) Serial.println(F("Emergency Stop Command Active"));
      if (SERIAL_ON && SERIAL_STATES && ((millis() - receive_time) > MAX_COM_DELAY)) Serial.println(F("COM timeout"));
      if (SERIAL_ON && SERIAL_STATES && (digitalRead(ENABLE) == LOW)) Serial.println(F("Emergency Stop Button Pressed"));
    }
  } else if (state == SHORT_CAN_WAIT) {
    digitalWrite(ALL_GOOD_LED, LOW);
    if (emergency_stop) state = EMERGENCY_STOP;
    else if (digitalRead(ENABLE) == LOW) state = STOP_SWITCH;
    else if ((millis() - receive_time) < MAX_COM_DELAY) {
      state = STARTING;
      if (SERIAL_ON && SERIAL_STATES) Serial.println(F("COM timeout not exceeded"));
    }
    if ((millis() - receive_time) > MAX_COM_DELAY * 10) state = LARGE_CAN_DELAY;
    if (SERIAL_ON && SERIAL_STATES && (digitalRead(ENABLE) == LOW)) Serial.println(F("Emergency Stop Button Pressed"));
    if (SERIAL_ON && SERIAL_STATES && ((millis() - receive_time) > MAX_COM_DELAY)) {
      Serial.print(F("Timeout of: "));
      Serial.print(millis() - receive_time);
      Serial.println(F("ms"));
    }
  } else if (state == EMERGENCY_STOP) {
    digitalWrite(ALL_GOOD_LED, LOW);
    if (!emergency_stop) {
      if (SERIAL_ON && SERIAL_STATES) Serial.println(F("Emergency Stop Command Inactive"));
      state = SHORT_CAN_WAIT;
    }
  } else if (state == STOP_SWITCH) {
    digitalWrite(ALL_GOOD_LED, LOW);
    if (digitalRead(ENABLE) == HIGH) {
      if (SERIAL_ON && SERIAL_STATES) Serial.println(F("Emergency Stop Button Released"));
      state = ZERO;
    }
  } else if (state == LARGE_CAN_DELAY) {
    if (SERIAL_ON && SERIAL_STATES) Serial.println(F("LARGE CAN TIMEOUT"));
    start_CAN(BAUD_RATE);
    state = LONG_CAN_WAIT;  
  } else if (state == LONG_CAN_WAIT) {
    digitalWrite(ALL_GOOD_LED, LOW);
    if (emergency_stop) state = EMERGENCY_STOP;
    else if (digitalRead(ENABLE) == LOW) state = STOP_SWITCH;
    else if ((millis() - receive_time) < MAX_COM_DELAY) {
      if (SERIAL_ON && SERIAL_STATES) Serial.println(F("COM timeout not exceeded"));
      state = STARTING;
    }
    if (SERIAL_ON && SERIAL_STATES && (digitalRead(ENABLE) == LOW)) Serial.println(F("Emergency Stop Button Pressed"));
  }

  if (state == STARTING) {
    if (SERIAL_ON) Serial.println(F("STARTING"));
    if (emergency_stop) {
      if (SERIAL_ON && SERIAL_STATES) Serial.println(F("Emergency Stop Command Active"));
      state = DISABLING;
    } else {
      
      state = RUNNING;
      if (SERIAL_ON) Serial.println(F("STARTED"));
    }
  }
  if (state == DISABLING) {
    if (SERIAL_ON) Serial.println(F("DISABLING"));
  
    if (emergency_stop) {
      //Not really used anymore, could be implemented and would be good, we just ran out of time
      if (SERIAL_ON && SERIAL_STATES) Serial.println(F("Emergency Stop Command Active"));
      state = EMERGENCY_STOP;
    } else if (digitalRead(ENABLE) == LOW) {
      if (SERIAL_ON && SERIAL_STATES) Serial.println(F("Emergency Stop Button Pressed"));
      state = STOP_SWITCH;
    } else state = SHORT_CAN_WAIT;
  }
}
//This code is messy, and hard to understand at points, but by golly does it have character. Treat her well. 

/*⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⢀⡠⠴⠒⠒⠲⠤⠤⣀⡀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀
⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⡴⠋⠀⠀⠀⠀⠠⢚⣂⡀⠈⠲⣄⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⣀⣀⡀⠀⠀
⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⡎⡴⠆⠀⠀⠀⠀⠀⢎⠐⢟⡇⠀⠈⢣⣠⠞⠉⠉⠑⢄⠀⠀⣰⠋⡯⠗⣚⣉⣓⡄
⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⣸⢠⢞⠉⡆⠀⠀⠀⠀⠀⠓⠋⠀⠀⠀⠀⢿⠀⠀⠀⠀⠈⢧⠀⢹⣠⠕⠘⢧⠀⠀⠀
⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⡇⠘⠮⠔⠁⠀⠀⠀⠀⢀⠀⠀⠀⠀⠀⠀⠸⡀⠀⠀⠀⠀⠈⣇⠀⢳⠀⠀⠘⡆⠀⠀
⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⡴⠋⠉⠓⠦⣧⠀⠀⠀⠀⢦⠤⠤⠖⠋⠇⠀⠀⠀⠀⠀⠀⡇⠀⠀⠀⠀⠀⠸⡄⠈⡇⠀⠀⢹⡀⠀
⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⢸⠁⠀⠀⠀⠀⠙⡆⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⡇⠀⠈⣆⠀⠀⠀⢱⠀⡇⠀⠀⠀⡇⠀
⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⣼⠀⠀⠀⠀⠀⠀⠘⢆⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⡰⠁⠀⠀⠸⡄⠀⠀⠀⠳⠃⠀⠀⠀⡇⠀
⠀⠀⠀⠀⠀⢠⢏⠉⢳⡀⠀⠀⢹⠀⠀⠀⠀⢠⠀⠀⠀⠑⠤⣄⣀⡀⠀⠀⠀⠀⠀⣀⡤⠚⠀⠀⠀⠀⠀⢸⢢⡀⠀⠀⠀⠀⠀⢰⠁⠀
⠀⠀⣀⣤⡞⠓⠉⠁⠀⢳⠀⠀⢸⠀⠀⠀⠀⢸⡆⠀⠀⠀⠀⠀⠀⠉⠉⠉⠉⠉⠉⠁⠀⠀⠀⠀⠀⠀⠀⢸⠀⠙⠦⣤⣀⣀⡤⠃⠀⠀
⠀⣰⠗⠒⣚⠀⢀⡤⠚⠉⢳⠀⠈⡇⠀⠀⠀⢸⡧⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⢸⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀
⠸⠵⡾⠋⠉⠉⡏⠀⠀⠀⠈⠣⣀⣳⠀⠀⠀⢸⡇⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⢸⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀
⠀⠀⠀⠀⠀⠀⠹⡄⠀⠀⠀⠀⠀⠀⠀⠀⠀⡼⡇⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⣸⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀
⠀⠀⠀⠀⠀⠀⠀⠳⡄⠀⠀⠀⠀⠀⠀⠀⡰⠁⡇⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⣿⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀
⠀⠀⠀⠀⠀⠀⠀⠀⠈⠓⠲⠤⠤⠤⠴⠚⠁⠀⡇⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⣿⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀
⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠇⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠘⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀
*/