CREATE DATABASE [DEMO] ON  PRIMARY 
( 
	NAME = N'DEMO', FILENAME = N'C:\Program Files\Microsoft SQL Server\MSSQL14.MSSQLSERVER\MSSQL\DATA\DEMO.mdf' , SIZE = 4096KB , MAXSIZE = UNLIMITED, FILEGROWTH = 1024KB )
	LOG ON ( NAME = N'DEMOlog', FILENAME = N'C:\Program Files\Microsoft SQL Server\MSSQL14.MSSQLSERVER\MSSQL\DATA\DEMO_log.ldf' , SIZE = 3456KB , MAXSIZE = 2048GB , FILEGROWTH = 10%)
GO

USE [DEMO]
GO

ALTER DATABASE [DEMO] SET COMPATIBILITY_LEVEL = 100
GO

CREATE TYPE [TVP_ADDRESS] AS TABLE
(
	[ID] [int] NULL,
	[Address] [varchar](100) NULL,
	[UserCode] [int] NULL,
	[Enabled] [bit] NULL
)
GO

CREATE TABLE [TB_ADDRESS]
(
	[ID] [int] IDENTITY(1,1) NOT NULL,
	[Address] [varchar](100) NOT NULL,
	[UserCode] [int] NULL,
	[Enabled] [bit] NOT NULL,
 CONSTRAINT [PK_TB_ADDRESS] PRIMARY KEY CLUSTERED ([ID] ASC) ON [PRIMARY]
)
GO

CREATE TABLE [TB_USER]
(
	[ID] [int] IDENTITY(1,1) NOT NULL,
	[Name] [varchar](80) NOT NULL,
	[Nickname] [varchar](50) NOT NULL,
	[RG] [varchar](50) NOT NULL,
	[CPF] [varchar](50) NOT NULL,
	[Enabled] [bit] NOT NULL,
	CONSTRAINT [PK_TB_USER] PRIMARY KEY CLUSTERED ([ID] ASC)
) 
GO

SET IDENTITY_INSERT [TB_ADDRESS] ON
GO

INSERT [TB_ADDRESS] ([ID], [Address], [UserCode], [Enabled]) VALUES (51, N'Rua teste 1', 1, 1)
GO

INSERT [TB_ADDRESS] ([ID], [Address], [UserCode], [Enabled]) VALUES (52, N'Rua teste 2', 1, 1)
GO

SET IDENTITY_INSERT [TB_ADDRESS] OFF
GO

SET IDENTITY_INSERT [TB_USER] ON 
GO

