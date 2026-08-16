using MongoDB.Driver;
using NexusBakery.Domain.Entities;
using NexusBakery.Domain.Enums;

namespace NexusBakery.Infrastructure.Persistence.Seeders;

public class DatabaseSeeder
{
    private readonly MongoDbContext _context;

    public DatabaseSeeder(MongoDbContext context)
    {
        _context = context;
    }

    public async Task SeedAsync()
    {
        await CreateIndexesAsync();
        await SeedUsersAsync();
        await SeedProductsAndInventoryAsync();
        await SeedTutorialsAsync();
        await SeedMetricsAsync();
    }

    private async Task CreateIndexesAsync()
    {
        // Users Indexes
        var userBuilder = Builders<User>.IndexKeys;
        await _context.Users.Indexes.CreateManyAsync(new[]
        {
            new CreateIndexModel<User>(userBuilder.Ascending(u => u.Email), new CreateIndexOptions { Unique = true, Sparse = true }),
            new CreateIndexModel<User>(userBuilder.Ascending(u => u.Phone), new CreateIndexOptions { Sparse = true }),
            new CreateIndexModel<User>(userBuilder.Ascending(u => u.GoogleId), new CreateIndexOptions { Sparse = true }),
            new CreateIndexModel<User>(userBuilder.Ascending(u => u.Role))
        });

        // Products Indexes
        var prodBuilder = Builders<Product>.IndexKeys;
        await _context.Products.Indexes.CreateManyAsync(new[]
        {
            new CreateIndexModel<Product>(prodBuilder.Ascending(p => p.Slug), new CreateIndexOptions { Unique = true }),
            new CreateIndexModel<Product>(prodBuilder.Ascending(p => p.Sku), new CreateIndexOptions { Unique = true }),
            new CreateIndexModel<Product>(prodBuilder.Ascending(p => p.Category).Ascending(p => p.SubCategory)),
            new CreateIndexModel<Product>(prodBuilder.Ascending(p => p.Price)),
            new CreateIndexModel<Product>(prodBuilder.Ascending(p => p.IsFeatured).Ascending(p => p.IsAvailable))
        });

        // Tutorials Indexes
        var tutBuilder = Builders<Tutorial>.IndexKeys;
        await _context.Tutorials.Indexes.CreateManyAsync(new[]
        {
            new CreateIndexModel<Tutorial>(tutBuilder.Ascending(t => t.Slug), new CreateIndexOptions { Unique = true }),
            new CreateIndexModel<Tutorial>(tutBuilder.Ascending(t => t.Category).Ascending(t => t.SkillLevel)),
            new CreateIndexModel<Tutorial>(tutBuilder.Descending(t => t.ViewCount))
        });

        // Orders Indexes
        var orderBuilder = Builders<Order>.IndexKeys;
        await _context.Orders.Indexes.CreateManyAsync(new[]
        {
            new CreateIndexModel<Order>(orderBuilder.Ascending(o => o.OrderNumber), new CreateIndexOptions { Unique = true }),
            new CreateIndexModel<Order>(orderBuilder.Ascending(o => o.UserId).Descending(o => o.CreatedAt)),
            new CreateIndexModel<Order>(orderBuilder.Ascending(o => o.PaymentStatus)),
            new CreateIndexModel<Order>(orderBuilder.Ascending(o => o.OrderStatus))
        });

        // Warehouse Indexes
        var whBuilder = Builders<WarehouseInventory>.IndexKeys;
        await _context.WarehouseInventories.Indexes.CreateManyAsync(new[]
        {
            new CreateIndexModel<WarehouseInventory>(whBuilder.Ascending(w => w.ProductId), new CreateIndexOptions { Unique = true }),
            new CreateIndexModel<WarehouseInventory>(whBuilder.Ascending(w => w.Sku), new CreateIndexOptions { Unique = true }),
            new CreateIndexModel<WarehouseInventory>(whBuilder.Ascending(w => w.IsLowStock))
        });
    }

