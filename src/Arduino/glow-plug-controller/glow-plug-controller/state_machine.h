#ifndef STATE_MACHINE_H
#define STATE_MACHINE_H

#include "config.h"

// State machine functions
void initializeStateMachine();
void updateStateMachine();
void enterLowPowerMode();
void updateIndividualOutputs();
void startFullPowerPhase();

#endif