#ifndef FAULT_INDICATION_H
#define FAULT_INDICATION_H

#include "config.h"

// LED fault indication constants
const int BLINK_ON_TIME_MS = 300;       // LED on duration for each blink
const int BLINK_OFF_TIME_MS = 200;      // LED off duration between blinks
const int SEQUENCE_PAUSE_MS = 1500;     // Pause between blink sequences
const int FAULT_CHECK_INTERVAL_MS = 100; // How often to update fault indication

// Function declarations
void initializeFaultIndication();
void updateFaultIndication();
void setOutputFault(int outputIndex, bool faulted);
bool hasAnyFaults();
int getFirstFaultedOutput();

#endif