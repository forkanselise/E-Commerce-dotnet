using Microsoft.Extensions.Configuration;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Conventions;
using MongoDB.Driver;
using NexusBakery.Domain.Entities;

namespace NexusBakery.Infrastructure.Persistence;

public class MongoDbContext
{
    private readonly IMongoDatabase _database;

    public MongoDbContext(IConfiguration configuration)
    {
        // Register camelCase & enum string representation conventions
        var conventionPack = new ConventionPack
        {
            new CamelCaseElementNameConvention(),
            new EnumRepresentationConvention(BsonType.String),
            new IgnoreIfNullConvention(true)
        };
        ConventionRegistry.Register("NexusBakeryConventions", conventionPack, _ => true);

        var connectionString = configuration["MONGODB_URI"] 
            ?? configuration.GetConnectionString("MongoDb")
            ?? Environment.GetEnvironmentVariable("MONGODB_URI")
            ?? "mongodb://localhost:27017";

        var databaseName = configuration["MONGODB_DB_NAME"]
            ?? configuration["MongoDb:DatabaseName"]
            ?? Environment.GetEnvironmentVariable("MONGODB_DB_NAME")
            ?? "nexus_bakery";

        var mongoUrl = new MongoUrl(connectionString);
        var mongoClientSettings = MongoClientSettings.FromUrl(mongoUrl);
        mongoClientSettings.ServerSelectionTimeout = TimeSpan.FromSeconds(10);
        if (mongoClientSettings.UseTls || connectionString.Contains("mongodb+srv") || connectionString.Contains("ssl=true"))
        {
            mongoClientSettings.SslSettings = new SslSettings
            {
                CheckCertificateRevocation = false,
                EnabledSslProtocols = System.Security.Authentication.SslProtocols.Tls12,
                ServerCertificateValidationCallback = (sender, certificate, chain, sslPolicyErrors) => true
            };
            mongoClientSettings.AllowInsecureTls = true;
        }

        var client = new MongoClient(mongoClientSettings);
        _database = client.GetDatabase(databaseName);
    }

    public IMongoDatabase Database => _database;

    public IMongoCollection<User> Users => _database.GetCollection<User>("users");
    public IMongoCollection<Product> Products => _database.GetCollection<Product>("products");
    public IMongoCollection<Tutorial> Tutorials => _database.GetCollection<Tutorial>("tutorials");
    public IMongoCollection<Order> Orders => _database.GetCollection<Order>("orders");
    public IMongoCollection<Subscription> Subscriptions => _database.GetCollection<Subscription>("subscriptions");
    public IMongoCollection<WarehouseInventory> WarehouseInventories => _database.GetCollection<WarehouseInventory>("warehouse_inventory");
    public IMongoCollection<SiteMetrics> SiteMetrics => _database.GetCollection<SiteMetrics>("site_metrics");
}
