# Domain

Place business entities, value objects, and business rules here. This layer has no project or external package dependencies.

`User` stores the CookEase profile, and `FridgeItem` stores an owned inventory entry. Storage locations and measurement units are domain enums. Authentication accounts live in Infrastructure and share their IDs with domain users. Keep ASP.NET Core, Entity Framework Core, and external API clients out of this layer.
