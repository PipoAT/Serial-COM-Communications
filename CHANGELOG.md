# Serial-COM-Communications
A C# Developed Diagnostic Tool designed to test the Serial Communications between the PC and a designated Microcontroller via USB connections

Current Release: AT-SCC v.1.0.0

Next Release: AT-SCC v.1.1.0

## 6/12/2024
### Added Features
- Added COM Port checking on COM setting change
- Added ability to cancel an current transmission if attempting to start a new tranmission while another one is going
### Bugs/Issues Addressed
- Cleanup of initializations and declarations
- Cleanup of Log Form for readability
- Logic Cleanup
- Seperated Serial tasks and functions into its own file for readability
### Other Matters
- Adjusted logging to have most recent log on top instead of bottom


## 6/5/2024
### Added Features
- Added send and receive mode
### Bugs/Issues Addressed
- Addressed null reference warnings
- Addressed warning NETSDK1206
- Misc. cleanup/simplification of functions and declarations
### Other Matters
- Seperated UI Functions into its own file for readability

## 6/4/2024
### Added Features
- Added changelog
### Bugs/Issues Addressed
- Fixed issue where it was not logging any transmissions for the Byte/Byte Collection setting
- Fixed issue where the program sends and receives data despite having a desired mode set
    - A "Send and Receive" Mode will be implemented later on
### Other Matters
- Simplified logic for the Byte/Byte Collection setting