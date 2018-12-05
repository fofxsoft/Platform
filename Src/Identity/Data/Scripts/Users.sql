CREATE TABLE IF NOT EXISTS `__EFMigrationsHistory` (
    `MigrationId` varchar(95) NOT NULL,
    `ProductVersion` varchar(32) NOT NULL,
    CONSTRAINT `PK___EFMigrationsHistory` PRIMARY KEY (`MigrationId`)
);

CREATE TABLE `Roles` (
    `Id` varchar(256) NOT NULL,
    `Name` varchar(256) NULL,
    `NormalizedName` varchar(256) NULL,
    `ConcurrencyStamp` varchar(256) NULL,
    CONSTRAINT `PK_Roles` PRIMARY KEY (`Id`)
);

CREATE TABLE `Users` (
    `Id` varchar(256) NOT NULL,
    `UserName` varchar(256) NULL,
    `NormalizedUserName` varchar(256) NULL,
    `Email` varchar(256) NULL,
    `NormalizedEmail` varchar(256) NULL,
    `EmailConfirmed` int NOT NULL,
    `PasswordHash` varchar(256) NULL,
    `SecurityStamp` varchar(256) NULL,
    `ConcurrencyStamp` varchar(256) NULL,
    `PhoneNumber` varchar(256) NULL,
    `PhoneNumberConfirmed` int NOT NULL,
    `TwoFactorEnabled` int NOT NULL,
    `LockoutEnd` datetime(6) NULL,
    `LockoutEnabled` int NOT NULL,
    `AccessFailedCount` int NOT NULL,
    CONSTRAINT `PK_Users` PRIMARY KEY (`Id`)
);

CREATE TABLE `RoleClaims` (
    `Id` int NOT NULL AUTO_INCREMENT,
    `RoleId` varchar(256) NOT NULL,
    `ClaimType` varchar(256) NULL,
    `ClaimValue` varchar(256) NULL,
    CONSTRAINT `PK_RoleClaims` PRIMARY KEY (`Id`),
    CONSTRAINT `FK_RoleClaims_Roles_RoleId` FOREIGN KEY (`RoleId`) REFERENCES `Roles` (`Id`) ON DELETE CASCADE
);

CREATE TABLE `UserClaims` (
    `Id` int NOT NULL AUTO_INCREMENT,
    `UserId` varchar(256) NOT NULL,
    `ClaimType` varchar(256) NULL,
    `ClaimValue` varchar(256) NULL,
    CONSTRAINT `PK_UserClaims` PRIMARY KEY (`Id`),
    CONSTRAINT `FK_UserClaims_Users_UserId` FOREIGN KEY (`UserId`) REFERENCES `Users` (`Id`) ON DELETE CASCADE
);

CREATE TABLE `UserLogins` (
    `LoginProvider` varchar(256) NOT NULL,
    `ProviderKey` varchar(256) NOT NULL,
    `ProviderDisplayName` varchar(256) NULL,
    `UserId` varchar(256) NOT NULL,
    CONSTRAINT `PK_UserLogins` PRIMARY KEY (`LoginProvider`, `ProviderKey`),
    CONSTRAINT `FK_UserLogins_Users_UserId` FOREIGN KEY (`UserId`) REFERENCES `Users` (`Id`) ON DELETE CASCADE
);

CREATE TABLE `UserRoles` (
    `UserId` varchar(256) NOT NULL,
    `RoleId` varchar(256) NOT NULL,
    CONSTRAINT `PK_UserRoles` PRIMARY KEY (`UserId`, `RoleId`),
    CONSTRAINT `FK_UserRoles_Roles_RoleId` FOREIGN KEY (`RoleId`) REFERENCES `Roles` (`Id`) ON DELETE CASCADE,
    CONSTRAINT `FK_UserRoles_Users_UserId` FOREIGN KEY (`UserId`) REFERENCES `Users` (`Id`) ON DELETE CASCADE
);

CREATE TABLE `UserTokens` (
    `UserId` varchar(256) NOT NULL,
    `LoginProvider` varchar(256) NOT NULL,
    `Name` varchar(256) NOT NULL,
    `Value` varchar(256) NULL,
    CONSTRAINT `PK_UserTokens` PRIMARY KEY (`UserId`, `LoginProvider`, `Name`),
    CONSTRAINT `FK_UserTokens_Users_UserId` FOREIGN KEY (`UserId`) REFERENCES `Users` (`Id`) ON DELETE CASCADE
);

CREATE INDEX `IX_RoleClaims_RoleId` ON `RoleClaims` (`RoleId`);

CREATE UNIQUE INDEX `RoleNameIndex` ON `Roles` (`NormalizedName`);

CREATE INDEX `IX_UserClaims_UserId` ON `UserClaims` (`UserId`);

CREATE INDEX `IX_UserLogins_UserId` ON `UserLogins` (`UserId`);

CREATE INDEX `IX_UserRoles_RoleId` ON `UserRoles` (`RoleId`);

CREATE INDEX `EmailIndex` ON `Users` (`NormalizedEmail`);

CREATE UNIQUE INDEX `UserNameIndex` ON `Users` (`NormalizedUserName`);

INSERT INTO `__EFMigrationsHistory` (`MigrationId`, `ProductVersion`)
VALUES ('20181205143954_Users', '2.1.4-rtm-31024');

