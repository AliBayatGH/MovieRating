using Xunit;

namespace MovieRating.IntegrationTests.Fixtures;

[CollectionDefinition("TestCollection")]
public class TestCollection : ICollectionFixture<SharedTestContext>
{
    // This class has no code, and is never created. Its purpose is simply
    // to be the place to apply [CollectionDefinition] and all the
    // ICollectionFixture<> interfaces.
}