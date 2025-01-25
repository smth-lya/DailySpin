public interface IUserBusinessRulePredicates
{
    Task<bool> IsUsernameFree(string username);
}
