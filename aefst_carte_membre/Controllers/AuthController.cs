using aefst_carte_membre.DbContexts;
using aefst_carte_membre.Dtos;
using aefst_carte_membre.Identity;
using aefst_carte_membre.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

[ApiController]
[Route("api/auth")]
public class AuthController : ControllerBase
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IConfiguration _config;
    private readonly ITokenService _tokenService;
    private readonly AppDbContext _db;

    public AuthController(
        UserManager<ApplicationUser> userManager,
        IConfiguration config,
        ITokenService tokenService,
        AppDbContext db = null)
    {
        _userManager = userManager;
        _config = config;
        _tokenService = tokenService;
        _db = db;
    }

    //[HttpPost("login")]
    //public async Task<IActionResult> Login(LoginDto dto)
    //{
    //    var user = await _userManager.FindByEmailAsync(dto.Email);
    //    if (user == null)
    //        return Unauthorized("Identifiants invalides");

    //    if (!await _userManager.CheckPasswordAsync(user, dto.Password))
    //        return Unauthorized("Identifiants invalides");

    //    var roles = await _userManager.GetRolesAsync(user);

    //    var claims = new List<Claim>
    //    {
    //        new(ClaimTypes.NameIdentifier, user.Id),
    //        new(ClaimTypes.Email, user.Email!)
    //    };

    //    claims.AddRange(
    //        roles.Select(r => new Claim(ClaimTypes.Role, r))
    //    );

    //    var key = new SymmetricSecurityKey(
    //        Encoding.UTF8.GetBytes(_config["Jwt:Key"]!)
    //    );

    //    var token = new JwtSecurityToken(
    //        issuer: _config["Jwt:Issuer"],
    //        audience: _config["Jwt:Audience"],
    //        claims: claims,
    //        expires: DateTime.UtcNow.AddMinutes(
    //            int.Parse(_config["Jwt:ExpireMinutes"]!)
    //        ),
    //        signingCredentials:
    //            new SigningCredentials(key, SecurityAlgorithms.HmacSha256)
    //    );

    //    return Ok(new
    //    {
    //        token = new JwtSecurityTokenHandler().WriteToken(token)
    //    });
    //}

    [HttpPost("login")]
    public async Task<IActionResult> Login(LoginDto dto)
    {
        var user = await _userManager.FindByEmailAsync(dto.Email);
        if (user == null)
            return Unauthorized("Identifiants invalides");

        if (!await _userManager.CheckPasswordAsync(user, dto.Password))
            return Unauthorized("Identifiants invalides");

        var roles = await _userManager.GetRolesAsync(user);

        var claims = new List<Claim>
{
    new Claim(ClaimTypes.NameIdentifier, user.Id),
    new Claim(ClaimTypes.Email, user.Email!)
};

        foreach (var role in roles)
        {
            claims.Add(new Claim(ClaimTypes.Role, role));
        }


        var accessToken = _tokenService.GenerateAccessToken(user, roles);
        var refreshToken = _tokenService.GenerateRefreshToken(user.Id);

        _db.RefreshTokens.Add(refreshToken);
        await _db.SaveChangesAsync();

        return Ok(new
        {
            accessToken,
            refreshToken = refreshToken.Token,
            email = user.Email,
            roles = roles,
            mustChangePassword = user.MustChangePassword
        });
    }


    [Authorize]
    [HttpPost("change-password")]
    public async Task<IActionResult> ChangePassword(ChangePasswordDto dto)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var user = await _userManager.FindByIdAsync(userId!);

        if (user == null)
            return Unauthorized();

        var result = await _userManager.ChangePasswordAsync(
            user,
            dto.CurrentPassword,
            dto.NewPassword
        );

        if (!result.Succeeded)
            return BadRequest(result.Errors);

        user.MustChangePassword = false;
        await _userManager.UpdateAsync(user);

        return Ok(new { message = "Mot de passe modifié avec succès" });
    }


    [HttpPost("refresh")]
    public async Task<IActionResult> Refresh([FromBody] string refreshToken)
    {
        var token = await _db.RefreshTokens
            .Include(t => t.User)
            .FirstOrDefaultAsync(t => t.Token == refreshToken);

        if (token == null || token.IsRevoked || token.ExpiresAt < DateTime.UtcNow)
            return Unauthorized();

        var roles = await _userManager.GetRolesAsync(token.User);

        // Rotation
        token.IsRevoked = true;
        var newRefresh = _tokenService.GenerateRefreshToken(token.UserId);

        _db.RefreshTokens.Add(newRefresh);
        await _db.SaveChangesAsync();

        return Ok(new
        {
            accessToken = _tokenService.GenerateAccessToken(token.User, roles),
            refreshToken = newRefresh.Token
        });
    }

    [Authorize]
    [HttpPost("logout")]
    public async Task<IActionResult> Logout([FromBody] string refreshToken)
    {
        var token = await _db.RefreshTokens.FirstOrDefaultAsync(t => t.Token == refreshToken);
        if (token != null)
        {
            token.IsRevoked = true;
            await _db.SaveChangesAsync();
        }

        return Ok();
    }


    [Authorize]
    [HttpGet("secure-test")]
    public IActionResult Secure()
    {
        return Ok("Protégé");
    }

}
