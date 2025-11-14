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
