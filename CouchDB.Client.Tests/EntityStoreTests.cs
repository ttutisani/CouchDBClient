using System;
using Xunit;

namespace CouchDB.Client.Tests
{
    public sealed class EntityStoreTests
    {
        public void Ctor_Requires_DB()
        {
            Assert.Throws<ArgumentNullException>(() => new EntityStore(null));
        }
    }
}
