USE [TestDb]
GO

/****** Object:  Table [reusable].[SmartConfig]    Script Date: 21/10/2018 09:24:52 ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE TABLE [reusable].[SmartConfig](
  [_id] [int] IDENTITY(1,1) NOT NULL,
  [_name] [nvarchar](50) NOT NULL,
  [_value] [nvarchar](max) NULL,
  [_other] [nvarchar](50) NOT NULL,
  CONSTRAINT [PK_SmartConfig] PRIMARY KEY CLUSTERED
    (
      [_id] ASC
    )WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO

