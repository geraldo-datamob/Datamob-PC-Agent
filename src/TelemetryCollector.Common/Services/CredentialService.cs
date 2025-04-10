using Microsoft.EntityFrameworkCore;
using TelemetryCollector.Common.Data;
using TelemetryCollector.Common.Data.Entities;
using TelemetryCollector.Common.Data.Extensions;

namespace TelemetryCollector.Core.Services;

public class CredentialService
{
    private readonly TelemetryDbContext _context;

    public CredentialService()
    {
        _context = new TelemetryDbContext();
        _context.Database.EnsureCreated();
        _context.SeedDatabaseAsync().Wait();
    }

    public async Task SaveCredentialsAsync(string username, string password)
    {
        var existingCred = await _context.Credentials.FirstOrDefaultAsync();
        if (existingCred != null)
        {
            existingCred.Username = username;
            existingCred.Password = password;
        }
        else
        {
            _context.Credentials.Add(new Credential { Username = username, Password = password });
        }
        await _context.SaveChangesAsync();
    }

    public async Task<Credential> GetCredentialsAsync()
    {
        return await _context.Credentials.FirstOrDefaultAsync();
    }
}
