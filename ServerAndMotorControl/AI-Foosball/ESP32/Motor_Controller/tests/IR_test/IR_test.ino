#define IN 4

void setup() {
  // put your setup code here, to run once:
  Serial.begin(115200);
  while (!Serial);

}

void loop() {
  // put your main code here, to run repeatedly:
  Serial.println(analogRead(IN));
}
