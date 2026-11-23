namespace CookEase.Application.Users;

public interface ICurrentUser
{
    Guid? UserId { get; }
}
