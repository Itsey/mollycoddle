namespace mollycoddle; 

using System.Collections.Generic;
using System.Diagnostics;

/// <summary>
/// The job here is to describe the validations technically so that the checker can execute them but this class should not
/// know how to do the validations just what the validations must be.
/// </summary>
[DebuggerDisplay("Validator For {TriggeringRule}")]
public class FileValidator : ValidatorBase {

    /// <summary>
    /// This is the name of the validator, it must be specified exactly in the rules files.  It does not use nameof to prevent accidental refactoring.
    /// </summary>
    public const string VALIDATORNAME = "FileValidationChecks";

    private List<string> completeBypasses = new List<string>();
    private List<PrimaryCopyFile> masterMatchers = new List<PrimaryCopyFile>();
    private List<string> mustExistPaths = new List<string>();
    private List<MatchWithSecondaryMatches> precisePositions = new List<MatchWithSecondaryMatches>();
    private List<MatchWithSecondaryMatches> prohibittions = new List<MatchWithSecondaryMatches>();

    public FileValidator(string owningRuleName) : base(owningRuleName) {
    }

    /// <summary>
    /// Adds a bypass that prevents any rules checking for this match
    /// </summary>
    /// <param name="bypassPattern"></param>
    public void AddBypass(string bypassPattern) {
        completeBypasses.Add(bypassPattern);
    }

    public void AddProhibitedPattern(string prohibited, params string[] exceptions) {
        prohibittions.Add(new MatchWithSecondaryMatches(prohibited) {
            SecondaryList = exceptions
        });
    }

    /// <summary>
    /// A list of files that if they are found on the disk must be in a specific matched place.
    /// </summary>
    /// <returns>Match with secondary matches for files that must be in the right place</returns>
    public IEnumerable<MatchWithSecondaryMatches> FilesInSpecificPlaces() {
        return precisePositions;
    }

    public IEnumerable<string> FilesThatMustExist() {
        return mustExistPaths;
    }

    public IEnumerable<PrimaryCopyFile> FilesThatMustMatchTheirMaster() {
        return masterMatchers;
    }

    /// <summary>
    /// Files that must not exist ( except, where secondary exceptions matches are provided).
    /// </summary>
    /// <returns>A matchwithsecondary where the primary match finds a not allowed file and the secondary are exceptions</returns>
    public IEnumerable<MatchWithSecondaryMatches> FilesThatMustNotExist() {
        return prohibittions;
    }

    public IEnumerable<string> FullBypasses() {
        return completeBypasses;
    }

    /// <summary>
    /// Adds a file pattern that must be present
    /// </summary>
    /// <param name="patternThatMustExist">The file pattern</param>
    public void MustExist(string patternThatMustExist) {
        mustExistPaths.Add(patternThatMustExist);
    }

    /// <summary>
    /// Add a file matching pattern that must match a master file, exactly.
    /// </summary>
    /// <param name="pathToMatch">The matching pattern</param>
    /// <param name="masterToMatch">The path to the master that must match</param>
    public void MustMatchMaster(string pathToMatch, string masterToMatch) {
        mustExistPaths.Add(pathToMatch);
        masterMatchers.Add(new PrimaryCopyFile(pathToMatch, masterToMatch));
    }

    internal void MustBeInSpecificLocation(string patternMatch, string[] additionalData) {
        precisePositions.Add(new MatchWithSecondaryMatches(patternMatch) {
            SecondaryList = additionalData
        });
    }
}