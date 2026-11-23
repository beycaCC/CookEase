# Application

Place use cases, request/response models, and interfaces for persistence or external services here. This layer depends only on Domain.

`ICurrentUser` exposes the authenticated user's ID without depending on HTTP. Infrastructure implements it using the request's authenticated claims. API endpoints will call application use cases as they are added. Keep HTTP handling and database implementations in the outer layers.
