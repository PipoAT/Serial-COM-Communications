# Serial-COM-Communications
A C# Developed Diagnostic Tool designed to test the Serial Communications between the PC and a designated Microcontroller via USB connections

Current Release: AT-SCC v.1.0.0

Next Release: AT-SCC v.1.1.0

## 6/5/2024
### Added Features
### Bugs/Issues Addressed
- Addressed null reference warnings
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