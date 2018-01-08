using System.Collections.Generic;
using DumbQQ.Helpers;
using DumbQQ.Models.Abstract;
using DumbQQ.Models.Utilities;
using NUnit.Framework;

namespace DumbQQ.Test.Helpers
{
    [TestFixture]
    public class ExtensionMethodsTest
    {
        [TestFixture]
        public class SeparateDomainTest
        {
            [Test]
            public void SeparatingDomainAndPath()
            {
                Assert.That(ExtensionMethods.SeperateDomain(@"http://a/b"), Is.EqualTo((@"http://a", @"b")));
                Assert.That(ExtensionMethods.SeperateDomain(@"https://ccc/def/gh/"),
                    Is.EqualTo((@"https://ccc", @"def/gh/")));
            }
        }

        [TestFixture]
        public class ReassembleTest
        {
            private struct Example : IClientExclusive
            {
                public int Id { get; set; }

                [LazyProperty]
                public string IgnoreMe
                {
                    set => Assert.Fail($"Tried to set IgnoreMe with value {value}");
                }

                public int Integer { get; set; }
                public List<int> List { get; set; }
                public DumbQQClient Client { get; set; }

                public override string ToString()
                {
                    return $"#{Id}: Integer={Integer}, List={List?.Capacity}, ClientSet={Client != null}";
                }
            }

            private static readonly DumbQQClient ExampleClient = new DumbQQClient();
            private static readonly List<int> List0 = new List<int>(0);
            private static readonly List<int> List1 = new List<int>(1);
            private static readonly List<int> List2 = new List<int>(2);

            [Test]
            public void ReassemblingRegularObjects()
            {
                Assert.That(
                    new[]
                    {
                        new Example {Id = 0, Integer = 42},
                        new Example {Id = 1, Integer = 32767},
                        new Example {Id = 2, Integer = 0}
                    }.Reassemble(x => x.Id, ExampleClient, new[]
                    {
                        new Example {Id = 1, List = List1},
                        new Example {Id = 0, List = List0},
                        new Example {Id = 2, List = List2}
                    }).Values, Is.EquivalentTo(new[]
                    {
                        new Example {Id = 0, Integer = 42, List = List0, Client = ExampleClient},
                        new Example {Id = 1, Integer = 32767, List = List1, Client = ExampleClient},
                        new Example {Id = 2, Integer = 0, List = List2, Client = ExampleClient}
                    }));
            }
        }
    }
}