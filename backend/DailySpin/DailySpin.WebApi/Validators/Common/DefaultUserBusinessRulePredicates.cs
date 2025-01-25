using DailySpin.Domain;

public sealed class DefaultUserBusinessRulePredicates : IUserBusinessRulePredicates
{
    private readonly IUserRepository _repository;

    public DefaultUserBusinessRulePredicates(IUserRepository repository)
    {
        _repository = repository;
    }

    public async Task<bool> IsUsernameFree(string username)
    {
        return await _repository.GetUserByUsernameAsync(username) is { IsSuccess: false };
    }
}
