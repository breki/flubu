using Flubu.Builds.VSSolutionBrowsing;
using NUnit.Framework;

namespace Flubu.Tests.BuildsTests
{
    public class VSSolutionTests
    {
        [Test]
        public void ParseSample2013Solution()
        {
            VSSolution.Load(TestHelper.GetRepositoryPath(@"data\samples\ScalableMaps.sln"));
        } 
    }
}