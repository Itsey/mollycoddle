### Mollycoddle

> For when you just cant let the babbers code on their own.

MollyCoddle is a directory and file linting solution for source control projects designed to check the structure of the source repository rather than the code itself.  It is NOT a code linting solution there are plenty of those out there already.

#### Using Mollycoddle

Execute MollyCoddle from the command line, passing a parameter of the path to scan and a -masterRoot parameter for using master files.

```text
❯ .\mollycoddle.exe C:\Files\Code\git\mollycoddle -masterRoot=C:\Files\Code\git\mollycoddle\src\_Dependencies\TestMasterPath\
```

When mollycoddle executes violations will be returned to standard output.  The number of violations will be returned as the exit code for using in automations and scripts.

```text
❯ .\mollycoddle.exe C:\Files\Code\git\mollycoddle -masterRoot=C:\Files\Code\git\mollycoddle\src\_Dependencies\TestMasterPath\
Violation M-0004 (c:\files\code\git\mollycoddle\.gitignore does not match master.)
Total Violations 1
```


### Documentation

The main documentation is here: https://itsey.github.io/mollycoddle/

This includes information relating to the rules and how to run it as part of a pipeline.


### Developer Notes.

Attempted to migrate to the Microsoft Globbing implementation but it does not support windows paths, or filters or directories.  Ultimately aborted this and rolled back to the existing globbing implementation. 

