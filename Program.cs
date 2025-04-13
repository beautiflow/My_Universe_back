using Microsoft.EntityFrameworkCore;
using VisitorCounter.Data;
using VisitorCounter.Models;

var builder = WebApplication.CreateBuilder(args);

// 로깅 설정
builder.Services.AddLogging(logging =>
{
    logging.AddConsole(); // 콘솔에 로그 출력
    logging.SetMinimumLevel(LogLevel.Information); // 최소 로그 레벨 설정
});

// SQLite 설정
builder.Services.AddDbContext<VisitorContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection")));

var app = builder.Build();

// 데이터베이스 초기화
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<VisitorContext>();
    db.Database.EnsureCreated();
    if (!db.VisitorCounts.Any())
    {
        db.VisitorCounts.Add(new VisitorCount { Count = 0 });
        db.SaveChanges();
    }
}

// 방문자 수 조회
app.MapGet("/visitor", async (VisitorContext db, ILogger<Program> logger) =>
{
    var count = await db.VisitorCounts.FirstOrDefaultAsync();
    
    // GET 요청 로그
    logger.LogInformation("GET /visitor 요청 - 시간: {Time}, 반환된 방문자 수: {Count}", 
        DateTime.UtcNow, count?.Count ?? -1);
    
    return count != null ? Results.Ok(count.Count) : Results.NotFound();
});

// 방문자 수 증가
app.MapPost("/visitor", async (VisitorContext db, ILogger<Program> logger) =>
{
    var count = await db.VisitorCounts.FirstOrDefaultAsync();
    if (count == null)
    {
        // 에러 로그
        logger.LogError("POST /visitor 요청 - 시간: {Time}, VisitorCount 데이터 없음", DateTime.UtcNow);
        return Results.NotFound();
    }
    
    count.Count++;
    await db.SaveChangesAsync();
    
    // POST 요청 로그
    logger.LogInformation("POST /visitor 요청 - 시간: {Time}, 증가된 방문자 수: {Count}", 
        DateTime.UtcNow, count.Count);
    
    return Results.Ok(count.Count);
});

app.Run();