using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SPBackend.Data;
using SPBackend.Views.Mains;

namespace SPBackend.Services.Mains;

public class PowerSourceService
{
    private readonly IAppDbContext _context;

    public PowerSourceService(IAppDbContext context)
    {
        _context = context;
    }

    public async Task<PowerSourceViewModel> GetPowerSource(int householdId)
    {
        var mainsLog = await _context.MainsLogs.Include(x => x.PowerSource).OrderByDescending(p => p.Time).FirstOrDefaultAsync(x => x.Household.Id == householdId);
        if (mainsLog == null)
        {
            throw new KeyNotFoundException("The household id provided is invalid or no logs have been recorded yet.");
        }
        
        return new PowerSourceViewModel()
        {
            Id = mainsLog.PowerSource.Id,
            Name = mainsLog.PowerSource.Name,
            Voltage = mainsLog.Voltage,
            LastUpdated = mainsLog.Time,
        };
    }
}