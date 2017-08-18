#include "Debug.h"   // define MyClass

void Debug::WaitforMonitor(){
    while (!Serial) {
      ; // wait for serial port to connect. Needed for native USB port only
  }
}

String Debug::BoolToString(bool variable){
  if(variable){
    return "true";
  }
  return "false";
}

String Debug::ByteToString(byte val){
  String valAsString = String(val,HEX);
  String rVal = valAsString;

  //Format valAsString into two places.
  if (valAsString.length() == 1){
    rVal = "0" + valAsString;
  }

  //Final Format
  rVal.toUpperCase();
  return rVal;
}
 

String Debug::MemoryDump(byte* mem, int len){
  String rVal = "00 01 02 03 04 05 06 07 08 09";
  for (int i =0; i < len; i++){
    if (i%10==0 ) rVal += "\n"; // every 10 chars add a newline
    rVal += Debug::ByteToString(mem[i]); //Dump the Hex out
    rVal += " ";    
    }
  rVal += "\n00 01 02 03 04 05 06 07 08 09\n";
  return rVal;
}



