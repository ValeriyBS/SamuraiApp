using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SamuraiApp.Data.Migrations
{
    public partial class SamuraiBattleStats : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(
                @"USE [SamuraiAppData]
GO

/****** Object:  UserDefinedFunction [dbo].[EarliestBattleFoughtBySamurai]    Script Date: 23/02/2022 13:06:15 ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE OR ALTER   function [dbo].[EarliestBattleFoughtBySamurai](@samuraiId int)
returns char(30) as
begin
declare @result char(30);
select top 1 @result = Name
from [dbo].Battles b
inner join [dbo].[BattleSamurai] bs
on b.BattleId = bs.BattleId
order by b.StartDate
return @result;
end;
GO");

            migrationBuilder.Sql(
                @"USE [SamuraiAppData]
GO

/****** Object:  View [dbo].[SamuraiBattleStats]    Script Date: 23/02/2022 13:05:47 ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE OR ALTER view [dbo].[SamuraiBattleStats]
as
select dbo.Samurais.Name,
count(dbo.BattleSamurai.BattleId) as NumberOfBattles,
dbo.EarliestBattleFoughtBySamurai(Min(dbo.Samurais.Id)) as EarliestBattle
from dbo.BattleSamurai 
inner join
dbo.Samurais 
on dbo.BattleSamurai.SamuraiId = dbo.Samurais.Id
group by dbo.Samurais.Name, dbo.BattleSamurai.SamuraiId
GO
");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("drop view dbo.SamuraiBattleStats");
            migrationBuilder.Sql("drop funcion dbo.EarliestBattleFoughtBySamurai");
        }
    }
}
