using FluentAssertions;
using MongoDB.Driver;
using MongoDB.Driver.GeoJsonObjectModel;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace cosmosdb_emulator_bugs_countdocuments_sortbycount
{
    public class EmulatorTests
    {
        // CosmosDb emulator started with 'Microsoft.Azure.Cosmos.Emulator.exe /startwprtraces /EnableMongoDbEndpoint=3.6'

        // All tests fail with emulator
        private static string CONNECTION_STRNG = "mongodb://localhost:C2y6yDjf5%2FR%2Bob0N8A7Cgv30VRDJIWEHLM%2B4QDU5DE2nQ9nDuVTqobD4b8mGGyPMbIZnqyMsEcaGQy67XIw%2FJw%3D%3D@localhost:10255/admin?ssl=true&retryWrites=false";

        // All tests success with real cosmosdb
        //private static string CONNECTION_STRNG = "real cosmosdb connection string";

        private readonly IMongoCollection<Dog> _collection;

        public EmulatorTests()
        {
            _collection = SetupDb();
            AddDogs();
        }

        private void AddDogs()
        {
            _collection.InsertMany(new List<Dog>()
            {
                new Dog()
                {
                    Alive = false,
                    City = "Paris",
                    Coordinates = new GeoJsonPoint<GeoJson2DGeographicCoordinates>(
                        new GeoJson2DGeographicCoordinates(50, 4)
                    )
                },
                new Dog()
                {
                    Alive = true,
                    City = "Paris",
                    Coordinates = new GeoJsonPoint<GeoJson2DGeographicCoordinates>(
                        new GeoJson2DGeographicCoordinates(50, 4)
                    )
                },
                new Dog()
                {
                    Alive = true,
                    City = "New-York",
                    Coordinates = new GeoJsonPoint<GeoJson2DGeographicCoordinates>(
                        new GeoJson2DGeographicCoordinates(5, 0)
                    )
                }
            });
        }

        private IMongoCollection<Dog> SetupDb()
        {
            var mongoClient = new MongoClient(CONNECTION_STRNG);
            IMongoDatabase db = mongoClient.GetDatabase("dogs-db");
            IMongoCollection<Dog> collection = db.GetCollection<Dog>("dogs-collection");

            collection.Indexes.CreateOne(new CreateIndexModel<Dog>(Builders<Dog>.IndexKeys.Geo2DSphere(d => d.Coordinates)));

            collection.DeleteMany(d => true);

            return collection;
        }

        [Fact]
        public async Task CountDocuments_TestAsync()
        {
            // arrange

            // act
            int nbMatched = (int)await _collection.Find(d => d.Alive).CountDocumentsAsync();

            // assert
            nbMatched.Should().Be(2);
        }

        [Fact]
        public async Task SortByCount_TestAsync()
        {
            // arrange

            // act
            List<AggregateSortByCountResult<string>> sortedDogsByCity = await _collection.Aggregate().SortByCount(d => d.City).ToListAsync();

            // assert
            sortedDogsByCity.First(d => d.Id.Equals("Paris")).Count.Should().Be(2);
            sortedDogsByCity.First(d => d.Id.Equals("New-York")).Count.Should().Be(1);
        }
    }
}
