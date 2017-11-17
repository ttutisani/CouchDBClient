using Moq;
using System;
using System.Linq;
using Xunit;

namespace CouchDB.Client.Tests.HandlersTests
{
    public sealed class When_Empty_Username_and_Password : HttpCouchDBHandlerTests
    {
        internal override HttpCouchDBHandler CreateSut(string baseUrl, IStatelessHttpClient httpClient)
        {
            return new AuthenticatedCouchDBHandler(null, null, baseUrl, httpClient);
        }

        internal override string CreateInitialBaseUrl()
        {
            return "http://anothervalidurl";
        }

        internal override string CreateExpectedBaseUrl()
        {
            return "http://anothervalidurl";
        }
    }

    public sealed class When_Empty_Username : HttpCouchDBHandlerTests
    {
        public static new object[] DataFor_Ctor_Requires_Arguments
        {
            get
            {
                return new Func<AuthenticatedCouchDBHandler>[]
                {
                    () => new AuthenticatedCouchDBHandler(null, "password123", "http://validurl", new Mock<IStatelessHttpClient>().Object),
                    () => new AuthenticatedCouchDBHandler(string.Empty, "password123", "http://validurl", new Mock<IStatelessHttpClient>().Object),
                    () => new AuthenticatedCouchDBHandler("   ", "password123", "http://validurl", new Mock<IStatelessHttpClient>().Object)
                }
                .Select(func => new object[] { func })
                .ToArray();
            }
        }

        [Theory]
        [MemberData(nameof(DataFor_Ctor_Requires_Arguments))]
        public override void Ctor_Requires_Arguments(Func<ICouchDBHandler> sutFactory)
        {
            //arrange / act / assert.
            Assert.Throws<ArgumentException>(sutFactory);
        }
    }

    public sealed class When_Empty_Password : HttpCouchDBHandlerTests
    {
        public static new object[] DataFor_Ctor_Requires_Arguments
        {
            get
            {
                return new Func<AuthenticatedCouchDBHandler>[]
                {
                    () => new AuthenticatedCouchDBHandler("user123", null, "http://validurl", new Mock<IStatelessHttpClient>().Object),
                    () => new AuthenticatedCouchDBHandler("user123", string.Empty, "http://validurl", new Mock<IStatelessHttpClient>().Object),
                    () => new AuthenticatedCouchDBHandler("user123", "   ", "http://validurl", new Mock<IStatelessHttpClient>().Object)
                }
                .Select(func => new object[] { func })
                .ToArray();
            }
        }

        [Theory]
        [MemberData(nameof(DataFor_Ctor_Requires_Arguments))]
        public override void Ctor_Requires_Arguments(Func<ICouchDBHandler> sutFactory)
        {
            //arrange / act / assert.
            Assert.Throws<ArgumentException>(sutFactory);
        }
    }

    public class When_Given_Username_and_Password : HttpCouchDBHandlerTests
    {
        internal override sealed HttpCouchDBHandler CreateSut(string baseUrl, IStatelessHttpClient httpClient)
        {
            return new AuthenticatedCouchDBHandler("user123", "password123", baseUrl, httpClient);
        }

        internal override string CreateInitialBaseUrl()
        {
            return "http://validurl";
        }

        internal override string CreateExpectedBaseUrl()
        {
            return "http://user123:password123@validurl";
        }
    }

    public class When_BaseUrl_Contains_Port : When_Given_Username_and_Password
    {
        internal override string CreateInitialBaseUrl()
        {
            return "http://validurl:1934";
        }

        internal override string CreateExpectedBaseUrl()
        {
            return "http://user123:password123@validurl:1934";
        }
    }
}
