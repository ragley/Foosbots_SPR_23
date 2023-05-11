#include <ESP_FlexyStepper.h>
#include "Controller_Constants.h"

#define CORE 0

const int REVOLUTIONS = 1;

int target_ROT = REVOLUTIONS;
int direction_ROT = 1;
double timer_ROT = millis();

int count = 0;
int board_ID = 0;

ESP_FlexyStepper Rotational_Driver;
ESP_FlexyStepper Translation_Driver;

void setup() {
  Serial.begin(115200);
  pinMode(ID_1, INPUT);
  pinMode(ID_2, INPUT);
  pinMode(ENABLE, INPUT);
  board_ID = digitalRead(ID_2)*2 + digitalRead(ID_1);
  Serial.println(board_ID);
  Serial.println(DIRECTIONS[board_ID][TRANSLATION]);
  Serial.println(MAX_TRANSLATIONS[board_ID]);
  Serial.println(DIRECTIONS[board_ID][TRANSLATION]*MAX_TRANSLATIONS[board_ID]*(-2));
  Serial.println("WAITING");
  while(digitalRead(ENABLE) == LOW);
  Serial.println("MEASURING");
  Translation_Driver.connectToPins(TRANSLATION_DRIVER_PULSE, TRANSLATION_DRIVER_DIR);
  Translation_Driver.setStepsPerMillimeter(STEP_PULSE_TRANSLATION_CONVERSION);
  Translation_Driver.setSpeedInMillimetersPerSecond(HOME_SPEED_TRANSLATION);
  Translation_Driver.setAccelerationInMillimetersPerSecondPerSecond(MAX_ACCELERATION_TRANSLATION);
  Translation_Driver.setDecelerationInMillimetersPerSecondPerSecond(MAX_ACCELERATION_TRANSLATION);
  Translation_Driver.startAsService(0);
  Translation_Driver.setCurrentPositionAsHomeAndStop();
  Translation_Driver.setTargetPositionInMillimeters(DIRECTIONS[board_ID][TRANSLATION]*MAX_TRANSLATIONS[board_ID]*(-2));
}

bool ONCE = false;
void loop() {
  if (!ONCE) {
      while(count <= 10){
          if (digitalRead(TRANSLATION_DRIVER_ZERO) == LOW) {
            count += 1;
          } else {
            count = 0;
          }
          Serial.println(Translation_Driver.getCurrentPositionInSteps());
          delay(1);
      }
      Translation_Driver.emergencyStop(false);
      Serial.println(Translation_Driver.getCurrentPositionInSteps());
      ONCE = true;
  }
}
