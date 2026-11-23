using CookEase.Domain.Entities;
using Microsoft.AspNetCore.Identity;

namespace CookEase.Infrastructure.Identity;

public sealed class ApplicationUser : IdentityUser<Guid>
{
    public ApplicationUser()
    {
        Id = Guid.NewGuid();
    }

    public User? Profile { get; set; }
}
