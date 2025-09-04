#ifndef OUTPUT_CONTROL_H
#define OUTPUT_CONTROL_H

#include "config.h"

// Output control functions
void setOutput(int outputIndex, float dutyCycle);
void setAllOutputs(float dutyCycle);
void enableOutput(int outputIndex, bool enabled);
bool isOutputEnabled(int outputIndex);
void initializeOutputs();

#endif