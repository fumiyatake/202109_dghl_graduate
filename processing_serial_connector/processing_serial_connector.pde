import java.util.*;
import processing.serial.*;
import netP5.*;
import oscP5.*;

final int SELF_PORT = 10001;
final int UNITY_PORT = 11001;
final int BAUD_RATE = 9600;

final boolean IS_BEAT_USE = false;

final String HEARTBEAT_SERIAL_PORT = "/COM8";
final String HEARTBEAT_OSC_IP = "localhost";
final String HEARTBEAT_OSC_ADDRESS = "/heartbeat";
final int THRESHOLD_HIGH = 4000;
final int THRESHOLD_LOW = 2000;
final int BEAT_HISTORTY_SIZE = 30;
final int RESET_HISTORY_COUNT = 150; // 一定以上連続して同じ値がシリアル送信されたらリセットする閾値
final int INTERVAL = 3000;  // ms

int startMillis;
String prevVal;
int valKeepCount = 0;
boolean isPulseCheckOn = false;
ArrayDeque<Integer> beatMillisList;
float resetMillis;

HashMap<String,Serial> serialMap;
Serial heartBeatSerial;
boolean active;
String[] portList;

OscP5 osc;
NetAddress unityAddress;

void setup(){
  frameRate( 60 );
  size( 480, 320 );
  background( 0 );
  textSize( 20 );
  textAlign( CENTER );
  text( "playing", width / 2, height / 2 );
  serialMap = new HashMap<String,Serial>();
  
  osc = new OscP5( this, SELF_PORT );
  unityAddress = new NetAddress( HEARTBEAT_OSC_IP, UNITY_PORT );
  portList = Serial.list();
  
  if( IS_BEAT_USE ){
    heartBeatSerial = new Serial( this, HEARTBEAT_SERIAL_PORT, BAUD_RATE);
  }
  
  beatMillisList = new ArrayDeque<Integer>();
  
  resetMillis = millis();
}

void draw(){
  // 平均の脈拍を計算してOSCで送信
  if( beatMillisList.size() <= 1 ){
    sendBeatRate(0);
    return;
  }

  ArrayDeque<Integer> millisList = beatMillisList.clone();
  int sum = 0;
  int prev = 0;
  for( int millisec : millisList ){
    if( prev == 0 ){
      prev = millisec;
      continue;
    }
    sum += millisec - prev;
    prev = millisec;
  }
  float beatRate = ( sum / ( millisList.size() - 1 ) ) / 1000f;
  sendBeatRate( beatRate );
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

void serialEvent(Serial p){
   // 設定したシリアルポートからデータを読み取り
    if( heartBeatSerial.available() <= 0 ) return;
    
    String val = heartBeatSerial.readStringUntil('\n');
    if( Objects.isNull( val ) ) return;
    val = trim( val );
    int currentPulse;
    try{
      currentPulse = Integer.valueOf( val );
    }catch( Exception e ){
      return;
    }
    
    if( millis() - resetMillis < INTERVAL ) return;
    println( millis() - resetMillis );
    println( INTERVAL );
    if( val.equals( prevVal ) || currentPulse < THRESHOLD_HIGH && currentPulse > THRESHOLD_LOW ){
      valKeepCount++;
      if( valKeepCount > RESET_HISTORY_COUNT ){
        beatMillisList.clear();
        resetMillis = millis();
        println( "clear" );
      }
      return;
    }
    
    if( valKeepCount >= RESET_HISTORY_COUNT ){
      sendBeatRate(1);
    }
    valKeepCount = 0;
    prevVal = val;
    
    // 高域の状態に切り替わった時のミリ秒を記録 
    if( !isPulseCheckOn && currentPulse >= THRESHOLD_HIGH ){
      isPulseCheckOn = true;
      if( beatMillisList.size() >= BEAT_HISTORTY_SIZE ){
        beatMillisList.removeFirst();
      }
      beatMillisList.addLast( millis() );
    
    // 低域に切り替わった時にフラグを折って連続する高域をはじく 
    }else if( isPulseCheckOn && currentPulse <= THRESHOLD_LOW ){
      isPulseCheckOn = false;
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

void sendBeatRate( float beatRate ){
  if( !IS_BEAT_USE ) return;
  OscMessage msg = new OscMessage( HEARTBEAT_OSC_ADDRESS );
  msg.add( beatRate );
  osc.send( msg, unityAddress );
}
