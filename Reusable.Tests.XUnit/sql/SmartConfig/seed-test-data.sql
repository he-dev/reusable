USE [TestDb]
--GO

-- drop table [reusable].[SmartConfig]

BEGIN TRANSACTION

if (not exists (select * from INFORMATION_SCHEMA.TABLES where TABLE_SCHEMA = 'reusable' and TABLE_NAME = 'SmartConfig'))
begin
	
	SET ANSI_NULLS ON;
	SET QUOTED_IDENTIFIER ON;

	-- use nullable columns for easier updates
	CREATE TABLE [reusable].[SmartConfig]
	(
		[_id] [int] NOT NULL,
		[_name] [nvarchar](50) NULL,
		[_value] [nvarchar](max) NULL,
		[_other] [nvarchar](50) NULL,		
	 CONSTRAINT [PK_SmartConfig] PRIMARY KEY CLUSTERED 
	 (
		[_id] ASC
	 )WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
	) ON [PRIMARY]

end

-- clear old data
truncate table [reusable].[SmartConfig]

DECLARE @CRLF nvarchar(10) = char(13)+char(10);

-- insert new data
insert into [reusable].[SmartConfig]([_id])
select [_id]
from (values (1),(2),(3),(4),(7)) v ([_id]);

--- set actual data by id
  
update [reusable].[SmartConfig]
set
    [_name] = 'User.Name',
    [_value] = 'Bob',    
    [_other] = null
WHERE [_id] = 1;

update [reusable].[SmartConfig]
set
	[_name] = 'User.IsCool',
	[_value] = 'true',
	[_other] = null
WHERE [_id] = 2;

update [reusable].[SmartConfig]
set
	[_name] = 'User.Name',
	[_value] = 'Tom',
	[_other] = 'Someone-else'
WHERE [_id] = 3;


insert into [reusable].[SmartConfig]([_id], [_name], [_value], [_other])
select [_id], [_name], [_value], [_other]
from 
(values 
  (100, 'Type.String', 'str', 't'),
	(101, 'Type.Bool', 'true', 't'),
	(103, 'Type.Int', '3', 't'),
	(104, 'Type.Double', '1.25', 't'),
	(105, 'Type.DateTime', '2019-01-02', 't')
) v ([_id], [_name], [_value], [_other]);

COMMIT;

-- display current data
SELECT [_id], [_name], [_value], [_other]
FROM [reusable].[SmartConfig]


