using System.Linq;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MySqlConnector;
using Sample.WebApi.Domain.Coupons;

namespace Sample.WebApi.Infrastructure.Coupons;

public sealed class CouponDatabaseInitializer : IHostedService
{
    private readonly string _connectionString;
    private readonly IOptionsMonitor<CouponOptions> _optionsMonitor;
    private readonly ILogger<CouponDatabaseInitializer> _logger;

    public CouponDatabaseInitializer(
        IConfiguration configuration,
        IOptionsMonitor<CouponOptions> optionsMonitor,
        ILogger<CouponDatabaseInitializer> logger)
    {
        _connectionString = configuration.GetConnectionString("MySql")
                             ?? throw new InvalidOperationException("MySql 연결 문자열이 설정되지 않았습니다.");
        _optionsMonitor = optionsMonitor;
        _logger = logger;
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        await EnsureSchemaAsync(cancellationToken).ConfigureAwait(false);
        await SeedCouponsAsync(cancellationToken).ConfigureAwait(false);
    }

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;

    private async Task EnsureSchemaAsync(CancellationToken cancellationToken)
    {
        await using var connection = new MySqlConnection(_connectionString);
        await connection.OpenAsync(cancellationToken).ConfigureAwait(false);

        var ensureCommands = new[]
        {
            """
            CREATE TABLE IF NOT EXISTS CouponCodes
            (
                Id CHAR(36) NOT NULL PRIMARY KEY,
                CouponId VARCHAR(64) NOT NULL,
                CodeValue VARCHAR(128) NOT NULL UNIQUE,
                Status TINYINT NOT NULL,
                AvailableFrom DATETIME(6) NOT NULL,
                UserId VARCHAR(128) NULL,
                IssuedAt DATETIME(6) NULL,
                INDEX IX_CouponCodes_CouponId_Status_AvailableFrom (CouponId, Status, AvailableFrom)
            ) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;
            """,
            """
            CREATE TABLE IF NOT EXISTS CouponIssueHistory
            (
                Id CHAR(36) NOT NULL PRIMARY KEY,
                CouponCodeId CHAR(36) NOT NULL,
                CouponId VARCHAR(64) NOT NULL,
                CodeValue VARCHAR(128) NOT NULL,
                UserId VARCHAR(128) NOT NULL,
                IssuedAt DATETIME(6) NOT NULL,
                INDEX IX_CouponIssueHistory_CouponId_IssuedAt (CouponId, IssuedAt DESC)
            ) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;
            """
        };

        foreach (var script in ensureCommands)
        {
            await using var command = new MySqlCommand(script, connection);
            await command.ExecuteNonQueryAsync(cancellationToken).ConfigureAwait(false);
        }
    }

    private async Task SeedCouponsAsync(CancellationToken cancellationToken)
    {
        var definitions = _optionsMonitor.CurrentValue.Definitions;
        if (definitions.Count == 0)
        {
            return;
        }

        await using var connection = new MySqlConnection(_connectionString);
        await connection.OpenAsync(cancellationToken).ConfigureAwait(false);

        foreach (var definition in definitions)
        {
            cancellationToken.ThrowIfCancellationRequested();

            await using var countCommand = new MySqlCommand(
                """
                SELECT COUNT(1)
                FROM CouponCodes
                WHERE CouponId = @couponId;
                """,
                connection);

            countCommand.Parameters.Add("@couponId", MySqlDbType.VarChar, 64).Value = definition.Id;
            var existingCountObj = await countCommand.ExecuteScalarAsync(cancellationToken).ConfigureAwait(false);
            var existingCount = Convert.ToInt64(existingCountObj ?? 0L);

            if (existingCount > 0)
            {
                _logger.LogInformation("Coupon {CouponId} already has {ExistingCount} codes, skipping seeding.", definition.Id, existingCount);
                continue;
            }

            if (definition.TotalQuantity <= 0)
            {
                _logger.LogWarning("Coupon {CouponId} has no total quantity configured; skipping seeding.", definition.Id);
                continue;
            }

            var allocations = BuildAllocations(definition);
            await using var transaction = await connection.BeginTransactionAsync(cancellationToken).ConfigureAwait(false);

            try
            {
                foreach (var allocation in allocations)
                {
                    for (var index = 0; index < allocation.Quantity; index++)
                    {
                        var couponCodeId = Guid.NewGuid();
                        var codeValue = $"{definition.Id}-{allocation.WeekIndex + 1:D2}-{index + 1 + allocation.Offset:D4}";

                        await using var insertCommand = new MySqlCommand(
                            """
                            INSERT INTO CouponCodes (Id, CouponId, CodeValue, Status, AvailableFrom)
                            VALUES (@id, @couponId, @codeValue, 0, @availableFrom);
                            """,
                            connection,
                            transaction);

                        insertCommand.Parameters.Add("@id", MySqlDbType.VarChar, 36).Value = couponCodeId.ToString();
                        insertCommand.Parameters.Add("@couponId", MySqlDbType.VarChar, 64).Value = definition.Id;
                        insertCommand.Parameters.Add("@codeValue", MySqlDbType.VarChar, 128).Value = codeValue;
                        insertCommand.Parameters.Add("@availableFrom", MySqlDbType.DateTime).Value = allocation.AvailableFrom.UtcDateTime;

                        await insertCommand.ExecuteNonQueryAsync(cancellationToken).ConfigureAwait(false);
                    }
                }

                await transaction.CommitAsync(cancellationToken).ConfigureAwait(false);
                _logger.LogInformation("Seeded {Count} coupon codes for {CouponId}.", allocations.Sum(x => x.Quantity), definition.Id);
            }
            catch (Exception ex) when (ex is MySqlException or InvalidOperationException)
            {
                await transaction.RollbackAsync(cancellationToken).ConfigureAwait(false);
                _logger.LogError(ex, "Failed to seed coupon codes for {CouponId}.", definition.Id);
            }
        }
    }

    private static IReadOnlyList<CouponAllocation> BuildAllocations(CouponDefinitionOptions definition)
    {
        if (definition.WeeklyAllocation.Count == 0 || definition.CampaignStartDate is null)
        {
            return new[]
            {
                new CouponAllocation(
                    WeekIndex: 0,
                    Quantity: definition.TotalQuantity,
                    AvailableFrom: DateTimeOffset.UtcNow,
                    Offset: 0)
            };
        }

        var startDate = DateTime.SpecifyKind(definition.CampaignStartDate.Value.ToDateTime(TimeOnly.MinValue), DateTimeKind.Utc);
        var allocations = new List<CouponAllocation>(definition.WeeklyAllocation.Count);
        var totalQuantity = definition.TotalQuantity;
        var accumulated = 0;

        for (var index = 0; index < definition.WeeklyAllocation.Count; index++)
        {
            var quantity = definition.WeeklyAllocation[index];
            if (quantity <= 0)
            {
                continue;
            }

            accumulated += quantity;
            if (accumulated > totalQuantity)
            {
                quantity -= accumulated - totalQuantity;
                accumulated = totalQuantity;
            }

            if (quantity <= 0)
            {
                break;
            }

            var availableFrom = DateTime.SpecifyKind(startDate.AddDays(index * 7), DateTimeKind.Utc);
            allocations.Add(new CouponAllocation(
                WeekIndex: index,
                Quantity: quantity,
                AvailableFrom: new DateTimeOffset(availableFrom),
                Offset: accumulated - quantity));

            if (accumulated >= totalQuantity)
            {
                break;
            }
        }

        return allocations;
    }

    private readonly record struct CouponAllocation(int WeekIndex, int Quantity, DateTimeOffset AvailableFrom, int Offset);
}
