using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FDAAPI.Domain.RelationalDb.Entities;
using FDAAPI.Domain.RelationalDb.RealationalDB;
using FDAAPI.Domain.RelationalDb.Repositories;
using Microsoft.EntityFrameworkCore;

namespace FDAAPI.Infra.Persistence.Repositories
{
    /// <summary>
    /// PostgreSQL implementation of IOtpCodeRepository
    /// Manages OTP codes for phone verification
    /// </summary>
    public class PgsqlOtpCodeRepository : IOtpCodeRepository
    {
        private readonly AppDbContext _context;

        public PgsqlOtpCodeRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<Guid> CreateAsync(OtpCode otpCode, CancellationToken ct = default)
        {
            _context.OtpCodes.Add(otpCode);
            await _context.SaveChangesAsync(ct);
            return otpCode.Id;
        }

        public async Task<OtpCode?> GetLatestValidOtpAsync(string phoneNumber, CancellationToken ct = default)
        {
            return await _context.OtpCodes
                .Where(otp => otp.PhoneNumber == phoneNumber
                    && !otp.IsUsed
                    && otp.ExpiresAt > DateTime.UtcNow)
                .OrderByDescending(otp => otp.CreatedAt)
                .AsNoTracking()
                .FirstOrDefaultAsync(ct);
        }

        public async Task<OtpCode?> GetLatestValidOtpByIdentifierAsync(string identifier, CancellationToken ct = default)
        {
            return await _context.OtpCodes
                .AsNoTracking()
                .Where(o =>
                    (o.Identifier == identifier || o.PhoneNumber == identifier) && // Support both old and new
                    !o.IsUsed &&
                    o.ExpiresAt > DateTime.UtcNow
                )
                .OrderByDescending(o => o.CreatedAt)
                .FirstOrDefaultAsync(ct);
        }


        public async Task<bool> MarkAsUsedAsync(Guid otpId, CancellationToken ct = default)
        {
            var otp = await _context.OtpCodes.FindAsync(new object[] { otpId }, ct);

            if (otp == null)
            {
                return false;
            }

            otp.IsUsed = true;
            otp.UsedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync(ct);
            return true;
        }

        public async Task<int> IncrementAttemptCountAsync(Guid otpId, CancellationToken ct = default)
        {
            var otp = await _context.OtpCodes.FindAsync(new object[] { otpId }, ct);

            if (otp == null)
            {
                return 0;
            }

            otp.AttemptCount++;
            await _context.SaveChangesAsync(ct);

            return otp.AttemptCount;
        }
    }
}






