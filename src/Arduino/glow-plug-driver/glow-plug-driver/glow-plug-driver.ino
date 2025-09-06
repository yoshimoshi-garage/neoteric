/*
 * Neoteric Transfer Case Switch Converter
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

 #include <Arduino.h>

// Uncomment this line to enable debug output
#define DEBUG

const int OUTPUT_PIN = 11;
const int STATE_TICK_DURATION_MS = 100;

// Debug macros.  If #debug is not defined, they do nothing
#ifdef DEBUG
  #define DEBUG_PRINT(x) Serial.print(x)
  #define DEBUG_PRINTLN(x) Serial.println(x)
#else
  #define DEBUG_PRINT(x)
  #define DEBUG_PRINTLN(x)
#endif

void setup() {
  Serial.begin(9600);
  DEBUG_PRINTLN("Glow plug driver Initialized");

  pinMode(OUTPUT_PIN, OUTPUT);  // Set output for PWM
  pinMode(LED_BUILTIN, OUTPUT);  // Set output for PWM
  digitalWrite(OUTPUT_PIN, 0);  
}

static int tick = 0;
static int start_tick = 0;
static bool ramp_started = false;

// Function to set PWM duty cycle as percentage (0.0 to 1.0)
void setPWMDutyCycle(int pin, float dutyCycle) {
  // Constrain input to valid range
  dutyCycle = constrain(dutyCycle, 0.0, 1.0);
  
  // Convert percentage to 0-255 range
  int pwmValue = (int)(dutyCycle * 255);
  
  // Set the PWM output
  analogWrite(pin, pwmValue);
  
  // Optional: print for debugging (every 500ms to avoid spam)
  static unsigned long lastPrint = 0;
  if (millis() - lastPrint > 500) {
    DEBUG_PRINT("Duty Cycle: ");
    DEBUG_PRINT(dutyCycle * 100);
    DEBUG_PRINT("%, PWM Value: ");
    DEBUG_PRINTLN(pwmValue);
    lastPrint = millis();
  }
}

// Ramping function with decreasing slope
void rampPlug(int pin, float initial, int initialDurationSeconds, int rampDurationSeconds) {
  // Initialize on first call
  if (!ramp_started) {
    start_tick = tick;
    ramp_started = true;
    DEBUG_PRINTLN("Ramp started");
  }
  
  // Calculate elapsed time in seconds (assuming tick is milliseconds)
  float elapsedSeconds = (tick - start_tick) / 1000.0;
  
  float currentDutyCycle;
  
  // Phase 1: Hold initial duty cycle
  if (elapsedSeconds < initialDurationSeconds) {
    currentDutyCycle = initial;
  }
  // Phase 2: Ramp down linearly
  else if (elapsedSeconds < (initialDurationSeconds + rampDurationSeconds)) {
    float rampElapsed = elapsedSeconds - initialDurationSeconds;
    float rampProgress = rampElapsed / rampDurationSeconds; // 0.0 to 1.0
    currentDutyCycle = initial * (1.0 - rampProgress); // Linear decrease
  }
  // Phase 3: Finished ramping
  else {
    currentDutyCycle = 0.0;
    // Optionally reset for next cycle
    // ramp_started = false; // Uncomment to restart automatically
  }
  
  // Set the PWM output
  setPWMDutyCycle(pin, currentDutyCycle);
}

// Function to reset the ramp (call this to start a new ramp cycle)
void resetRamp() {
  ramp_started = false;
}

void loop() {
  tick++; // Increment tick counter (assuming 1ms per tick)
  
//  rampPlug(OUTPUT_PIN, 1.0, 3, 5);
  
  
  delay(1); // 1ms tick
}
