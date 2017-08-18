#ifndef __DEBUG_H_INCLUDED__   // if x.h hasn't been included yet...
#define __DEBUG_H_INCLUDED__   // #define this so the compiler knows it has been included
#include <SPI.h>

class Debug
{
private:
public:
  static void WaitforMonitor();
  static String BoolToString(bool variable);
  static String MemoryDump(byte* mem, int len);
  static String ByteToString(byte val);
};
#endif

