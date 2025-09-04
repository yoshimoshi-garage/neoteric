# Dorman 601-604 4WD Control Switch

OE fitment: Ford F-150 2014-09
FoMoCo part #: 9L3Z14B166AA

This switch uses an 8-pin male connector on the back, however only pins 1-4 exist. Pins 5-8 are not populated.

## Pinout (4-pin male connector)

| pin | function |
|-|-|
| 1 | Illumination Vcc |
| 2 | Position IN |
| 3 | Position Out |
| 4 | GND |

Note: I tested illumination with 5V, not sure if this is 12V-tolerant (probably is).  Need to verify with a OE wiring diagram or meter on an existing truck install

## Operation

To illuminate the switch, ground Pin 4 and apply voltage to Pin 1.
The switch position changes the resitance between Pin 2 and Pin 3 (bi-directional, so they can be swapped on install)

4L = 130 ohms
4H = 270 ohms
2H = 620 ohms
Between gears is an open connection

