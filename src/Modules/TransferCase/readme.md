# Neoteric Transfer Case support

## Current Hardware

The current controller is the `NTCv1b`.  It supports:

- Borg Warner 4419
- Magna Power 3023NQH

## Notes/Work Items for next Version

Below is a running list of tasks/features that I hope to include in a next version (no scheduled date)

- Add an on-board 12V->5V Automotive power supply
- Add on-board relays for shift motor driving 
- Add 2 more ground terminal connectors
- Add a jumper for disconnecting 5V from the 12V supply (power from USB)
- Add silkscreen for t-case selection jumper
- Add a jumper to pull the ENA safety interlock low
- Add a jumper or expander to allow software read of hardware revision 
- Add CAN bus support
- Add jumper-optional sleep/wake on switched ACC power
- Add interlock disable (ground) jumper
- Add schottky diode to 12Vin and 5V out for reverse-polarity protection
- Add TVS diodes for transient protection