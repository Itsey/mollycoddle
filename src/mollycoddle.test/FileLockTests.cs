using System;
using System.IO;
using Plisky.Diagnostics;
using Plisky.Test;
using Shouldly;
using Xunit;

namespace mollycoddle.test;

public class FileLockTests {
    protected const string HANDLERNAME = "test-handler";
    private Bilge b = new Bilge();
    private byte[] fileData = System.Security.Cryptography.RandomNumberGenerator.GetBytes(1000);
    private MollyOptions mo;
    private UnitTestHelper u;

    public FileLockTests() {
        b.Info.Flow();
        u = new UnitTestHelper();
        mo = new MollyOptions();
    }

    [Fact(DisplayName = nameof(File_does_not_lock_when_filecontents_unchanged))]
    [Trait(Traits.Style, Traits.Integration)]
    [Trait(Traits.LiveBug, "Issue#1")]
    public void File_does_not_lock_when_filecontents_unchanged() {
        b.Info.Flow();
        bool didFileLockRetryOccur = false;
        var sut = new NexusSupportShim(mo);
        var nsb = sut.GetBilge();
        FileStream? fl = null;

        Action<IBilgeActionEvent> act = (a) => {
            didFileLockRetryOccur = true;
        };

        nsb.Action.RegisterHandler(act, HANDLERNAME);

        string targetFilename = u.NewTemporaryFileName();
        File.WriteAllBytes(targetFilename, fileData);

        try {
            fl = File.Open(targetFilename, FileMode.OpenOrCreate, FileAccess.Read, FileShare.Read);

            sut.PhysicallyWriteFile(fileData, targetFilename);

            fl.Close();

            File.Exists(targetFilename).ShouldBeTrue();
            didFileLockRetryOccur.ShouldBeFalse();
        } finally {
            nsb.Action.UnregisterHandler(act, HANDLERNAME);
            u.ClearUpTestFiles();
        }
    }

    [Fact(DisplayName = nameof(File_lock_retry_allows_update_when_locked))]
    [Trait(Traits.Style, Traits.Integration)]
    [Trait(Traits.LiveBug, "#LFY-60")]
    public void File_lock_retry_allows_update_when_locked() {
        b.Info.Flow();
        bool didFileLockRetryOccur = false;
        var sut = new NexusSupportShim(mo);
        var nsb = sut.GetBilge();
        FileStream? fl = null;

        Action<IBilgeActionEvent> act = (a) => {
            didFileLockRetryOccur = true;
            fl?.Close();
        };

        nsb.Action.RegisterHandler(act, HANDLERNAME);

        string targetFilename = u.NewTemporaryFileName();

        try {
            fl = File.Open(targetFilename, FileMode.OpenOrCreate, FileAccess.Write, FileShare.Read);

            sut.PhysicallyWriteFile(fileData, targetFilename);

            File.Exists(targetFilename).ShouldBeTrue();
            didFileLockRetryOccur.ShouldBeTrue();
        } finally {
            nsb.Action.UnregisterHandler(act, HANDLERNAME);
            u.ClearUpTestFiles();
        }
    }

    [Fact(DisplayName = nameof(Molly_file_reads_do_not_lock_other_reads))]
    [Trait(Traits.Style, Traits.Integration)]
    [Trait(Traits.LiveBug, "Issue#1")]
    public void Molly_file_reads_do_not_lock_other_reads() {
        b.Info.Flow();

        var sut = new MollyRuleFactory();
        string targetFilename = u.NewTemporaryFileName();
        var nss = new NexusSupportShim(mo);
        bool didFileLockRetryOccur = false;
        FileStream? fl = null;
        var nsb = nss.GetBilge();

        Action<IBilgeActionEvent> act = (a) => {
            didFileLockRetryOccur = true;
            fl?.Close();
        };

        nsb.Action.RegisterHandler(act, HANDLERNAME);

        try {
            fl = File.Open(targetFilename, FileMode.Open, FileAccess.Write, FileShare.Read);
            sut.PhysicallyReadMollyRulefile(targetFilename);

            didFileLockRetryOccur.ShouldBeTrue();
            File.Exists(targetFilename).ShouldBeTrue();
        } finally {
            nsb.Action.UnregisterHandler(act, HANDLERNAME);
            u.ClearUpTestFiles();
        }
    }

    [Fact(DisplayName = nameof(Molly_file_reads_do_not_lock_other_reads))]
    [Trait(Traits.Style, Traits.Integration)]
    [Trait(Traits.LiveBug, "Issue#1")]
    public void Molly_read_retry_allows_lock_to_be_removed() {
        b.Info.Flow();

        var sut = new NexusSupport(mo);
        string targetFilename = u.NewTemporaryFileName();

        var fl = File.Open(targetFilename, FileMode.Open, FileAccess.Read, FileShare.Read);

        var mrf = new MollyRuleFactory();
        mrf.PhysicallyReadMollyRulefile(targetFilename);

        fl.Close();

        File.Exists(targetFilename).ShouldBeTrue();
        File.Delete(targetFilename);
        u.ClearUpTestFiles();
    }
}