INSERT [TB_USER] ([ID], [Name], [Nickname], [RG], [CPF], [Enabled]) VALUES (1, N'Jose Carlos de Jesus Junior UPDATED AT 17/01/1441  9:06:11:830AM', N'Junior', N'4091', N'3841', 1)
GO
INSERT [TB_USER] ([ID], [Name], [Nickname], [RG], [CPF], [Enabled]) VALUES (8, N'Joao das Couves', N'Joao', N'222', N'222', 1)
GO
INSERT [TB_USER] ([ID], [Name], [Nickname], [RG], [CPF], [Enabled]) VALUES (9, N'Kiko Loureiro', N'Kiko Loureiro', N'333', N'333', 1)
GO
INSERT [TB_USER] ([ID], [Name], [Nickname], [RG], [CPF], [Enabled]) VALUES (10, N'Ingwie Mallmsteeen', N'Malmsteen', N'444', N'444', 1)
GO
INSERT [TB_USER] ([ID], [Name], [Nickname], [RG], [CPF], [Enabled]) VALUES (12, N'Joe Satriani', N'Satriani', N'555', N'555', 1)
GO
INSERT [TB_USER] ([ID], [Name], [Nickname], [RG], [CPF], [Enabled]) VALUES (13, N'Steve Vai', N'Vai', N'666', N'666', 1)
GO
INSERT [TB_USER] ([ID], [Name], [Nickname], [RG], [CPF], [Enabled]) VALUES (14, N'Joe Pettrucci', N'Pettruci', N'777', N'777', 1)
GO
INSERT [TB_USER] ([ID], [Name], [Nickname], [RG], [CPF], [Enabled]) VALUES (15, N'Ozielzinho do Nordeste', N'Ozielzinho', N'43', N'343', 1)
GO
INSERT [TB_USER] ([ID], [Name], [Nickname], [RG], [CPF], [Enabled]) VALUES (16, N'Rafael Bittencourt', N'Rafael do Angra', N'139', N'90', 1)
GO
INSERT [TB_USER] ([ID], [Name], [Nickname], [RG], [CPF], [Enabled]) VALUES (17, N'Zack Wild', N'Zack', N'8098', N'900', 1)
GO
INSERT [TB_USER] ([ID], [Name], [Nickname], [RG], [CPF], [Enabled]) VALUES (191, N'Steve Morse', N'Steve Morse', N'8253', N'1153', 1)
GO
INSERT [TB_USER] ([ID], [Name], [Nickname], [RG], [CPF], [Enabled]) VALUES (192, N'Steve Marriott ', N'Steve Marriott ', N'8254', N'1154', 1)
GO
INSERT [TB_USER] ([ID], [Name], [Nickname], [RG], [CPF], [Enabled]) VALUES (193, N'Steve Vai ', N'Steve Vai ', N'8255', N'1155', 1)
GO
INSERT [TB_USER] ([ID], [Name], [Nickname], [RG], [CPF], [Enabled]) VALUES (194, N'Syd Barrett', N'Syd Barrett', N'8256', N'1156', 1)
GO
INSERT [TB_USER] ([ID], [Name], [Nickname], [RG], [CPF], [Enabled]) VALUES (195, N'Synyster Gates', N'Synyster Gates', N'8257', N'1157', 1)
GO
INSERT [TB_USER] ([ID], [Name], [Nickname], [RG], [CPF], [Enabled]) VALUES (196, N'Ted Nugent', N'Ted Nugent', N'8258', N'1158', 1)
GO
INSERT [TB_USER] ([ID], [Name], [Nickname], [RG], [CPF], [Enabled]) VALUES (197, N'Terry Balsamo ', N'Terry Balsamo ', N'8259', N'1159', 1)
GO
INSERT [TB_USER] ([ID], [Name], [Nickname], [RG], [CPF], [Enabled]) VALUES (198, N'Tom DeLonge ', N'Tom DeLonge ', N'8260', N'1160', 1)
GO
INSERT [TB_USER] ([ID], [Name], [Nickname], [RG], [CPF], [Enabled]) VALUES (199, N'Tom Linton', N'Tom Linton', N'8261', N'1161', 1)
GO
INSERT [TB_USER] ([ID], [Name], [Nickname], [RG], [CPF], [Enabled]) VALUES (200, N'Tom Morello ', N'Tom Morello ', N'8262', N'1162', 1)
GO
INSERT [TB_USER] ([ID], [Name], [Nickname], [RG], [CPF], [Enabled]) VALUES (201, N'Tom Petty ', N'Tom Petty ', N'8263', N'1163', 1)
GO
INSERT [TB_USER] ([ID], [Name], [Nickname], [RG], [CPF], [Enabled]) VALUES (202, N'Tom Verlaine', N'Tom Verlaine', N'8264', N'1164', 1)
GO
INSERT [TB_USER] ([ID], [Name], [Nickname], [RG], [CPF], [Enabled]) VALUES (203, N'Trey Anastasio ', N'Trey Anastasio ', N'8265', N'1165', 1)
GO
INSERT [TB_USER] ([ID], [Name], [Nickname], [RG], [CPF], [Enabled]) VALUES (204, N'Tom Kaulitz', N'Tom Kaulitz', N'8266', N'1166', 1)
GO
INSERT [TB_USER] ([ID], [Name], [Nickname], [RG], [CPF], [Enabled]) VALUES (205, N'Uruha ', N'Uruha ', N'8267', N'1167', 1)
GO
INSERT [TB_USER] ([ID], [Name], [Nickname], [RG], [CPF], [Enabled]) VALUES (206, N'Vernon Reid', N'Vernon Reid', N'8268', N'1168', 1)
GO
INSERT [TB_USER] ([ID], [Name], [Nickname], [RG], [CPF], [Enabled]) VALUES (207, N'Vinnie Moore', N'Vinnie Moore', N'8269', N'1169', 1)
GO
INSERT [TB_USER] ([ID], [Name], [Nickname], [RG], [CPF], [Enabled]) VALUES (208, N'Warren Cuccurullo ', N'Warren Cuccurullo ', N'8270', N'1170', 1)
GO
INSERT [TB_USER] ([ID], [Name], [Nickname], [RG], [CPF], [Enabled]) VALUES (209, N'Zacky Vengeance', N'Zacky Vengeance', N'8271', N'1171', 1)
GO
INSERT [TB_USER] ([ID], [Name], [Nickname], [RG], [CPF], [Enabled]) VALUES (210, N'Ace Frehley ', N'Ace Frehley ', N'8099', N'999', 1)
GO
INSERT [TB_USER] ([ID], [Name], [Nickname], [RG], [CPF], [Enabled]) VALUES (211, N'Adam Gontier', N'Adam Gontier', N'8100', N'1000', 1)
GO
INSERT [TB_USER] ([ID], [Name], [Nickname], [RG], [CPF], [Enabled]) VALUES (212, N'Adrian Belew', N'Adrian Belew', N'8101', N'1001', 1)
GO
INSERT [TB_USER] ([ID], [Name], [Nickname], [RG], [CPF], [Enabled]) VALUES (213, N'Adrian Smith', N'Adrian Smith', N'8102', N'1002', 1)
GO
INSERT [TB_USER] ([ID], [Name], [Nickname], [RG], [CPF], [Enabled]) VALUES (214, N'Albert Hammond Jr', N'Albert Hammond Jr', N'8103', N'1003', 1)
GO
INSERT [TB_USER] ([ID], [Name], [Nickname], [RG], [CPF], [Enabled]) VALUES (215, N'Alex Lifeson', N'Alex Lifeson', N'8104', N'1004', 1)
GO
INSERT [TB_USER] ([ID], [Name], [Nickname], [RG], [CPF], [Enabled]) VALUES (216, N'Andy Summers', N'Andy Summers', N'8105', N'1005', 1)
GO
INSERT [TB_USER] ([ID], [Name], [Nickname], [RG], [CPF], [Enabled]) VALUES (217, N'Andy Taylor', N'Andy Taylor', N'8106', N'1006', 1)
GO
INSERT [TB_USER] ([ID], [Name], [Nickname], [RG], [CPF], [Enabled]) VALUES (218, N'Angus Young', N'Angus Young', N'8107', N'1007', 1)
GO
INSERT [TB_USER] ([ID], [Name], [Nickname], [RG], [CPF], [Enabled]) VALUES (219, N'Aoi', N'Aoi', N'8108', N'1008', 1)
GO
INSERT [TB_USER] ([ID], [Name], [Nickname], [RG], [CPF], [Enabled]) VALUES (220, N'Arthur Netto ', N'Arthur Netto ', N'8109', N'1009', 1)
GO
INSERT [TB_USER] ([ID], [Name], [Nickname], [RG], [CPF], [Enabled]) VALUES (221, N'Barry Melton ', N'Barry Melton ', N'8110', N'1010', 1)
GO
INSERT [TB_USER] ([ID], [Name], [Nickname], [RG], [CPF], [Enabled]) VALUES (222, N'Barry Stock ', N'Barry Stock ', N'8111', N'1011', 1)
GO
INSERT [TB_USER] ([ID], [Name], [Nickname], [RG], [CPF], [Enabled]) VALUES (223, N'Benji Madden ', N'Benji Madden ', N'8112', N'1012', 1)
GO
INSERT [TB_USER] ([ID], [Name], [Nickname], [RG], [CPF], [Enabled]) VALUES (224, N'Billie Joe Armstrong ', N'Billie Joe Armstrong ', N'8113', N'1013', 1)
GO
INSERT [TB_USER] ([ID], [Name], [Nickname], [RG], [CPF], [Enabled]) VALUES (225, N'Billy Corgan ', N'Billy Corgan ', N'8114', N'1014', 1)
GO
INSERT [TB_USER] ([ID], [Name], [Nickname], [RG], [CPF], [Enabled]) VALUES (226, N'Billy Gibbons ', N'Billy Gibbons ', N'8115', N'1015', 1)
GO
INSERT [TB_USER] ([ID], [Name], [Nickname], [RG], [CPF], [Enabled]) VALUES (227, N'Billy Martin ', N'Billy Martin ', N'8116', N'1016', 1)
GO
INSERT [TB_USER] ([ID], [Name], [Nickname], [RG], [CPF], [Enabled]) VALUES (228, N'Billy Squier', N'Billy Squier', N'8117', N'1017', 1)
GO
INSERT [TB_USER] ([ID], [Name], [Nickname], [RG], [CPF], [Enabled]) VALUES (229, N'Blues Saraceno', N'Blues Saraceno', N'8118', N'1018', 1)
GO
INSERT [TB_USER] ([ID], [Name], [Nickname], [RG], [CPF], [Enabled]) VALUES (230, N'Bo Diddley', N'Bo Diddley', N'8119', N'1019', 1)
GO
INSERT [TB_USER] ([ID], [Name], [Nickname], [RG], [CPF], [Enabled]) VALUES (231, N'Bob Mould ', N'Bob Mould ', N'8120', N'1020', 1)
GO
INSERT [TB_USER] ([ID], [Name], [Nickname], [RG], [CPF], [Enabled]) VALUES (232, N'Bob Kulick', N'Bob Kulick', N'8121', N'1021', 1)
GO
INSERT [TB_USER] ([ID], [Name], [Nickname], [RG], [CPF], [Enabled]) VALUES (233, N'Bou', N'Bou', N'8122', N'1022', 1)
GO
INSERT [TB_USER] ([ID], [Name], [Nickname], [RG], [CPF], [Enabled]) VALUES (234, N'Brad Delson ', N'Brad Delson ', N'8123', N'1023', 1)
GO
INSERT [TB_USER] ([ID], [Name], [Nickname], [RG], [CPF], [Enabled]) VALUES (235, N'Brian May ', N'Brian May ', N'8124', N'1024', 1)
GO
INSERT [TB_USER] ([ID], [Name], [Nickname], [RG], [CPF], [Enabled]) VALUES (236, N'Brian Setzer ', N'Brian Setzer ', N'8125', N'1025', 1)
GO
INSERT [TB_USER] ([ID], [Name], [Nickname], [RG], [CPF], [Enabled]) VALUES (237, N'Bruce Springsteen', N'Bruce Springsteen', N'8126', N'1026', 1)
GO
INSERT [TB_USER] ([ID], [Name], [Nickname], [RG], [CPF], [Enabled]) VALUES (238, N'Bruce Kulick ', N'Bruce Kulick ', N'8127', N'1027', 1)
GO
INSERT [TB_USER] ([ID], [Name], [Nickname], [RG], [CPF], [Enabled]) VALUES (239, N'Buddy Holly', N'Buddy Holly', N'8128', N'1028', 1)
GO
INSERT [TB_USER] ([ID], [Name], [Nickname], [RG], [CPF], [Enabled]) VALUES (240, N'Captain Sensible ', N'Captain Sensible ', N'8129', N'1029', 1)
GO
INSERT [TB_USER] ([ID], [Name], [Nickname], [RG], [CPF], [Enabled]) VALUES (241, N'Carlos Cat', N'Carlos Cat', N'8130', N'1030', 1)
GO
INSERT [TB_USER] ([ID], [Name], [Nickname], [RG], [CPF], [Enabled]) VALUES (242, N'Carlos Santana', N'Carlos Santana', N'8131', N'1031', 1)
GO
INSERT [TB_USER] ([ID], [Name], [Nickname], [RG], [CPF], [Enabled]) VALUES (243, N'C.C. DeVille', N'C.C. DeVille', N'8132', N'1032', 1)
GO
INSERT [TB_USER] ([ID], [Name], [Nickname], [RG], [CPF], [Enabled]) VALUES (244, N'Chad Kroeger ', N'Chad Kroeger ', N'8133', N'1033', 1)
GO
INSERT [TB_USER] ([ID], [Name], [Nickname], [RG], [CPF], [Enabled]) VALUES (245, N'Charlie Christian', N'Charlie Christian', N'8134', N'1034', 1)
GO
INSERT [TB_USER] ([ID], [Name], [Nickname], [RG], [CPF], [Enabled]) VALUES (246, N'Chet Atkins', N'Chet Atkins', N'8135', N'1035', 1)
GO
INSERT [TB_USER] ([ID], [Name], [Nickname], [RG], [CPF], [Enabled]) VALUES (247, N'Chris Rea', N'Chris Rea', N'8136', N'1036', 1)
GO
INSERT [TB_USER] ([ID], [Name], [Nickname], [RG], [CPF], [Enabled]) VALUES (248, N'Chrissie Hynde', N'Chrissie Hynde', N'8137', N'1037', 1)
GO
INSERT [TB_USER] ([ID], [Name], [Nickname], [RG], [CPF], [Enabled]) VALUES (249, N'Chuck Berry', N'Chuck Berry', N'8138', N'1038', 1)
GO
INSERT [TB_USER] ([ID], [Name], [Nickname], [RG], [CPF], [Enabled]) VALUES (250, N'Dan Estrin ', N'Dan Estrin ', N'8139', N'1039', 1)
GO
INSERT [TB_USER] ([ID], [Name], [Nickname], [RG], [CPF], [Enabled]) VALUES (251, N'Daniel Johns ', N'Daniel Johns ', N'8140', N'1040', 1)
GO
INSERT [TB_USER] ([ID], [Name], [Nickname], [RG], [CPF], [Enabled]) VALUES (252, N'Dave Davies', N'Dave Davies', N'8141', N'1041', 1)
GO
INSERT [TB_USER] ([ID], [Name], [Nickname], [RG], [CPF], [Enabled]) VALUES (253, N'Dave Grohl', N'Dave Grohl', N'8142', N'1042', 1)
GO
INSERT [TB_USER] ([ID], [Name], [Nickname], [RG], [CPF], [Enabled]) VALUES (254, N'Dave Navarro', N'Dave Navarro', N'8143', N'1043', 1)
GO
INSERT [TB_USER] ([ID], [Name], [Nickname], [RG], [CPF], [Enabled]) VALUES (255, N'David Bates', N'David Bates', N'8144', N'1044', 1)
GO
INSERT [TB_USER] ([ID], [Name], [Nickname], [RG], [CPF], [Enabled]) VALUES (256, N'David Evans', N'David Evans', N'8145', N'1045', 1)
GO
INSERT [TB_USER] ([ID], [Name], [Nickname], [RG], [CPF], [Enabled]) VALUES (257, N'David Gilmour', N'David Gilmour', N'8146', N'1046', 1)
GO
INSERT [TB_USER] ([ID], [Name], [Nickname], [RG], [CPF], [Enabled]) VALUES (258, N'Dexter Holand ', N'Dexter Holand ', N'8147', N'1047', 1)
GO
INSERT [TB_USER] ([ID], [Name], [Nickname], [RG], [CPF], [Enabled]) VALUES (259, N'Dick Dale', N'Dick Dale', N'8148', N'1048', 1)
GO
INSERT [TB_USER] ([ID], [Name], [Nickname], [RG], [CPF], [Enabled]) VALUES (260, N'Die ', N'Die ', N'8149', N'1049', 1)
GO
INSERT [TB_USER] ([ID], [Name], [Nickname], [RG], [CPF], [Enabled]) VALUES (261, N'Dj Ashba', N'Dj Ashba', N'8150', N'1050', 1)
GO
INSERT [TB_USER] ([ID], [Name], [Nickname], [RG], [CPF], [Enabled]) VALUES (262, N'Drake Bell', N'Drake Bell', N'8151', N'1051', 1)
GO
INSERT [TB_USER] ([ID], [Name], [Nickname], [RG], [CPF], [Enabled]) VALUES (263, N'Duane Allman ', N'Duane Allman ', N'8152', N'1052', 1)
GO
INSERT [TB_USER] ([ID], [Name], [Nickname], [RG], [CPF], [Enabled]) VALUES (264, N'Duane Eddy', N'Duane Eddy', N'8153', N'1053', 1)
GO
INSERT [TB_USER] ([ID], [Name], [Nickname], [RG], [CPF], [Enabled]) VALUES (265, N'Eddie Van Halen ', N'Eddie Van Halen ', N'8154', N'1054', 1)
GO
INSERT [TB_USER] ([ID], [Name], [Nickname], [RG], [CPF], [Enabled]) VALUES (266, N'Elvis Presley', N'Elvis Presley', N'8155', N'1055', 1)
GO
INSERT [TB_USER] ([ID], [Name], [Nickname], [RG], [CPF], [Enabled]) VALUES (267, N'Enrico Schiavo', N'Enrico Schiavo', N'8156', N'1056', 1)
GO
INSERT [TB_USER] ([ID], [Name], [Nickname], [RG], [CPF], [Enabled]) VALUES (268, N'Eric Clapton ', N'Eric Clapton ', N'8157', N'1057', 1)
GO
INSERT [TB_USER] ([ID], [Name], [Nickname], [RG], [CPF], [Enabled]) VALUES (269, N'Eric Johnson', N'Eric Johnson', N'8158', N'1058', 1)
GO
INSERT [TB_USER] ([ID], [Name], [Nickname], [RG], [CPF], [Enabled]) VALUES (270, N'Frank Zappa ', N'Frank Zappa ', N'8159', N'1059', 1)
GO
INSERT [TB_USER] ([ID], [Name], [Nickname], [RG], [CPF], [Enabled]) VALUES (271, N'Francis Rossi ', N'Francis Rossi ', N'8160', N'1060', 1)
GO
INSERT [TB_USER] ([ID], [Name], [Nickname], [RG], [CPF], [Enabled]) VALUES (272, N'George Harrison ', N'George Harrison ', N'8161', N'1061', 1)
GO
INSERT [TB_USER] ([ID], [Name], [Nickname], [RG], [CPF], [Enabled]) VALUES (273, N'Goktan Kural', N'Goktan Kural', N'8162', N'1062', 1)
GO
INSERT [TB_USER] ([ID], [Name], [Nickname], [RG], [CPF], [Enabled]) VALUES (274, N'Graham Coxon', N'Graham Coxon', N'8163', N'1063', 1)
GO
INSERT [TB_USER] ([ID], [Name], [Nickname], [RG], [CPF], [Enabled]) VALUES (275, N'Hank B. Marvin ', N'Hank B. Marvin ', N'8164', N'1064', 1)
GO
INSERT [TB_USER] ([ID], [Name], [Nickname], [RG], [CPF], [Enabled]) VALUES (276, N'Hide', N'Hide', N'8165', N'1065', 1)
GO
INSERT [TB_USER] ([ID], [Name], [Nickname], [RG], [CPF], [Enabled]) VALUES (277, N'Hillel Slovak ', N'Hillel Slovak ', N'8166', N'1066', 1)
GO
INSERT [TB_USER] ([ID], [Name], [Nickname], [RG], [CPF], [Enabled]) VALUES (278, N'Hizaki ', N'Hizaki ', N'8167', N'1067', 1)
GO
INSERT [TB_USER] ([ID], [Name], [Nickname], [RG], [CPF], [Enabled]) VALUES (279, N'Izzy Stradlin ', N'Izzy Stradlin ', N'8168', N'1068', 1)
GO
INSERT [TB_USER] ([ID], [Name], [Nickname], [RG], [CPF], [Enabled]) VALUES (280, N'Jack White ', N'Jack White ', N'8169', N'1069', 1)
GO
INSERT [TB_USER] ([ID], [Name], [Nickname], [RG], [CPF], [Enabled]) VALUES (281, N'Jade Puget ', N'Jade Puget ', N'8170', N'1070', 1)
GO
INSERT [TB_USER] ([ID], [Name], [Nickname], [RG], [CPF], [Enabled]) VALUES (282, N'James Honeyman-Scott ', N'James Honeyman-Scott ', N'8171', N'1071', 1)
GO
INSERT [TB_USER] ([ID], [Name], [Nickname], [RG], [CPF], [Enabled]) VALUES (283, N'Jamie Cook ', N'Jamie Cook ', N'8172', N'1072', 1)
GO
INSERT [TB_USER] ([ID], [Name], [Nickname], [RG], [CPF], [Enabled]) VALUES (284, N'Jason White ', N'Jason White ', N'8173', N'1073', 1)
GO
INSERT [TB_USER] ([ID], [Name], [Nickname], [RG], [CPF], [Enabled]) VALUES (285, N'Jeff Beck', N'Jeff Beck', N'8174', N'1074', 1)
GO
INSERT [TB_USER] ([ID], [Name], [Nickname], [RG], [CPF], [Enabled]) VALUES (286, N'Jim Adkins ', N'Jim Adkins ', N'8175', N'1075', 1)
GO
INSERT [TB_USER] ([ID], [Name], [Nickname], [RG], [CPF], [Enabled]) VALUES (287, N'Jimi Hendrix ', N'Jimi Hendrix ', N'8176', N'1076', 1)
GO
INSERT [TB_USER] ([ID], [Name], [Nickname], [RG], [CPF], [Enabled]) VALUES (288, N'Jimmy Page ', N'Jimmy Page ', N'8177', N'1077', 1)
GO
INSERT [TB_USER] ([ID], [Name], [Nickname], [RG], [CPF], [Enabled]) VALUES (289, N'Joe Perry ', N'Joe Perry ', N'8178', N'1078', 1)
GO
INSERT [TB_USER] ([ID], [Name], [Nickname], [RG], [CPF], [Enabled]) VALUES (290, N'Joe Satriani', N'Joe Satriani', N'8179', N'1079', 1)
GO
INSERT [TB_USER] ([ID], [Name], [Nickname], [RG], [CPF], [Enabled]) VALUES (291, N'Joe Strummer ', N'Joe Strummer ', N'8180', N'1080', 1)
GO
INSERT [TB_USER] ([ID], [Name], [Nickname], [RG], [CPF], [Enabled]) VALUES (292, N'John Frusciante ', N'John Frusciante ', N'8181', N'1081', 1)
GO
INSERT [TB_USER] ([ID], [Name], [Nickname], [RG], [CPF], [Enabled]) VALUES (293, N'John Lennon ', N'John Lennon ', N'8182', N'1082', 1)
GO
INSERT [TB_USER] ([ID], [Name], [Nickname], [RG], [CPF], [Enabled]) VALUES (294, N'John Mayer', N'John Mayer', N'8183', N'1083', 1)
GO
INSERT [TB_USER] ([ID], [Name], [Nickname], [RG], [CPF], [Enabled]) VALUES (295, N'John Norum ', N'John Norum ', N'8184', N'1084', 1)
GO
INSERT [TB_USER] ([ID], [Name], [Nickname], [RG], [CPF], [Enabled]) VALUES (296, N'John Squire ', N'John Squire ', N'8185', N'1085', 1)
GO
INSERT [TB_USER] ([ID], [Name], [Nickname], [RG], [CPF], [Enabled]) VALUES (297, N'Johnny Marr', N'Johnny Marr', N'8186', N'1086', 1)
GO
INSERT [TB_USER] ([ID], [Name], [Nickname], [RG], [CPF], [Enabled]) VALUES (298, N'Johnny Ramone ', N'Johnny Ramone ', N'8187', N'1087', 1)
GO
INSERT [TB_USER] ([ID], [Name], [Nickname], [RG], [CPF], [Enabled]) VALUES (299, N'Johnny Thunder', N'Johnny Thunder', N'8188', N'1088', 1)
GO
INSERT [TB_USER] ([ID], [Name], [Nickname], [RG], [CPF], [Enabled]) VALUES (300, N'Jordy Gomes ', N'Jordy Gomes ', N'8189', N'1089', 1)
GO
INSERT [TB_USER] ([ID], [Name], [Nickname], [RG], [CPF], [Enabled]) VALUES (301, N'Josh Homme ', N'Josh Homme ', N'8190', N'1090', 1)
GO
INSERT [TB_USER] ([ID], [Name], [Nickname], [RG], [CPF], [Enabled]) VALUES (302, N'Juninho Afram ', N'Juninho Afram ', N'8191', N'1091', 1)
GO
INSERT [TB_USER] ([ID], [Name], [Nickname], [RG], [CPF], [Enabled]) VALUES (303, N'Kai Hansen ', N'Kai Hansen ', N'8192', N'1092', 1)
GO
INSERT [TB_USER] ([ID], [Name], [Nickname], [RG], [CPF], [Enabled]) VALUES (304, N'Kee Marcello', N'Kee Marcello', N'8193', N'1093', 1)
GO
INSERT [TB_USER] ([ID], [Name], [Nickname], [RG], [CPF], [Enabled]) VALUES (305, N'Keith Richards', N'Keith Richards', N'8194', N'1094', 1)
GO
INSERT [TB_USER] ([ID], [Name], [Nickname], [RG], [CPF], [Enabled]) VALUES (306, N'Kim Mitchell', N'Kim Mitchell', N'8195', N'1095', 1)
GO
INSERT [TB_USER] ([ID], [Name], [Nickname], [RG], [CPF], [Enabled]) VALUES (307, N'Kurt Cobain ', N'Kurt Cobain ', N'8196', N'1096', 1)
GO
INSERT [TB_USER] ([ID], [Name], [Nickname], [RG], [CPF], [Enabled]) VALUES (308, N'Lindsey Buckingham ', N'Lindsey Buckingham ', N'8197', N'1097', 1)
GO
INSERT [TB_USER] ([ID], [Name], [Nickname], [RG], [CPF], [Enabled]) VALUES (309, N'Malcolm Young ', N'Malcolm Young ', N'8198', N'1098', 1)
GO
INSERT [TB_USER] ([ID], [Name], [Nickname], [RG], [CPF], [Enabled]) VALUES (310, N'Mark Knopfler ', N'Mark Knopfler ', N'8199', N'1099', 1)
GO
INSERT [TB_USER] ([ID], [Name], [Nickname], [RG], [CPF], [Enabled]) VALUES (311, N'Mark Tremonti', N'Mark Tremonti', N'8200', N'1100', 1)
GO
INSERT [TB_USER] ([ID], [Name], [Nickname], [RG], [CPF], [Enabled]) VALUES (312, N'Martin Noble ', N'Martin Noble ', N'8201', N'1101', 1)
GO
INSERT [TB_USER] ([ID], [Name], [Nickname], [RG], [CPF], [Enabled]) VALUES (313, N'Matthew Bellamy ', N'Matthew Bellamy ', N'8202', N'1102', 1)
GO
INSERT [TB_USER] ([ID], [Name], [Nickname], [RG], [CPF], [Enabled]) VALUES (314, N'Mattias Eklundh ', N'Mattias Eklundh ', N'8203', N'1103', 1)
GO
INSERT [TB_USER] ([ID], [Name], [Nickname], [RG], [CPF], [Enabled]) VALUES (315, N'Michael Angelo Batio', N'Michael Angelo Batio', N'8204', N'1104', 1)
GO
INSERT [TB_USER] ([ID], [Name], [Nickname], [RG], [CPF], [Enabled]) VALUES (316, N'Michael Monarch ', N'Michael Monarch ', N'8205', N'1105', 1)
GO
INSERT [TB_USER] ([ID], [Name], [Nickname], [RG], [CPF], [Enabled]) VALUES (317, N'Michael Nesmith ', N'Michael Nesmith ', N'8206', N'1106', 1)
GO
INSERT [TB_USER] ([ID], [Name], [Nickname], [RG], [CPF], [Enabled]) VALUES (318, N'Michael Olga Algar ', N'Michael Olga Algar ', N'8207', N'1107', 1)
GO
INSERT [TB_USER] ([ID], [Name], [Nickname], [RG], [CPF], [Enabled]) VALUES (319, N'Michael Roe', N'Michael Roe', N'8208', N'1108', 1)
GO
INSERT [TB_USER] ([ID], [Name], [Nickname], [RG], [CPF], [Enabled]) VALUES (320, N'Michel Petrella', N'Michel Petrella', N'8209', N'1109', 1)
GO
INSERT [TB_USER] ([ID], [Name], [Nickname], [RG], [CPF], [Enabled]) VALUES (321, N'Mick Jones ', N'Mick Jones ', N'8210', N'1110', 1)
GO
INSERT [TB_USER] ([ID], [Name], [Nickname], [RG], [CPF], [Enabled]) VALUES (322, N'Mick Mars ', N'Mick Mars ', N'8211', N'1111', 1)
GO
INSERT [TB_USER] ([ID], [Name], [Nickname], [RG], [CPF], [Enabled]) VALUES (323, N'Mick Ronson ', N'Mick Ronson ', N'8212', N'1112', 1)
GO
INSERT [TB_USER] ([ID], [Name], [Nickname], [RG], [CPF], [Enabled]) VALUES (324, N'Mike McCready ', N'Mike McCready ', N'8213', N'1113', 1)
GO
INSERT [TB_USER] ([ID], [Name], [Nickname], [RG], [CPF], [Enabled]) VALUES (325, N'Miyavi', N'Miyavi', N'8214', N'1114', 1)
GO
INSERT [TB_USER] ([ID], [Name], [Nickname], [RG], [CPF], [Enabled]) VALUES (326, N'Nancy Wilson', N'Nancy Wilson', N'8215', N'1115', 1)
GO
INSERT [TB_USER] ([ID], [Name], [Nickname], [RG], [CPF], [Enabled]) VALUES (327, N'Neal Schon', N'Neal Schon', N'8216', N'1116', 1)
GO
INSERT [TB_USER] ([ID], [Name], [Nickname], [RG], [CPF], [Enabled]) VALUES (328, N'Neil Young ', N'Neil Young ', N'8217', N'1117', 1)
GO
INSERT [TB_USER] ([ID], [Name], [Nickname], [RG], [CPF], [Enabled]) VALUES (329, N'Nick Valensi ', N'Nick Valensi ', N'8218', N'1118', 1)
GO
INSERT [TB_USER] ([ID], [Name], [Nickname], [RG], [CPF], [Enabled]) VALUES (330, N'Noodles ', N'Noodles ', N'8219', N'1119', 1)
GO
INSERT [TB_USER] ([ID], [Name], [Nickname], [RG], [CPF], [Enabled]) VALUES (331, N'Noel Gallagher', N'Noel Gallagher', N'8220', N'1120', 1)
GO
INSERT [TB_USER] ([ID], [Name], [Nickname], [RG], [CPF], [Enabled]) VALUES (332, N'Norman Blake ', N'Norman Blake ', N'8221', N'1121', 1)
GO
INSERT [TB_USER] ([ID], [Name], [Nickname], [RG], [CPF], [Enabled]) VALUES (333, N'Olga ', N'Olga ', N'8222', N'1122', 1)
GO
INSERT [TB_USER] ([ID], [Name], [Nickname], [RG], [CPF], [Enabled]) VALUES (334, N'Paul Gilbert ', N'Paul Gilbert ', N'8223', N'1123', 1)
GO
INSERT [TB_USER] ([ID], [Name], [Nickname], [RG], [CPF], [Enabled]) VALUES (335, N'Paul Simon ', N'Paul Simon ', N'8224', N'1124', 1)
GO
INSERT [TB_USER] ([ID], [Name], [Nickname], [RG], [CPF], [Enabled]) VALUES (336, N'Paul Stanley ', N'Paul Stanley ', N'8225', N'1125', 1)
GO
INSERT [TB_USER] ([ID], [Name], [Nickname], [RG], [CPF], [Enabled]) VALUES (337, N'Paul Westerberg ', N'Paul Westerberg ', N'8226', N'1126', 1)
GO
INSERT [TB_USER] ([ID], [Name], [Nickname], [RG], [CPF], [Enabled]) VALUES (338, N'Paul Weller ', N'Paul Weller ', N'8227', N'1127', 1)
GO
INSERT [TB_USER] ([ID], [Name], [Nickname], [RG], [CPF], [Enabled]) VALUES (339, N'Perry Bamonte', N'Perry Bamonte', N'8228', N'1128', 1)
GO
INSERT [TB_USER] ([ID], [Name], [Nickname], [RG], [CPF], [Enabled]) VALUES (340, N'Pedro Castilho ', N'Pedro Castilho ', N'8229', N'1129', 1)
GO
INSERT [TB_USER] ([ID], [Name], [Nickname], [RG], [CPF], [Enabled]) VALUES (341, N'Pete Shelley ', N'Pete Shelley ', N'8230', N'1130', 1)
GO
INSERT [TB_USER] ([ID], [Name], [Nickname], [RG], [CPF], [Enabled]) VALUES (342, N'Pete Townshend ', N'Pete Townshend ', N'8231', N'1131', 1)
GO
INSERT [TB_USER] ([ID], [Name], [Nickname], [RG], [CPF], [Enabled]) VALUES (343, N'Peter Buck ', N'Peter Buck ', N'8232', N'1132', 1)
GO
INSERT [TB_USER] ([ID], [Name], [Nickname], [RG], [CPF], [Enabled]) VALUES (344, N'Phil Keaggy', N'Phil Keaggy', N'8233', N'1133', 1)
GO
INSERT [TB_USER] ([ID], [Name], [Nickname], [RG], [CPF], [Enabled]) VALUES (345, N'Porl Thompson', N'Porl Thompson', N'8234', N'1134', 1)
GO
INSERT [TB_USER] ([ID], [Name], [Nickname], [RG], [CPF], [Enabled]) VALUES (346, N'Prince', N'Prince', N'8235', N'1135', 1)
GO
INSERT [TB_USER] ([ID], [Name], [Nickname], [RG], [CPF], [Enabled]) VALUES (347, N'Ray Davies ', N'Ray Davies ', N'8236', N'1136', 1)
GO
INSERT [TB_USER] ([ID], [Name], [Nickname], [RG], [CPF], [Enabled]) VALUES (348, N'Rick Parfitt ', N'Rick Parfitt ', N'8237', N'1137', 1)
GO
INSERT [TB_USER] ([ID], [Name], [Nickname], [RG], [CPF], [Enabled]) VALUES (349, N'Richie Sambora ', N'Richie Sambora ', N'8238', N'1138', 1)
GO
INSERT [TB_USER] ([ID], [Name], [Nickname], [RG], [CPF], [Enabled]) VALUES (350, N'Ritchie Blackmore ', N'Ritchie Blackmore ', N'8239', N'1139', 1)
GO
INSERT [TB_USER] ([ID], [Name], [Nickname], [RG], [CPF], [Enabled]) VALUES (351, N'Robert Fripp', N'Robert Fripp', N'8240', N'1140', 1)
GO
INSERT [TB_USER] ([ID], [Name], [Nickname], [RG], [CPF], [Enabled]) VALUES (352, N'Robert Smith', N'Robert Smith', N'8241', N'1141', 1)
GO
INSERT [TB_USER] ([ID], [Name], [Nickname], [RG], [CPF], [Enabled]) VALUES (353, N'Roger McGuinn ', N'Roger McGuinn ', N'8242', N'1142', 1)
GO
INSERT [TB_USER] ([ID], [Name], [Nickname], [RG], [CPF], [Enabled]) VALUES (354, N'Ron Wood ', N'Ron Wood ', N'8243', N'1143', 1)
GO
INSERT [TB_USER] ([ID], [Name], [Nickname], [RG], [CPF], [Enabled]) VALUES (355, N'Ron Asheton', N'Ron Asheton', N'8244', N'1144', 1)
GO
INSERT [TB_USER] ([ID], [Name], [Nickname], [RG], [CPF], [Enabled]) VALUES (356, N'Ryan Peake', N'Ryan Peake', N'8245', N'1145', 1)
GO
INSERT [TB_USER] ([ID], [Name], [Nickname], [RG], [CPF], [Enabled]) VALUES (357, N'Sebastien Lefebvre', N'Sebastien Lefebvre', N'8246', N'1146', 1)
GO
INSERT [TB_USER] ([ID], [Name], [Nickname], [RG], [CPF], [Enabled]) VALUES (358, N'Shawn Lane', N'Shawn Lane', N'8247', N'1147', 1)
GO
INSERT [TB_USER] ([ID], [Name], [Nickname], [RG], [CPF], [Enabled]) VALUES (359, N'Sheldon Reynolds ', N'Sheldon Reynolds ', N'8248', N'1148', 1)
GO
INSERT [TB_USER] ([ID], [Name], [Nickname], [RG], [CPF], [Enabled]) VALUES (360, N'Slash', N'Slash', N'8249', N'1149', 1)
GO
INSERT [TB_USER] ([ID], [Name], [Nickname], [RG], [CPF], [Enabled]) VALUES (361, N'Steve Diggle ', N'Steve Diggle ', N'8250', N'1150', 1)
GO
INSERT [TB_USER] ([ID], [Name], [Nickname], [RG], [CPF], [Enabled]) VALUES (362, N'Steve Howe', N'Steve Howe', N'8251', N'1151', 1)
GO
INSERT [TB_USER] ([ID], [Name], [Nickname], [RG], [CPF], [Enabled]) VALUES (363, N'Steve Jones ', N'Steve Jones ', N'8252', N'1152', 1)
GO

