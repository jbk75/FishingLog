FishingNewsWebScraper (.NET 8 console application)
===============================================

Purpose
-------
The FishingNewsWebScraper project collects Icelandic fishing reports (with an emphasis on salmon, trout and char) and writes the
structured result to a SQL Server database. In addition to the core catch information it tries to enrich each report with weather
data and optional tide notes. Downloaded photos related to the report are saved locally under an images directory.

Key Features
------------
* Scrapes configurable Icelandic fishing news sources and filters for relevant fish species (salmon, trout, char, brown trout).
* Persists fishing reports (including the originating source reference) to the dbo.FishingNews table and ensures that every referenced fishing place exists in dbo.FishingPlace.
* Automatically creates missing fishing places. FishingPlaceTypeID is set to 1 for lakes and 2 for rivers based on the name.
* Resolves or enriches weather details (temperature, wind speed, wind direction, precipitation and tide state) for the catch day and stores them in the dbo.FishingNewsWeatherDetail table.
* Searches configured social media feeds (Instagram, Facebook, etc.) in addition to traditional news sources.
* (Optional) Persists metadata for downloaded images into dbo.FishingNewsImage. Files are written to an images folder using the
  format yyyymmdd_<fishingplacename>_<fishingnewsid><counter>.
* Allows operators to configure how many years back the scraper should look for fishing news.

Database Structure
------------------
The project expects the following SQL Server schema (FishingPlace table already exists in the FishingLog database):

CREATE TABLE [dbo].[FishingNews](
    [Id] INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
    [Date] DATE NOT NULL,
    [Time] TIME NULL,
    [FishingPlaceId] INT NOT NULL,
    [NumberOfFishesCaught] INT NULL,
    [NumberOfFishesSeen] INT NULL,
    [WeatherOnFishingDay] NVARCHAR(4000) NULL,
    [Description] NVARCHAR(MAX) NOT NULL,
    [TideState] NVARCHAR(200) NULL,
    [PeakActivityTime] TIME NULL,
    [Source] NVARCHAR(500) NULL,
    CONSTRAINT [FK_FishingNews_FishingPlace] FOREIGN KEY ([FishingPlaceId]) REFERENCES [dbo].[FishingPlace]([Id])
);

CREATE TABLE [dbo].[FishingNewsWeatherDetail](
    [Id] INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
    [FishingNewsId] INT NOT NULL,
    [ObservationTime] DATETIMEOFFSET NOT NULL,
    [Summary] NVARCHAR(400) NOT NULL,
    [TemperatureC] DECIMAL(5,2) NULL,
    [WindSpeedMetersPerSecond] DECIMAL(5,2) NULL,
    [PrecipitationMillimeters] DECIMAL(5,2) NULL,
    [TideState] NVARCHAR(200) NULL,
    [WindDirection] NVARCHAR(50) NULL,
    CONSTRAINT [FK_FishingNewsWeatherDetail_FishingNews] FOREIGN KEY ([FishingNewsId]) REFERENCES [dbo].[FishingNews]([Id])
);

CREATE TABLE [dbo].[FishingNewsImage](
    [Id] INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
    [FishingNewsId] INT NOT NULL,
    [SourceUri] NVARCHAR(500) NOT NULL,
    [LocalPath] NVARCHAR(500) NULL,
    [Caption] NVARCHAR(500) NULL,
    CONSTRAINT [FK_FishingNewsImage_FishingNews] FOREIGN KEY ([FishingNewsId]) REFERENCES [dbo].[FishingNews]([Id])
);

Configuration
-------------
The scraper settings (including years back, news sources, social media feeds, and weather API templates) are stored in appsettings.json.
Provide a valid SQL Server connection string under Database:ConnectionString.

Usage
-----
Run the console application with `dotnet run` (or publish it as required). The service will scrape the configured news sources,
download related images, and persist the catch information to SQL Server.
