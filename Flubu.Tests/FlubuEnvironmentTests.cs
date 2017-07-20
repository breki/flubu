using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;

namespace Flubu.Tests
{
    public class FlubuEnvironmentTests
    {
        [Test]

        // ReSharper disable once InconsistentNaming
        public void ListMSBuilds()
        {
            IDictionary<Version, string> msbuilds =
                FlubuEnvironment.ListAvailableMSBuildToolsVersions();
            Assert.NotNull(msbuilds);
            CollectionAssert.IsNotEmpty(msbuilds);

            AssertAtLeastOneKnownVersionWasFound(msbuilds);
            AssertVersionsAreSorted(msbuilds);
            AssertAllToolsPathsAreNonNullAndNonEmpty(msbuilds);
        }

        [Test, Explicit]
        public void FindMSBuild15()
        {
            IDictionary<Version, string> msbuilds = FlubuEnvironment.ListAvailableMSBuildToolsVersions();
            CollectionAssert.Contains(msbuilds.Keys, new Version("15.0"));
            Assert.AreEqual(
                "C:\\Program Files (x86)\\Microsoft Visual Studio\\2017\\Community\\MSBuild\\15.0\\Bin",
                msbuilds[new Version("15.0")]);
        }

        private static void AssertAtLeastOneKnownVersionWasFound(IDictionary<Version, string> msbuilds)
        {
            HashSet<Version> versions = new HashSet<Version>(msbuilds.Keys);
            CollectionAssert.IsNotEmpty(versions.Intersect(new[] { new Version("2.0"), new Version("3.5"), new Version("4.0"), new Version("12.0") }));
        }

        private static void AssertVersionsAreSorted(IDictionary<Version, string> msbuilds)
        {
            List<Version> versions = new List<Version>();
            foreach (KeyValuePair<Version, string> value in msbuilds)
                versions.Add(value.Key);

            for (int i = 0; i < versions.Count - 1; i++)
            {
                Version version1 = versions[i];
                Version version2 = versions[i+1];
                Assert.LessOrEqual(version1, version2);
            }
        }

        // ReSharper disable once UnusedParameter.Local
        private static void AssertAllToolsPathsAreNonNullAndNonEmpty(IDictionary<Version, string> msbuilds)
        {
            // assert that are all tools versions paths are not null
            Assert.IsTrue(msbuilds.Values.All(x => !string.IsNullOrEmpty(x)));
        }
    }
}