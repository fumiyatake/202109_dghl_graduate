
#include <ArduinoJson.h>
#include <BluetoothSerial.h>
#include <Arduino.h>
#include <analogWrite.h>

BluetoothSerial SerialBT;

const int LED_R_PIN = 21;
const int LED_G_PIN = 19;
const int LED_B_PIN = 18;

const int PINS[] = {
  LED_R_PIN, LED_G_PIN, LED_B_PIN
};

float brightness = 0;
String incomingByte = ""; // for incoming serial data

void setup() {
  Serial.begin(9600);
  SerialBT.begin("ESP32");
//  SerialBT.begin("ESP32-3");

  pinMode( LED_R_PIN, OUTPUT );
  pinMode( LED_G_PIN, OUTPUT );
  pinMode( LED_B_PIN, OUTPUT );
}

void loop() {
  if( SerialBT.available() == 0 ) return;
  
  String str = SerialBT.readStringUntil(';');
  int str_len = str.length()+1;
  char json[str_len];
  str.toCharArray( json, str_len );
  
  StaticJsonDocument<100> doc;
  DeserializationError error = deserializeJson(doc, json);
  if (error) {
    Serial.print(F("deserializeJson() failed: "));
    Serial.println(error.f_str());
    return;
  }
  double r = doc["r"];
  double g = doc["g"];
  double b = doc["b"];
  writeColor( r, g, b );
}

int ctoi(char c) {
  if (c >= '0' && c <= '9') {
    return c - '0';
  }
  return 0;
}

void writeColor( double r, double g, double b ){
  Serial.print( r );
  Serial.print( "-" );
  Serial.print( g );
  Serial.print( "-" );
  Serial.println( b );
  analogWrite( LED_R_PIN, r );
  analogWrite( LED_G_PIN, g );
  analogWrite( LED_B_PIN, b );
}
