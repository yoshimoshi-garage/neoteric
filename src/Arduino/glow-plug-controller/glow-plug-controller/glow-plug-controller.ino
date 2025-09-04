/*
 * Neoteric Glow Plug Controller
 *
 * MIT LICENSE
 * 
 * © 2025 Chris Tacke
 * Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated documentation files (the "Software"), 
 * to deal in the Software without restriction, including without limitation the rights to use, copy, modify, merge, publish, distribute, sublicense, 
 * and/or sell copies of the Software, and to permit persons to whom the Software is furnished to do so, subject to the following conditions:
 * 
 * The above copyright notice and this permission notice shall be included in all copies or substantial portions of the Software.
 * THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, 
 * FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER 
 * LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS 
 * IN THE SOFTWARE.
 */

#include "config.h"
#include "output_control.h"
#include "state_machine.h"

// Global variable definitions
ControllerState currentState;
unsigned long stateStartTime;
bool outputEnabled[NUM_OUTPUTS];
float currentDutyCycle[NUM_OUTPUTS];

void setup() {
  Serial.begin(9600);
  DEBUG_PRINTLN("Glow Plug Controller Initializing");

  // Initialize outputs
  initializeOutputs();

  // Initialize inputs for future features
  for(int i = 0 ; i < NUM_INPUTS ; i++) {
    pinMode(INPUT_PINS[i], INPUT);
  }
  DEBUG_PRINTLN("All inputs initialized");

  pinMode(LED_BUILTIN, OUTPUT);
  digitalWrite(LED_BUILTIN, LOW);

  // Initialize state machine
  initializeStateMachine();
}

void loop() {
  updateStateMachine();
  delay(10);
}
