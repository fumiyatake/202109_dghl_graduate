import java.util.*;
import processing.serial.*;
import netP5.*;
import oscP5.*;

final int SELF_PORT = 10001;
final int BAUD_RATE = 9600;

HashMap<String,Serial> serialMap;
boolean active;

OscP5 osc;

void setup(){
  frameRate( 60 );
  size( 480, 320 );
  background( 0 );
  textSize( 20 );
  textAlign( CENTER );
  text( "playing", width / 2, height / 2 );
  serialMap = new HashMap<String,Serial>();
  
  osc = new OscP5( this, SELF_PORT );
}

void oscEvent( OscMessage msg ){
  switch( msg.addrPattern() ){
    case "/serial/connect":
      // コネクションをつくるのみ
      connectSerialDevice( msg.get(0).stringValue() );
      break;
    case "/serial/write":
      // コネクションがない場合は作ったうえで書き込み
      Serial serial = connectSerialDevice( msg.get(0).stringValue() );
      serial.write( msg.get(1).stringValue() );
      break;
    default:
      println( "Undefined address :: " + msg.addrPattern() );
      return;
  }
  
}

/**
 * シリアル通信デバイスと接続します。すでに接続済みの場合は単純に接続済みのSerialを返します。
 * @param String portName 接続対象のポート名
 * @return Serial 接続したシリアル
 */
Serial connectSerialDevice( String portName ){
  if( serialMap.containsKey( portName ) ){
    Serial serial = serialMap.get( portName );
    if( serial.active() ) return serial;
  }
  try{
    Serial serial = new Serial( this, portName, BAUD_RATE );
    serialMap.put( portName, serial );
    return serial;
  }catch( Exception e ){
    println( "fail to connect port :: " + portName );
  }
  return null;
}
