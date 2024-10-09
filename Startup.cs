using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using MinimalApi;
using MinimalApi.Domain.Entities;
using MinimalApi.Domain.Enuns;
using MinimalApi.Domain.Interfaces;
using MinimalApi.Domain.ModelViews;
using MinimalApi.Domain.Services;
using MinimalApi.DTOs;
using MinimalApi.Infrastructure.Context;

public class Startup
{
    public Startup(IConfiguration configuration)
    {
        Configuration = configuration;
        key = Configuration?.GetSection("Jwt")?.ToString() ?? "";
    }

    private string key = "";
    public IConfiguration Configuration { get;set; } = default!;

    public void ConfigureServices(IServiceCollection services)
    {
        services.AddAuthentication(option => {
            option.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
            option.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
        }).AddJwtBearer(option => {
            option.TokenValidationParameters = new TokenValidationParameters{
                ValidateLifetime = true,
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key)),
                ValidateIssuer = false,
                ValidateAudience = false,
            };
        });

        services.AddAuthorization();

        services.AddScoped<IAdministratorService, AdministratorService>();
        services.AddScoped<IVehicleService, VehicleService>();

        services.AddEndpointsApiExplorer();
        services.AddSwaggerGen(options => {
            options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme{
                Name = "Authorization",
                Type = SecuritySchemeType.Http,
                Scheme = "bearer",
                BearerFormat = "JWT",
                In = ParameterLocation.Header,
                Description = "Insira o token JWT aqui"
            });

            options.AddSecurityRequirement(new OpenApiSecurityRequirement
            {
                {
                    new OpenApiSecurityScheme{
                        Reference = new OpenApiReference 
                        {
                            Type = ReferenceType.SecurityScheme,
                            Id = "Bearer"
                        }
                    },
                    new string[] {}
                }
            });
        });

        services.AddDbContext<MyDbContext>(options => {
            options.UseSqlServer(
                Configuration.GetConnectionString("DefaultConnection")
            );
        });

        services.AddCors(options =>
        {
            options.AddDefaultPolicy(
                builder =>
                {
                    builder.AllowAnyOrigin()
                        .AllowAnyMethod()
                        .AllowAnyHeader();
                });
        });
    }

    public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
    {
        app.UseSwagger();
        app.UseSwaggerUI();

        app.UseRouting();

        app.UseAuthentication();
        app.UseAuthorization();

        app.UseCors();

        app.UseEndpoints(endpoints => {

            #region Home
            endpoints.MapGet("/", () => Results.Json(new Home())).AllowAnonymous().WithTags("Home");
            #endregion

            #region Administrators
            string GerarTokenJwt(Administrator administrator){
                if(string.IsNullOrEmpty(key)) return string.Empty;

                var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key));
                var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

                var claims = new List<Claim>()
                {
                    new Claim("Email", administrator.Email),
                    new Claim("Profile", administrator.Profile),
                    new Claim(ClaimTypes.Role, administrator.Profile),
                };
                
                var token = new JwtSecurityToken(
                    claims: claims,
                    expires: DateTime.Now.AddDays(1),
                    signingCredentials: credentials
                );

                return new JwtSecurityTokenHandler().WriteToken(token);
            }

            endpoints.MapPost("/administrators/login", ([FromBody] LoginDTO loginDTO, IAdministratorService administratorService) => {
                var adm = administratorService.Authenticate(loginDTO);
                if(adm != null)
                {
                    string token = GerarTokenJwt(adm);
                    return Results.Ok(new AdministratorLogged
                    {
                        Email = adm.Email,
                        Profile = adm.Profile,
                        Token = token
                    });
                }
                else
                    return Results.Unauthorized();
            }).AllowAnonymous().WithTags("Administrators");

            endpoints.MapGet("/administrators", ([FromQuery] int? pagina, IAdministratorService administratorService) => {
                var adms = new List<AdministratorModelView>();
                var administrators = administratorService.GetAll(pagina);
                foreach(var adm in administrators)
                {
                    adms.Add(new AdministratorModelView{
                        Id = adm.Id,
                        Email = adm.Email,
                        Profile = adm.Profile
                    });
                }
                return Results.Ok(adms);
            })
            .RequireAuthorization()
            .RequireAuthorization(new AuthorizeAttribute { Roles = "Adm" })
            .WithTags("administrators");

            endpoints.MapGet("/administrators/{id}", ([FromRoute] int id, IAdministratorService administratorService) => {
                var administrator = administratorService.GetById(id);
                if(administrator == null) return Results.NotFound();
                return Results.Ok(new AdministratorModelView{
                        Id = administrator.Id,
                        Email = administrator.Email,
                        Profile = administrator.Profile
                });
            })
            .RequireAuthorization()
            .RequireAuthorization(new AuthorizeAttribute { Roles = "Adm" })
            .WithTags("Administrators");

            endpoints.MapPost("/administrators", ([FromBody] AdministratorDTO administratorDTO, IAdministratorService administratorService) => {
                var Validation = new ValidationError{
                    Messages = new List<string>()
                };

                if(string.IsNullOrEmpty(administratorDTO.Email))
                    Validation.Messages.Add("Email can't be empty");
                if(string.IsNullOrEmpty(administratorDTO.Password))
                    Validation.Messages.Add("Password can't be empty");
                if(administratorDTO.Profile == null)
                    Validation.Messages.Add("Profile can't be empty");

                if(Validation.Messages.Count > 0)
                    return Results.BadRequest(Validation);
                
                var administrator = new Administrator{
                    Email = administratorDTO.Email,
                    Password = administratorDTO.Password,
                    Profile = administratorDTO.Profile.ToString() ?? Profile.User.ToString()
                };

                administratorService.Create(administrator);

                return Results.Created($"/administrator/{administrator.Id}", new AdministratorModelView{
                    Id = administrator.Id,
                    Email = administrator.Email,
                    Profile = administrator.Profile
                });
                
            })
            .RequireAuthorization()
            .RequireAuthorization(new AuthorizeAttribute { Roles = "Adm" })
            .WithTags("Administradores");
            #endregion

            #region Vehicle
            ValidationError validationDTO(VehicleDTO vehicleDTO)
            {
                var validation = new ValidationError{
                    Messages = new List<string>()
                };

                if(string.IsNullOrEmpty(vehicleDTO.Name))
                    validation.Messages.Add("O nome não pode ser vazio");

                if(string.IsNullOrEmpty(vehicleDTO.Brand))
                    validation.Messages.Add("A Marca não pode ficar em branco");

                if(vehicleDTO.Year < 1950)
                    validation.Messages.Add("Veículo muito antigo, aceito somete anos superiores a 1950");

                return validation;
            }

            endpoints.MapPost("/vehicles", ([FromBody] VehicleDTO vehicleDTO, IVehicleService vehicleService) => {
                var validacao = validationDTO(vehicleDTO);
                if(validacao.Messages.Count > 0)
                    return Results.BadRequest(validacao);
                
                var vehicle = new Vehicle{
                    Name = vehicleDTO.Name,
                    Brand = vehicleDTO.Brand,
                    Year = vehicleDTO.Year
                };
                vehicleService.Create(vehicle);

                return Results.Created($"/vehicle/{vehicle.Id}", vehicle);
            })
            .RequireAuthorization()
            .RequireAuthorization(new AuthorizeAttribute { Roles = "Adm,Editor" })
            .WithTags("Vehicles");

            endpoints.MapGet("/vehicles", ([FromQuery] int? page, IVehicleService vehicleService) => {
                var vehicles = vehicleService.GetAll(page);

                return Results.Ok(vehicles);
            }).RequireAuthorization().WithTags("Vehicles");

            endpoints.MapGet("/vehicles/{id}", ([FromRoute] int id, IVehicleService vehicleService) => {
                var veiculo = vehicleService.GetById(id);
                if(veiculo == null) return Results.NotFound();
                return Results.Ok(veiculo);
            })
            .RequireAuthorization()
            .RequireAuthorization(new AuthorizeAttribute { Roles = "Adm,Editor" })
            .WithTags("Vehicles");

            endpoints.MapPut("/vehicles/{id}", ([FromRoute] int id, VehicleDTO vehicleDTO, IVehicleService vehicleService) => {
                var vehicle = vehicleService.GetById(id);
                if(vehicle == null) return Results.NotFound();
                
                var validation = validationDTO(vehicleDTO);
                if(validation.Messages.Count > 0)
                    return Results.BadRequest(validation);
                
                vehicle.Name = vehicleDTO.Name;
                vehicle.Brand = vehicleDTO.Brand;
                vehicle.Year = vehicleDTO.Year;

                vehicleService.Update(vehicle);

                return Results.Ok(vehicle);
            })
            .RequireAuthorization()
            .RequireAuthorization(new AuthorizeAttribute { Roles = "Adm" })
            .WithTags("Vehicles");

            endpoints.MapDelete("/vehicles/{id}", ([FromRoute] int id, IVehicleService vehicleService) => {
                var vehicle = vehicleService.GetById(id);
                if(vehicle == null) return Results.NotFound();

                vehicleService.Delete(vehicle);

                return Results.NoContent();
            })
            .RequireAuthorization()
            .RequireAuthorization(new AuthorizeAttribute { Roles = "Adm" })
            .WithTags("Vehicles");
            #endregion
        });
    }
}