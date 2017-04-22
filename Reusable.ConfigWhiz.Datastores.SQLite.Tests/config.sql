DROP TABLE IF EXISTS Setting1;

CREATE TABLE IF NOT EXISTS Setting1( 
        [Name] TEXT NOT NULL COLLATE NOCASE,
        [Value] TEXT NOT NULL COLLATE NOCASE
);

INSERT OR REPLACE INTO Setting1([Name], [Value]) VALUES('Utf8SettingDE', 'äöüß');
INSERT OR REPLACE INTO Setting1([Name], [Value]) VALUES('Utf8SettingPL', 'ąęśćżźó');
INSERT OR REPLACE INTO Setting1([Name], [Value]) VALUES('ArraySetting[0]', '5');
INSERT OR REPLACE INTO Setting1([Name], [Value]) VALUES('ArraySetting[1]', '8');
INSERT OR REPLACE INTO Setting1([Name], [Value]) VALUES('DictionarySetting[foo]', '21');
INSERT OR REPLACE INTO Setting1([Name], [Value]) VALUES('DictionarySetting[bar]', '34');
INSERT OR REPLACE INTO Setting1([Name], [Value]) VALUES('NestedConfig.StringSetting', 'Bar');
INSERT OR REPLACE INTO Setting1([Name], [Value]) VALUES('IgnoredConfig.StringSettingDE', 'Qux');

INSERT OR REPLACE INTO Setting1([Name], [Value]) VALUES('TestConfig1.Utf8SettingDE', 'äöüß');
INSERT OR REPLACE INTO Setting1([Name], [Value]) VALUES('TestConfig1.Utf8SettingPL', 'ąęśćżźó');
INSERT OR REPLACE INTO Setting1([Name], [Value]) VALUES('TestConfig1.ArraySetting[0]', '51');
INSERT OR REPLACE INTO Setting1([Name], [Value]) VALUES('TestConfig1.ArraySetting[1]', '81');
INSERT OR REPLACE INTO Setting1([Name], [Value]) VALUES('TestConfig1.DictionarySetting[foo]', '212');
INSERT OR REPLACE INTO Setting1([Name], [Value]) VALUES('TestConfig1.DictionarySetting[bar]', '342');
INSERT OR REPLACE INTO Setting1([Name], [Value]) VALUES('TestConfig1.NestedConfig.StringSetting', 'Bar');
INSERT OR REPLACE INTO Setting1([Name], [Value]) VALUES('TestConfig1.IgnoredConfig.StringSettingDE', 'Qux');


DROP TABLE IF EXISTS Setting3;

CREATE TABLE IF NOT EXISTS Setting3( 
        [Name] TEXT NOT NULL COLLATE NOCASE,
        [Value] TEXT NOT NULL COLLATE NOCASE,
        [Environment] TEXT NOT NULL COLLATE NOCASE,
        [Config] TEXT NOT NULL COLLATE NOCASE
);

INSERT OR REPLACE INTO Setting3([Name], [Value], [Environment], [Config]) VALUES('Utf8SettingDE', 'äöüß', 'TEST', 'TestConfig3');
INSERT OR REPLACE INTO Setting3([Name], [Value], [Environment], [Config]) VALUES('Utf8SettingPL', 'ąęśćżźó', 'TEST', 'TestConfig3');
INSERT OR REPLACE INTO Setting3([Name], [Value], [Environment], [Config]) VALUES('ArraySetting[0]', '52', 'TEST', 'TestConfig3');
INSERT OR REPLACE INTO Setting3([Name], [Value], [Environment], [Config]) VALUES('ArraySetting[1]', '82', 'TEST', 'TestConfig3');
INSERT OR REPLACE INTO Setting3([Name], [Value], [Environment], [Config]) VALUES('DictionarySetting[foo]', '213', 'TEST', 'TestConfig3');
INSERT OR REPLACE INTO Setting3([Name], [Value], [Environment], [Config]) VALUES('DictionarySetting[bar]', '343', 'TEST', 'TestConfig3');
INSERT OR REPLACE INTO Setting3([Name], [Value], [Environment], [Config]) VALUES('NestedConfig.StringSetting', 'Bar', 'TEST', 'TestConfig3');
INSERT OR REPLACE INTO Setting3([Name], [Value], [Environment], [Config]) VALUES('IgnoredConfig.StringSettingDE', 'Qux', 'TEST', 'TestConfig3');

-- some noise settings to be sure they don't get selected
INSERT OR REPLACE INTO Setting3([Name], [Value], [Environment], [Config]) VALUES('Utf8SettingDE', 'äöüß-', 'TEST', 'UndefinedConfig');
INSERT OR REPLACE INTO Setting3([Name], [Value], [Environment], [Config]) VALUES('Utf8SettingPL', 'ąęśćżźó-', 'TEST', 'UndefinedConfig');
INSERT OR REPLACE INTO Setting3([Name], [Value], [Environment], [Config]) VALUES('ArraySetting[0]', '54-', 'TEST', 'UndefinedConfig');
INSERT OR REPLACE INTO Setting3([Name], [Value], [Environment], [Config]) VALUES('ArraySetting[1]', '84-', 'TEST', 'UndefinedConfig');
INSERT OR REPLACE INTO Setting3([Name], [Value], [Environment], [Config]) VALUES('DictionarySetting[foo]', '214-', 'TEST', 'UndefinedConfig');
INSERT OR REPLACE INTO Setting3([Name], [Value], [Environment], [Config]) VALUES('DictionarySetting[bar]', '344-', 'TEST', 'UndefinedConfig');
INSERT OR REPLACE INTO Setting3([Name], [Value], [Environment], [Config]) VALUES('NestedConfig.StringSetting', 'Bar-', 'TEST', 'UndefinedConfig');
INSERT OR REPLACE INTO Setting3([Name], [Value], [Environment], [Config]) VALUES('IgnoredConfig.StringSettingDE', 'Qux-', 'TEST', 'UndefinedConfig');
