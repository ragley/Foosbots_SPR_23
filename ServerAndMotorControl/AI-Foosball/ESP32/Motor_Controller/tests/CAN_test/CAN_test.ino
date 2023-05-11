#include <CAN.h>

#define RXR_CAN 15 //GPIO13
#define TXD_CAN 13 //GPIO15

void CAN_Handler(int packet_size);

void setup() {
  Serial.begin(115200);
  while (!Serial);
  
  CAN.setPins(RXR_CAN, TXD_CAN);
  
  // start the CAN bus at 500 kbps
  if (!CAN.begin(500E3)) {
    //make sure to display this failure onscreen
    Serial.println("Starting CAN failed!");
    while (1);
  }
  Serial.println("Starting CAN success");
  CAN.onReceive(CAN_Handler);

}

void loop() {
  // // send packet: id is 11 bits, packet can contain up to 8 bytes of data
  // Serial.print("Sending packet ... ");

  // CAN.beginPacket(0x12);
  // CAN.write('h');
  // CAN.write('e');
  // CAN.write('l');
  // CAN.write('l');
  // CAN.write('o');
  // CAN.endPacket();

  // Serial.println("done");

  // delay(3000);
}

void CAN_Handler(int packet_size){
  // received a packet
  Serial.print("Received ");

  if (CAN.packetExtended()) {
    Serial.print("extended ");
  }

  if (CAN.packetRtr()) {
    // Remote transmission request, packet contains no data
    Serial.print("RTR ");
  }

  Serial.print("packet with id 0x");
  Serial.print(CAN.packetId(), HEX);

  if (CAN.packetRtr()) {
    Serial.print(" and requested length ");
    Serial.println(CAN.packetDlc());
  } else {
    Serial.print(" and length ");
    Serial.println(packet_size);

    // only print packet data for non-RTR packets
    while (CAN.available()) {
      Serial.print((char)CAN.read());
    }
    Serial.println();
  }

  Serial.println();
}
