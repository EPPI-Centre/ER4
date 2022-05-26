using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System.Security.Cryptography.X509Certificates;


namespace ERIdentityProvider
{
	public class Startup
	{
		private readonly IHostingEnvironment _environment;
		public Startup(IHostingEnvironment env)
		{
			var builder = new ConfigurationBuilder()
				.SetBasePath(env.ContentRootPath)
				.AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
				.AddJsonFile($"appsettings.{env.EnvironmentName}.json", optional: true);

			_environment = env;

			builder.AddEnvironmentVariables();
			Configuration = builder.Build();
		}

		public IConfiguration Configuration { get; }

		// This method gets called by the runtime. Use this method to add services to the container.
		public void ConfigureServices(IServiceCollection services)
		{

			//var cert = new X509Certificate2(Path.Combine(_environment.ContentRootPath, "damienbodserver.pfx"), "");

			services.AddMvc();
			if (_environment.IsEnvironment("Development") && Configuration["AppSettings:UseInMemoryStores"] == "True")
			{//easy option: use DeveloperSigningCredential and all in-memory stores, no custom persistence to SQL
				
				services.AddIdentityServer(options =>
				{
					options.Events.RaiseSuccessEvents = true;
					options.Events.RaiseFailureEvents = true;
					options.Events.RaiseErrorEvents = true;
				})
					.AddDeveloperSigningCredential()
					.AddInMemoryPersistedGrants()
					.AddInMemoryIdentityResources(Config.GetIdentityResources())
					.AddInMemoryApiResources(Config.GetApiResources())
					.AddInMemoryClients(Config.GetClients(Configuration))
					.AddInMemoryDevUserStore();
			}
			else if (_environment.IsEnvironment("Development"))
			{//more thorough option: use DeveloperSigningCredential but all persistence goes to SQL

				services.AddTransient<IdentityServer4.Stores.IPersistedGrantStore, TokenServices.ERxPersistedGrantStore>();
				services.AddIdentityServer(options =>
				{
					options.Events.RaiseSuccessEvents = true;
					options.Events.RaiseFailureEvents = true;
					options.Events.RaiseErrorEvents = true;
				})
					.AddDeveloperSigningCredential()
					//.AddInMemoryPersistedGrants()
					.AddInMemoryIdentityResources(Config.GetIdentityResources())
					.AddInMemoryApiResources(Config.GetApiResources())
					.AddInMemoryClients(Config.GetClients(Configuration))
					.AddCustomUserStore();
			}
			else
			{//production! Use the REAL Signingcertificates (.AddSigningCredential(cert)) AND persist to SQL
				string certpath = Configuration["AppSettings:Certificatepath"];
				string certsecret = Configuration["AppSettings:CertificateSecret"];
				var cert = new X509Certificate2(certpath, certsecret);
				services.AddTransient<IdentityServer4.Stores.IPersistedGrantStore, TokenServices.ERxPersistedGrantStore>();
				services.AddIdentityServer(options =>
				{
					options.Events.RaiseSuccessEvents = true;
					options.Events.RaiseFailureEvents = true;
					options.Events.RaiseErrorEvents = true;
				})
					.AddSigningCredential(cert)
					.AddInMemoryIdentityResources(Config.GetIdentityResources())
					.AddInMemoryApiResources(Config.GetApiResources())
					.AddInMemoryClients(Config.GetClients(Configuration))
					.AddCustomUserStore();
			}
		}

		// This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
		public void Configure(IApplicationBuilder app, IHostingEnvironment env)
		{
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            
            app.UseStaticFiles();
			app.UseIdentityServer();
           
            app.UseMvc();
		}
	}

  
}
