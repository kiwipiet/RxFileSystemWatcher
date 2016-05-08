using System;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using Microsoft.Reactive.Testing;

namespace RxFileSystemWatcher.Tests
{
    [ExcludeFromCodeCoverage]
    public abstract class FileIntegrationTestsBase : ReactiveTest
    {
        protected string TempPath;

        [SetUp]
        public virtual void BeforeEachTest()
        {
            TempPath = Guid.NewGuid().ToString();
            Directory.CreateDirectory(TempPath);
        }

        [TearDown]
        public void AfterEachTest()
        {
            if (!Directory.Exists(TempPath))
            {
                return;
            }
            Directory.Delete(TempPath, true);
        }
    }
}