using Ardalis.Result;
using FluentValidation;
using DailySpin.Application;
using DailySpin.Domain;

namespace DailySpin.WebApi;

public sealed class SignInEndpoint : IController
{
    private readonly IUserService _userService;
    private readonly IUserBusinessRulePredicates _rulePredicates;

    public SignInEndpoint(IUserService userService, IUserBusinessRulePredicates rulePredicates)
    {
        _userService = userService;
        _rulePredicates = rulePredicates;
    }

    [HttpPost]
    [Route("/api/auth/signin")]
    public async Task<IActionResult> SignIn([FromBody] SignInRequest request, HttpContext context)
    {
        var validator = new SignInValidator(_rulePredicates);
        var validationResult = await validator.ValidateAsync(request);

        if (!validationResult.IsValid)
        {
            var errors = string.Join("; ", validationResult.Errors.Select(e => e.ErrorMessage));
            return this.BadRequest(errors);
        }

        Result<JwtTokenPair> result = request switch
        {
            { Username: not null } => await _userService.SignInAsync(request.Username, request.Password),
            _ => Result<JwtTokenPair>.Error("Username must be set")
        };

        if (!result.IsSuccess)
        {
            return this.BadRequest(string.Join("; ", result.Errors));
        }

        var refreshCookie = $"{JwtTokenConstants.REFRESH_TOKEN_TYPE}={result.Value.RefreshToken}; " +
                        $"HttpOnly; Secure; SameSite=Strict; Expires={DateTime.UtcNow.AddDays(7):R}";

        context.Response.Headers.Add("Set-Cookie", refreshCookie);

        var response = new SignInResponse(result.Value.AccessToken.Token, result.Value.RefreshToken.Token);
        return this.Ok(response);
    }

    public record SignInRequest(string Username, string Password);

    public record SignInResponse(string AccessToken, string RefreshToken);

    public sealed class SignInValidator : AbstractValidator<SignInRequest>
    {
        public SignInValidator(IUserBusinessRulePredicates rulePredicates)
        {
            RuleFor(x => x.Username)
                    .NotEmpty()
                    .MustAsync(async (username, _) => !await rulePredicates.IsUsernameFree(username))
                    .WithMessage("User with this username doesn't exist");

            RuleFor(x => x.Password)
                .NotEmpty()
                .WithMessage("Password cannot be empty");
        }
    }
}
