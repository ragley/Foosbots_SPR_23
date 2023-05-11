#include <CAN.h>
#include <ESP_FlexyStepper.h>
#include "Constants.h"

ESP_FlexyStepper traMotor;
ESP_FlexyStepper rotMotor;

int boardID = -1;
int delayTime = 1200;

void setup() {
  Serial.begin(constant_baudRate);
  Serial.println("Starting");
  pinMode(constant_eStopPin, INPUT);
  pinMode(constant_id1Pin, INPUT);
  pinMode(constant_id2Pin, INPUT);
  pinMode(constant_traZeroPin, INPUT);
  pinMode(constant_rotZeroPin, INPUT);
  delay(1000);
  boardID = (digitalRead(constant_id2Pin) * 2) + digitalRead(constant_id1Pin);
  Serial.println("Board ID:");
  Serial.println(boardID);
  delay(10);
  Serial.println("Is rod disabled?");
  if (constant_disableRod[boardID]) {
    Serial.println("True");
    exit(0);
  }
  Serial.println("False");
  
  traMotor.connectToPins(constant_traPulsePin, constant_traDirPin);
  rotMotor.connectToPins(constant_rotPulsePin, constant_rotDirPin);

  traMotor.setStepsPerMillimeter(constant_traPulse);
  traMotor.startAsService(constant_stepperCore);
  traMotor.setAccelerationInMillimetersPerSecondPerSecond(constant_traMaxAccel);
  traMotor.setDecelerationInMillimetersPerSecondPerSecond(constant_traMaxAccel);
  
  rotMotor.setStepsPerRevolution(constant_rotPulse);
  rotMotor.startAsService(constant_stepperCore);
  rotMotor.setAccelerationInRevolutionsPerSecondPerSecond(constant_rotMaxAccel);
  rotMotor.setDecelerationInRevolutionsPerSecondPerSecond(constant_rotMaxAccel);
  rotMotor.setSpeedInRevolutionsPerSecond(constant_rotSpeed);
  
  zeroTranslation();
}

void loop() {
  if (digitalRead(constant_eStopPin) == LOW) {
    exit(0);
  }
  if (digitalRead(constant_traZeroPin) == HIGH && digitalRead(constant_eStopPin) == HIGH) {
    while (digitalRead(constant_traZeroPin) == HIGH && digitalRead(constant_eStopPin) == HIGH) {
      rotatePositive();
      translateNegative();
      delay(delayTime);
      rotatePositive();
      translatePositive();
      delay(delayTime);
      rotatePositive();
      translatePositive();
      delay(delayTime);
      rotatePositive();
      translateNegative();
      delay(delayTime);
    }
    traMotor.emergencyStop(false);
    rotMotor.emergencyStop(false);
  }
}

void zeroTranslation() {
  delay(10);
  Serial.println("Zero Translation Started");
  measureCurrentPoint();
  traMotor.setSpeedInMillimetersPerSecond(constant_traZeroingSpeed);
  traMotor.setCurrentPositionAsHomeAndStop();
  if (digitalRead(constant_traZeroPin) == HIGH && digitalRead(constant_eStopPin) == HIGH) {
    traMotor.setTargetPositionInMillimeters(constant_traDirs[boardID] * 1000);
    while (digitalRead(constant_traZeroPin) == HIGH && digitalRead(constant_eStopPin) == HIGH) {
    }
    traMotor.emergencyStop(false);
    measureCurrentPoint();
  }
  traMotor.setCurrentPositionAsHomeAndStop();
  Serial.println("Zero Translation Success");
  moveToMidpoint();
  delay(10);
}

void moveToMidpoint() {
  delay(10);
  if (digitalRead(constant_eStopPin) == HIGH) {
    Serial.println("Move To Midpoint Started");
    traMotor.setSpeedInMillimetersPerSecond(constant_traDefaultSpeed);
    traMotor.setTargetPositionInMillimeters(-1 * constant_traDirs[boardID] * constant_traMidpoints[boardID]);
    Serial.println("Move To Midpoint Success");
  }
  delay(3000);
}

void measureCurrentPoint() {
  Serial.println(traMotor.getCurrentPositionInSteps());
  Serial.println(traMotor.getCurrentPositionInMillimeters());
}

void rotatePositive() {
  rotMotor.setCurrentPositionAsHomeAndStop();
  rotMotor.setTargetPositionInRevolutions(constant_rotDirs[boardID]);
}

void rotateNegative() {
  rotMotor.setCurrentPositionAsHomeAndStop();
  rotMotor.setTargetPositionInRevolutions(-1 * constant_rotDirs[boardID]);
}

void translatePositive() {
  traMotor.setCurrentPositionAsHomeAndStop();
  traMotor.setTargetPositionInMillimeters(constant_traDirs[boardID] * constant_traRanges[boardID]);
}

void translateNegative() {
  traMotor.setCurrentPositionAsHomeAndStop();
  traMotor.setTargetPositionInMillimeters(-1 * constant_traDirs[boardID] * constant_traRanges[boardID]);
}
