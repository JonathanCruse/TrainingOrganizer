using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Serializers;
using TrainingOrganizer.Infrastructure.Persistence.Documents;

namespace TrainingOrganizer.Infrastructure.Persistence.Serialization;

/// <summary>
/// Registers BSON class maps for all document types used by the MongoDB persistence layer.
/// Since we use a Document pattern (separate BSON-serializable classes rather than mapping
/// domain objects directly), the class maps are straightforward.
/// </summary>
public static class MongoDbClassMaps
{
    private static bool _registered;
    private static readonly object Lock = new();

    public static void RegisterAll()
    {
        lock (Lock)
        {
            if (_registered) return;

            // Register global serializers for common types
            RegisterGlobalSerializers();

            // Register document class maps
            RegisterDocumentClassMaps();

            _registered = true;
        }
    }

    private static void RegisterGlobalSerializers()
    {
        // Store GUIDs as standard UUID representation
        BsonSerializer.TryRegisterSerializer(new GuidSerializer(GuidRepresentation.Standard));

        // Store DateTimeOffset as document with DateTime + Offset
        BsonSerializer.TryRegisterSerializer(new DateTimeOffsetSerializer(BsonType.Document));
    }

    private static void RegisterDocumentClassMaps()
    {
        if (!BsonClassMap.IsClassMapRegistered(typeof(MemberDocument)))
        {
            BsonClassMap.RegisterClassMap<MemberDocument>(cm =>
            {
                cm.AutoMap();
                cm.SetIgnoreExtraElements(true);
            });
        }

        if (!BsonClassMap.IsClassMapRegistered(typeof(TrainingDocument)))
        {
            BsonClassMap.RegisterClassMap<TrainingDocument>(cm =>
            {
                cm.AutoMap();
                cm.SetIgnoreExtraElements(true);
            });
        }

        if (!BsonClassMap.IsClassMapRegistered(typeof(RecurringTrainingDocument)))
        {
            BsonClassMap.RegisterClassMap<RecurringTrainingDocument>(cm =>
            {
                cm.AutoMap();
                cm.SetIgnoreExtraElements(true);
            });
        }

        if (!BsonClassMap.IsClassMapRegistered(typeof(TrainingSessionDocument)))
        {
            BsonClassMap.RegisterClassMap<TrainingSessionDocument>(cm =>
            {
                cm.AutoMap();
                cm.SetIgnoreExtraElements(true);
            });
        }

        if (!BsonClassMap.IsClassMapRegistered(typeof(LocationDocument)))
        {
            BsonClassMap.RegisterClassMap<LocationDocument>(cm =>
            {
                cm.AutoMap();
                cm.SetIgnoreExtraElements(true);
            });
        }

        if (!BsonClassMap.IsClassMapRegistered(typeof(BookingDocument)))
        {
            BsonClassMap.RegisterClassMap<BookingDocument>(cm =>
            {
                cm.AutoMap();
                cm.SetIgnoreExtraElements(true);
            });
        }

        if (!BsonClassMap.IsClassMapRegistered(typeof(RoomDocument)))
        {
            BsonClassMap.RegisterClassMap<RoomDocument>(cm =>
            {
                cm.AutoMap();
                cm.SetIgnoreExtraElements(true);
            });
        }

        if (!BsonClassMap.IsClassMapRegistered(typeof(ParticipantDocument)))
        {
            BsonClassMap.RegisterClassMap<ParticipantDocument>(cm =>
            {
                cm.AutoMap();
                cm.SetIgnoreExtraElements(true);
            });
        }

        if (!BsonClassMap.IsClassMapRegistered(typeof(RoomRequirementDocument)))
        {
            BsonClassMap.RegisterClassMap<RoomRequirementDocument>(cm =>
            {
                cm.AutoMap();
                cm.SetIgnoreExtraElements(true);
            });
        }

        if (!BsonClassMap.IsClassMapRegistered(typeof(SessionOverridesDocument)))
        {
            BsonClassMap.RegisterClassMap<SessionOverridesDocument>(cm =>
            {
                cm.AutoMap();
                cm.SetIgnoreExtraElements(true);
            });
        }
    }
}
