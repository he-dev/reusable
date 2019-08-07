USE [TestDb]
--GO

-- drop table [reusable].[TestConfig]

BEGIN TRANSACTION

if (not exists (select * from INFORMATION_SCHEMA.TABLES where TABLE_SCHEMA = 'reusable' and TABLE_NAME = 'TestConfig'))
begin
	
	SET ANSI_NULLS ON;
	SET QUOTED_IDENTIFIER ON;

	-- use nullable columns for easier updates
	CREATE TABLE [reusable].[TestConfig]
	(
		[_id] [int] NOT NULL,
		[_name] [nvarchar](50) NULL,
		[_value] [nvarchar](max) NULL,
		[_env] [nvarchar](50) NULL,		
		[_ver] [nvarchar](50) NULL,		
	 CONSTRAINT [PK_TestConfig] PRIMARY KEY CLUSTERED 
	 (
		[_id] ASC
	 )WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
	) ON [PRIMARY]

end

-- clear old data
truncate table [reusable].[TestConfig]

--DECLARE @CRLF nvarchar(10) = char(13)+char(10);

insert into [reusable].[TestConfig]([_id], [_name], [_value], [_env], [_ver])
select [_id], [_name], [_value], [_env], [_ver]
from
    (values
     (10, 'TestDto.String', 'Tower Bridge', 'test', '1'),
     (11, 'TestDto.Boolean', 'true', 'test', '1'),
     (13, 'TestDto.Int32', '65', 'test', '1'),
     (14, 'TestDto.Double', '2.25', 'test', '1'),
     (15, 'TestDto.Decimal', '10.3', 'test', '1'),
     (16, 'TestDto.DateTime', '1894-06-30', 'test', '1'),
     (17, 'TestDto.TimeSpan', '01:15:00', 'test', '1'),
     (18, 'TestDto.ListOfInt32', '[11, 12]', 'else', '1')
    ) v ([_id], [_name], [_value], [_env], [_ver]);

-- -- insert new data
-- insert into [reusable].[TestConfig]([_id])
-- select [_id]
-- from (values (1),(2),(3),(4),(7)) v ([_id]);
-- 
-- --- set actual data by id
--   
-- update [reusable].[TestConfig]
-- set
--     [_name] = 'User.Name',
--     [_value] = 'Bob',    
--     [_other] = null
-- WHERE [_id] = 1;
-- 
-- update [reusable].[TestConfig]
-- set
-- 	[_name] = 'User.IsCool',
-- 	[_value] = 'true',
-- 	[_other] = null
-- where [_id] = 2;
-- 
-- update [reusable].[TestConfig]
-- set
-- 	[_name] = 'User.Name',
-- 	[_value] = 'Tom',
-- 	[_other] = 'Someone-else'
-- where [_id] = 3;




COMMIT;

-- display current data
SELECT [_id], [_name], [_value], [_env], [_ver]
FROM [reusable].[TestConfig]


