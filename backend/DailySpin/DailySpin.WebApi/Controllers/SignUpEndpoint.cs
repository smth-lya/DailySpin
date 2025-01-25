using FluentValidation;
using DailySpin.Application;

namespace DailySpin.WebApi;

public sealed class SignUpEndpoint : IController
{
    private readonly IUserService _userService;
    private readonly IUserBusinessRulePredicates _rulePredicates;

    public SignUpEndpoint(IUserService userService, IUserBusinessRulePredicates rulePredicates)
    {
        _userService = userService;
        _rulePredicates = rulePredicates;
    }

    [HttpPost]
    [Route("/api/auth/signup")]
    public async Task<IActionResult> CreateUser([FromBody] SignUpRequest request)
    {
        var validator = new SingUpRequestValidator(_rulePredicates);
        var validationResult = await validator.ValidateAsync(request);

        if (!validationResult.IsValid)
        {
            var errors = string.Join("; ", validationResult.Errors.Select(e => e.ErrorMessage));
            return this.BadRequest(errors);
        }

        var result = await _userService.SignUpAsync(request.Email, request.Password);

        var response = new SignUpResponse(result.Value.Id);
        return this.Ok(response);
    }

    public record SignUpRequest(string Username, string Email, string Password);

    public record SignUpResponse(Guid? UserId);

    public class SingUpRequestValidator : AbstractValidator<SignUpRequest>
    {
        public SingUpRequestValidator(IUserBusinessRulePredicates rulePredicates)
        {
            RuleFor(x => x.Username)
                .NotEmpty()
                .WithMessage("Username cannot be empty")
                .MustAsync(async (username, _) => await rulePredicates.IsUsernameFree(username))
                .WithMessage("Username is not free");

            RuleFor(x => x.Password)
                .NotEmpty()
                .WithMessage("Password cannot be empty")
                .MinimumLength(6)
                .WithMessage("Password must be at least 6 characters long");
        }
    }
}
