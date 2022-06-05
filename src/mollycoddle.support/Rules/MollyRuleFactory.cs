namespace mollycoddle {

    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text.Json;
    using Plisky.Diagnostics;

    public class MollyRuleFactory {
        Bilge b = new Bilge("molly-rules");

 

        public MollyRuleFactory() {
        }

        public IEnumerable<MollyRule> LoadRulesFromFile(string filename) {
            b.Info.Log($"Loading {filename}");
            
            if (!File.Exists(filename)) {
                b.Error.Log($"File {filename} not found, about to error");
                throw new FileNotFoundException("molly rules file was not found.", filename);                
            } else {
                string xtn = Path.GetExtension(filename).ToLowerInvariant();
                if (xtn == ".molly") {
                    b.Info.Log($"Rule File Loaded {filename}");
                    foreach (var r in LoadAllMollyRules(filename)) {
                        b.Verbose.Log($"File Loaded Rule {r.Identifier}");
                        yield return r;
                    }
                } else if (xtn == ".mollyset"){
                    b.Info.Log($"Rule Collection File Loaded {filename}");
                    foreach (string mfile in File.ReadAllLines(filename)) {
                        string mfile2 = Path.Combine(Path.GetDirectoryName(filename), mfile);
                        b.Verbose.Log($"Parsing Single Rulefile {mfile2}");
                        foreach (var r in LoadAllMollyRules(mfile2)) {
                            b.Verbose.Log($"File Loaded Rule {r.Identifier}");
                            yield return r;
                        }
                    }
                } else {
                    b.Error.Log($"File extension {xtn} is neither molly, nor mollyset therefore erroring");
                    throw new InvalidOperationException("The ruleset file type is not known and can not be loaded");
                }

            }

        }

        private IEnumerable<MollyRule> LoadAllMollyRules(string filename) {
            string json = File.ReadAllText(filename);
            var mrs = JsonSerializer.Deserialize<MollyRuleStorage>(json);

            if (mrs == null) {
                throw new InvalidOperationException($"Json did not result in valid MollyRuleStorage - Check [{filename}]");
            }
            if (mrs.Rules == null) {
                b.Warning.Log($"The file {filename} was loaded but no rules were returned");
                yield break;
            }

            foreach (var m in mrs.Rules) {
                var result = new MollyRule() {
                    Identifier = m.RuleReference,
                    Name = m.Name,
                    Link = m.Link
                };
                foreach(var vl in m.Validators) {
                    result.AddValidator(LoadValidatorStep(m.Name, vl));
                }
                yield return result;
            }
            b.Verbose.Log("Rules file load completed");
        }


        /// <summary>
        /// This loads the validator from the rule step.  The Rule step describes what is required for the rule and ths method will return a validator
        /// that has been loaded using the information in the rule step.
        /// </summary>
        /// <param name="ruleName">The name of the rule for violation reporting purposes.</param>
        /// <param name="nextRuleStep">The step which contains the configuration required.</param>
        /// <returns></returns>
        /// <exception cref="InvalidOperationException"></exception>
        public ValidatorBase LoadValidatorStep(string ruleName, MollyRuleStepStorage nextRuleStep) {
            switch (nextRuleStep.ValidatorName) {
                case DirectoryValidationChecks.VALIDATORNAME:
                    return CreateDirectoryValidator(ruleName, nextRuleStep);
                case FileValidationChecks.VALIDATORNAME:
                    return CreateFileValidatorFromConfiguration(ruleName, nextRuleStep);
                case NugetValidationChecks.VALIDATORNAME:
                    return CreateNugetValidatorFromConfiguration(ruleName, nextRuleStep);
                default:
                    throw new InvalidOperationException($"The validator [{nextRuleStep.ValidatorName}] was not recognised from the MollyRule file.");
            }
          
          
        }

        private ValidatorBase CreateNugetValidatorFromConfiguration(string ruleName, MollyRuleStepStorage nextRuleStep) {
            var vv = new NugetValidationChecks(ruleName);
            switch (nextRuleStep.Control) {
                case "ProhibitedPackagesList":
                    vv.AddProhibitedPackageList(nextRuleStep.PatternMatch, nextRuleStep.AdditionalData);
                    break;

                default:
                    b.Error.Log($"Invalid Control found in file {nextRuleStep.Control}");
                    throw new InvalidOperationException($"Json data [{nextRuleStep.Control}] is invalid for nuget MollyRule");
            }
            return vv;
        }

        private FileValidationChecks CreateFileValidatorFromConfiguration(string ruleName, MollyRuleStepStorage nextRuleStep) {
            var vv = new FileValidationChecks(ruleName);
            switch (nextRuleStep.Control) {
                case "MustExist":
                    vv.MustExist(nextRuleStep.PatternMatch);
                    break;

                case "MustNotExist":
                    vv.AddProhibitedPattern(nextRuleStep.PatternMatch, nextRuleStep.AdditionalData);
                    break;

                case "MatchWithMaster":
                    if (nextRuleStep.AdditionalData == null || !nextRuleStep.AdditionalData.Any()) {
                        throw new InvalidOperationException("Require additional data for master match rule in mollyrule file");
                    }
                    vv.MustMatchMaster(nextRuleStep.PatternMatch, nextRuleStep.AdditionalData.First());
                    break;
                case "IfExistMustBeHere":
                    if (nextRuleStep.AdditionalData == null || !nextRuleStep.AdditionalData.Any()) {
                        throw new InvalidOperationException("Require additional data for precise position rule in mollyrule file");
                    }
                    vv.MustBeInSpecificLocation(nextRuleStep.PatternMatch, nextRuleStep.AdditionalData);
                    break;
                default:
                    b.Error.Log($"Invalid Control found in file {nextRuleStep.Control}");
                    throw new InvalidOperationException($"Json data [{nextRuleStep.Control}] is invalid for file validation6 MollyRule");
            }

            return vv;
        }

        private ValidatorBase CreateDirectoryValidator(string ruleName, MollyRuleStepStorage nextRuleStep) {
            var vs = new DirectoryValidationChecks(ruleName);
            b.Verbose.Log($"Loading validator step {nextRuleStep.Control}");
            switch (nextRuleStep.Control) {
                case "MustNotExist":
                    vs.AddProhibitedPattern(nextRuleStep.PatternMatch);
                    break;

                case "MustExist":
                    vs.MustExist(nextRuleStep.PatternMatch);
                    break;

                case "ProhibitedExcept":
                    vs.AddProhibitedPattern(nextRuleStep.PatternMatch, nextRuleStep.AdditionalData);
                    break;
                default:
                    b.Error.Log($"Invalid Control found in file {nextRuleStep.Control}");
                    throw new InvalidOperationException("Json data is invalid for directory validation Mollyrule");
            }
            return vs;
        }

        public ValidatorBase LoadValidatorStep(string ruleName, string jsonContent) {
            var valstep = JsonSerializer.Deserialize<MollyRuleStepStorage>(jsonContent);
            if (valstep == null) {
                throw new InvalidOperationException("The Json Step Cant Not Be Loaded");
            }
            return LoadValidatorStep(ruleName, valstep);

            
        }
    }
}