namespace signatus {

    using Plisky.Plumbing;

    public class SignatusOptions {

        public SignatusOptions() {
            Directory = @"C:\Temp\deleteme";
            InputFile = @"C:\Files\Code\nosccm\mollycoddle\_Dependencies\match.mm";
            ScriptToExecute = @"C:\temp\scr.ps1";
        }

        [CommandLineArg("directory", Description = "The directory to search for matches.", FullDescription = "Used to pass a directory which is scanned for each of the matches in the multimatch")]
        public string Directory { get; set; }

        [CommandLineArg("mm", Description = "The name of the file containing minmatch expressions, one per line")]
        public string InputFile { get; set; }

        [CommandLineArg("script", Description = "The full name of the script to execute on match")]
        public string ScriptToExecute { get; set; }

        [CommandLineArg("stopOnFail", Description = "Should processing stop after the first failure to execute the script")]
        public bool StopOnFail { get; set; }
    }
}