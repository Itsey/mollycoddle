### Mollycoddle

> For when you just cant let the babbers code on their own.

Mollycoddle is a directory and file linting solution for source control projects designed to check the structure of the source repository rather than the code itself.  It is NOT a code linting solution there are plenty of those out there already.

#### Using Mollycoddle

Execute Mollycoddle from the command line, passing a parameter of the path to scan, a -primaryRoot parameter for using common files, and -rulesFile parameter for the ruleset to use.

```text
❯ .\mollycoddle.exe C:\Files\Code\git\mollycoddle -primaryRoot=C:\Files\Code\git\mollycoddle\src\mollycoddle.testdata\TestMasterPath\ -rulesFile=C:\Files\Code\git\mollycoddle\src\_Dependencies\RulesFiles\Default\defaultrules.mollyset
```

When Mollycoddle executes violations will be returned to standard output.  The number of violations will be returned as the exit code for using in automations and scripts.

```text
❯ .\mollycoddle.exe C:\Files\Code\git\mollycoddle -primaryRoot=C:\Files\Code\git\mollycoddle\src\mollycoddle.testdata\TestMasterPath\ -rulesFile=C:\Files\Code\git\mollycoddle\src\_Dependencies\RulesFiles\Default\defaultrules.mollyset
💩 Violation:  Local gitignore file must match common git ignore file. (c:\files\code\git\mollycoddle\.gitignore does not match common file definition.)
😢 Completed.Total Violations 1.
```


### Documentation

The main documentation is here: https://itsey.github.io/mollycoddle/

This includes information relating to the rules and how to run it as part of a pipeline.


### Developer Notes.

Attempted to migrate to the Microsoft Globbing implementation but it does not support windows paths, or filters or directories.  Ultimately aborted this and rolled back to the existing globbing implementation. 

