using DemoAspNetApp.Data;
using DemoAspNetApp.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

//Configure for ASP net identity
builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options =>
{
    //Defaults Options
    //options.SignIn.RequireConfirmedAccount = true;
})
    .AddEntityFrameworkStores<DatabaseContext>()
    .AddDefaultTokenProviders();

//Link database with dataContext
builder.Services.AddDbContext<DatabaseContext>(options =>
{
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")); //Map from appsettings.json
});

//Life Cycle DI: AddSingleton(), AddTransient(), AddScoped()
builder.Services.AddScoped<ILoaiRepository, LoaiRepository>();
builder.Services.AddScoped<IHangHoaRepository, HangHoaRepository>();
builder.Services.AddScoped<IAccountRepository, AccountRepository>();

//Configure Password for identity framework
builder.Services.Configure<IdentityOptions>(options =>
{
    options.Password.RequiredLength = 3; // Độ dài tối thiểu
    options.Password.RequireDigit = false; // Yêu cầu chứa ít nhất một chữ số
    options.Password.RequireNonAlphanumeric = false; // Yêu cầu chứa ít nhất một ký tự đặc biệt
    options.Password.RequireLowercase = false; // Yêu cầu chứa ít nhất một chữ thường
    options.Password.RequireUppercase = false; // Yêu cầu chứa ít nhất một chữ hoa
});

//Configure JWT authentication
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
}).AddJwtBearer(options =>
{
    options.SaveToken = true;
    options.RequireHttpsMetadata = false;
    options.TokenValidationParameters = new
    Microsoft.IdentityModel.Tokens.TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidAudience = builder.Configuration["JWT:ValidAudience"], //Map from appsettings.json
        ValidIssuer = builder.Configuration["JWT:ValidIssuer"],//Map from appsettings.json
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes
            (builder.Configuration["JWT:Secret"])) //Map from appsettings.json
    };
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthentication(); // middleware check author with jwt

app.UseAuthorization();

app.MapControllers();

//Create Roles

using(var scope = app.Services.CreateScope())
{
    var roleManager = scope.ServiceProvider
        .GetRequiredService<RoleManager<IdentityRole>>();
    var roles = new[] { "Admin", "Manager", "Member" };

    foreach(var role in roles)
    {
        if(!await roleManager.RoleExistsAsync(role))
        {
            await roleManager.CreateAsync(new IdentityRole(role));
        }
    }
}

using (var scope = app.Services.CreateScope())
{
    var userManager = scope.ServiceProvider
        .GetRequiredService<UserManager<ApplicationUser>>();

    string firstName = "Quan Ly";
    string lastName = "admin#0001";
    string email = "admin@admin.com";
    string password = "admin";

    //string firstName = "QLALL";
    //string lastName = "all#0001";
    //string email = "all@all.com";
    //string password = "all";

    //string firstName = "Manager Admin";
    //string lastName = "managerAdmin#0001";
    //string email = "madmin@madmin.com";
    //string password = "madmin";

    if (await userManager.FindByEmailAsync(email) == null)
    {
        var user = new ApplicationUser
        {
            FirstName = firstName,
            LastName = lastName,
            Email = email,
            UserName = email,
        };

        await userManager.CreateAsync(user, password);

        await userManager.AddToRoleAsync(user, "Admin");
        //await userManager.AddToRoleAsync(user, "Manager");
    }
}

app.Run();
