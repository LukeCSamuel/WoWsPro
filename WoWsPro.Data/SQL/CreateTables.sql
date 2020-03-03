-- DROP TABLE Account
-- DROP TABLE ApplicationSetting
-- DROP TABLE Claim
-- DROP TABLE DiscordGuild
-- DROP TABLE DiscordRole
-- DROP TABLE DiscordUser
-- DROP TABLE DiscordToken
-- DROP TABLE FileContent
-- DROP TABLE SessionCache
-- DROP TABLE Tournament
-- DROP TABLE TournamentRegistrationRules
-- DROP TABLE TournamentClaim
-- DROP TABLE TournamentGame
-- DROP TABLE TournamentGroup
-- DROP TABLE TournamentMatch
-- DROP TABLE TournamentParticipant
-- DROP TABLE TournamentSeed
-- DROP TABLE TournamentStage
-- DROP TABLE TournamentTeam
-- DROP TABLE TournamentTeamClaim
-- DROP TABLE WarshipsClan
-- DROP TABLE WarshipsMap
-- DROP TABLE WarshipsPlayer

CREATE TABLE ApplicationSetting (
    ApplicationSettingId bigint NOT NULL IDENTITY(1,1) PRIMARY KEY,
    Environment varchar(255),
    [Key] varchar(255) NOT NULL,
    [Value] varchar(255),
    [Timestamp] datetime2 NOT NULL
)

CREATE TABLE Account (
    AccountId bigint NOT NULL IDENTITY(1,1) PRIMARY KEY,
    Nickname nvarchar(255) NOT NULL,
    Created datetime2 NOT NULL
)

CREATE TABLE DiscordGuild (
    GuildId bigint NOT NULL PRIMARY KEY,
    [Name] nvarchar(200) NOT NULL,
    Icon varchar(255),
    Invite varchar(255)
)

CREATE TABLE DiscordRole (
    RoleKeyId bigint NOT NULL IDENTITY(1,1) PRIMARY KEY,
    RoleId bigint NOT NULL,
    GuildId bigint NOT NULL FOREIGN KEY REFERENCES DiscordGuild(GuildId),
    [Name] nvarchar(255) NOT NULL,
    CONSTRAINT UNQ_RoleGuild UNIQUE (RoleId, GuildId)
)

CREATE TABLE DiscordUser (
    DiscordId bigint NOT NULL PRIMARY KEY,
    AccountId bigint FOREIGN KEY REFERENCES Account(AccountId),
    Username nvarchar(64) NOT NULL,
    Discriminator char(4) NOT NULL,
    Avatar varchar(255),
    IsPrimary bit NOT NULL
)

CREATE TABLE DiscordToken (
    DiscordTokenId bigint NOT NULL IDENTITY(1,1) PRIMARY KEY,
    DiscordId bigint NOT NULL FOREIGN KEY REFERENCES DiscordUser(DiscordId),
    AccessToken varchar(255) NOT NULL,
    TokenType varchar(255) NOT NULL,
    Expires datetime2 NOT NULL,
    RefreshToken varchar(255),
    Scope varchar(255) NOT NULL
)

CREATE TABLE WarshipsClan (
    ClanId bigint NOT NULL PRIMARY KEY,
    Region int NOT NULL,
    [Name] nvarchar(255) NOT NULL,
    [Tag] varchar(5) NOT NULL,
    [MemberCount] int NOT NULL,
    Created datetime2 NOT NULL
)

CREATE TABLE WarshipsPlayer (
    PlayerId bigint NOT NULL PRIMARY KEY,
    AccountId bigint FOREIGN KEY REFERENCES Account(AccountId),
    Region int NOT NULL,
    Nickname varchar(255) NOT NULL,
    Created datetime2,
    ClanId bigint FOREIGN KEY REFERENCES WarshipsClan(ClanId),
    ClanRole varchar(255),
    JoinedClan datetime2,
    IsPrimary bit NOT NULL
)

CREATE TABLE WarshipsMap (
    MapId bigint NOT NULL PRIMARY KEY,
    [Description] varchar(255) NOT NULL,
    Icon varchar(255) NOT NULL,
    [Name] varchar(255) NOT NULL
)

CREATE TABLE Tournament (
    TournamentId bigint NOT NULL IDENTITY(1,1) PRIMARY KEY,
    GuildId bigint FOREIGN KEY REFERENCES DiscordGuild(GuildId),
    [Name] nvarchar(255) NOT NULL,
    Icon varchar(255),
    [Description] nvarchar(2000) NOT NULL,
    [Owner] bigint NOT NULL FOREIGN KEY REFERENCES Account(AccountId),
    Created datetime2 NOT NULL,
    ParticipantRole bigint FOREIGN KEY REFERENCES DiscordRole(RoleKeyId),
    TeamOwnerRole bigint FOREIGN KEY REFERENCES DiscordRole(RoleKeyId)
)

CREATE TABLE TournamentRegistrationRules (
    TournamentRegistrationRulesId bigint NOT NULL IDENTITY(1,1) PRIMARY KEY,
    TournamentId bigint NOT NULL FOREIGN KEY REFERENCES Tournament(TournamentId),
    Region int NOT NULL,
    [Open] datetime2 NOT NULL,
    [Close] datetime2 NOT NULL,
    Capacity int NOT NULL,
    MinTeamSize int NOT NULL,
    MaxTeamSize int NOT NULL,
    Rules int NOT NULL,
    RegionParticipantRole bigint FOREIGN KEY REFERENCES DiscordRole(RoleKeyId),
    RegionTeamOwnerRole bigint FOREIGN KEY REFERENCES DiscordRole(RoleKeyId),
    CONSTRAINT UNQ_TournamentRegion UNIQUE (TournamentId, Region)
)