    private async Task SeedUsersAsync()
    {
        if (await _context.Users.CountDocumentsAsync(_ => true) > 0) return;

        var defaultPasswordHash = BCrypt.Net.BCrypt.HashPassword("NexusBakery@2026");

        var users = new List<User>
        {
            new()
            {
                FullName = "System Super Admin",
                Email = "sysadmin@nexusbakery.com",
                EmailVerified = true,
                Phone = "+8801700000001",
                PasswordHash = defaultPasswordHash,
                Role = UserRole.SystemAdmin,
                AvatarUrl = "https://images.unsplash.com/photo-1534528741775-53994a69daeb?w=150",
                Subscription = new UserSubscriptionInfo
                {
                    Tier = SubscriptionTier.VipBakerPass,
                    IsActive = true,
                    ExpiresAt = DateTime.UtcNow.AddYears(10)
                }
            },
            new()
            {
                FullName = "Chef Rahim (Store Admin)",
                Email = "admin@nexusbakery.com",
                EmailVerified = true,
                Phone = "+8801700000002",
                PasswordHash = defaultPasswordHash,
                Role = UserRole.Admin,
                AvatarUrl = "https://images.unsplash.com/photo-1507003211169-0a1dd7228f2d?w=150",
                Subscription = new UserSubscriptionInfo
                {
                    Tier = SubscriptionTier.MasterclassPro,
                    IsActive = true,
                    ExpiresAt = DateTime.UtcNow.AddYears(5)
                }
            },
            new()
            {
                FullName = "Sarah Baker",
                Email = "sarah@nexusbakery.com",
                EmailVerified = true,
                Phone = "+8801700000003",
                PasswordHash = defaultPasswordHash,
                Role = UserRole.User,
                AvatarUrl = "https://images.unsplash.com/photo-1494790108377-be9c29b29330?w=150",
                Subscription = new UserSubscriptionInfo
                {
                    Tier = SubscriptionTier.MasterclassPro,
                    IsActive = true,
                    ExpiresAt = DateTime.UtcNow.AddMonths(6)
                },
                ShippingAddresses = new List<ShippingAddress>
                {
                    new()
                    {
                        Label = "Home",
                        FullAddress = "House 12, Road 4, Dhanmondi",
                        City = "Dhaka",
                        PostalCode = "1205",
                        Phone = "+8801700000003",
                        IsDefault = true
                    }
                }
            }
        };

        await _context.Users.InsertManyAsync(users);
    }