SET IDENTITY_INSERT [TB_USER] OFF
GO

ALTER TABLE [TB_USER] ADD  CONSTRAINT [UK_TB_USER_CPF] UNIQUE NONCLUSTERED 
(
	[CPF] ASC
)
GO

ALTER TABLE [TB_USER] ADD  CONSTRAINT [UK_TB_USER_RG] UNIQUE NONCLUSTERED 
(
	[RG] ASC
)
GO

ALTER TABLE [TB_ADDRESS]  WITH CHECK ADD  CONSTRAINT [FK_TB_ADDRESS_TB_USER] FOREIGN KEY([UserCode]) REFERENCES [TB_USER] ([ID])
GO

ALTER TABLE [TB_ADDRESS] CHECK CONSTRAINT [FK_TB_ADDRESS_TB_USER] 
GO

CREATE PROCEDURE [SP_DROP_IF_EXISTS]
(
	@Name	VARCHAR(255)
)
AS
BEGIN

	DECLARE @TYPE CHAR(2) = (SELECT TYPE FROM SYS.objects WHERE NAME = @Name)

	SET @TYPE = REPLACE(@TYPE, ' ', '')

	IF(LTRIM(RTRIM(@TYPE)) IN('U','P'))
	BEGIN
		
		DECLARE @CMD VARCHAR(255) =''

		IF(@TYPE = 'U')
		BEGIN
			IF EXISTS(SELECT NAME FROM sys.objects WHERE name=@Name AND type=@TYPE)
			BEGIN
				SET @CMD += 'DROP TABLE '+ @Name
			END
		END
		
		IF(@TYPE = 'P')
		BEGIN
			IF EXISTS(SELECT NAME FROM sys.objects WHERE name=@Name AND type=@TYPE)
			BEGIN
				SET @CMD += 'DROP PROCEDURE '+ @Name
			END
		END

		-- See the output
		PRINT @CMD

		-- Execute the t-sql statement
		EXEC(@CMD)
		
	END
	ELSE
	BEGIN

		IF(@TYPE IS NULL)
		BEGIN
			PRINT 'The current object does not exist in the current database'
		END
		ELSE
		BEGIN
			PRINT 'SQL DBType not allowed at the moment. Type: ' + @TYPE
		END
	END