CREATE TABLE TournamentStage (
    StageId bigint NOT NULL IDENTITY(1,1) PRIMARY KEY,
    TournamentId bigint NOT NULL FOREIGN KEY REFERENCES Tournament(TournamentId),
    [Name] nvarchar(255),
    Capacity int NOT NULL,
    [Format] int NOT NULL,
    [Sequence] int NOT NULL
)

CREATE TABLE TournamentGroup (
    GroupId bigint NOT NULL IDENTITY(1,1) PRIMARY KEY,
    TournamentStageId bigint NOT NULL FOREIGN KEY REFERENCES TournamentStage(StageId),
    [Name] nvarchar(255),
    Capacity int NOT NULL
)

CREATE TABLE TournamentTeam (
    TeamId bigint NOT NULL IDENTITY(1,1) PRIMARY KEY,
    TournamentId bigint NOT NULL FOREIGN KEY REFERENCES Tournament(TournamentId),
    [Name] nvarchar(255) NOT NULL,
    Tag varchar(5) NOT NULL,
    [Description] nvarchar(1000),
    Icon varchar(255),
    [Owner] bigint NOT NULL FOREIGN KEY REFERENCES Account(AccountId),
    [Status] int NOT NULL,
    Region int NOT NULL,
    Created datetime2 NOT NULL
)

CREATE TABLE TournamentSeed (
    GroupSeedId bigint NOT NULL IDENTITY(1,1) PRIMARY KEY,
    GroupId bigint NOT NULL FOREIGN KEY REFERENCES TournamentGroup(GroupId),
    TeamId bigint FOREIGN KEY REFERENCES TournamentTeam(TeamId),
    Seed int NOT NULL,
    UNIQUE (GroupId, Seed)
)

CREATE TABLE TournamentMatch (
    MatchId bigint NOT NULL IDENTITY(1,1) PRIMARY KEY,
    GroupId bigint NOT NULL FOREIGN KEY REFERENCES TournamentGroup(GroupId),
    [Sequence] int NOT NULL,
    AlphaTeamId bigint FOREIGN KEY REFERENCES TournamentTeam(TeamId),
    BravoTeamId bigint FOREIGN KEY REFERENCES TournamentTeam(TeamId),
    MaxGames int NOT NULL,
    Complete bit NOT NULL,
    StartTime datetime2,
    CONSTRAINT CHK_DifferentTeams CHECK (AlphaTeamId <> BravoTeamId)
)

CREATE TABLE TournamentGame (
    GameId bigint NOT NULL IDENTITY(1,1) PRIMARY KEY,
    MatchId bigint NOT NULL FOREIGN KEY REFERENCES TournamentMatch(MatchId),
    WinnerTeamId bigint FOREIGN KEY REFERENCES TournamentTeam(TeamId),
    Complete bit NOT NULL,
    MapId bigint FOREIGN KEY REFERENCES WarshipsMap(MapId)
)

CREATE TABLE TournamentParticipant (
    ParticipantId bigint NOT NULL IDENTITY(1,1) PRIMARY KEY,
    TeamId bigint NOT NULL FOREIGN KEY REFERENCES TournamentTeam(TeamId),
    PlayerId bigint NOT NULL FOREIGN KEY REFERENCES WarshipsPlayer(PlayerId),
    [Status] int NOT NULL
)

CREATE TABLE Claim (
    ClaimId bigint NOT NULL IDENTITY(1,1) PRIMARY KEY,
    Permission varchar(255) NOT NULL
)

CREATE TABLE AccountClaim (
    AccountId bigint NOT NULL FOREIGN KEY REFERENCES Account(AccountId),
    ClaimId bigint NOT NULL FOREIGN KEY REFERENCES Claim(ClaimId),
    PRIMARY KEY (AccountId, ClaimId)
)

CREATE TABLE TournamentClaim (
    TournamentId bigint NOT NULL FOREIGN KEY REFERENCES Tournament(TournamentId),
    AccountId bigint NOT NULL FOREIGN KEY REFERENCES Account(AccountId),
    ClaimId bigint NOT NULL FOREIGN KEY REFERENCES [Claim](ClaimId),
    PRIMARY KEY (TournamentId, AccountId, ClaimId)
)

CREATE TABLE TournamentTeamClaim (
    TeamId bigint NOT NULL FOREIGN KEY REFERENCES TournamentTeam(TeamId),
    AccountId bigint NOT NULL FOREIGN KEY REFERENCES Account(AccountId),
    ClaimId bigint NOT NULL FOREIGN KEY REFERENCES [Claim](ClaimId),
    PRIMARY KEY (TeamId, AccountId, ClaimId)
)

CREATE TABLE FileContent (
    FileContentId bigint NOT NULL IDENTITY(1,1) PRIMARY KEY,
    Title varchar(255) NOT NULL,
    Content varbinary(MAX)
)

CREATE TABLE SessionCache (
    Id nvarchar(449) NOT NULL PRIMARY KEY,
    [Value] varbinary(MAX) NOT NULL,
    ExpiresAtTime datetimeoffset(7) NOT NULL,
    SlidingExpirationInSeconds bigint,
    AbsoluteExpiration datetimeoffset(7)
)