    private async Task SeedProductsAndInventoryAsync()
    {
        if (await _context.Products.CountDocumentsAsync(_ => true) > 0) return;

        var products = new List<Product>
        {
            // === ELECTRONICS ===
            new()
            {
                Title = "Professional 7-Speed Stand Mixer – Titanium Silver",
                Slug = "professional-7-speed-stand-mixer-titanium-silver",
                Category = ProductCategory.Electronics,
                SubCategory = "Mixers & Blenders",
                Description = "High-torque 800W motor engineered for heavy bread dough and delicate airy meringues. Includes 4.8L stainless steel bowl, dough hook, wire whisk, and flex-edge beater.",
                ShortDescription = "800W high-torque stand mixer with 4.8L stainless steel bowl.",
                Price = 14999.00m,
                CompareAtPrice = 18500.00m,
                Sku = "ELEC-MIX-001",
                WarehouseStock = 45,
                IsAvailable = true,
                IsFeatured = true,
                AverageRating = 4.9,
                ReviewCount = 184,
                Images = new List<ProductImage>
                {
                    new() { Url = "https://images.unsplash.com/photo-1594385208974-2e75f8d7bb48?w=800", Alt = "Stand Mixer Titanium Silver", IsPrimary = true }
                },
                Specifications = new List<ProductSpecification>
                {
                    new() { Key = "Motor Power", Value = "800 Watts" },
                    new() { Key = "Bowl Capacity", Value = "4.8 Liters" },
                    new() { Key = "Speeds", Value = "7 Speeds + Pulse" },
                    new() { Key = "Warranty", Value = "2 Years Official" }
                },
                Tags = new List<string> { "stand-mixer", "electronics", "baking", "heavy-duty" }
            },
            new()
            {
                Title = "Ultra-Precision Digital Kitchen Scale (0.1g Precision)",
                Slug = "ultra-precision-digital-kitchen-scale",
                Category = ProductCategory.Electronics,
                SubCategory = "Scales & Thermometers",
                Description = "Essential for artisan sourdough, macarons, and pastry measurements. Reads down to 0.1g increments with tare function and backlit LCD display.",
                ShortDescription = "0.1g high-precision digital scale with stainless steel surface.",
                Price = 1450.00m,
                CompareAtPrice = 1800.00m,
                Sku = "ELEC-SCALE-002",
                WarehouseStock = 120,
                IsAvailable = true,
                IsFeatured = true,
                AverageRating = 4.8,
                ReviewCount = 92,
                Images = new List<ProductImage>
                {
                    new() { Url = "https://images.unsplash.com/photo-1590794056226-79ef3a8147e1?w=800", Alt = "Precision Scale", IsPrimary = true }
                },
                Specifications = new List<ProductSpecification>
                {
                    new() { Key = "Accuracy", Value = "0.1 grams" },
                    new() { Key = "Max Weight", Value = "3000g / 3kg" },
                    new() { Key = "Power", Value = "USB Rechargeable" }
                },
                Tags = new List<string> { "scale", "precision", "measuring", "tools" }
            },
            new()
            {
                Title = "Infrared Laser Food & Oven Surface Thermometer",
                Slug = "infrared-laser-food-thermometer",
                Category = ProductCategory.Electronics,
                SubCategory = "Scales & Thermometers",
                Description = "Non-contact laser thermometer for measuring baking stone temperature, chocolate tempering, and hot sugar syrups accurately within 0.5 seconds.",
                ShortDescription = "Instant laser temperature gun with -50°C to 550°C range.",
                Price = 2200.00m,
                CompareAtPrice = 2800.00m,
                Sku = "ELEC-THERM-003",
                WarehouseStock = 65,
                IsAvailable = true,
                IsFeatured = false,
                AverageRating = 4.7,
                ReviewCount = 45,
                Images = new List<ProductImage>
                {
                    new() { Url = "https://images.unsplash.com/photo-1584282479904-54c3cf7b2355?w=800", Alt = "Infrared Thermometer", IsPrimary = true }
                },
                Tags = new List<string> { "thermometer", "laser", "sugar-craft", "electronics" }
            },

            // === BAKING TOOLS ===
            new()
            {
                Title = "Premium Silicone Macaron Baking Mat (Set of 2)",
                Slug = "premium-silicone-macaron-baking-mat-set-of-2",
                Category = ProductCategory.BakingTools,
                SubCategory = "Molds & Mats",
                Description = "Heat-resistant platinum silicone mats with printed concentric guide rings for piping perfectly uniform French macarons, éclairs, and cookies.",
                ShortDescription = "Food-grade non-stick silicone mats with piping circles.",
                Price = 650.00m,
                CompareAtPrice = 850.00m,
                Sku = "TOOL-MAT-004",
                WarehouseStock = 180,
                IsAvailable = true,
                IsFeatured = true,
                AverageRating = 4.9,
                ReviewCount = 210,
                Images = new List<ProductImage>
                {
                    new() { Url = "https://images.unsplash.com/photo-1558961363-fa8fdf82db35?w=800", Alt = "Macaron Mats", IsPrimary = true }
                },
                Tags = new List<string> { "macaron", "silicone-mat", "baking-tools", "non-stick" }
            },
            new()
            {
                Title = "52-Piece Stainless Steel Piping Nozzles & Bag Kit",
                Slug = "52-piece-stainless-steel-piping-nozzles-kit",
                Category = ProductCategory.BakingTools,
                SubCategory = "Decorating Tools",
                Description = "Complete cake decorating kit featuring Russian piping tips, leaf nozzles, star tips, flower nails, coupler adapters, and reusable TPU silicone pastry bags.",
                ShortDescription = "52-piece professional cake decorating nozzle kit with case.",
                Price = 1250.00m,
                CompareAtPrice = 1600.00m,
                Sku = "TOOL-PIP-005",
                WarehouseStock = 95,
                IsAvailable = true,
                IsFeatured = true,
                AverageRating = 4.8,
                ReviewCount = 140,
                Images = new List<ProductImage>
                {
                    new() { Url = "https://images.unsplash.com/photo-1578985545062-69928b1d9587?w=800", Alt = "Piping Tips Kit", IsPrimary = true }
                },
                Tags = new List<string> { "piping-nozzles", "cake-decorating", "pastry-tools" }
            },
            new()
            {
                Title = "Heavy-Duty Aluminum Revolving Cake Turntable",
                Slug = "heavy-duty-aluminum-revolving-cake-turntable",
                Category = ProductCategory.BakingTools,
                SubCategory = "Decorating Tools",
                Description = "12-inch cast aluminum rotating cake stand with ultra-smooth dual ball bearings and non-slip rubber silicone base for flawless frosting application.",
                ShortDescription = "12-inch heavy-duty aluminum revolving turntable with silent bearing.",
                Price = 2400.00m,
                CompareAtPrice = 3000.00m,
                Sku = "TOOL-TURN-006",
                WarehouseStock = 40,
                IsAvailable = true,
                IsFeatured = false,
                AverageRating = 4.9,
                ReviewCount = 68,
                Images = new List<ProductImage>
                {
                    new() { Url = "https://images.unsplash.com/photo-1621303837174-89787a7d4729?w=800", Alt = "Cake Turntable", IsPrimary = true }
                },
                Tags = new List<string> { "turntable", "cake-decorating", "rotary-stand" }
            },

            // === BAKERY PHYSICAL GOODS ===
            new()
            {
                Title = "Artisan San Francisco Sourdough Boule (850g)",
                Slug = "artisan-san-francisco-sourdough-boule",
                Category = ProductCategory.Bakery,
                SubCategory = "Artisan Breads",
                Description = "Slow-fermented for 36 hours using our 10-year-old sourdough mother culture. Blistered golden crust, open airy crumb, and a deeply aromatic tangy flavor.",
                ShortDescription = "36-hour slow fermented wild sourdough boule baked on stone.",
                Price = 380.00m,
                Sku = "BAKE-BREAD-007",
                WarehouseStock = 30,
                IsAvailable = true,
                IsFeatured = true,
                AverageRating = 5.0,
                ReviewCount = 310,
                Images = new List<ProductImage>
                {
                    new() { Url = "https://images.unsplash.com/photo-1589367920969-ab8e050bbb04?w=800", Alt = "Sourdough Bread Boule", IsPrimary = true }
                },
                Tags = new List<string> { "sourdough", "bread", "fresh-bakery", "artisan" }
            },
            new()
            {
                Title = "French Pure Butter Croissant (Box of 4)",
                Slug = "french-pure-butter-croissant-box-of-4",
                Category = ProductCategory.Bakery,
                SubCategory = "Pastries & Croissants",
                Description = "Crafted with 82% Normandy cultured butter. 27 delicate lamination layers yielding a honeycomb interior and crisp flaky exterior.",
                ShortDescription = "Traditional French lamination with Normandy butter.",
                Price = 520.00m,
                Sku = "BAKE-PAST-008",
                WarehouseStock = 50,
                IsAvailable = true,
                IsFeatured = true,
                AverageRating = 4.9,
                ReviewCount = 195,
                Images = new List<ProductImage>
                {
                    new() { Url = "https://images.unsplash.com/photo-1555507036-ab1f4038808a?w=800", Alt = "French Butter Croissants", IsPrimary = true }
                },
                Tags = new List<string> { "croissant", "french-pastry", "butter", "bakery" }
            },
            new()
            {
                Title = "Belgian Dark Chocolate Ganache Gateau (1 kg)",
                Slug = "belgian-dark-chocolate-ganache-gateau",
                Category = ProductCategory.Bakery,
                SubCategory = "Cakes & Gateaux",
                Description = "Moist cocoa sponge layered with 70% Callebaut dark chocolate whipped ganache and glazed with glossy dark mirror glaze.",
                ShortDescription = "Rich 70% Belgian chocolate cake with mirror glaze.",
                Price = 1850.00m,
                Sku = "BAKE-CAKE-009",
                WarehouseStock = 20,
                IsAvailable = true,
                IsFeatured = true,
                AverageRating = 5.0,
                ReviewCount = 280,
                Images = new List<ProductImage>
                {
                    new() { Url = "https://images.unsplash.com/photo-1578985545062-69928b1d9587?w=800", Alt = "Chocolate Ganache Cake", IsPrimary = true }
                },
                Tags = new List<string> { "cake", "chocolate", "gateau", "celebration" }
            }
        };

        await _context.Products.InsertManyAsync(products);

        // Seed corresponding warehouse inventory records
        var inventoryRecords = products.Select(p => new WarehouseInventory
        {
            ProductId = p.Id,
            Sku = p.Sku,
            ProductTitle = p.Title,
            Category = p.Category,
            TotalStock = p.WarehouseStock,
            AvailableStock = p.WarehouseStock,
            ReservedStock = 0,
            LowStockThreshold = p.LowStockThreshold,
            IsLowStock = p.WarehouseStock <= p.LowStockThreshold,
            AdjustmentLog = new List<StockAdjustmentLogEntry>
            {
                new()
                {
                    AdjustedBy = "SYSTEM",
                    AdjustedByName = "Initial Inventory Seeder",
                    AdjustedByRole = "SystemAdmin",
                    PreviousStock = 0,
                    NewStock = p.WarehouseStock,
                    ChangeAmount = p.WarehouseStock,
                    Reason = "Initial batch inventory intake",
                    Timestamp = DateTime.UtcNow
                }
            }
        }).ToList();

        await _context.WarehouseInventories.InsertManyAsync(inventoryRecords);
    }

