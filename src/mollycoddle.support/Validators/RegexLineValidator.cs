﻿namespace mollycoddle;

using System.Text.RegularExpressions;
using Minimatch;

public enum RegexBehaviour {
    MustNotMatch,
    MustMatchOnce,
    ValueMustNotEqual,
    ValueMustEqual
}

/// <summary>
/// Uses regular expressions to validate the contents of a file one line at a time.  Regular expression based matching can be used to match or force a no match
/// for a specific pattern in the file.
/// </summary>
public class RegexLineValidator : ValidatorBase {
    private string? endStringValue;
    private bool hasStopped = false;
    private bool isActive = true;
    private string? startStringValue;

    public RegexLineValidator(string ruleName) : base(ruleName) {
        RegexValueExtract = string.Empty;
        RegexMatch = new Regex(string.Empty);
        FileMinmatch = new Minimatcher(string.Empty);
    }

    public string? EndString {
        get { return endStringValue; }
        set {
            endStringValue = value;
            isActive = (value != null);
        }
    }

    public Minimatcher FileMinmatch { get; set; }

    public RegexBehaviour MatchType { get; set; }

    public Regex RegexMatch { get; set; }

    public string RegexValueExtract { get; set; }

    public string? StartString {
        get { return startStringValue; }
        set {
            startStringValue = value;
            isActive = (value == null);
        }
    }

    public bool IsExecuting(string currentLine) {
        if (hasStopped) { return false; }

        if (!isActive) {
            if (startStringValue == null) {
                isActive = true;
            } else {
                if (startStringValue == currentLine) {
                    // Matched this line to start, but that means we start checking on the next line,
                    // so set active to true but return false from the method.
                    isActive = true;
                    return false;
                }
            }
        }

        if (isActive && endStringValue != null && EndString == currentLine) {
            isActive = false;
            hasStopped = true;
        }

        return isActive;
    }
}