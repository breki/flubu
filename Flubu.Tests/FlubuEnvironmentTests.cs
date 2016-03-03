using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;

namespace Flubu.Tests
{
    public class FlubuEnvironmentTests
    {
        [Test]
        public void ListMSBuilds()
        {
            IDictionary<Version, string> msbuilds = FlubuEnvironment.ListAvailableMSBuildToolsVersions();
            Assert.NotNull(msbuilds);
            CollectionAssert.IsNotEmpty(msbuilds);

            AssertAtLeastOneKnownVersionWasFound(msbuilds);
            AssertVersionsAreSorted(msbuilds);           
            AssertAllToolsPathsAreNonNullAndNonEmpty(msbuilds);
        }

        private static void AssertAtLeastOneKnownVersionWasFound(IDictionary<Version, string> msbuilds)
        {
            HashSet<Version> versions = new HashSet<Version>(msbuilds.Keys);
            Assert.IsTrue(versions.IsSubsetOf(new[] { new Version("2.0"), new Version("3.5"), new Version("4.0"), new Version("12.0") }));
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

        private static void AssertAllToolsPathsAreNonNullAndNonEmpty(IDictionary<Version, string> msbuilds)
        {
            // assert that are all tools versions paths are not null
            Assert.IsTrue(msbuilds.Values.All(x => !string.IsNullOrEmpty(x)));
        }
    }
}