

Gagnagrunns taflan Weather búin til með því að nota EF core code first.

Fylgt leiðbeiningum hér
https://entityframeworkcore.com/approach-code-first

Búinn til weather klasi og MyContext.cs klasi (þar sem skilgreint er hvaða klasa á að nota)

Sóttir nuget pakkar:
Microsoft.EntityFrameworkCore.Tools
Microsoft.EntityFrameworkCore.Design

keyrð eftirfarandi skipun :
Add-Migration Initial
Það býr til klasa

svo er keyrð skipunin:
Update-Database
Það býr til database-ið í grunninum.

ATH. til að fara database first leiðina
PM> Scaffold-DbContext -Provider Microsoft.EntityFrameworkCore.SqlServer -Connection "Data Source=(localdb)\ProjectsV13;Initial Catalog=StoreDB;"

PM> Scaffold-DbContext -Provider Microsoft.EntityFrameworkCore.SqlServer -Connection "Server=tcp:fishinglog.database.windows.net,1433;Initial Catalog=Fishinglogg;Persist Security Info=False;User ID=Adminfishing;Password=Fififi75;MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;"


############
Til að setja upp config skrár virkni.
Installa eftirfarandi pökkum:
Microsoft.Extensions.Configuration
Microsoft.Extensions.Configuration.Binder
Microsoft.Extensions.Configuration.EnvironmentVariables
Microsoft.Extensions.Configuration.FileExtensions
Microsoft.Extensions.Configuration.Json

https://blog.hildenco.com/2020/05/configuration-in-net-core-console.html


Velja appsettings skrárnar og í properties velja : Always copy

###############

Til að bæta við secret skrá í console forrit
Install-Package Microsoft.Extensions.Configuration.UserSecrets
https://makolyte.com/how-to-add-user-secrets-in-a-dotnetcore-console-app/