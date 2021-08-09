import java.util.*;
import processing.serial.*;
import netP5.*;
import oscP5.*;

final int SELF_PORT = 10001;
final int BAUD_RATE = 9600;

HashMap<String,Serial> serialMap;
boolean active;
String[] portList;

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
  portList = Serial.list();
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
      if( Objects.isNull( serial ) ) return;
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
  
  // 存在しない場合はnullで返す
  if( !existsPort( portName ) ) return null;
  
  try{
    Serial serial = new Serial( this, portName, BAUD_RATE );
    serialMap.put( portName, serial );
    return serial;
  }catch( Exception e ){
    println( "fail to connect port :: " + portName );
  }
  return null;
}

/**
 * 指定のポート名が検知されているか判定します
 * ※高速化のため、アプリケーションの開始時に検知されているデバイスが対象
 * @param String targetPort
 * @return boolean 存在する場合true
 */
boolean existsPort( String targetPort ){
  for( String portName : portList ){
    if( portName.equals( targetPort ) ) return true;
  }
  return false;
}
