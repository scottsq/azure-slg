using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using VS_SLG6.Api.Extensions;
using VS_SLG6.Model;
using VS_SLG6.Model.Entities;
using VS_SLG6.Repositories.Repositories;
using VS_SLG6.Services.Services;
using VS_SLG6.Services.Interfaces;
using VS_SLG6.Services.Validators;
using Microsoft.AspNetCore.Mvc.Formatters;
using Microsoft.Extensions.Options;
using Microsoft.AspNetCore.Mvc;
using System.Linq;
using VS_SLG6.Services.Models;
using VS_SLG6.Api;
using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using VS_SLG6.Api.ControllerAccess;
using VS_SLG6.Api.Interfaces;

namespace VS_SecondLifeGrp6
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddControllers();
            services.AddControllersWithViews().AddNewtonsoftJson();
            services.AddControllersWithViews(options =>
            {
                options.InputFormatters.Insert(0, GetJsonPatchInputFormatter());
            });
            InjectControllerAccess(services);
            InjectServices(services);
            InjectValidators(services);
            InjectRepositories(services);
            services.AddDbContextPool<VS_SLG6DbContext>(x => x.UseMySql(Configuration.GetConnectionString("Slg6_PROD")));


            var appsettingsRead = Configuration.GetSection("AppSettings");
            services.Configure<AppSettings>(appsettingsRead);
            var settings = appsettingsRead.Get<AppSettings>();
            var key = Encoding.ASCII.GetBytes(settings.Key);
            services.AddAuthentication(au =>
            {
                au.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                au.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            })
                .AddJwtBearer(jwt => {
                    jwt.RequireHttpsMetadata = false;
                    jwt.SaveToken = true;
                    jwt.TokenValidationParameters = new TokenValidationParameters
                    {
                        ValidateIssuerSigningKey = true,
                        IssuerSigningKey = new SymmetricSecurityKey(key),
                        ValidateIssuer = false,
                        ValidateAudience = false
                    };
                });

        }

        private static NewtonsoftJsonPatchInputFormatter GetJsonPatchInputFormatter()
        {
            var builder = new ServiceCollection()
                .AddLogging()
                .AddMvc()
                .AddNewtonsoftJson()
                .Services.BuildServiceProvider();

            return builder
                .GetRequiredService<IOptions<MvcOptions>>()
                .Value
                .InputFormatters
                .OfType<NewtonsoftJsonPatchInputFormatter>()
                .First();
        }

        private void InjectControllerAccess(IServiceCollection services)
        {
            services.AddScoped(typeof(IControllerAccess<>), typeof(GenericControllerAccess<>));
            services.AddScoped<IControllerAccess<Message>, MessageControllerAccess>();
            services.AddScoped<IControllerAccess<Photo>, PhotoControllerAccess>();
            services.AddScoped<IControllerAccess<Product>, ProductControllerAccess>();
            services.AddScoped<IControllerAccess<ProductRating>, ProductRatingControllerAccess>();
            services.AddScoped<IControllerAccess<ProductTag>, ProductTagControllerAccess>();
            services.AddScoped<IControllerAccess<Proposal>, ProposalControllerAccess>();
            services.AddScoped<IControllerAccess<Tag>, TagControllerAccess>();
            services.AddScoped<IControllerAccess<User>, UserControllerAccess>();
            services.AddScoped<IControllerAccess<UserRating>, UserRatingControllerAccess>();
            services.AddScoped<IMessageControllerAccess, MessageControllerAccess>();
            services.AddScoped<IProposalControllerAccess, ProposalControllerAccess>();
            services.AddScoped<IUserControllerAccess, UserControllerAccess>();
        }

        private void InjectServices(IServiceCollection services)
        {
            services.AddScoped(typeof(ValidationModel<>));
            services.AddScoped(typeof(IService<>), typeof(GenericService<>));
            services.AddScoped<IMessageService, MessageService>();
            services.AddScoped<IPhotoService, PhotoService>();
            services.AddScoped<IProductRatingService, ProductRatingService>();
            services.AddScoped<IProductService, ProductService>();
            services.AddScoped<IProductTagService, ProductTagService>();
            services.AddScoped<IProposalService, ProposalService>();
            services.AddScoped<ITagService, TagService>();
            services.AddScoped<IUserRatingService, UserRatingService>();
            services.AddScoped<IUserService, UserService>();
            services.AddScoped<IService<Message>, MessageService>();
            services.AddScoped<IService<Photo>, PhotoService>();
            services.AddScoped<IService<Product>, ProductService>();
            services.AddScoped<IService<ProductRating>, ProductRatingService>();
            services.AddScoped<IService<ProductTag>, ProductTagService>();
            services.AddScoped<IService<Proposal>, ProposalService>();
            services.AddScoped<IService<Tag>, TagService>();
            services.AddScoped<IService<User>, UserService>();
            services.AddScoped<IService<UserRating>, UserRatingService>();
        }

        private void InjectValidators(IServiceCollection services)
        {
            services.AddScoped(typeof(IValidator<>), typeof(GenericValidator<>));
            services.AddScoped<IValidator<Message>, MessageValidator>();
            services.AddScoped<IValidator<Photo>, PhotoValidator>();
            services.AddScoped<IValidator<Product>, ProductValidator>();
            services.AddScoped<IValidator<ProductRating>, ProductRatingValidator>();
            services.AddScoped<IValidator<Proposal>, ProposalValidator>();
            services.AddScoped<IValidator<Tag>, TagValidator>();
            services.AddScoped<IValidator<User>, UserValidator>();
            services.AddScoped<IValidator<UserRating>, UserRatingValidator>();

        }

        private void InjectRepositories(IServiceCollection services)
        {
            services.AddScoped(typeof(IRepository<>), typeof(GenericRepository<>));
            services.AddScoped<IRepository<Message>, MessageRepository>();
            services.AddScoped<IRepository<Photo>, PhotoRepository>();
            services.AddScoped<IRepository<ProductRating>, ProductRatingRepository>();
            services.AddScoped<IRepository<Product>, ProductRepository>();
            services.AddScoped<IRepository<ProductTag>, ProductTagRepository>();
            services.AddScoped<IRepository<Proposal>, ProposalRepository>();
            services.AddScoped<IRepository<User>, UserRepository>();
            services.AddScoped<IRepository<UserRating>, UserRatingRepository>();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseRouting();

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });

            EnsureMigration.EnsureMigrationOfContext<VS_SLG6DbContext>(app);
        }
    }
}