    private async Task SeedTutorialsAsync()
    {
        if (await _context.Tutorials.CountDocumentsAsync(_ => true) > 0) return;

        var tutorials = new List<Tutorial>
        {
            new()
            {
                Title = "Mastering French Macarons & Ganache Fillings",
                Slug = "mastering-french-macarons-ganache-fillings",
                Category = "Pastry & Macarons",
                SkillLevel = SkillLevel.Intermediate,
                DurationMinutes = 45,
                Description = "A complete 45-minute masterclass covering the Italian meringue method for foolproof macarons. Learn precise macaronage technique, resting times, and 3 signature ganache recipes.",
                Instructor = new TutorialInstructor
                {
                    Name = "Chef Aminul Haque",
                    Bio = "Pastry Chef with 15 years experience in Parisian patisseries.",
                    AvatarUrl = "https://images.unsplash.com/photo-1577219491135-ce391730fb2c?w=150"
                },
                Thumbnail = "https://images.unsplash.com/photo-1569864358642-9d1684040f43?w=800",
                VideoUrl = "https://commondatastorage.googleapis.com/gtv-videos-bucket/sample/BigBuckBunny.mp4",
                PreviewVideoUrl = "https://commondatastorage.googleapis.com/gtv-videos-bucket/sample/ForBiggerBlazes.mp4",
                AccessType = "SubscriberOnly",
                OneTimePurchasePrice = 499.00m,
                ViewCount = 8420,
                AverageRating = 4.9,
                ReviewCount = 312,
                IsPublished = true,
                PublishedAt = DateTime.UtcNow.AddMonths(-2),
                Ingredients = new List<string>
                {
                    "200g Extra-fine Almond Flour",
                    "200g Confectioner's Powdered Sugar",
                    "150g Aged Egg Whites",
                    "150g Granulated Sugar (for sugar syrup)",
                    "200g Callebaut 70% Dark Chocolate"
                },
                Chapters = new List<VideoChapter>
                {
                    new() { TimestampSeconds = 0, TimestampDisplay = "00:00", Title = "Course Overview & Ingredient Science" },
                    new() { TimestampSeconds = 135, TimestampDisplay = "02:15", Title = "Preparing Italian Meringue & Sugar Syrup" },
                    new() { TimestampSeconds = 940, TimestampDisplay = "15:40", Title = "The Macaronage Technique (Folding to Lava Consistency)" },
                    new() { TimestampSeconds = 1690, TimestampDisplay = "28:10", Title = "Piping, Tapping, and Skin Formation" },
                    new() { TimestampSeconds = 2280, TimestampDisplay = "38:00", Title = "Baking & Dark Chocolate Ganache Assembly" }
                },
                Tags = new List<string> { "macaron", "french-pastry", "ganache", "masterclass" }
            },
            new()
            {
                Title = "The Ultimate Wild Sourdough Bread Guide",
                Slug = "the-ultimate-wild-sourdough-bread-guide",
                Category = "Artisan Breads",
                SkillLevel = SkillLevel.Beginner,
                DurationMinutes = 60,
                Description = "Learn how to build a vigorous sourdough starter, calculate baker's percentages, perform coil folds, score decorative patterns, and bake with steam.",
                Instructor = new TutorialInstructor
                {
                    Name = "Chef Aminul Haque",
                    Bio = "Master Baker & Sourdough Fermentation Specialist.",
                    AvatarUrl = "https://images.unsplash.com/photo-1577219491135-ce391730fb2c?w=150"
                },
                Thumbnail = "https://images.unsplash.com/photo-1589367920969-ab8e050bbb04?w=800",
                VideoUrl = "https://commondatastorage.googleapis.com/gtv-videos-bucket/sample/ElephantsDream.mp4",
                PreviewVideoUrl = "https://commondatastorage.googleapis.com/gtv-videos-bucket/sample/ForBiggerEscapes.mp4",
                AccessType = "SubscriberOnly",
                OneTimePurchasePrice = 599.00m,
                ViewCount = 12500,
                AverageRating = 5.0,
                ReviewCount = 420,
                IsPublished = true,
                PublishedAt = DateTime.UtcNow.AddMonths(-3),
                Ingredients = new List<string>
                {
                    "500g High-Protein Bread Flour (13% protein)",
                    "375g Water (75% hydration)",
                    "100g Active Sourdough Starter",
                    "10g Fine Sea Salt"
                },
                Chapters = new List<VideoChapter>
                {
                    new() { TimestampSeconds = 0, TimestampDisplay = "00:00", Title = "Understanding Wild Yeast & Levain Build" },
                    new() { TimestampSeconds = 300, TimestampDisplay = "05:00", Title = "Autolyse & Dough Mixing" },
                    new() { TimestampSeconds = 1200, TimestampDisplay = "20:00", Title = "Bulk Fermentation & Coil Folds" },
                    new() { TimestampSeconds = 2400, TimestampDisplay = "40:00", Title = "Pre-shaping and Final Banneton Shaping" },
                    new() { TimestampSeconds = 3100, TimestampDisplay = "51:40", Title = "Lame Scoring Techniques & Dutch Oven Baking" }
                },
                Tags = new List<string> { "sourdough", "artisan-bread", "fermentation", "baking" }
            },
            new()
            {
                Title = "Modern Buttercream Palette Knife Cake Artistry",
                Slug = "modern-buttercream-palette-knife-cake-artistry",
                Category = "Cake Artistry",
                SkillLevel = SkillLevel.Advanced,
                DurationMinutes = 50,
                Description = "Transform standard frosted cakes into painterly canvases using Swiss meringue buttercream, artist palette knives, and natural botanical color palettes.",
                Instructor = new TutorialInstructor
                {
                    Name = "Elena Rostova",
                    Bio = "International Cake Sculptor & Floral Buttercream Pioneer.",
                    AvatarUrl = "https://images.unsplash.com/photo-1544005313-94ddf0286df2?w=150"
                },
                Thumbnail = "https://images.unsplash.com/photo-1535141192574-5d4897c13136?w=800",
                VideoUrl = "https://commondatastorage.googleapis.com/gtv-videos-bucket/sample/TearsOfSteel.mp4",
                PreviewVideoUrl = "https://commondatastorage.googleapis.com/gtv-videos-bucket/sample/ForBiggerFun.mp4",
                AccessType = "SubscriberOnly",
                OneTimePurchasePrice = 699.00m,
                ViewCount = 6100,
                AverageRating = 4.9,
                ReviewCount = 185,
                IsPublished = true,
                PublishedAt = DateTime.UtcNow.AddMonths(-1),
                Chapters = new List<VideoChapter>
                {
                    new() { TimestampSeconds = 0, TimestampDisplay = "00:00", Title = "Preparing Stable Swiss Meringue Buttercream" },
                    new() { TimestampSeconds = 480, TimestampDisplay = "08:00", Title = "Color Blending with Gel and Oil Pigments" },
                    new() { TimestampSeconds = 1500, TimestampDisplay = "25:00", Title = "Palette Knife Petal Techniques (Ranunculus & Peonies)" },
                    new() { TimestampSeconds = 2520, TimestampDisplay = "42:00", Title = "Texturing, Gold Leaf, and Final Presentation" }
                },
                Tags = new List<string> { "cake-artistry", "palette-knife", "buttercream", "advanced" }
            }
        };

        await _context.Tutorials.InsertManyAsync(tutorials);
    }

    private async Task SeedMetricsAsync()
    {
        var existing = await _context.SiteMetrics.Find(x => x.Id == "000000000000000000000001").FirstOrDefaultAsync();
        if (existing == null)
        {
            var metrics = new SiteMetrics
            {
                Id = "000000000000000000000001",
                TotalLifetimeVisits = 14280,
                ActiveLiveUsers = 1,
                TotalRegisteredUsers = 3,
                TotalOrdersCompleted = 42,
                TotalRevenue = 185000.00m,
                DailySnapshots = new List<DailyMetricSnapshot>
                {
                    new()
                    {
                        Date = DateTime.UtcNow.ToString("yyyy-MM-dd"),
                        Visits = 450,
                        UniqueVisitors = 320,
                        NewRegistrations = 3,
                        OrdersPlaced = 6,
                        Revenue = 32500.00m
                    }
                }
            };
            await _context.SiteMetrics.InsertOneAsync(metrics);
        }
    }
}
