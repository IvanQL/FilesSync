# FileSync

Application for one-way directory synchronization content of the replica and source folders

## Features
- Synchronization performs periodically;
- File creation/copying/removal operations logs to a file and to the
  console output;
- Folder paths, synchronization interval and log file path provides using
  the command line arguments;

## Usage
FileSync <source> <replica> <intervalSec> <logFile>

Example:
FileSync "C:\Source" "C:\Replica" 10 "C:\Logs\log.txt"