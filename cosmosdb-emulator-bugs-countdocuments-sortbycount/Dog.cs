using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Driver.GeoJsonObjectModel;

namespace cosmosdb_emulator_bugs_countdocuments_sortbycount
{
    public class Dog
    {
        [BsonId]
        [BsonRepresentation(MongoDB.Bson.BsonType.ObjectId)]
        public string Id { get; set; }

        public bool Alive { get; set; }

        public string City { get; set; }

        public GeoJsonPoint<GeoJson2DGeographicCoordinates> Coordinates { get; set; }
    }
}
