

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

