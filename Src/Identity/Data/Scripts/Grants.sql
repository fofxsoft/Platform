CREATE TABLE IF NOT EXISTS `__EFMigrationsHistory` (
    `MigrationId` varchar(95) NOT NULL,
    `ProductVersion` varchar(32) NOT NULL,
    CONSTRAINT `PK___EFMigrationsHistory` PRIMARY KEY (`MigrationId`)
);

CREATE TABLE `DeviceCodes` (
    `DeviceCode` varchar(200) NOT NULL,
    `UserCode` varchar(200) NOT NULL,
    `SubjectId` varchar(200) NULL,
    `ClientId` varchar(200) NOT NULL,
    `CreationTime` datetime(6) NOT NULL,
    `Expiration` datetime(6) NOT NULL,
    `Data` longtext NOT NULL,
    CONSTRAINT `PK_DeviceCodes` PRIMARY KEY (`UserCode`)
);

CREATE TABLE `PersistedGrants` (
    `Key` varchar(200) NOT NULL,
    `Type` varchar(50) NOT NULL,
    `SubjectId` varchar(200) NULL,
    `ClientId` varchar(200) NOT NULL,
    `CreationTime` datetime(6) NOT NULL,
    `Expiration` datetime(6) NULL,
    `Data` longtext NOT NULL,
    CONSTRAINT `PK_PersistedGrants` PRIMARY KEY (`Key`)
);

CREATE UNIQUE INDEX `IX_DeviceCodes_DeviceCode` ON `DeviceCodes` (`DeviceCode`);

CREATE INDEX `IX_PersistedGrants_SubjectId_ClientId_Type` ON `PersistedGrants` (`SubjectId`, `ClientId`, `Type`);

INSERT INTO `__EFMigrationsHistory` (`MigrationId`, `ProductVersion`)
VALUES ('20181205143943_Grants', '2.1.4-rtm-31024');