END
GO

CREATE PROCEDURE [SP_USER_D]
(
	@P_ID INT
)
AS
BEGIN
 
	DELETE FROM TB_ADDRESS
	WHERE USERCODE = @P_ID

	DELETE FROM TB_USER
	WHERE ID = @P_ID

END
GO

CREATE PROCEDURE [SP_USER_I]
(
	@P_ID		INT = NULL OUTPUT,
	@P_Name		varchar(80),
	@P_Nickname	varchar(50),
	@P_RG		varchar(50),
	@P_CPF		varchar(50),
	@P_Enabled	bit,	
	@P_TVP_ADDRESS TVP_ADDRESS READONLY
 )
 AS
 BEGIN
 
	IF EXISTS
	(
		SELECT 1 FROM TB_USER WITH(NOLOCK)
		WHERE CPF = @P_CPF
	)
	BEGIN
		RAISERROR ('There is already a user with this CPF.Please, try another one.', 16, 1);
	END
	ELSE
	BEGIN
	
		BEGIN TRY

			BEGIN TRANSACTION

			INSERT TB_USER
			(
				Name,
				Nickname,
				RG,
				CPF,
				Enabled
			)
			VALUES
			(
				@P_Name,
				@P_Nickname,
				@P_RG,
				@P_CPF,
				@P_Enabled
			)

			SET @P_ID = @@IDENTITY

			-- Insert the new ones
			INSERT TB_ADDRESS
			SELECT 
				Address,
				@P_ID,
				Enabled
			FROM @P_TVP_ADDRESS

			COMMIT TRANSACTION

		END TRY
		BEGIN CATCH

			IF(@@TRANCOUNT !=0)
			BEGIN
				ROLLBACK TRANSACTION
			END
			
		END CATCH

	END

