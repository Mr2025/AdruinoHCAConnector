#include <SPI.h>
#include <SoftwareSerial.h>


String DigitalPinList(int lowPin, int highPin);
String MemoryHex(byte* mem, int len);

int LED = 13;
byte LOW_DIGITAL = 22;
byte HIGH_DIGITAL = 53;

byte dataInbound = 0;
byte lowPin =LOW_DIGITAL;
byte highPin =HIGH_DIGITAL;

byte BIT_MASK[] = {B10000000, B1000000, B100000, B10000, B1000, B100, B10, B1}; //Establishing a collection of bit masks, that will be used to pack the bits left to right. 
int BIT_MASK_Length = sizeof(BIT_MASK); // the number of elements in a bit. 

Debug m_Debug;

void setup() {
  Serial.begin(9600);
    
  //Set all digital pin to input (enviorment will make flicker.  Make sure to build known state pins.
  for(int i = LOW_DIGITAL; i < HIGH_DIGITAL; i++)
  {
    pinMode(i,INPUT);
  }

  //Ready Pattern
  pinMode(LED ,OUTPUT);
  for (int i= 0; i<3;i++)
  {
    digitalWrite(LED ,HIGH);
    delay(100);
    digitalWrite(LED ,LOW);
    delay(200);
  }
}

void loop(){
  //Buffer for more complicated requests.
  byte buf[2] = {0,0};
  
  //Check if the system has any data avaliable from the PC.   
  if ( Serial.available() > 0 )
  {    
    dataInbound = Serial.read();
    
    switch(dataInbound)
    {       
      case 'f': // f-actory Reset      
        LOW_DIGITAL = 22;  
        HIGH_DIGITAL = 53;    
      break;  
      
      case 's': // S-et LowPin HighPin   's'0x\012x\30        
        Serial.readBytes(buf,2);
        LOW_DIGITAL = buf[0];
        HIGH_DIGITAL = buf[1];        
      break;  

      case 'q': // Q-uery LowPin HighPin   'q'0x\012x\30
        Serial.readBytes(buf,2);
        lowPin = buf[0];        
        highPin = buf[1];

        if (lowPin > 0 && highPin > 0) 
          Serial.println(DigitalPinList(lowPin,highPin));         
      break;  
      
      case 'r': // r-eport
      if (LOW_DIGITAL > 0 && HIGH_DIGITAL > 0) 
      {
        String data = DigitalPinList(LOW_DIGITAL, HIGH_DIGITAL); 
        Serial.println(data); 
      }
      break;
      
      default:
        //Do nothing
        //Burn all the remaining bytes in the Serial Line.         
      break;
    }//end-switch
  }// end-if  
}//end-loop()

/* DigitalPinList will build a bit-array and return it to the caller as a ascii encoded string.
lowpin - the begining pin of the query
highpoin - the end of the query
*/
String DigitalPinList(int lowPin, int highPin){  
  byte rVal[] = {0,0,0,0, 0,0,0,0};
  int rValLength = sizeof(rVal);
  int rValIndex =0;

	// Loop over each of the interested pins
  for(int i = lowPin; i <= highPin; i++)
  {
    rValIndex = i / BIT_MASK_Length; //Located the current Byte in the array
    uint8_t pinVal = digitalRead(i); // read the Arduino Pin. 
    if (HIGH==pinVal)
    {      
      rVal[rValIndex] = rVal[rValIndex] | BIT_MASK[i%BIT_MASK_Length];    //Mask this element of the Byte High.
    }
  }//end-for 
    
  return MemoryHex(rVal,rValLength);
}//end-DigitalPinLins

/* MemoryHex dumps an area of memory, as a ascii encoded string*/
String MemoryHex(byte* mem, int len){
  String rVal = "";
  
  for (int i =0; i < len; i++){    
    rVal += ByteToString(mem[i]); //Dump the Hex out    
    rVal += " ";    
  }  
  return rVal;
}

/*ByteToString - Will format a single byte as a string with the leading 0 in the event it is a single hex digit*/
String ByteToString(byte val){
  String valAsString = String(val,HEX); //Convert to string. 
  String rVal = valAsString; // Prep return value

  //if the string is not two digits, add a leading "0"
  if (valAsString.length() == 1){
    rVal = "0" + valAsString;
  }

  //Final Format
  rVal.toUpperCase();
  return rVal;
}
