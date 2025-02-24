[![Quality Gate Status](https://sonarq.abreuhd.com/api/project_badges/measure?project=Auth&metric=alert_status&token=sqb_d7c96802205a85dff2eb2159c2a338f4f5f7565a)](https://sonarq.abreuhd.com/dashboard?id=Auth) 
[![Lines of Code](https://sonarq.abreuhd.com/api/project_badges/measure?project=Auth&metric=ncloc&token=sqb_d7c96802205a85dff2eb2159c2a338f4f5f7565a)](https://sonarq.abreuhd.com/dashboard?id=Auth)
[![Maintainability Rating](https://sonarq.abreuhd.com/api/project_badges/measure?project=Auth&metric=software_quality_maintainability_rating&token=sqb_d7c96802205a85dff2eb2159c2a338f4f5f7565a)](https://sonarq.abreuhd.com/dashboard?id=Auth)
[![Security Rating](https://sonarq.abreuhd.com/api/project_badges/measure?project=Auth&metric=software_quality_security_rating&token=sqb_d7c96802205a85dff2eb2159c2a338f4f5f7565a)](https://sonarq.abreuhd.com/dashboard?id=Auth)

# Shōmei
##### _The easiest way to have Identity in your project_

### Instalation

- **Step 0: Install NuGet**
```bash
  Auth.Infraestructure.Identity
  ```
- **Step 1: Install EntityFrameworkCore.Tools NuGet**
```bash
  Microsoft.EntityFrameworkCore.Tools
  ```
- **Step 2: Add this two lines of code in Program.cs**
```csharp
builder.Services.AddAuthentication();
builder.Services.AddIdentityInfrastructure(builder.Configuration);
  ```
  
- **Step 3: Add this lines of code in Program.cs**
```csharp
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    await services.AddIdentityRolesAsync();
} //
  ```
- **Step 4: Add this variables in AppSettings.json**
```bash
  "ConnectionStrings": {
    "IdentityConnection": "server={YourServer};user={YourDbUser};password={YourDbPassword};database={YourDbName}"
  },
  "JWTSettings": {
    "Key": "{Key}",
    "Issuer": "{Issuer}",
    "Audience": "{Audience}",
    "DurationInMinutes": {DurationOftTokens}
  },
  "MailSettings": {
    "EmailFrom": "{EmailFrom}",
    "SmtpHost": "{SmtpHost}",
    "SmtpPort": {SmtpPort},
    "SmtpUser": "{SmtpUser}",
    "SmtpPassword": "{SmtpPassword}",
    "DisplayName": "{DisplayName}"
  },
  ```
- **Step 5: That's All :p**
