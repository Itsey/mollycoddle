namespace mollycoddle.test;
using System.Collections;
using System.Collections.Generic;


internal class NexusPrimaryFilesTestData : IEnumerable<object[]> {
    public IEnumerator<object[]> GetEnumerator() {
        yield return new object[] { "[NEXUS][U::a[P::b[R::plisky[G::/primaryfiles/default[L::http://20.254.238.148:8081/repository/plisky/primaryfiles/default/",
            new NexusConfig() {
                FilenameUrl = "",
                Username = "a",
                Password = "b",
                Url = "http://20.254.238.148:8081/repository/plisky/primaryfiles/default/",
                Server = "http://20.254.238.148:8081",
                Repository = "plisky",
                SearchPath = "/primaryfiles/default"
        } };

        yield return new object[] { "[NEXUS][U::xxx[P::yyyy[L::http://20.254.238.148:8081/repository/plisky/primaryfiles/default/",
         new NexusConfig() {
                FilenameUrl = "defaultrules.mollyset",
                Username = "xxx",
                Password = "yyyy",
                Url = "http://20.254.238.148:8081/repository/plisky/molly/default/",
                Server = "http://20.254.238.148:8081",
         } };


        yield return new object[] { "[NEXUS][U::a123$[P::##@@##[L::http://20.254.238.148:8081/repository/plisky/molly/default/defaultrules.mollyset",
         new NexusConfig() {
                FilenameUrl = "defaultrules.mollyset",
                Username = "a123$",
                Password = "##@@##",
                Url = "http://20.254.238.148:8081/repository/plisky/molly/default/defaultrules.mollyset",
                Server = "http://20.254.238.148:8081",
         } };


    }

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();


}


internal class NexusRulesetTestData : IEnumerable<object[]> {
    public IEnumerator<object[]> GetEnumerator() {
        yield return new object[] { "[NEXUS][U::a[P::b[R::plisky[G::/primaryfiles/default[L::http://20.254.238.148:8081/repository/",
            new NexusConfig() {
                FilenameUrl = "",
                Username = "a",
                Password = "b",
                Url = "http://20.254.238.148:8081/repository/",
                Server = "http://20.254.238.148:8081",
                Repository = "plisky",
                SearchPath = "/primaryfiles/default"
        } };

        yield return new object[] { "[NEXUS][U::xxx[P::yyyy[L::http://20.254.238.148:8081/repository/plisky/molly/default/defaultrules.mollyset",
         new NexusConfig() {
                FilenameUrl = "defaultrules.mollyset",
                Username = "xxx",
                Password = "yyyy",
                Url = "http://20.254.238.148:8081/repository/plisky/molly/default/defaultrules.mollyset",
                Server = "http://20.254.238.148:8081",
                Repository = "",
                SearchPath = ""
         } };


        yield return new object[] { "[NEXUS][U::a123$[P::##@@##[L::http://20.254.238.148:8081/repository/plisky/molly/default/defaultrules.mollyset",
         new NexusConfig() {
                FilenameUrl = "defaultrules.mollyset",
                Username = "a123$",
                Password = "##@@##",
                Url = "http://20.254.238.148:8081/repository/plisky/molly/default/defaultrules.mollyset",
                Server = "http://20.254.238.148:8081",
                Repository = "",
                SearchPath = ""
         } };


    }

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();


}
