
using Microsoft.EntityFrameworkCore;
using CustomerManager;
using CustomerManager.Model;
using System.Security.Claims;
using System.IdentityModel.Tokens.Jwt;
using Microsoft.IdentityModel.Tokens;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<CustomerContext>(opt => opt.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<CustomerContext>();
    db.Database.Migrate();
}

app.MapPost("/register", async (Customer customer, CustomerContext db) =>
{

    await db.Customers.AddAsync(customer);
    await db.SaveChangesAsync();

    return Results.Created("/login", "Registered customer successfully!");

});

app.MapPost("/login", async (CustomerLogin customerLogin, CustomerContext db) =>
{

Customer? customer = await db.Customers.FirstOrDefaultAsync(customer => customer.email.Equals(customerLogin.email) && customer.password.Equals(customerLogin.password));

if (customer == null)
{
    return Results.NotFound("The username or password is not correct!");
}

var secretkey = builder.Configuration["Jwt:Key"];

if (secretkey == null)
{
    return Results.StatusCode(500);
}

var claims = new[]
{
        new Claim(ClaimTypes.NameIdentifier, customer.id.ToString()),
        new Claim(ClaimTypes.Email, customer.email),
        new Claim(ClaimTypes.GivenName, customer.name),
        new Claim(ClaimTypes.Surname, customer.name),
     
    };

var token = new JwtSecurityToken
(
    issuer: builder.Configuration["Jwt:Issuer"],
    audience: builder.Configuration["Jwt:Audience"],
    claims: claims,
    expires: DateTime.UtcNow.AddMinutes(30),
    notBefore: DateTime.UtcNow,
    signingCredentials: new SigningCredentials(new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretkey)), SecurityAlgorithms.HmacSha256)
);

var tokenString = new JwtSecurityTokenHandler().WriteToken(token);

return Results.Ok(tokenString);


});

app.Run();
