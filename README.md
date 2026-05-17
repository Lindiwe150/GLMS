GLMS - EC2 Windows Deployment Instructions
===========================================
Target: Windows Server 2022 EC2 + SQL Server

─────────────────────────────────────
1. PROVISION EC2
─────────────────────────────────────
- AMI:           Windows Server 2022 Base
- Instance type: t3.medium (minimum)
- Security Group inbound rules:
    Port 3389  (RDP)
    Port 80    (HTTP)
    Port 443   (HTTPS)
    Port 1433  (SQL Server, only if accessed externally)

─────────────────────────────────────
2. CONNECT TO EC2
─────────────────────────────────────
- In AWS Console → EC2 → select instance → Connect → RDP Client
- Download the RDP file and decrypt the Administrator password using your key pair
- Open the RDP file and log in

─────────────────────────────────────
3. INSTALL .NET 8 HOSTING BUNDLE
─────────────────────────────────────
On the EC2 instance, open a browser and download:

    https://dotnet.microsoft.com/en-us/download/dotnet/8.0
    → "ASP.NET Core Runtime 8.x" → Windows → Hosting Bundle

Run the installer, then open a new Command Prompt and verify:

    dotnet --version    # should print 8.x.x

─────────────────────────────────────
4. INSTALL SQL SERVER
─────────────────────────────────────
Option A – SQL Server Developer Edition (free, on same EC2):
    Download: https://www.microsoft.com/en-us/sql-server/sql-server-downloads
    - Choose "Developer" edition
    - During setup, note the instance name (default: MSSQLSERVER)
    - Enable SQL Server Authentication and set the SA password
    - Open SQL Server Configuration Manager → enable TCP/IP on port 1433
    - Restart the SQL Server service

Option B – Use Amazon RDS for SQL Server:
    - Create RDS SQL Server instance in AWS Console
    - Note the endpoint, port (1433), username, password

─────────────────────────────────────
5. UPLOAD PROJECT FILES
─────────────────────────────────────
From your local machine, copy the GLMS folder to the EC2 instance via RDP:
    - Open RDP → Local Resources tab → More → check your local drive
    - Drag and drop the GLMS folder to C:\inetpub\ or C:\GLMS\ on the server

Alternatively use WinSCP or AWS S3:
    aws s3 cp C:\Users\<you>\Documents\GLMS s3://your-bucket/GLMS --recursive
    # then on EC2:
    aws s3 cp s3://your-bucket/GLMS C:\GLMS --recursive

─────────────────────────────────────
6. CONFIGURE CONNECTION STRING
─────────────────────────────────────
Open C:\GLMS\GLMS.Web\appsettings.json in Notepad and update:

    "DefaultConnection": "Server=localhost;Database=GlmsDb;User Id=sa;Password=<YOUR_SA_PASSWORD>;TrustServerCertificate=True;"

For Windows Authentication (if using the same machine):
    "DefaultConnection": "Server=localhost;Database=GlmsDb;Trusted_Connection=True;TrustServerCertificate=True;"

─────────────────────────────────────
7. RESTORE PACKAGES & RUN MIGRATIONS
─────────────────────────────────────
Open Command Prompt as Administrator:

    cd C:\GLMS\GLMS.Web
    dotnet restore

Install EF Core tools if not present:
    dotnet tool install --global dotnet-ef

Run migrations to create the database:
    dotnet ef database update

─────────────────────────────────────
8. PUBLISH THE APP
─────────────────────────────────────
    cd C:\GLMS\GLMS.Web
    dotnet publish -c Release -o C:\inetpub\glms

─────────────────────────────────────
9. HOST WITH IIS (Recommended on Windows)
─────────────────────────────────────
a) Enable IIS:
   - Open Server Manager → Add Roles and Features
   - Check "Web Server (IIS)" → install

b) Create IIS Site:
   - Open IIS Manager (inetmgr)
   - Right-click "Sites" → Add Website
       Site name:    GLMS
       Physical path: C:\inetpub\glms
       Port:          80

c) Set Application Pool:
   - Click the GLMS site → Basic Settings → Application Pool → Edit
   - Set .NET CLR version to "No Managed Code"
   - Set Identity to "ApplicationPoolIdentity" or a service account

d) Grant write permission on uploads folder:
   - Right-click C:\inetpub\glms\wwwroot\uploads → Properties → Security
   - Add "IIS AppPool\GLMS" with Modify permission

e) Browse to http://localhost — the GLMS dashboard should load.

─────────────────────────────────────
10. ALTERNATIVE: RUN WITH KESTREL (no IIS)
─────────────────────────────────────
If you prefer not to use IIS, run directly:

    cd C:\inetpub\glms
    dotnet GLMS.Web.dll --urls "http://0.0.0.0:80"

To keep it running after RDP disconnect, use NSSM (Non-Sucking Service Manager):
    Download NSSM: https://nssm.cc/download
    nssm install GLMS "C:\Program Files\dotnet\dotnet.exe" "C:\inetpub\glms\GLMS.Web.dll --urls http://0.0.0.0:80"
    nssm start GLMS

─────────────────────────────────────
11. RUN UNIT TESTS
─────────────────────────────────────
Open Command Prompt:

    cd C:\GLMS\GLMS.Tests
    dotnet test --logger "console;verbosity=detailed"

All 14 tests should pass. Take a screenshot of the output for your submission.

─────────────────────────────────────
12. GITHUB SUBMISSION
─────────────────────────────────────
From your local machine (not the EC2):

    cd C:\Users\<you>\Documents\GLMS
    git init
    git add .
    git commit -m "GLMS Part 2 - ASP.NET Core MVC Monolith"
    git remote add origin https://github.com/<YOUR_USERNAME>/GLMS.git
    git push -u origin main

─────────────────────────────────────
NOTES
─────────────────────────────────────
- The free currency API used is: https://open.er-api.com/v6/latest/USD
  No API key required. Rate limit: 1500 requests/month on free tier.

- Ensure Windows Firewall allows inbound traffic on port 80 (and 443 if using HTTPS).
  Server Manager → Windows Defender Firewall → Inbound Rules → New Rule → Port 80.

- For HTTPS, install a certificate via IIS Manager → Server Certificates,
  or use Let's Encrypt with win-acme: https://www.win-acme.com/
