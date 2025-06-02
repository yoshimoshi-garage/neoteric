# AC DELCO 84254255 4WD Control Switch

OE Fitment: Various GM vehicles, 2014-2020

PINOUT (12-pin male connector)

| pin | function |
|-|-|
| 1 | not connected |
| 2 | Ingition voltage |
| 3 | Neutral indicator |
| 4 | Indicator dimmer control |
| 5 | LED Backlight dimmer control |
| 6 | Ground |
| 7 | AWD indicator control |
| 8 | 2 HI indicator control |
| 9 | 4 HI indicator control |
| 10 | 4 LO indicator control |
| 11 | Drive mode 5V reference |
| 12 | Drive Mode signal |

## Probe Notes

Testing with a meter, 11<->12 appears to just be a mechanical resistor switch.  Resistance between 11 and 12 is as follows:

| position | resistance (ohms) |
|-|-|
| 2H | 1.5k |
| AUTO | 140 |
| 4H | 660 |
| 4L | 2.34k |

- Applying 12V to Pin 5 illuminates the border ring and knob backlight - it dims with lower voltage.

- To illuminate a gear indicator you must:
  - Apply 12V to Pin 4
  - Ground the appropriate indicator control pin
  - put the knob into that position

So to illiminate the 2H indicator, you would need to:
  - 12V to pin 4
  - pin 8 to ground
  - knob in 2H 