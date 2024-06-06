# Serial-COM-Communications
A C# Developed Diagnostic Tool designed to test the Serial Communications between the PC and a designated Microcontroller via USB connections

Current Release: AT-SCC v.1.0.0

Next Release: AT-SCC v.1.1.0

## 6/6/2024
### Added Features
- N/A
### Bugs/Issues Addressed
- Addressed warnings CS8622
- Added exception handling for connection issues
### Other Matters
- N/A

---

## 6/5/2024
### Added Features
- Added send and receive mode
### Bugs/Issues Addressed
- Addressed null reference warnings
- Addressed warning NETSDK1206
- Misc. cleanup/simplification of functions and declarations
### Other Matters
- Seperated UI Functions into its own file for readability

---

## 6/4/2024
### Added Features
- Added changelog
### Bugs/Issues Addressed
- Fixed issue where it was not logging any transmissions for the Byte/Byte Collection setting
- Fixed issue where the program sends and receives data despite having a desired mode set
    - A "Send and Receive" Mode will be implemented later on
### Other Matters
- Simplified logic for the Byte/Byte Collection setting