#include "Controller_Constants.h"

void IRAM_ATTR Increment_Translation();
void IRAM_ATTR Increment_Translation_Down();

void setup(){
    Serial.begin(115200);

    while (!Serial);
    attachInterrupt(TRANSLATION_SENSOR, Increment_Translation, CHANGE);
}

void loop(){
    //Serial.println(analogRead(TRANSLATION_SENSOR));
    //delay(50);
}

const int HIGH_SHORT = 1;
const int HIGH_LONG = 2;
const int LOW_SHORT = 3;
const int LOW_LONG = 4;

int last = 0;
int current = 0;
void IRAM_ATTR Increment_Translation(){
    int analog = analogRead(TRANSLATION_SENSOR);
    bool digital = digitalRead(TRANSLATION_SENSOR);
    Serial.print(analog);
    Serial.print(" ");
    Serial.println(digital);
    if (1000 < analog < 3000) {
        if (digital == HIGH){
            current = HIGH_SHORT;
        } else {
            current = LOW_SHORT;
        }
    } else {
        if (digital == HIGH){
            current = HIGH_LONG;
        } else {
            current = LOW_LONG;
        }
    }
    if (last == HIGH_LONG && current == LOW_SHORT) {
        Serial.println("+");
    } else if (last == HIGH_SHORT && current == LOW_LONG) {
        Serial.println("-");
    } else if (last == LOW_SHORT && current == HIGH_LONG) {
        Serial.println("+");
    } else if (last == LOW_LONG && current == HIGH_SHORT) {
        Serial.println("-");
    }
    last = current;
}
