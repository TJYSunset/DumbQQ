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
            private class Example : IClientExclusive
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

                private bool Equals(Example other)
                {
                    return Id == other.Id && Integer == other.Integer && Equals(List, other.List) &&
                           Equals(Client, other.Client);
                }

                public override bool Equals(object obj)
                {
                    if (ReferenceEquals(null, obj)) return false;
                    if (ReferenceEquals(this, obj)) return true;
                    return obj.GetType() == GetType() && Equals((Example) obj);
                }

                public override int GetHashCode()
                {
                    unchecked
                    {
                        var hashCode = Id;
                        hashCode = (hashCode * 397) ^ Integer;
                        hashCode = (hashCode * 397) ^ (List != null ? List.GetHashCode() : 0);
                        hashCode = (hashCode * 397) ^ (Client != null ? Client.GetHashCode() : 0);
                        return hashCode;
                    }
                }

                public static bool operator ==(Example left, Example right)
                {
                    return Equals(left, right);
                }

                public static bool operator !=(Example left, Example right)
                {
                    return !Equals(left, right);
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