﻿using Ardalis.Result;

namespace DailySpin.Domain;

public interface IUserRepository : IUserReadRepository
{
    Task<Result> AddAsync(User user, CancellationToken cancellationToken = default);
    
    Task<Result> UpdateAsync(User user, CancellationToken cancellationToken = default);
    
    Task<Result> DeleteAsync(User user, CancellationToken cancellationToken = default);
}