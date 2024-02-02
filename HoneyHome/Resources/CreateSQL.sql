CREATE TABLE Rooms (
    ID INTEGER PRIMARY KEY AUTOINCREMENT,
    Name TEXT NOT NULL
);

INSERT INTO Rooms(Name) VALUES("Room 1");
INSERT INTO Rooms(Name) VALUES("Room 2");

CREATE TABLE Devices(
    ID INTEGER PRIMARY KEY AUTOINCREMENT,
    RoomID INTEGER NULL,
    DeviceTypeId  INTEGER NOT NULL,
    Name TEXT NOT NULL,
    Information TEXT NOT NULL,    
    PluginID INTEGER NOT NULL,
    PluginParameter TEXT NULL,
    HasCommand BOOL NOT NULL DEFAULT FALSE,
    ExecuteButtonName TEXT NULL         
);

CREATE TABLE Plugins (
    ID INTEGER PRIMARY KEY AUTOINCREMENT,
    Name TEXT NOT NULL,
    Path TEXT NOT NULL
);

INSERT INTO Plugins (Name, Path) VALUES("Lamp Switcher", "Plugins/LampSwitcher.dll");
INSERT INTO Plugins (Name, Path) VALUES("Temperature Detector", "Plugins/TempDetector.dll");
INSERT INTO Plugins (Name, Path) VALUES("Weather", "Plugins/Weather.dll");
INSERT INTO Plugins (Name, Path) VALUES("Voltage", "Plugins/Voltage.dll");
INSERT INTO Plugins (Name, Path) VALUES("Security", "Plugins/Security.dll");

CREATE TABLE DeviceValues (
    ID INTEGER PRIMARY KEY AUTOINCREMENT,
    DeviceID INTEGER NOT NULL,
    DeviceValue TEXT NOT NULL,
    UpdateDate DATETIME NOT NULL DEFAULT (strftime('%s', 'now'))
)

