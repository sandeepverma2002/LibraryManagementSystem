    using LibraryManagementSystem.Data;
    using LibraryManagementSystem.Models;
    using LibraryManagementSystem.Services;
    using Microsoft.EntityFrameworkCore;

    var builder = WebApplication.CreateBuilder(args);

    // Add services to the container
    builder.Services.AddControllersWithViews();
    builder.Services.AddRazorPages();

    // Configure MySQL Database
    var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
    builder.Services.AddDbContext<ApplicationDbContext>(options =>
        options.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString)));

    // Register Services
    builder.Services.AddScoped<IBookService, BookService>();
    builder.Services.AddScoped<ITransactionService, TransactionService>();

    // Add session support
    builder.Services.AddSession(options =>
    {
        options.IdleTimeout = TimeSpan.FromMinutes(30);
        options.Cookie.HttpOnly = true;
        options.Cookie.IsEssential = true;
    });

    // Configure HTTP context accessor
    builder.Services.AddHttpContextAccessor();

    var app = builder.Build();

    // Configure the HTTP request pipeline
    if (!app.Environment.IsDevelopment())
    {
        app.UseExceptionHandler("/Home/Error");
        app.UseHsts();
    }

    app.UseHttpsRedirection();
    app.UseStaticFiles();
    app.UseRouting();
    app.UseAuthorization();
    app.UseSession();

    // Create and seed database
    using (var scope = app.Services.CreateScope())
    {
        var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        dbContext.Database.Migrate();

       
    }

    app.MapControllerRoute(
        name: "default",
        pattern: "{controller=Books}/{action=Index}/{id?}");

    app.Run();
