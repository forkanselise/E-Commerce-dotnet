---
name: setup-mongodb-collection
description: Guidelines for setting up a new MongoDB collection, BSON models, and index creation in the Nexus Bakery & Tech backend.
---

# Setting up a MongoDB Collection

When instructed to add a new database collection to the backend:

1. **Create the Entity Model:**
   - Location: `src/NexusBakery.Domain/Entities/`
   - Class must use `[BsonId]` and `[BsonRepresentation(BsonType.ObjectId)]` for the primary key `Id`.
   - Always include `CreatedAt` and `UpdatedAt` properties (type `DateTime`).

2. **Configure the DbContext:**
   - Location: `src/NexusBakery.Infrastructure/Persistence/MongoDbContext.cs`
   - Add `public IMongoCollection<YourEntity> YourEntities => _database.GetCollection<YourEntity>("your_collection_name");`
   - Use snake_case for the actual MongoDB collection name.

3. **Define Indexes:**
   - Location: `src/NexusBakery.Infrastructure/Persistence/Seeders/DatabaseSeeder.cs`
   - Add the index creation logic inside the `CreateIndexesAsync()` method to ensure fast querying (e.g., unique constraints on slugs or SKUs).

4. **Repository Implementation:**
   - Extend the base `MongoRepository<T>` and override methods if atomic operations are needed (e.g., using `$inc` or `$push`).
