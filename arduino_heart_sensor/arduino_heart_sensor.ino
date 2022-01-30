#include <analogWrite.h>
#include <BluetoothSerial.h>

int SENSOR_PIN = 12;

BluetoothSerial SerialBT;
int Signal;
float mapVal;

void setup() {
  pinMode(SENSOR_PIN,INPUT);
  Serial.begin(9660);
  SerialBT.begin("ESP32-3");
}

void loop() {
   Signal = analogRead(SENSOR_PIN);  // Read the PulseSensor's value.
   mapVal =  map(Signal, 2000, 4000, 0, 255);
   if( mapVal < 0 ) mapVal   = 0;
   if( mapVal > 255 ) mapVal = 255;
   Serial.flush();
   Serial.println(Signal);
   SerialBT.flush();
   SerialBT.println(Signal);
   delay(16);
}
