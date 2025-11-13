namespace EasyBuy.Application.Common.Interfaces;

public interface ITokenService
{
    string GenerateAccessToken(Guid userId, string email, string userName, IList<string> roles);
    string GenerateRefreshToken();
    Task<string> ValidateRefreshTokenAsync(string refreshToken);
}