END
GO

CREATE PROCEDURE [SP_USER_S]
AS
BEGIN
 
	SELECT 
		ID,
		Name,
		Nickname,
		RG,
		CPF,
		Enabled
	FROM TB_USER A WITH(NOLOCK)
	ORDER BY Name


 END
GO

CREATE PROCEDURE [SP_USER_S_BY_ID]
(
	@P_ID INT
)
AS
BEGIN
 
	SELECT 
		ID,
		Name,
		Nickname,
		RG,
		CPF,
		Enabled
	FROM TB_USER A WITH(NOLOCK)

	WHERE A.ID = @P_ID

	-- Get all address
	SELECT 
		A.ID,
		A.Address,
		A.Enabled
	FROM TB_ADDRESS A WITH(NOLOCK)

	WHERE A.UserCode = @P_ID

END
GO

CREATE PROCEDURE [SP_USER_U]
(
	@P_ID INT,
	@P_Name		varchar(80),
	@P_Nickname	varchar(50),
	@P_RG		varchar(50),
	@P_CPF		varchar(50),
	@P_Enabled	bit,	
	@P_TVP_ADDRESS TVP_ADDRESS READONLY
 )
 AS
 BEGIN
 
	BEGIN TRY

		BEGIN TRANSACTION

		SET @P_Name = ' UPDATED AT ' + CONVERT(VARCHAR, GETDATE(),131)
		
		UPDATE TB_USER SET
			Name		= Name + @P_Name
			--Nickname	=  @P_Nickname,
			--RG		= @P_RG,
			--CP		= @P_CPF,			
			--Enabled	= @P_Enabled
		WHERE ID = @P_ID
		
		-- Delete the previous address
		DELETE FROM TB_ADDRESS WHERE USERCODE = @P_ID
	
		-- Insert the new ones
		INSERT TB_ADDRESS
		SELECT 
			Address,
			UserCode,
			Enabled
		FROM @P_TVP_ADDRESS			

		COMMIT TRANSACTION

	END TRY
	BEGIN CATCH
		
		IF(@@TRANCOUNT>0)
		BEGIN
			ROLLBACK TRANSACTION
			SELECT ERROR_MESSAGE()
		END

	END CATCH

 END
GO
ALTER DATABASE [DEMO] SET  READ_WRITE 
GO