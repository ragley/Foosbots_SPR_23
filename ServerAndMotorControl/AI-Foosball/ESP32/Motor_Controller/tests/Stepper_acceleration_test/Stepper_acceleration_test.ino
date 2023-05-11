#include <ESP_FlexyStepper.h>

#define PULSE_1 23
#define DIRECTION_1 22
#define ANALOG 4
#define CORE 1

const float stepPerRevolution = 1600; //according to the 1.80 degree step angle from data sheet and 400 on the driver
const int maxRPS = 89;
const int maxACC = 3000;
const int maxAnalog = 2048*2;
const int REVOLUTIONS = 10;

int RPS = maxRPS;
int target = REVOLUTIONS;
int direction = 1;
double timer = millis();

int ACC;

ESP_FlexyStepper stepper_1;

void setup() {
  Serial.begin(115200);
  stepper_1.connectToPins(PULSE_1, DIRECTION_1);
  stepper_1.setStepsPerRevolution(stepPerRevolution);
  stepper_1.setSpeedInRevolutionsPerSecond(RPS);
  stepper_1.setTargetPositionToStop();
  stepper_1.startAsService(CORE);
}

void loop() {
  ACC = (analogRead(ANALOG)*maxACC/maxAnalog) + 1;
  stepper_1.setAccelerationInRevolutionsPerSecondPerSecond(ACC);
  stepper_1.setDecelerationInRevolutionsPerSecondPerSecond(ACC);
  Serial.print("ACC: ");
  Serial.print(ACC);
  Serial.print(" SPEED: ");
  Serial.print(stepper_1.getCurrentVelocityInRevolutionsPerSecond());
  Serial.print(" REV: ");
  Serial.print(stepper_1.getCurrentPositionInRevolutions());
  Serial.print(" TARGET: ");
  Serial.println(stepper_1.getTargetPositionInRevolutions());
  if (stepper_1.getDistanceToTargetSigned() == 0 && direction*target > 0) {
    direction *= -1;
    timer = millis() + 1000;
  }
  if (millis() > timer && direction*target < 0) {
    target =  direction*REVOLUTIONS;
    stepper_1.setTargetPositionInRevolutions(target);
    timer = millis() + 1000;
  }
}
